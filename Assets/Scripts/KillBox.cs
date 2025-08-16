using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KillBox : MonoBehaviour
{
    // public Transform respawnPoint;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag =="Player")
        {
            Debug.Log("Player has entered the kill box");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        }
    }
}
