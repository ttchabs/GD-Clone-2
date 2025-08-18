using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerFollow : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform player;

    public Vector3 offset;

    private PlayerController playerController;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
    }



    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = player.position + offset;

        if (playerController.isflipped)
        
        { 
        offset.x -= offset.x;
            
        
        }
    }
}
