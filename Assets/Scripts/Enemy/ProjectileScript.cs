using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float damage = 10f;
    
    [Header("Movement")]
    [SerializeField] private bool moveRight = true;
    
    private Rigidbody2D rb;
    private bool hasHitTarget = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        
        float direction = moveRight ? 1f : -1f;
        rb.velocity = new Vector2(speed * direction, 0);
        
       
        Destroy(gameObject, lifetime);
    }
    
    void Update()
    {
        // destroy if moving too slowly (hit something)
        if (Mathf.Abs(rb.velocity.x) < 0.1f && !hasHitTarget)
        {
            DestroyProjectile();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHitTarget) return;
        
        if (other.CompareTag("Player"))
        {
            // damage  player
            HealthManager healthManager = other.GetComponent<HealthManager>();
            if (healthManager != null)
            {
                healthManager.updateHealth(-damage);
              
            }
            
            hasHitTarget = true;
            DestroyProjectile();
        }
        else if (other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
           
            hasHitTarget = true;
            DestroyProjectile();
        }
    }
    
    private void DestroyProjectile()
    {
        // we can add visual effects here
        Destroy(gameObject);
    }
    
    public void SetDirection(bool goRight)
    {
        moveRight = goRight;
        if (rb != null)
        {
            float direction = moveRight ? 1f : -1f;
            rb.velocity = new Vector2(speed * direction, rb.velocity.y);
        }
    }
}