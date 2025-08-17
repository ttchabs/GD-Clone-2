using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KillBox : MonoBehaviour
{
    // public Transform respawnPoint;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            RespawnPlayer respawnPlayer = collision.GetComponent<RespawnPlayer>();
            if (respawnPlayer != null)
            {
                Debug.Log("Player died, respawning...");
                respawnPlayer.Respawn();
            }
        }
    }
}
