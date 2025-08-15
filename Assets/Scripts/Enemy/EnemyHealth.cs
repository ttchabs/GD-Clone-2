using System.Collections;
using System.Collections.Generic;
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
    
    [Header("Ash Reference")]
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
    

    public System.Action<int> OnHealthChanged;
    public System.Action OnDeath;

    private AshSystem ashManager;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        enemyAI = GetComponent<EnemyAI>();
        
        currentHealth = maxHealth;
        originalColor = spriteRenderer.color;

        ashManager = FindFirstObjectByType<AshSystem>();
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
        //ashManager.addDeath();
        
 
        OnDeath?.Invoke();
        
        
     
        Destroy(gameObject, 1f);
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
    
 
    private void OnDrawGizmosSelected()
    {
        
        Vector3 healthBarPos = transform.position + Vector3.up * 1.5f;
        float healthBarWidth = 1f;
        float healthBarHeight = 0.2f;
        
      
        Gizmos.color = Color.red;
        Gizmos.DrawCube(healthBarPos, new Vector3(healthBarWidth, healthBarHeight, 0));
        
      
        if (Application.isPlaying)
        {
            float healthPercent = (float)currentHealth / maxHealth;
            Gizmos.color = Color.green;
            Vector3 healthFillPos = healthBarPos - Vector3.right * (healthBarWidth * (1 - healthPercent) * 0.5f);
            Gizmos.DrawCube(healthFillPos, new Vector3(healthBarWidth * healthPercent, healthBarHeight, 0));
        }
    }
}