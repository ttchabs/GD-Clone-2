using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerFollow : MonoBehaviour
{
    public Transform player;
    public float smoothSpeed;
    public Vector3 offset;

    public PlayerController playerController;

    private float originalX;   
    private float flippedX = -0.4f;

    private void Awake()
    {
       
        originalX = offset.x;
    }

    void LateUpdate()
    {
        Vector3 desiredPosition = player.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

      
        if (playerController.isflipped)
        {
            offset.x = flippedX;
        }
        else
        {
            offset.x = originalX;
        }
    }
}
