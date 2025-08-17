using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkpointText : MonoBehaviour
{
    public GameObject checkpointUI;

    public void Start()
    {
        checkpointUI.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            checkpointUI.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            checkpointUI.SetActive(false);
        }
    }
}
