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
    void LateUpdate()

    {
        Vector3 desiredPosition = player.position + offset;

        // Smooth transition
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Only move the camera, NOT the player
        transform.position = smoothedPosition;

        if (playerController.isflipped)
        
        { 
        //offset.x -= offset.x;
            
        
        }
    }
}
