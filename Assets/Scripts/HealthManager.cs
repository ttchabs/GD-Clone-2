using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public Image healthBar;
    public GameObject healthBarBase;
    public Image healthBarUpgradeOne;
    public GameObject healthBarLevelOne;
    public Image healthBarUpgradeTwo;
    public GameObject healthBarLevelTwo;
    public Image healthBarUpgradeThree;
    public GameObject healthBarLevelThree;
    public Image healthBarUpgradeFour;
    public GameObject healthBarLevelFour;
    public Image healthBarUpgradeFive;
    public GameObject healthBarLevelFive;
    public GameObject gameOverScreen;
    public GameObject player;
    public CharacterController characterController;
    public GameObject playerWaitingPoint;
    public PlayerController playerScript;

    [SerializeField] private float blinkDuration;
    private Color defaultColor;

    public void Start()
    {
        currentHealth = maxHealth;
        updateHealthBar();
    }

    public void Update()
    {
        if (currentHealth <= 0)
        {
            gameOverScreen.SetActive(true);
            // playerScript.isPaused = true;
        }
    }

    public void updateHealth(float amount)
    {
        currentHealth += amount;
        updateHealthBar();

    }

    public void updateHealthBar()
    {
        float targetFillAmount = currentHealth / maxHealth;
        healthBar.fillAmount = targetFillAmount;
        healthBarUpgradeOne.fillAmount = targetFillAmount;
        healthBarUpgradeTwo.fillAmount = targetFillAmount;
        healthBarUpgradeThree.fillAmount = targetFillAmount;
        healthBarUpgradeFour.fillAmount = targetFillAmount;
        healthBarUpgradeFive.fillAmount = targetFillAmount;
    }

    public void PlayerHit()
    {
        currentHealth = currentHealth - 10f;
        updateHealthBar();
    }

    public void PLayerHitAlot()
    {
        currentHealth = currentHealth - 20f;
        updateHealthBar();
    }

    public void PlayerHeal()
    {
        currentHealth = currentHealth + 50f;
        updateHealthBar();
    }

    public void FullHeal()
    {
        currentHealth = maxHealth;
        updateHealthBar();
    }

    public void FullKill()
    {
        currentHealth = currentHealth - 500f;
        updateHealthBar();
    }

    public void GotUpgradeOne()
    {
        maxHealth = 120f;
        currentHealth = maxHealth;
        healthBarBase.SetActive(false);
        healthBarLevelOne.SetActive(true);
        updateHealthBar();
    }

    public void GotUpgradeTwo()
    {
        maxHealth = 140f;
        currentHealth = maxHealth;
        healthBarLevelOne.SetActive(false);
        healthBarLevelTwo.SetActive(true);
        updateHealthBar();
    }

    public void GotUpgradeThree()
    {
        maxHealth = 160f;
        currentHealth = maxHealth;
        healthBarLevelTwo.SetActive(false);
        healthBarLevelThree.SetActive(true);
        updateHealthBar();
    }

    public void GotUpgradeFour()
    {
        maxHealth = 180f;
        currentHealth = maxHealth;
        healthBarLevelThree.SetActive(false);
        healthBarLevelFour.SetActive(true);
        updateHealthBar();
    }

    public void GotUpgradeFive()
    {
        maxHealth = 200f;
        currentHealth = maxHealth;
        healthBarLevelFour.SetActive(false);
        healthBarLevelFive.SetActive(true);
        updateHealthBar();
    }

    public IEnumerator PlayerDied()
    {
        yield return new WaitForSeconds(0f);
        characterController.enabled = false;
        player.transform.position = playerWaitingPoint.transform.position;
        yield return new WaitForSeconds(0.5f);
        characterController.enabled = true;
    }
}
