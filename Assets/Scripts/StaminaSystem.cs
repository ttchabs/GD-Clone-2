using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class StaminaSystem : MonoBehaviour
{
    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina;
    
    [Header("Sprint Settings")]
    public float sprintDepletionRate = 20f;  // stamina lost per second while sprinting
    
    [Header("Attack Stamina Costs")]
    public float weapon1StaminaCost = 10f;   
    public float weapon2StaminaCost = 15f;    
    public float weapon3StaminaCost = 25f;   
    
    [Header("Regeneration Settings")]
    public float regenerationRate = 15f;     // stamina gained per second (slower than depletion)
    public float regenerationDelay = 1f;     // delay before regeneration starts after using stamina
    
    [Header("UI References")]
    public Slider staminaBar;
    public GameObject staminaUI;
    public GameObject characterIcon;
    
    [Header("Low Stamina Warning")]
    public float lowStaminaThreshold = 20f;
    public Color lowStaminaColor = Color.red;
    public Color normalStaminaColor = Color.cyan; 
    public Color criticalStaminaColor = Color.red;
    
    [Header("Events")]
    public UnityEvent OnStaminaDepleted;
    public UnityEvent OnStaminaRecovered;
    
    private Image staminaFillImage;
    private bool isSprinting = false;
    private bool canUseStamina = true;
    private bool isFlashing = false;
    private float lastStaminaUseTime;
    private WeaponSystem weaponSystem;
    
    private void Start()
    {
        currentStamina = maxStamina;
        
        
        weaponSystem = GetComponent<WeaponSystem>();
        if (weaponSystem == null)
        {
            weaponSystem = FindObjectOfType<WeaponSystem>();
        }
        
        if (staminaBar != null)
        {
            staminaBar.maxValue = maxStamina;
            staminaBar.value = currentStamina;
            staminaFillImage = staminaBar.fillRect.GetComponent<Image>();
        }
        
        if (staminaUI != null)
            staminaUI.SetActive(true);
            
        if (characterIcon != null)
            characterIcon.SetActive(true);
    }
    
    private void Update()
    {
        UpdateStamina();
        UpdateUI();
        CheckCriticalStamina();
    }
    
    private void UpdateStamina()
    {
     
        if (isSprinting && currentStamina > 0)
        {
            currentStamina -= sprintDepletionRate * Time.deltaTime;
            currentStamina = Mathf.Max(0, currentStamina);
            lastStaminaUseTime = Time.time;
            
            if (currentStamina <= 0)
            {
                canUseStamina = false;
                OnStaminaDepleted?.Invoke();
            }
        }
        
        
        bool canRegenerate = !isSprinting && 
                           currentStamina < maxStamina && 
                           Time.time >= lastStaminaUseTime + regenerationDelay;
        
        if (canRegenerate)
        {
            float previousStamina = currentStamina;
            currentStamina += regenerationRate * Time.deltaTime;
            currentStamina = Mathf.Min(maxStamina, currentStamina);
            
         
            if (previousStamina <= 0 && currentStamina > 0)
            {
                canUseStamina = true;
                OnStaminaRecovered?.Invoke();
            }
            

            if (currentStamina > lowStaminaThreshold)
            {
                StopFlashing();
            }
        }
    }
    
    private void CheckCriticalStamina()
    {
        if (currentStamina <= lowStaminaThreshold && currentStamina > 0 && !isFlashing)
        {
            StartFlashing();
        }
        else if (currentStamina > lowStaminaThreshold && isFlashing)
        {
            StopFlashing();
        }
    }
    
    private void StartFlashing()
    {
        if (!isFlashing)
        {
            isFlashing = true;
            InvokeRepeating(nameof(FlashWarning), 0f, 0.5f);
        }
    }
    
    private void StopFlashing()
    {
        if (isFlashing)
        {
            isFlashing = false;
            CancelInvoke(nameof(FlashWarning));
            
            if (staminaFillImage != null)
            {
                staminaFillImage.color = normalStaminaColor;
            }
        }
    }
    
    private void FlashWarning()
    {
        if (staminaFillImage != null)
        {
            staminaFillImage.color = staminaFillImage.color == criticalStaminaColor ? 
                                   lowStaminaColor : criticalStaminaColor;
        }
    }
    
    private void UpdateUI()
    {
        if (staminaBar != null)
        {
            staminaBar.value = currentStamina;
            
            if (staminaFillImage != null && !isFlashing)
            {
                if (currentStamina <= lowStaminaThreshold)
                {
                    staminaFillImage.color = lowStaminaColor;
                }
                else
                {
                    staminaFillImage.color = normalStaminaColor;
                }
            }
        }
    }
    
 
    public void StartSprinting()
    {
        if (canUseStamina && currentStamina > 0)
        {
            isSprinting = true;
        }
    }
    
    public void StopSprinting()
    {
        isSprinting = false;
    }
    
    public bool CanSprint()
    {
        return canUseStamina && currentStamina > 0;
    }
    
    public bool CanAttack()
    {
        if (!canUseStamina || currentStamina <= 0) return false;
        
        float staminaCost = GetCurrentWeaponStaminaCost();
        return currentStamina >= staminaCost;
    }
    
    public bool TryUseAttackStamina()
    {
        if (!CanAttack()) return false;
        
        float staminaCost = GetCurrentWeaponStaminaCost();
        currentStamina -= staminaCost;
        currentStamina = Mathf.Max(0, currentStamina);
        lastStaminaUseTime = Time.time;
        
        if (currentStamina <= 0)
        {
            canUseStamina = false;
            OnStaminaDepleted?.Invoke();
        }
        
        Debug.Log($"Used {staminaCost} stamina for attack. Remaining: {currentStamina:F1}");
        return true;
    }
    
    private float GetCurrentWeaponStaminaCost()
    {
        if (weaponSystem == null) return weapon1StaminaCost;
        
        Weapon currentWeapon = weaponSystem.GetCurrentWeapon();
        if (currentWeapon == null) return weapon1StaminaCost;
        
        switch (currentWeapon.weaponID)
        {
            case 1: return weapon1StaminaCost;  
            case 2: return weapon2StaminaCost; 
            case 3: return weapon3StaminaCost;  
            default: return weapon1StaminaCost;
        }
    }
    
   
    public void StartDepletion()
    {
        
        isSprinting = true;
    }
    
    public void StopDepletion()
    {
       
        isSprinting = false;
        StopFlashing();
    }
    
    public bool CanUseStamina()
    {
        return canUseStamina && currentStamina > 0;
    }
    
    public void RestoreFullStamina()
    {
        currentStamina = maxStamina;
        canUseStamina = true;
        StopFlashing();
    }
    
    public void ShowUI()
    {
        if (staminaUI != null)
            staminaUI.SetActive(true);
            
        if (characterIcon != null)
            characterIcon.SetActive(true);
    }
    
    public void HideUI()
    {
        if (staminaUI != null)
            staminaUI.SetActive(false);
            
        if (characterIcon != null)
            characterIcon.SetActive(false);
            
        StopFlashing();
    }
    
    public float GetStaminaPercentage()
    {
        return (currentStamina / maxStamina) * 100f;
    }
    
    public float GetCurrentStamina()
    {
        return currentStamina;
    }
    
    public float GetMaxStamina()
    {
        return maxStamina;
    }
    
   
    // [ContextMenu("Debug Weapon Costs")]
    // public void DebugWeaponCosts()
    // {
    //     Debug.Log($"Weapon 1 (Sword) Cost: {weapon1StaminaCost}");
    //     Debug.Log($"Weapon 2 (Axe) Cost: {weapon2StaminaCost}");
    //     Debug.Log($"Weapon 3 (Staff) Cost: {weapon3StaminaCost}");
        
    //     if (weaponSystem != null)
    //     {
    //         Weapon current = weaponSystem.GetCurrentWeapon();
    //         if (current != null)
    //         {
    //             Debug.Log($"Current Weapon: {current.weaponName} (Cost: {GetCurrentWeaponStaminaCost()})");
    //         }
    //     }
    // }
}