using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackScript : MonoBehaviour
{
    [Header("Enemy Type")] [SerializeField]
    private bool isRanged;
    [Header("References")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint; //  projectiles spawn from
    
    [Header("Attack Parameters")]
    [SerializeField] private float projectileSpeed = 5f;
    [SerializeField] private float projectileLifetime = 3f;
    [SerializeField] private float projectileDamage = 10f;
    public float targetingRange = 4f;
    public float meleeTime = 0.3f;
    public float meleeDamage;
    private bool isMeleeAttacking;

    [SerializeField] private Collider2D meleeCollider;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject muzzleFlash;
    [SerializeField] private float muzzleFlashDuration = 0.1f;
    
    private bool isFiring = false;
    
    void Awake()
    {
        if (isRanged)
        {
            if (firePoint == null)
            {
                GameObject firePointObj = new GameObject("FirePoint");
                firePointObj.transform.SetParent(transform);
                firePointObj.transform.localPosition = new Vector3(1f, 0f, 0f);
                firePoint = firePointObj.transform;
            }
            
        }
        else
        {
            if (meleeCollider == null)
            {
                Debug.LogWarning("NO Melee Collider2D SET");
                isRanged = true;
                
                if (firePoint == null)
                {
                    GameObject firePointObj = new GameObject("FirePoint");
                    firePointObj.transform.SetParent(transform);
                    firePointObj.transform.localPosition = new Vector3(1f, 0f, 0f);
                    firePoint = firePointObj.transform;
                }
            }
            else
            {
                meleeCollider.gameObject.SetActive(false);
            }
        }
        
    }
    
    public IEnumerator Fire(float direction)
    {
        
        if (isFiring) yield break;
        
        isFiring = true;
        
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        
        ProjectileScript projectileScript = projectile.GetComponent<ProjectileScript>();
        if (projectileScript != null)
        { 
            projectileScript.SetDirection(direction > 0);
        }
            
        if (muzzleFlash != null) 
        { 
            StartCoroutine(ShowMuzzleFlash());
        }
            
        yield return new WaitForSeconds(0.1f);
        isFiring = false;
    }
    
    private IEnumerator ShowMuzzleFlash()
    {
        if (muzzleFlash != null)
        {
            muzzleFlash.SetActive(true);
            yield return new WaitForSeconds(muzzleFlashDuration);
            muzzleFlash.SetActive(false);
        }
    }
    
    
    public bool CanFire()
    {
        return !isFiring;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        //if (isMeleeAttacking)
        //{
            if (other.collider.CompareTag("Player"))
            {
                // damage  player
                HealthManager healthManager = FindObjectOfType<HealthManager>();
                if (healthManager != null)
                {
                    healthManager.updateHealth(-meleeDamage);
                }
            }
        //}
        
    }
    
    public IEnumerator MeleeActiveTime(float time)
    {
        isMeleeAttacking = true;
        meleeCollider.gameObject.SetActive(true);
        yield return new WaitForSeconds(time);
        isMeleeAttacking = false;
        meleeCollider.gameObject.SetActive(false);
    }
}