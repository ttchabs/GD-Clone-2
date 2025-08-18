using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CamFollow : MonoBehaviour
{
    public Transform player;

    public Vector3 offset;



   

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = player.position + offset;
    }

}

