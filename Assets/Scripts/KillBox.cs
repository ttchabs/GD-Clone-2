using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KillBox : MonoBehaviour
{
    // public Transform respawnPoint;
    public GameObject youDiedUI;

    public void Start()
    { 
            youDiedUI.SetActive(false);
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            youDiedUI.SetActive(true);
            // RespawnPlayer respawnPlayer = collision.GetComponent<RespawnPlayer>();
            // if (respawnPlayer != null)
            // {
            //     youDiedUI.SetActive(true);
            //     Debug.Log("Player died, respawning...");
            //     respawnPlayer.Respawn();
            // }
        }
    }
}
