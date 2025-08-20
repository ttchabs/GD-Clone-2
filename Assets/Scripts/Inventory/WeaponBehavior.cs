using UnityEngine;
using System.Collections;

public class WeaponBehavior : MonoBehaviour
{
    [Header("Weapon Type")]
    [SerializeField] private WeaponType weaponType = WeaponType.Sword;
    
    [Header("Projectile Settings (for Axe)")]
    [SerializeField] private float projectileSpeed = 15f;
    [SerializeField] private float projectileRange = 8f;
    [SerializeField] private float returnSpeed = 12f;
    [SerializeField] private float maxFlightTime = 5f; //  time before return
    [SerializeField] private float rotationSpeed = 720f; // degrees per second while flying
    
    [Header("Audio & Effects")]
    [SerializeField] private AudioClip swordSlashSound;
    [SerializeField] private AudioClip axeThrowSound;
    [SerializeField] private AudioClip axeReturnSound;
    [SerializeField] private AudioClip axeHitSound;
    [SerializeField] private AudioClip scytheSwipeSound;
    [SerializeField] private ParticleSystem attackEffect;
    
   
    private SpriteRenderer weaponSprite;
    private AudioSource audioSource;
    private Transform player;
    private Animator playerAnimator;
    private LayerMask enemyLayerMask;
    
  
    private bool isProjectileActive = false;
    private bool hasHitEnemy = false; 
    private float flightTime = 0f; 
    private Vector3 idlePosition; 
    private Vector3 idleRotation; 
    
        public enum WeaponType
    {
        Sword = 1,
        Axe = 2,
        Scythe = 3
    }
    
    private void Awake()
    {
        weaponSprite = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        player = GetComponentInParent<PlayerController>()?.transform;
        playerAnimator = GetComponentInParent<Animator>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
       
        enemyLayerMask = LayerMask.GetMask("Enemy");
        
   
        CaptureInitialTransform();
    }
    
    private void CaptureInitialTransform()
    {
       
        idlePosition = transform.localPosition;
        idleRotation = transform.localEulerAngles;
        
        
    }
    
    private void SetIdleState()
    {
       
        transform.localPosition = idlePosition;
        transform.localEulerAngles = idleRotation;
    }
    
 
    public void OnEquipped()
    {
       
        CaptureInitialTransform();
        SetIdleState();
       
    }
    
 
    public void OnUnequipped()
    {
        StopAttack();
      
    }
    
    //  when player attacks with this weapon
    public void OnAttack()
    {
        if (isProjectileActive) return; // no't attack if axe is still flying
        
        switch (weaponType)
        {
            case WeaponType.Sword:
                TriggerSwordAttack();
                break;
            case WeaponType.Axe:
                TriggerAxeAttack();
                break;
            case WeaponType.Scythe:
                TriggerScytheAttack();
                break;
        }
    }
    
    private void TriggerSwordAttack()
    {
        // Play sound
        if (audioSource != null && swordSlashSound != null)
        {
            audioSource.PlayOneShot(swordSlashSound);
        }
        
        // Play effect
        if (attackEffect != null)
        {
            attackEffect.Play();
        }
        
        // Trigger player animation
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("SwordAttack");
        }
        DealDamage(Physics2D.OverlapCircleAll(transform.position, 0.5f, enemyLayerMask));
        Debug.Log("Sword attack ");
    }
    
    private void TriggerAxeAttack()
    {
        //  axe projectile
        StartCoroutine(AxeProjectile());
        
        //  player animation
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("AxeThrow");
        }
        
  
    }
    
    private void TriggerScytheAttack()
    {
        // Play sound
        if (audioSource != null && scytheSwipeSound != null)
        {
            audioSource.PlayOneShot(scytheSwipeSound);
        }
        
        // Play effect
        if (attackEffect != null)
        {
            attackEffect.Play();
        }
        
        
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("ScytheAttack");
        }

        DealDamage(Physics2D.OverlapCircleAll(transform.position, 1.5f, enemyLayerMask));
        // Check for overlapping enemies and damage them
        
        Debug.Log("Scythe attack triggered");
    }

    private void DealDamage(Collider2D[] hitEnemies)
    {
        var damage = 0;
        foreach (Collider2D enemy in hitEnemies)
        {
            var enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                // Get weapon damage from the weapon system
                WeaponSystem weaponSystem = player?.GetComponent<WeaponSystem>();

                if (weaponType == WeaponType.Sword)
                {
                    damage = 30;//weaponSystem.GetCurrentWeapon().damage
                }
                if (weaponType == WeaponType.Scythe)
                {
                    damage = 20; //weaponSystem.GetCurrentWeapon().damage
                }
                
                Vector2 attackDirection = (enemy.transform.position - transform.position).normalized;
                enemyHealth.TakeDamage(damage, attackDirection);
                // bool damageDealt = 
                // if (damageDealt)
                // {
                //     //  hit sound
                //     if (audioSource != null && axeHitSound != null)
                //     {
                //         audioSource.PlayOneShot(axeHitSound);
                //     }
                //     print("ENEMY HIT");
                //     hasHitEnemy = true;
                // }
            }
        }
    }

    private IEnumerator AxeProjectile()
    {
        isProjectileActive = true;
        hasHitEnemy = false;
        flightTime = 0f;
        
        // Play throw sound
        if (audioSource != null && axeThrowSound != null)
        {
            audioSource.PlayOneShot(axeThrowSound);
        }
        
        // Find nearest enemy or use max range direction
        Vector3 targetPoint = GetAxeTargetPoint();
        
        Vector3 startPos = transform.position; // World position for projectile
        Vector3 originalLocalPos = transform.localPosition;
        Vector3 originalLocalRot = transform.localEulerAngles;
        
        // Throw phase - continue until hit enemy, reach max distance, or max time (5 seconds)
        while (!hasHitEnemy && flightTime < maxFlightTime)
        {
            flightTime += Time.deltaTime;
            
            // Calculate progress based on speed and distance
            float distanceToTarget = Vector3.Distance(startPos, targetPoint);
            float expectedFlightTime = distanceToTarget / projectileSpeed;
            float progress = flightTime / expectedFlightTime;
            
            // If we've reached the target point and no enemy hit, continue flying for remaining time
            if (progress >= 1f)
            {
                // Continue flying in the same direction beyond target point
                Vector3 direction = (targetPoint - startPos).normalized;
                float extraDistance = (flightTime - expectedFlightTime) * projectileSpeed;
                transform.position = targetPoint + direction * extraDistance;
            }
            else
            {
                // Still flying toward initial target
                transform.position = Vector3.Lerp(startPos, targetPoint, progress);
            }
            
            // Rotate while flying (boomerang spin)
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
            
            // Check for enemy hits during flight
            CheckAxeEnemyHits();
            
            yield return null;
        }
        
        // Play return sound
        if (audioSource != null && axeReturnSound != null)
        {
            audioSource.PlayOneShot(axeReturnSound);
        }
        
        // Return phase - always returns to player regardless of hit or miss
        Vector3 returnStart = transform.position;
        float elapsedTime = 0f;
        
        while (elapsedTime < 10f) // Safety timeout for return
        {
            elapsedTime += Time.deltaTime;
            
            // Update return target to player's current position (follows player movement)
            if (player != null)
            {
                Vector3 returnTarget = player.position;
                float distanceToPlayer = Vector3.Distance(returnStart, returnTarget);
                float returnDuration = distanceToPlayer / returnSpeed;
                float returnProgress = elapsedTime / returnDuration;
                
                // Move back to player
                transform.position = Vector3.Lerp(returnStart, returnTarget, returnProgress);
                
                // Continue rotating during return
                transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
                
                // Check if close enough to player to "catch" the axe
                if (Vector3.Distance(transform.position, returnTarget) < 0.5f)
                {
                    break;
                }
                
                // Update return start position if needed for smooth following
                if (returnProgress > 1f)
                {
                    returnStart = transform.position;
                    elapsedTime = 0f;
                }
            }
            
            yield return null;
        }
        
        // go bac to original local position and rotation
        transform.localPosition = originalLocalPos;
        transform.localEulerAngles = originalLocalRot;
        
        isProjectileActive = false;
        
       
    }
    
    private Vector3 GetAxeTargetPoint()
    {
        
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, projectileRange, enemyLayerMask);
        
        if (enemies.Length > 0)
        {
            // Find closest enemy
            float closestDistance = float.MaxValue;
            Vector3 closestEnemyPos = Vector3.zero;
            
            foreach (Collider2D enemy in enemies)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemyPos = enemy.transform.position;
                }
            }
            
            return closestEnemyPos;
        }
        
        // No enemies found, throw in the direction player is facing
        if (player != null)
        {
            Vector3 direction = player.GetComponent<SpriteRenderer>().flipX ? Vector3.left : Vector3.right;
            return transform.position + direction * projectileRange;
        }
        
        return transform.position + Vector3.right * projectileRange;
    }
    
    private void CheckAxeEnemyHits()
    {
        // Check for overlapping enemies and damage them
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, 0.5f, enemyLayerMask);
        
        foreach (Collider2D enemy in hitEnemies)
        {
            var enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                // Get weapon damage from the weapon system
                WeaponSystem weaponSystem = player?.GetComponent<WeaponSystem>();
                int damage = weaponSystem?.GetCurrentWeapon()?.damage ?? 15;
                
                Vector2 attackDirection = (enemy.transform.position - transform.position).normalized;
                bool damageDealt = enemyHealth.TakeDamage(damage, attackDirection);
                
                if (damageDealt)
                {
                    //  hit sound
                    if (audioSource != null && axeHitSound != null)
                    {
                        audioSource.PlayOneShot(axeHitSound);
                    }
                    
                
                    hasHitEnemy = true;
                    
                   
                }
            }
        }
    }
    
    public void StopAttack()
    {
        StopAllCoroutines();
        isProjectileActive = false;
        hasHitEnemy = false;
        flightTime = 0f;
        SetIdleState();
    }
    
    // Public getters
    public bool IsAttacking => isProjectileActive;
    public WeaponType GetWeaponType() => weaponType;
    
    // Method to set weapon type (useful for dynamic assignment)
    public void SetWeaponType(WeaponType type)
    {
        weaponType = type;
        SetIdleState();
    }
}