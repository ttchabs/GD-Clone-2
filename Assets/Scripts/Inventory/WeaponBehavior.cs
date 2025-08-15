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
    [SerializeField] private float maxFlightTime = 5f; 
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
    private float flightTime = 0f; //  how long axe has been flying
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
    
  
    public void OnAttack()
    {
        if (isProjectileActive) return; 
        
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
        
        Debug.Log("sword a attack triggered");
    }
    
    private void TriggerAxeAttack()
    {
       
        StartCoroutine(AxeProjectile());
        
        
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("AxeThrow");
        }
        
        Debug.Log("axe throw ");
    }
    
    private void TriggerScytheAttack()
    {
     
        if (audioSource != null && scytheSwipeSound != null)
        {
            audioSource.PlayOneShot(scytheSwipeSound);
        }
        
        
        if (attackEffect != null)
        {
            attackEffect.Play();
        }
        
       
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("ScytheAttack");
        }
        
        Debug.Log("Scythe attack ");
    }
    
    private IEnumerator AxeProjectile()
    {
        isProjectileActive = true;
        hasHitEnemy = false;
        flightTime = 0f;
        
       
        if (audioSource != null && axeThrowSound != null)
        {
            audioSource.PlayOneShot(axeThrowSound);
        }
        
      
        Vector3 targetPoint = GetAxeTargetPoint();
        
        Vector3 startPos = transform.position; 
        Vector3 originalLocalPos = transform.localPosition;
        Vector3 originalLocalRot = transform.localEulerAngles;
        
       
        while (!hasHitEnemy && flightTime < maxFlightTime)
        {
            flightTime += Time.deltaTime;
            
          
            float distanceToTarget = Vector3.Distance(startPos, targetPoint);
            float expectedFlightTime = distanceToTarget / projectileSpeed;
            float progress = flightTime / expectedFlightTime;
            
        
            if (progress >= 1f)
            {
             
                Vector3 direction = (targetPoint - startPos).normalized;
                float extraDistance = (flightTime - expectedFlightTime) * projectileSpeed;
                transform.position = targetPoint + direction * extraDistance;
            }
            else
            {
               
                transform.position = Vector3.Lerp(startPos, targetPoint, progress);
            }
            
          
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
            
       
            CheckAxeEnemyHits();
            
            yield return null;
        }
        
  
        if (audioSource != null && axeReturnSound != null)
        {
            audioSource.PlayOneShot(axeReturnSound);
        }
        
     
        Vector3 returnStart = transform.position;
        float elapsedTime = 0f;
        
        while (elapsedTime < 10f)
        {
            elapsedTime += Time.deltaTime;
            
          
            if (player != null)
            {
                Vector3 returnTarget = player.position;
                float distanceToPlayer = Vector3.Distance(returnStart, returnTarget);
                float returnDuration = distanceToPlayer / returnSpeed;
                float returnProgress = elapsedTime / returnDuration;
                
              
                transform.position = Vector3.Lerp(returnStart, returnTarget, returnProgress);
                
             
                transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
                
                //  check if close enough to player to "catch" the axe
                if (Vector3.Distance(transform.position, returnTarget) < 0.5f)
                {
                    break;
                }
                
       
                if (returnProgress > 1f)
                {
                    returnStart = transform.position;
                    elapsedTime = 0f;
                }
            }
            
            yield return null;
        }
        
      
        transform.localPosition = originalLocalPos;
        transform.localEulerAngles = originalLocalRot;
        
        isProjectileActive = false;
        
        
    }
    
    private Vector3 GetAxeTargetPoint()
    {
       
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, projectileRange, enemyLayerMask);
        
        if (enemies.Length > 0)
        {
          
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
        
       
        if (player != null)
        {
            Vector3 direction = player.GetComponent<SpriteRenderer>().flipX ? Vector3.left : Vector3.right;
            return transform.position + direction * projectileRange;
        }
        
        return transform.position + Vector3.right * projectileRange;
    }
    
    private void CheckAxeEnemyHits()
    {
      
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, 0.5f, enemyLayerMask);
        
        foreach (Collider2D enemy in hitEnemies)
        {
            var enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
               
                WeaponSystem weaponSystem = player?.GetComponent<WeaponSystem>();
                int damage = weaponSystem?.GetCurrentWeapon()?.damage ?? 15;
                
                Vector2 attackDirection = (enemy.transform.position - transform.position).normalized;
                bool damageDealt = enemyHealth.TakeDamage(damage, attackDirection);
                
                if (damageDealt)
                {
                  
                    if (audioSource != null && axeHitSound != null)
                    {
                        audioSource.PlayOneShot(axeHitSound);
                    }
                    
                 
                    hasHitEnemy = true;
                    
                    Debug.Log($"axe hit {enemy.name} for {damage} damage");
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
    

    public bool IsAttacking => isProjectileActive;
    public WeaponType GetWeaponType() => weaponType;
    
    public void SetWeaponType(WeaponType type)
    {
        weaponType = type;
        SetIdleState();
    }
}