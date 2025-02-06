using UnityEngine;
using System;

public class PropVisibility : MonoBehaviour
{
    public bool enableByDate = true;
    public bool enableByLightLevel = false;
    public Light sceneLight; 
    public float requiredLightThreshold = 0.3f;
    public string showOnDate = "12/25"; // Default to Christmas

    private Renderer propRenderer;

    void Start()
    {
        propRenderer = GetComponent<Renderer>();
        UpdateVisibility();
    }

    void Update()
    {
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        bool dateMatch = enableByDate && DateTime.Now.ToString("MM/dd") == showOnDate;
        bool lightMatch = enableByLightLevel && sceneLight != null && sceneLight.intensity <= requiredLightThreshold;

        propRenderer.enabled = dateMatch || lightMatch;
    }
}
