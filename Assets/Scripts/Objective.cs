using UnityEngine;
using TMPro;

public class Objective : MonoBehaviour
{
    public TextMeshProUGUI objectiveText;  // Reference to the TextMeshProUGUI component
    private int enemiesDefeated = 0;
    private int totalEnemies = 15;  // Set your total objective count here

    void Start()
    {
        UpdateObjectiveText();
    }

    // Method to call when an enemy dies
    public void EnemyDefeated()
    {
        enemiesDefeated++;
        UpdateObjectiveText();
    }

    // Update the text to show the current objective status
    private void UpdateObjectiveText()
    {
        objectiveText.text = $"Clean Up {enemiesDefeated}/{totalEnemies}";
    }
}
