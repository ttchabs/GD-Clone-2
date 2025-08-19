using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float invulnerabilityTime = 0.5f;
    
    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackDuration = 0.2f;
    
    [Header("Visual Feedback")]
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] [NotNull] private ParticleSpawner1 particles;
    [SerializeField] private GameObject ashPrefab;
    
    [Header("Audio Effects")]
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private float explosionVolume = 0.8f;
    [SerializeField] private float damageVolume = 0.6f;
 
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    //private Animator animator;
    private EnemyAI enemyAI;
    private AudioSource audioSource;
    
    private int currentHealth;
    private bool isInvulnerable;
    private float invulnerabilityTimer;
    private bool isKnockedBack;
    private float knockbackTimer;
    private Color originalColor;
    private Collider2D rbCollider;
    private bool isDead = false;

    public System.Action<int> OnHealthChanged;
    public System.Action OnDeath;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
       // animator = GetComponent<Animator>();
        enemyAI = GetComponent<EnemyAI>();
        rbCollider = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();
        
        // Setup audio source if not found
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        currentHealth = maxHealth;
        originalColor = spriteRenderer.color;
    }
    
    private void Update()
    {
        HandleInvulnerability();
        HandleKnockback();
    }
    
    private void HandleInvulnerability()
    {
        if (isInvulnerable)
        {
            invulnerabilityTimer -= Time.deltaTime;
            
            float flashTimer = invulnerabilityTimer % (flashDuration * 2);
            bool shouldFlash = flashTimer < flashDuration;
            spriteRenderer.color = shouldFlash ? damageColor : originalColor;
            
            if (invulnerabilityTimer <= 0)
            {
                isInvulnerable = false;
                spriteRenderer.color = originalColor;
            }
        }
    }
    
    private void HandleKnockback()
    {
        if (isKnockedBack)
        {
            knockbackTimer -= Time.deltaTime;
            
            if (knockbackTimer <= 0)
            {
                isKnockedBack = false;
                
                if (enemyAI != null)
                {
                    enemyAI.enabled = true;
                }
            }
        }
    }
    
    public bool TakeDamage(int damage, Vector2 attackDirection = default)
    {
        if (isInvulnerable || isDead)
        {
            return false;
        }
        
        // Trigger combat music when enemy first takes damage
        if (CombatMusicManager.Instance != null && !CombatMusicManager.Instance.IsInCombat())
        {
            CombatMusicManager.Instance.ForceStartCombat();
        }
        
        currentHealth = Mathf.Max(0, currentHealth - damage);
        
        isInvulnerable = true;
        invulnerabilityTimer = invulnerabilityTime;
        
        // Play damage sound
        if (audioSource != null && damageSound != null)
        {
            audioSource.PlayOneShot(damageSound, damageVolume);
        }
        
        // Apply knockback
        if (attackDirection != Vector2.zero)
        {
            ApplyKnockback(attackDirection);
        }
        
        // Trigger damage animation
        //if (animator != null)
        //{
        //    animator.SetTrigger("TakeDamage");
        //}

        if (particles != null)
        {
            particles.spawn("blood");
        }
      
        OnHealthChanged?.Invoke(currentHealth);
        
        // Spawn ash when taking damage
        // if (ashPrefab != null)
        // {
        //     Instantiate(ashPrefab, transform.position, Quaternion.identity);
        // }
       
        if (currentHealth <= 0)
        {
            Die();
            return true;
        }
        
        return true;
    }
    
    private void ApplyKnockback(Vector2 direction)
    {
        if (rb == null) return;
        
        Vector2 knockbackDirection = direction.normalized;
        rb.velocity = new Vector2(knockbackDirection.x * knockbackForce, rb.velocity.y);
        
        if (enemyAI != null)
        {
            enemyAI.enabled = false;
        }
        
        isKnockedBack = true;
        knockbackTimer = knockbackDuration;
    }
    
    public void Heal(int healAmount)
    {
        if (currentHealth <= 0) return; 
        
        currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
        OnHealthChanged?.Invoke(currentHealth);
    }

    private void Die()
    {
        if (isDead) return; // Prevent multiple death calls
        
        isDead = true;
        
        // Play explosion sound on death
        if (audioSource != null && explosionSound != null)
        {
            audioSource.PlayOneShot(explosionSound, explosionVolume);
        }

        //if (///animator != null)
        //{
        //   // animator.SetTrigger("Die");
        //}

        if (enemyAI != null)
        {
            enemyAI.enabled = false;
        }

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        // Spawn ash particles on death
        if (particles != null)
        {
            particles.spawn("ash");
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Spawn ash prefab on death
        if (ashPrefab != null)
        {
            Instantiate(ashPrefab, transform.position, Quaternion.identity);
        }

        // Notify combat music manager that an enemy died
        if (CombatMusicManager.Instance != null)
        {
            CombatMusicManager.Instance.OnEnemyDeath();
        }

        OnDeath?.Invoke();

        // Notify ash system
        //testAshSystem ashSystem = FindObjectOfType<testAshSystem>();
        // if (ashSystem != null)
        // {
        //     ashSystem.EnemyKilled();
        // }
        //fogWallManager.Instance.EnemyDefeated();

        // Destroy after delay to allow sound to play
        Destroy(gameObject, 0.5f);
        gameObject.SetActive(false);
        fogWallManager.Instance.EnemyDefeated();
    }
    
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    
    public int GetMaxHealth()
    {
        return maxHealth;
    }
    
    public bool IsAlive()
    {
        return currentHealth > 0 && !isDead;
    }
    
    public bool IsInvulnerable()
    {
        return isInvulnerable;
    }
}