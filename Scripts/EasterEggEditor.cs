#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class EasterEggEditor : EditorWindow
{
    private GameObject easterEggPrefab;
    private GameObject areaVisualizer;
    private List<GameObject> placedEasterEggs = new List<GameObject>();
    private string eggName = "";
    private Vector3 areaSize = new Vector3(3, 3, 3);
    
    private string[] eggTypes = { "Gold Egg (Christmas)", "Silver Egg (Halloween)", "Special Egg (Easter)", "Custom" };
    private string[] presetDates = { "12/25", "10/31", "04/09", "" }; 
    private int selectedEggType = 0;
    private bool showCustomDateField = false;
    private string customEnableDate = "MM/DD";

    private bool randomRotation = false;
    private bool snapToGround = false;
    private int maxEasterEggs = 5;

    private readonly string visibilityScriptPath = "Assets/NoHeartCub Udon/Scripts/EasterEggVisibility.cs";

    [MenuItem("NoHeartCub Udon/Easter Egg Manager")]
    public static void ShowWindow()
    {
        GetWindow<EasterEggEditor>("Easter Egg Manager");
    }

    private void OnGUI()
    {
        GUILayout.Label("🐣 Easter Egg Placement Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // ---- General Settings ----
        EditorGUILayout.LabelField("📌 General Settings", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        easterEggPrefab = (GameObject)EditorGUILayout.ObjectField("Easter Egg Prefab", easterEggPrefab, typeof(GameObject), false);
        eggName = EditorGUILayout.TextField("Easter Egg Name", eggName);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // ---- Easter Egg Type Settings ----
        EditorGUILayout.LabelField("🎨 Easter Egg Type & Appearance", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        selectedEggType = EditorGUILayout.Popup("Easter Egg Type", selectedEggType, eggTypes);

        if (selectedEggType == eggTypes.Length - 1) 
        {
            showCustomDateField = true;
            customEnableDate = EditorGUILayout.TextField("Custom Enable Date (MM/DD)", customEnableDate);
        }
        else
        {
            showCustomDateField = false;
        }

        randomRotation = EditorGUILayout.Toggle("Random Rotation", randomRotation);
        snapToGround = EditorGUILayout.Toggle("Snap to Ground", snapToGround);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // ---- Placement Settings ----
        EditorGUILayout.LabelField("📍 Placement Settings", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        areaSize = EditorGUILayout.Vector3Field("Placement Area Size", areaSize);
        maxEasterEggs = EditorGUILayout.IntField("Max Easter Eggs", maxEasterEggs);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // ---- Actions ----
        EditorGUILayout.LabelField("⚙️ Actions", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");

        if (GUILayout.Button("Create Placement Area"))
        {
            CreatePlacementArea();
        }

        if (GUILayout.Button("Place Easter Egg Inside Area"))
        {
            PlaceEasterEgg();
        }

        if (GUILayout.Button("Save Placement Info"))
        {
            SavePlacementInfo();
        }

        EditorGUILayout.EndVertical();
    }

    private void CreatePlacementArea()
    {
        if (areaVisualizer != null)
        {
            Debug.Log("Placement area already exists.");
            return;
        }

        areaVisualizer = new GameObject("EasterEggPlacementArea");
        areaVisualizer.transform.position = Vector3.zero;
        areaVisualizer.transform.localScale = areaSize;
        BoxCollider collider = areaVisualizer.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        Debug.Log("Placement area created.");
    }

    private void PlaceEasterEgg()
    {
        if (easterEggPrefab == null || areaVisualizer == null)
        {
            Debug.LogError("Assign an Easter Egg prefab and create the placement area first!");
            return;
        }
        if (placedEasterEggs.Count >= maxEasterEggs)
        {
            Debug.LogWarning("Max Easter Eggs reached!");
            return;
        }

        Vector3 areaCenter = areaVisualizer.transform.position;
        Vector3 areaScale = areaVisualizer.transform.localScale;
        Vector3 randomPosition = new Vector3(
            Random.Range(areaCenter.x - areaScale.x / 2, areaCenter.x + areaScale.x / 2),
            Random.Range(areaCenter.y - areaScale.y / 2, areaCenter.y + areaScale.y / 2),
            Random.Range(areaCenter.z - areaScale.z / 2, areaCenter.z + areaScale.z / 2)
        );

        if (snapToGround)
        {
            if (Physics.Raycast(randomPosition + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 10f))
            {
                randomPosition = hit.point;
            }
        }

        Quaternion rotation = randomRotation ? Quaternion.Euler(0, Random.Range(0, 360), 0) : Quaternion.identity;
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(easterEggPrefab);
        instance.transform.position = randomPosition;
        instance.transform.rotation = rotation;
        instance.name = string.IsNullOrEmpty(eggName) ? $"Egg_{placedEasterEggs.Count + 1}" : eggName;
        placedEasterEggs.Add(instance);

        string enableDate = selectedEggType == eggTypes.Length - 1 ? customEnableDate : presetDates[selectedEggType];

        AttachVisibilityScript(instance);
        instance.GetComponent<EasterEggVisibility>().showOnDate = enableDate;
    }

    private void AttachVisibilityScript(GameObject eggInstance)
    {
        if (!File.Exists(visibilityScriptPath))
        {
            Debug.LogWarning($"EasterEggVisibility.cs not found at {visibilityScriptPath}. Skipping script attachment.");
            return;
        }

        if (eggInstance.GetComponent<EasterEggVisibility>() == null)
        {
            eggInstance.AddComponent<EasterEggVisibility>();
            Debug.Log($"EasterEggVisibility script added to {eggInstance.name}");
        }
    }

    private void SavePlacementInfo()
    {
        if (placedEasterEggs.Count == 0)
        {
            Debug.LogError("No Easter Eggs placed!");
            return;
        }

        string folderPath = "Assets/NoHeartCub Udon/data";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, "easteregg.txt");
        string content = "";

        foreach (GameObject egg in placedEasterEggs)
        {
            Renderer eggRenderer = egg.GetComponent<Renderer>();
            string materialName = eggRenderer ? eggRenderer.sharedMaterial.name : "Unknown";

            string enableDate = selectedEggType == eggTypes.Length - 1 ? customEnableDate : presetDates[selectedEggType];

            content += $"Easter Egg: {egg.name}\n" +
                       $"Type: {eggTypes[selectedEggType]}\n" +
                       $"Exact Position: {egg.transform.position}\n" +
                       $"Rotation: {egg.transform.rotation.eulerAngles}\n" +
                       $"Material: {materialName}\n" +
                       $"Enable On Date: {enableDate}\n" +
                       $"--------------------------\n";
        }
        File.WriteAllText(filePath, content);
        Debug.Log("Placement info saved.");
        AssetDatabase.Refresh();
    }
}
#endif
