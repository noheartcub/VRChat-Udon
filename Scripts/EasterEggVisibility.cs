using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class EasterEggVisibility : UdonSharpBehaviour
{
    // Visibility conditions
    [Tooltip("Enable visibility based on a specific date (MM/DD format).")]
    public bool enableByDate = true;

    [Tooltip("Date on which the Easter Egg should be visible (e.g., 12/25 for Christmas).")]
    public string showOnDate = "12/25";

    private Renderer eggRenderer;

    private void Start()
    {
        eggRenderer = GetComponent<Renderer>();
        UpdateVisibility();
    }

    private void Update()
    {
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        if (!enableByDate || string.IsNullOrWhiteSpace(showOnDate))
        {
            // Always visible if date is not set or disabled
            eggRenderer.enabled = true;
            return;
        }

        // Check if the current date matches the configured date
        string currentDate = GetCurrentDate();
        eggRenderer.enabled = currentDate == showOnDate;
    }

    private string GetCurrentDate()
    {
        // In Udon, we rely on VRC methods or constants instead of System.DateTime.
        // This can be a placeholder for manual configuration or future expansion.
        // For now, assume a date of "MM/DD".
        return "12/25"; // Example static date for Udon compatibility
    }
}
