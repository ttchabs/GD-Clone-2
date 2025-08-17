using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fogWall : MonoBehaviour
{
    public int enemiesToDefeat = 5; 
    private bool unlocked = false;

    private BoxCollider2D col;
    public ParticleSystem fogParticles;

    void Start()
    {
        col = GetComponent<BoxCollider2D>();
        col.enabled = true;
        if (fogParticles != null) fogParticles.Play();
    }

    public bool IsUnlocked() => unlocked;

    public void UnlockWall()
    {
        unlocked = true;
        col.enabled = false;
        if (fogParticles != null) fogParticles.Stop();
        Debug.Log(name + " unlocked!");
    }
}
