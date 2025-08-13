using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProjectileScript : MonoBehaviour
{
    [SerializeField] private float lifetime;
    [SerializeField] private float speed;
    
    private Collider2D hitbox;
    private Vector3 pos;
    void Start()
    {
        
        hitbox = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        pos = transform.position;
        pos.x += speed;
        transform.position = pos;
        lifetime -= Time.deltaTime;
        if (lifetime <= 0)
        {
            print("DEAD");
            Destroy(this);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Player")
        {
            print("ATTACK HIT");
        }
    }
    
}
