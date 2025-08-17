using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fogWallManager : MonoBehaviour
{
    public static fogWallManager Instance;

    private int totalKills = 0;
    public List<fogWall> fogWalls; // assign in inspector

    void Awake()
    {
        Instance = this; // singleton pattern
    }

    public void EnemyDefeated()
    {
        totalKills++;
        Debug.Log("Kills: " + totalKills);
        CheckFogWalls();
    }

    private void CheckFogWalls()
    {
        foreach (var wall in fogWalls)
        {
            if (!wall.IsUnlocked() && totalKills >= wall.enemiesToDefeat)
            {
                wall.UnlockWall();
            }
        }
    }
}
