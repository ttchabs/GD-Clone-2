using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class testAshSystem : MonoBehaviour
{
    [Header("Ash Settings")]
    public float maxAsh = 100f;
    public float minAsh = 0f;
    public float currentAsh;
    public Image ashFill;
    public GameObject ashBase;
    public GameObject gameOverScreen;
    public float ashPerKill = 10f;

    [Header("Player Settings")]

    public GameObject player;
    public CharacterController characterController;
    public GameObject playerWaitingPoint;
    public PlayerController playerScript;
    private SpriteRenderer playerSpriteRenderer;
    private Color originalColor;

    private bool hasWon = false;

    public void Awake()
    {
                gameOverScreen.SetActive(false);

    }

    public void Start()
    {
        currentAsh = minAsh;
        updateAshBar();
        if (player != null)
        {
            playerSpriteRenderer = player.GetComponent<SpriteRenderer>();
            if (playerSpriteRenderer != null)
            {
                originalColor = playerSpriteRenderer.color;
            }
        }
    }

    public void Update()
    {
        if (!hasWon && currentAsh >= maxAsh)
        {
            hasWon = true;
            gameOverScreen.SetActive(true);
        }
    }

    public void addAsh(float amount)
    {
        currentAsh += amount;
        currentAsh = Mathf.Clamp(currentAsh, minAsh, maxAsh);
        updateAshBar();
    }

    public void updateAshBar()
    {
        if (ashFill != null)
        {
            float targetFillAmount = currentAsh / maxAsh;
            ashFill.fillAmount = targetFillAmount;
        }
    }

    public void EnemyKilled()
    {
        addAsh(ashPerKill);
    }

}
