using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public Image healthBar;
    public GameObject healthBarBase;
    
    public GameObject gameOverScreen;
    public GameObject player;
    public CharacterController characterController;
    public GameObject playerWaitingPoint;
    public PlayerController playerScript;

    
    public void Start()
    {
        currentHealth = maxHealth;
        updateHealthBar();
    }

    public void Update()
    {
        if (currentHealth <= 0)
        {
            gameOverScreen.SetActive(true);
            // playerScript.isPaused = true;
        }
    }

    public void updateHealth(float amount)
    {
        currentHealth += amount;
        updateHealthBar();

    }

    public void updateHealthBar()
    {
        float targetFillAmount = currentHealth / maxHealth;
        healthBar.fillAmount = targetFillAmount;
        
    }

[ContextMenu("player hit")]
    public void PlayerHit()
    {
        currentHealth = currentHealth - 10f;
        updateHealthBar();
    }

    public void PLayerHitAlot()
    {
        currentHealth = currentHealth - 20f;
        updateHealthBar();
    }

    public void PlayerHeal()
    {
        currentHealth = currentHealth + 50f;
        updateHealthBar();
    }

    public void FullHeal()
    {
        currentHealth = maxHealth;
        updateHealthBar();
    }

    public void FullKill()
    {
        currentHealth = currentHealth - 500f;
        updateHealthBar();
    }

   
    public IEnumerator PlayerDied()
    {
        yield return new WaitForSeconds(0f);
        characterController.enabled = false;
        player.transform.position = playerWaitingPoint.transform.position;
        yield return new WaitForSeconds(0.5f);
        characterController.enabled = true;
    }
}
