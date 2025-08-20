using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AshPickup : MonoBehaviour
{
    [SerializeField] private Collider2D collider;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float ashAmount = 10;

    private bool playerRange;

    //private AshSystem1 ashManager;

    private testAshSystem testAshManager;

    // Start is called before the first frame update
    private void OnEnable()
    {
        //ashManager = FindFirstObjectByType<AshSystem1>();
        testAshManager = FindFirstObjectByType<testAshSystem>();
    }

    void Start()
    {
        //ashManager = FindFirstObjectByType<AshSystem1>();
        
        //print(ashManager);
    }

    // Update is called once per frame
    void Update()
    {
        if (playerRange && Input.GetKeyDown(interactKey))
        {
            print("ASH COLLECTED");
            //ashManager.addDeath();
            //testAshManager.addAsh(ashAmount);
            testAshManager.EnemyKilled();
            gameObject.SetActive(false);
            Destroy(this, 0.1f); 
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        playerRange = other.collider.CompareTag("Player");
    }
}
