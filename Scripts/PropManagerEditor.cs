#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class PropManagerEditor : EditorWindow
{
    private GameObject propPrefab;
    private GameObject areaVisualizer;
    private List<GameObject> placedProps = new List<GameObject>();
    private string propName = "";
    private Vector3 areaSize = new Vector3(3, 3, 3);
    
    private string[] propTypes = {"Christmas)", "Halloween", "Custom", "No Date (Always Visible)" };
    private string[] presetDates = { "12/25", "10/31", "", "" };
    private int selectedPropType = 0;
    private bool showCustomDateField = false;
    private bool alwaysVisible = false;
    private string customEnableDate = "MM/DD";

    private bool randomRotation = false;
    private bool snapToGround = false;
    private bool avoidColliders = true;
    private int numPropsToPlace = 1;
    private GameObject parentObject;

    private readonly string visibilityScriptPath = "Assets/NoHeartCub/Scripts/PropVisibility.cs";

    [MenuItem("NoHeartCub/World/Tools/Prop Manager")]
    public static void ShowWindow()
    {
        GetWindow<PropManagerEditor>("Prop Manager");
    }

    private void OnGUI()
    {
        GUILayout.Label("📌 Prop Placement Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // ---- General Settings ----
        EditorGUILayout.LabelField("🛠 General Settings", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        propPrefab = (GameObject)EditorGUILayout.ObjectField("Prop Prefab", propPrefab, typeof(GameObject), false);
        propName = EditorGUILayout.TextField("Prop Name", propName);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // ---- Prop Type Settings ----
        EditorGUILayout.LabelField("🎨 Prop Type & Visibility", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        selectedPropType = EditorGUILayout.Popup("Prop Type", selectedPropType, propTypes);

        if (selectedPropType == 4)  // Custom Date Option
        {
            showCustomDateField = true;
            alwaysVisible = false;
            customEnableDate = EditorGUILayout.TextField("Custom Enable Date (MM/DD)", customEnableDate);
        }
        else if (selectedPropType == 5)  // No Date = Always Visible
        {
            alwaysVisible = true;
            showCustomDateField = false;
        }
        else
        {
            showCustomDateField = false;
            alwaysVisible = false;
        }

        randomRotation = EditorGUILayout.Toggle("Random Rotation", randomRotation);
        snapToGround = EditorGUILayout.Toggle("Snap to Ground", snapToGround);
        avoidColliders = EditorGUILayout.Toggle("Avoid Collisions", avoidColliders);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // ---- Placement Settings ----
        EditorGUILayout.LabelField("📍 Placement Settings", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        areaSize = EditorGUILayout.Vector3Field("Placement Area Size", areaSize);
        numPropsToPlace = EditorGUILayout.IntSlider("Number of Props", numPropsToPlace, 1, 50);
        parentObject = (GameObject)EditorGUILayout.ObjectField("Parent Object", parentObject, typeof(GameObject), true);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // ---- Actions ----
        EditorGUILayout.LabelField("⚙️ Actions", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");

        if (GUILayout.Button("Create Placement Area"))
        {
            CreatePlacementArea();
        }

        if (GUILayout.Button("Place Props Inside Area"))
        {
            PlaceProps();
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

        areaVisualizer = new GameObject("PropPlacementArea");
        areaVisualizer.transform.position = Vector3.zero;
        areaVisualizer.transform.localScale = areaSize;
        BoxCollider collider = areaVisualizer.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        Debug.Log("Placement area created.");
    }

    private void PlaceProps()
    {
        if (propPrefab == null || areaVisualizer == null)
        {
            Debug.LogError("Assign a Prop prefab and create the placement area first!");
            return;
        }

        if (parentObject == null)
        {
            parentObject = new GameObject("PlacedProps");
        }

        for (int i = 0; i < numPropsToPlace; i++)
        {
            Vector3 randomPosition = GenerateValidPosition();
            Quaternion rotation = randomRotation ? Quaternion.Euler(0, Random.Range(0, 360), 0) : Quaternion.identity;

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(propPrefab);
            instance.transform.position = randomPosition;
            instance.transform.rotation = rotation;
            instance.transform.SetParent(parentObject.transform);
            instance.name = string.IsNullOrEmpty(propName) ? $"Prop_{placedProps.Count + 1}" : propName;
            placedProps.Add(instance);

            string enableDate = alwaysVisible ? "" : (selectedPropType == 4 ? customEnableDate : presetDates[selectedPropType]);

            AttachVisibilityScript(instance, enableDate);
        }

        DestroyImmediate(areaVisualizer);
    }

    private Vector3 GenerateValidPosition()
    {
        Vector3 areaCenter = areaVisualizer.transform.position;
        Vector3 areaScale = areaVisualizer.transform.localScale;
        Vector3 randomPosition = new Vector3(
            Random.Range(areaCenter.x - areaScale.x / 2, areaCenter.x + areaScale.x / 2),
            Random.Range(areaCenter.y - areaScale.y / 2, areaCenter.y + areaScale.y / 2),
            Random.Range(areaCenter.z - areaScale.z / 2, areaCenter.z + areaScale.z / 2)
        );

        if (snapToGround)
        {
            if (Physics.Raycast(randomPosition + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f))
            {
                randomPosition = hit.point;
            }
        }

        if (avoidColliders)
        {
            Collider[] colliders = Physics.OverlapSphere(randomPosition, 1f);
            if (colliders.Length > 0)
            {
                return GenerateValidPosition();
            }
        }

        return randomPosition;
    }

    private void AttachVisibilityScript(GameObject propInstance, string enableDate)
    {
        if (!File.Exists(visibilityScriptPath))
        {
            Debug.LogWarning($"PropVisibility.cs not found at {visibilityScriptPath}. Skipping script attachment.");
            return;
        }

        if (propInstance.GetComponent<PropVisibility>() == null)
        {
            PropVisibility visibility = propInstance.AddComponent<PropVisibility>();
            visibility.showOnDate = enableDate;
            Debug.Log($"PropVisibility script added to {propInstance.name}");
        }
    }
}
#endif
