#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class BlendShapeExplorer : EditorWindow
{
    private SkinnedMeshRenderer skinnedMeshRenderer; // Drag your SkinnedMeshRenderer here
    private Vector2 scrollPos; // For scrolling the list of blend shapes

    [MenuItem("NoHeartCub Udon/Blend Shape Explorer")]
    public static void ShowWindow()
    {
        GetWindow<BlendShapeExplorer>("Blend Shape Explorer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Blend Shape Explorer", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Drag and drop field for SkinnedMeshRenderer
        skinnedMeshRenderer = (SkinnedMeshRenderer)EditorGUILayout.ObjectField(
            "Skinned Mesh Renderer", 
            skinnedMeshRenderer, 
            typeof(SkinnedMeshRenderer), 
            true
        );

        if (skinnedMeshRenderer == null)
        {
            EditorGUILayout.HelpBox("Drag a SkinnedMeshRenderer to see its blend shapes.", MessageType.Info);
            return;
        }

        Mesh mesh = skinnedMeshRenderer.sharedMesh;

        if (mesh == null)
        {
            EditorGUILayout.HelpBox("The assigned SkinnedMeshRenderer has no mesh!", MessageType.Warning);
            return;
        }

        // Display blend shape names and indexes
        GUILayout.Label("Blend Shapes:", EditorStyles.boldLabel);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        for (int i = 0; i < mesh.blendShapeCount; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Index {i}:", GUILayout.Width(60));
            GUILayout.Label(mesh.GetBlendShapeName(i), EditorStyles.label);
            GUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }
}
#endif
