using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerFollow : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform player;

    public float smoothSpeed;

    public Vector3 offset;

    public PlayerController playerController;

    private void Awake()
    {
        //playerController = GetComponent<PlayerController>();
    }



    // Update is called once per frame
    void FixedUpdate()

    {
       // Vector3 desiredPosition = transform.position;



        transform.position = player.position + offset;

        if (playerController.isflipped)
        
        { 
        //offset.x -= offset.x;
            
        
        }
    }
}
