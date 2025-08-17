using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AshPickup1 : MonoBehaviour
{
    [SerializeField] private Collider2D collider;

    private AshSystem ashManager;
    // Start is called before the first frame update
    private void OnEnable()
    {
        ashManager = FindFirstObjectByType<AshSystem>();
    }

    void Start()
    {
        ashManager = FindFirstObjectByType<AshSystem>();
        //print(ashManager);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (ashManager != null)
        {
            if (other.collider.CompareTag("Player"))
            {
                print("ASH COLLECTED");
                ashManager.addDeath();
                gameObject.SetActive(false);
                //Destroy(this, 0.1f);
            }
        }
        
    }
}
