using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointSystem : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player has entered the checkpoint area.");

            RespawnPlayer respawnPlayer = collision.GetComponent<RespawnPlayer>();
            if (respawnPlayer != null)
            {
                respawnPlayer.SetCheckpoint(transform.position);
                Debug.Log("Checkpoint reached at " + transform.position);
            }
        }
    }
}
