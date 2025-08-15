using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AshSystem : MonoBehaviour
{
    [SerializeField] private Image bar;
    private int current;
    private int total = 2;
    private float enemyFillAmount;
    private float currentFill = 0;
    private EnemyHealth[] enemies;

    private int ashCount;
    // Start is called before the first frame update
    void Start()
    {
        enemies = FindObjectsOfType(typeof(EnemyHealth)) as EnemyHealth[];
        total = enemies.Length;
        print(total);
        if(total != 0)
        {
            enemyFillAmount = 1 / total;
        }
        else
        {
            currentFill = 1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //bar.fillAmount = 1 / Time.time;
        bar.fillAmount = currentFill;
    }

    public void addDeath()
    {
        print("Collected By Manager");
        currentFill += enemyFillAmount;
    }
}
