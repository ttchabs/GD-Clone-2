using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private ParticleSpawner particles;
    [SerializeField] private GameObject ashPrefab;
 
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private EnemyAI enemyAI;
    
  
    private int currentHealth;
    private bool isInvulnerable;
    private float invulnerabilityTimer;
    private bool isKnockedBack;
    private float knockbackTimer;
    private Color originalColor;
    private Collider2D rbCollider;

    public System.Action<int> OnHealthChanged;
    public System.Action OnDeath;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        enemyAI = GetComponent<EnemyAI>();
        rbCollider = GetComponent<Collider2D>();
        
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
       
        if (isInvulnerable)
        {
            return false;
        }
        
     
        currentHealth = Mathf.Max(0, currentHealth - damage);
        
       
        isInvulnerable = true;
        invulnerabilityTimer = invulnerabilityTime;
        
        //  knockback
        if (attackDirection != Vector2.zero)
        {
            ApplyKnockback(attackDirection);
        }
        
        //  damage animation
        if (animator != null)
        {
            animator.SetTrigger("TakeDamage");
        }

        if (particles != null)
        {
            particles.spawn("blood");
        }
      
        OnHealthChanged?.Invoke(currentHealth);
        Instantiate(ashPrefab, transform.position, Quaternion.identity);
       
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

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }


        if (enemyAI != null)
        {
            enemyAI.enabled = false;
        }


        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        if (particles != null)
        {
            particles.spawn("ash");
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        Instantiate(ashPrefab);

        OnDeath?.Invoke();


        Destroy(gameObject, 1f);

        testAshSystem ashSystem = FindObjectOfType<testAshSystem>();
        if (ashSystem != null)
        {
            ashSystem.EnemyKilled();
        }
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
        return currentHealth > 0;
    }
    
    public bool IsInvulnerable()
    {
        return isInvulnerable;
    }

}