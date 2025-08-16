using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnPlayer : MonoBehaviour
{
    private Vector2 respawnPoint;

    void Start()
    {
        respawnPoint = transform.position;
    }

    public void SetCheckpoint(Vector2 newCheckpoint)
    {
        respawnPoint = newCheckpoint;
    }

    public void Respawn()
    {
        transform.position = respawnPoint;
    }
}
