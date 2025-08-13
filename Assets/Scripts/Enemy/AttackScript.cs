using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class AttackScript : MonoBehaviour
{
    [Header("References")] [SerializeField]
    private GameObject hitbox;
    [Header("Parameters")]
    //[SerializeField] private bool isRanged;
    [SerializeField] private float projectileSpeed = 1f;
    [SerializeField] private float projectileLifetime = 2f; 
    public float targetingRange = 2f;

    private List<GameObject> attackList;
    private float projectileTime;
    private bool isFiring;
    
    void Awake()
    {
        isFiring = false;
    }

    private void FixedUpdate()
    {
        
    }
 
    public IEnumerator Fire(float direction)
    {
        GameObject attackInstantiated = Instantiate(hitbox, this.transform);
        if (direction < 0)
        {
            attackInstantiated.transform.localRotation = Quaternion.Euler(0,180,0);
        }
        
        yield return new WaitForSecondsRealtime(projectileLifetime);
        Destroy(attackInstantiated);
        
    }
}
