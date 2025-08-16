using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSpawner : MonoBehaviour
{
    [SerializeField] public ParticleSystem blood;
    [SerializeField] public ParticleSystem ash;
    private List<ParticleSystem> children;
    private ParticleSystem bloodChild;
    private ParticleSystem ashChild;
    private void Start()
    {
        bloodChild = blood;
        ashChild = ash;
    }

    public void spawn(string name) {
        switch (name)
        {
            case "blood":
                children.Add(bloodChild);
                break;
            case "ash":
                children.Add(ashChild);
                break;
            default: 
                print("Particle: " + name + " not found");
                break;
        }
    }
}
