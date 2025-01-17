using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image healthBarFill; // Assign the HealthBarFill Image here
    public float maxHealth = 100f; // Max health value
    private float currentHealth;
    public PlayerSounds ps;

    void Start()
    {
        currentHealth = maxHealth; // Start at max health
        UpdateHealthBar(); // Update UI to reflect starting health
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Ensure health doesn’t go below 0
        UpdateHealthBar(); // Update UI after taking damage
        ps.PlayHitSound();
      
    }

    private void UpdateHealthBar()
    {
        healthBarFill.fillAmount = currentHealth / maxHealth; // Set fill amount based on health percentage
    }
}
