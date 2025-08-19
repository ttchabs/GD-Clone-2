using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    
    [Header("UI References")]
    public Image healthBar;
    public GameObject healthBarBase;
    public GameObject youDiedUI;
    
    [Header("Player References")]
    public GameObject player;
    public CharacterController characterController;
    public GameObject playerWaitingPoint;
    public PlayerController playerScript;
    
    [Header("Damage Effects")]
    public float invulnerabilityTime = 1f;
    public Color damageColor = Color.red;
    public float flashDuration = 0.1f;
    
    private bool isInvulnerable = false;
    private SpriteRenderer playerSpriteRenderer;
    private Color originalColor;
    
    public void Start()
    {
        currentHealth = maxHealth;
        healthBarBase.SetActive(true);
        updateHealthBar();
        
        
        if (player != null)
        {
            playerSpriteRenderer = player.GetComponent<SpriteRenderer>();
            if (playerSpriteRenderer != null)
            {
                originalColor = playerSpriteRenderer.color;
            }
        }
    }
    
    public void Update()
    {
        if (currentHealth <= 0)
        {
            youDiedUI.SetActive(true);
            // playerScript.isPaused = true;
        }
    }
    
    public void updateHealth(float amount)
    {
        // If taking damage and invulnerable, ignore it
        if (amount < 0 && isInvulnerable)
        {
            Debug.Log("Player is invulnerable, damage ignored!");
            return;
        }
        
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        updateHealthBar();
        
        // If taking damage, trigger effects
        if (amount < 0)
        {
            StartCoroutine(DamageEffect());
        }
        
        Debug.Log($"Health updated by {amount}. Current health: {currentHealth}/{maxHealth}");
    }
    
    private IEnumerator DamageEffect()
    {
        isInvulnerable = true;
        
        // Flash effect
        if (playerSpriteRenderer != null)
        {
            for (int i = 0; i < 3; i++)
            {
                playerSpriteRenderer.color = damageColor;
                yield return new WaitForSeconds(flashDuration);
                playerSpriteRenderer.color = originalColor;
                yield return new WaitForSeconds(flashDuration);
            }
        }
        
        yield return new WaitForSeconds(invulnerabilityTime - (6 * flashDuration));
        isInvulnerable = false;
    }
    
    public void updateHealthBar()
    {
        if (healthBar != null)
        {
            float targetFillAmount = currentHealth / maxHealth;
            healthBar.fillAmount = targetFillAmount;
        }
    }
    
    // Convenience methods for testing
    [ContextMenu("Player Hit")]
    public void PlayerHit()
    {
        updateHealth(-10f);
    }
    
    public void PlayerHitALot()
    {
        updateHealth(-20f);
    }
    
    public void PlayerHeal()
    {
        updateHealth(50f);
    }
    
    public void FullHeal()
    {
        currentHealth = maxHealth;
        updateHealthBar();
    }
    
    public void FullKill()
    {
        updateHealth(-maxHealth);
    }
    
    public bool IsInvulnerable()
    {
        return isInvulnerable;
    }

    public IEnumerator PlayerDied()
    {
        yield return new WaitForSeconds(0f);
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        if (player != null && playerWaitingPoint != null)
        {
            player.transform.position = playerWaitingPoint.transform.position;
        }

        yield return new WaitForSeconds(0.5f);

        if (characterController != null)
        {
            characterController.enabled = true;
        }
        RespawnPlayer respawnPlayer = GetComponent<RespawnPlayer>();

        
        respawnPlayer.Respawn();
    }
}