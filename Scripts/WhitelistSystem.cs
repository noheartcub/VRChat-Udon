using UdonSharp;
using UnityEngine;
using UnityEditor;
using VRC.SDKBase;
using VRC.Udon;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

public class WhitelistSystem : UdonSharpBehaviour
{
    private string whitelistFolderPath = "Assets/NoHeartCub/Data/";
    private string selectedWhitelistFile = "whitelist_default.json";
    private List<string> whitelistedUsers = new List<string>();
    private List<string> allowedTrustLevels = new List<string>();

    private static readonly string[] defaultTrustLevels = { "Visitor", "New User", "User", "Known User", "Trusted User" };

    public int whitelistType = 0; // 0 = Display Name, 1 = Trust Level
    public Transform teleportLocation;
    public GameObject[] togglesForWhitelisted;
    public GameObject[] togglesForNonWhitelisted;
    public UdonBehaviour whitelistUdonHandler;

    #if UNITY_EDITOR
    [MenuItem("NoHeartCub/World/Tools/Whitelist System")]
    public static void ShowWindow()
    {
        WhitelistSystemEditor window = EditorWindow.GetWindow<WhitelistSystemEditor>("Whitelist System");
        window.Show();
    }
    #endif

    void Start()
    {
        LoadWhitelist();
    }

    public void LoadWhitelist(string fileName = "")
    {
        if (!string.IsNullOrEmpty(fileName))
        {
            selectedWhitelistFile = fileName;
        }

        string fullPath = Path.Combine(whitelistFolderPath, selectedWhitelistFile);
        if (!File.Exists(fullPath))
        {
            Debug.LogWarning("Whitelist file not found! Creating a new one.");
            SaveWhitelist();
            return;
        }

        string jsonData = File.ReadAllText(fullPath);
        var whitelistData = JsonConvert.DeserializeObject<WhitelistData>(jsonData);

        if (whitelistData != null)
        {
            whitelistedUsers = new List<string>(whitelistData.usernames);
            allowedTrustLevels = new List<string>(whitelistData.trustLevels);
            whitelistType = whitelistData.whitelistType;
        }
        Debug.Log("Whitelist loaded successfully: " + selectedWhitelistFile);
    }

    private void SaveWhitelist()
    {
        string fullPath = Path.Combine(whitelistFolderPath, selectedWhitelistFile);
        var whitelistData = new WhitelistData()
        {
            usernames = whitelistedUsers.ToArray(),
            trustLevels = allowedTrustLevels.ToArray(),
            whitelistType = whitelistType
        };

        string jsonData = JsonConvert.SerializeObject(whitelistData, Formatting.Indented);
        File.WriteAllText(fullPath, jsonData);
        Debug.Log("Whitelist saved successfully: " + fullPath);
    }
}

[System.Serializable]
public class WhitelistData
{
    public string[] usernames;
    public string[] trustLevels;
    public int whitelistType;
}

#if UNITY_EDITOR
public class WhitelistSystemEditor : EditorWindow
{
    private int selectedWhitelistIndex = 0;
    private List<string> availableWhitelistFiles = new List<string>();
    private string newWhitelistName = "";

    private List<string> whitelistedUsers = new List<string>();
    private List<string> allowedTrustLevels = new List<string>();
    private int whitelistType;

    private void OnEnable()
    {
        LoadWhitelistFiles();
    }

    private void LoadWhitelistFiles()
    {
        availableWhitelistFiles.Clear();
        availableWhitelistFiles.Add("Create New Whitelist");
        string path = "Assets/NoHeartCub/Data/";
        if (Directory.Exists(path))
        {
            foreach (string file in Directory.GetFiles(path, "whitelist_*.json"))
            {
                availableWhitelistFiles.Add(Path.GetFileName(file));
            }
        }
    }

    private void LoadSelectedWhitelist(string fileName)
    {
        string fullPath = "Assets/NoHeartCub/Data/" + fileName;
        if (File.Exists(fullPath))
        {
            string jsonData = File.ReadAllText(fullPath);
            var whitelistData = JsonConvert.DeserializeObject<WhitelistData>(jsonData);
            if (whitelistData != null)
            {
                whitelistedUsers = new List<string>(whitelistData.usernames);
                allowedTrustLevels = new List<string>(whitelistData.trustLevels);
                whitelistType = whitelistData.whitelistType;
            }
            Debug.Log("Whitelist settings loaded successfully: " + fileName);
        }
        else
        {
            Debug.LogWarning("Whitelist file not found: " + fileName);
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Whitelist System Editor", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Select an existing whitelist file or create a new one.", MessageType.Info);

        selectedWhitelistIndex = EditorGUILayout.Popup("Select Whitelist", selectedWhitelistIndex, availableWhitelistFiles.ToArray());
        
        if (selectedWhitelistIndex == 0)
        {
            newWhitelistName = EditorGUILayout.TextField("New Whitelist Name", newWhitelistName);
            if (!string.IsNullOrWhiteSpace(newWhitelistName))
            {
                string fullPath = "Assets/NoHeartCub/Data/whitelist_" + newWhitelistName + ".json";
                if (GUILayout.Button("Create New Whitelist"))
                {
                    if (!File.Exists(fullPath))
                    {
                        File.WriteAllText(fullPath, "{\"usernames\":[],\"trustLevels\":[],\"whitelistType\":0}");
                        Debug.Log("New whitelist created: " + fullPath);
                        LoadWhitelistFiles();
                    }
                    else
                    {
                        Debug.LogWarning("Whitelist file already exists: " + fullPath);
                    }
                }
            }
        }
        else
        {
            if (GUILayout.Button("Load Selected Whitelist"))
            {
                string selectedFile = availableWhitelistFiles[selectedWhitelistIndex];
                LoadSelectedWhitelist(selectedFile);
            }
            
            GUILayout.Label("Whitelist Settings", EditorStyles.boldLabel);
            whitelistType = EditorGUILayout.Popup("Whitelist Type", whitelistType, new string[] { "Display Name", "Trust Level" });

            GUILayout.Label("Whitelisted Users", EditorStyles.boldLabel);
            for (int i = 0; i < whitelistedUsers.Count; i++)
            {
                whitelistedUsers[i] = EditorGUILayout.TextField("User " + (i + 1), whitelistedUsers[i]);
            }
            if (GUILayout.Button("Add User"))
            {
                whitelistedUsers.Add("");
            }
        }
    }
}
#endif
