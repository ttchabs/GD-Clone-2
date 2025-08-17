using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KillBox : MonoBehaviour
{
    // public Transform respawnPoint;
    public GameObject youDiedUI;
    public float displayTime = 0.2f; 
    public void Start()
    {

        youDiedUI.SetActive(false);
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            StartCoroutine(ShowAndHide());
            

            RespawnPlayer respawnPlayer = collision.GetComponent<RespawnPlayer>();
            if (respawnPlayer != null)
            {
                youDiedUI.SetActive(true);
                Debug.Log("Player died, respawning...");
                respawnPlayer.Respawn();
            }
        }
    }
    
    private IEnumerator ShowAndHide()
    {
        youDiedUI.SetActive(true);
        yield return new WaitForSeconds(displayTime);
        youDiedUI.SetActive(false);
    }
}
