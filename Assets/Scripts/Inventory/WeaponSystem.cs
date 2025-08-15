using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Weapon
{
    public int weaponID;
    public string weaponName;
    public GameObject weaponPrefab; // Not used for pre-placed weapons, but kept for compatibility
    public Sprite weaponIcon; // For UI display
    public int damage;
    public float attackSpeed;
    public float range;
    public bool isUnlocked;
    
    [HideInInspector]
    public GameObject weaponInstance; // Runtime reference to the weapon GameObject
}

public class WeaponSystem : MonoBehaviour
{
    [Header("Weapon Configuration")]
    [SerializeField] private List<Weapon> availableWeapons = new List<Weapon>();
    [SerializeField] private Transform weaponHolder; // Your existing WeaponHolder under Player
    
    [Header("Pre-placed Weapon References")]
    [SerializeField] private GameObject weapon1GameObject; // Your existing "fakesword"
    [SerializeField] private GameObject weapon2GameObject; // Your existing axe (inactive)
    [SerializeField] private GameObject weapon3GameObject; // Your existing bow (inactive)
    
    [Header("UI References")]
    [SerializeField] private WeaponInventoryUI inventoryUI;
    
    // Current weapon state
    private int currentWeaponIndex = 0;
    private Weapon currentWeapon;
    
    // Events
    public System.Action<Weapon> OnWeaponChanged;
    public System.Action<Weapon> OnWeaponUnlocked;
    
    private void Awake()
    {
        // Find weapon holder if not assigned
        if (weaponHolder == null)
        {
            weaponHolder = transform.Find("WeaponHolder");
            if (weaponHolder == null)
            {
                Debug.LogError("WeaponHolder not found! Please assign it in the inspector.");
            }
        }
        
        // Auto-find weapons if not assigned
        AutoFindWeaponsIfNeeded();
        
        // Initialize weapons - only weapon 1 is unlocked at start
        InitializeWeapons();
    }
    
    private void Start()
    {
        // Link pre-placed weapons to weapon data
        LinkPrePlacedWeapons();
        
        // Equip the first weapon
        if (availableWeapons.Count > 0)
        {
            EquipWeapon(0);
        }
        
        // Update UI
        if (inventoryUI != null)
        {
            inventoryUI.Initialize(this);
        }
    }
    
    private void AutoFindWeaponsIfNeeded()
    {
        // Try to find weapons automatically if not assigned
        if (weapon1GameObject == null)
        {
            weapon1GameObject = transform.Find("fakesword")?.gameObject;
            if (weapon1GameObject == null && weaponHolder != null)
            {
                weapon1GameObject = weaponHolder.Find("fakesword")?.gameObject;
            }
        }
        
        if (weapon2GameObject == null && weaponHolder != null)
        {
            // Look for common axe names
            weapon2GameObject = weaponHolder.Find("axe")?.gameObject;
            if (weapon2GameObject == null)
                weapon2GameObject = weaponHolder.Find("Axe")?.gameObject;
            if (weapon2GameObject == null)
                weapon2GameObject = weaponHolder.Find("weapon2")?.gameObject;
        }
        
        if (weapon3GameObject == null && weaponHolder != null)
        {
            // Look for common bow names
            weapon3GameObject = weaponHolder.Find("bow")?.gameObject;
            if (weapon3GameObject == null)
                weapon3GameObject = weaponHolder.Find("Bow")?.gameObject;
            if (weapon3GameObject == null)
                weapon3GameObject = weaponHolder.Find("weapon3")?.gameObject;
        }
    }
    
    private void InitializeWeapons()
    {
        for (int i = 0; i < availableWeapons.Count; i++)
        {
            // Only weapon 1 (index 0) starts unlocked
            availableWeapons[i].isUnlocked = (i == 0);
        }
    }
    
    private void LinkPrePlacedWeapons()
    {
        // Link weapon GameObjects to weapon data
        GameObject[] weaponObjects = { weapon1GameObject, weapon2GameObject, weapon3GameObject };
        
        for (int i = 0; i < availableWeapons.Count && i < weaponObjects.Length; i++)
        {
            if (weaponObjects[i] != null)
            {
                // Link the pre-placed GameObject
                availableWeapons[i].weaponInstance = weaponObjects[i];
                
                // Set weapon name for identification
                weaponObjects[i].name = $"Weapon_{availableWeapons[i].weaponName}";
                
                // Set initial state - only weapon 1 should be active
                weaponObjects[i].SetActive(i == 0 && availableWeapons[i].isUnlocked);
                
                Debug.Log($"Linked pre-placed weapon: {availableWeapons[i].weaponName} (Active: {weaponObjects[i].activeInHierarchy})");
            }
            else
            {
                Debug.LogWarning($"Weapon {i + 1} GameObject not assigned or found!");
            }
        }
    }
    
    public void SwitchToWeapon(int weaponIndex)
    {
        // Check if weapon exists and is unlocked
        if (weaponIndex < 0 || weaponIndex >= availableWeapons.Count)
        {
            Debug.LogWarning($"Weapon index {weaponIndex} is out of range!");
            return;
        }
        
        if (!availableWeapons[weaponIndex].isUnlocked)
        {
            Debug.Log($"Weapon {availableWeapons[weaponIndex].weaponName} is not unlocked yet!");
            return;
        }
        
        EquipWeapon(weaponIndex);
    }
    
    private void EquipWeapon(int weaponIndex)
    {
        // Handle current weapon unequipping
        if (currentWeapon != null && currentWeapon.weaponInstance != null)
        {
            currentWeapon.weaponInstance.SetActive(false);
            
            // Call unequip behavior if available
            WeaponBehavior currentBehavior = currentWeapon.weaponInstance.GetComponent<WeaponBehavior>();
            if (currentBehavior != null)
            {
                currentBehavior.OnUnequipped();
            }
        }
        
        // Set new current weapon
        currentWeaponIndex = weaponIndex;
        currentWeapon = availableWeapons[weaponIndex];
        
        // Activate new weapon if it exists and is unlocked
        if (currentWeapon.weaponInstance != null && currentWeapon.isUnlocked)
        {
            currentWeapon.weaponInstance.SetActive(true);
            
            // Call equip behavior if available
            WeaponBehavior newBehavior = currentWeapon.weaponInstance.GetComponent<WeaponBehavior>();
            if (newBehavior != null)
            {
                newBehavior.OnEquipped();
            }
        }
        
        // Update UI
        if (inventoryUI != null)
        {
            inventoryUI.UpdateSelection(currentWeaponIndex);
        }
        
        // Invoke event
        OnWeaponChanged?.Invoke(currentWeapon);
        
        Debug.Log($"Equipped weapon: {currentWeapon.weaponName}");
    }
    
    public bool UnlockWeapon(int weaponID)
    {
        // Find weapon by ID
        for (int i = 0; i < availableWeapons.Count; i++)
        {
            if (availableWeapons[i].weaponID == weaponID)
            {
                if (!availableWeapons[i].isUnlocked)
                {
                    availableWeapons[i].isUnlocked = true;
                    
                    // Update UI
                    if (inventoryUI != null)
                    {
                        inventoryUI.UnlockWeapon(i);
                    }
                    
                    // Auto-equip the newly unlocked weapon
                    EquipWeapon(i);
                    
                    // Invoke event
                    OnWeaponUnlocked?.Invoke(availableWeapons[i]);
                    
                    Debug.Log($"Unlocked weapon: {availableWeapons[i].weaponName}");
                    return true;
                }
                else
                {
                    Debug.Log($"Weapon {availableWeapons[i].weaponName} is already unlocked!");
                    return false;
                }
            }
        }
        
        Debug.LogWarning($"Weapon with ID {weaponID} not found!");
        return false;
    }
    
    // Method to handle weapon flipping when player changes direction
    public void FlipWeapons(bool facingRight)
    {
        // Since all weapons are children of weaponHolder or player, flip only the active weapon
        if (currentWeapon != null && currentWeapon.weaponInstance != null)
        {
            SpriteRenderer weaponSprite = currentWeapon.weaponInstance.GetComponent<SpriteRenderer>();
            if (weaponSprite != null)
            {
                weaponSprite.flipX = !facingRight;
            }
        }
    }
    
    // Method to trigger attack animation on current weapon
    public void TriggerWeaponAttack()
    {
        if (currentWeapon != null && currentWeapon.weaponInstance != null && currentWeapon.weaponInstance.activeInHierarchy)
        {
            WeaponBehavior weaponBehavior = currentWeapon.weaponInstance.GetComponent<WeaponBehavior>();
            if (weaponBehavior != null)
            {
                weaponBehavior.OnAttack();
            }
        }
    }
    
    // Public getters
    public Weapon GetCurrentWeapon()
    {
        return currentWeapon;
    }
    
    public GameObject GetCurrentWeaponGameObject()
    {
        return currentWeapon?.weaponInstance;
    }
    
    public List<Weapon> GetAllWeapons()
    {
        return availableWeapons;
    }
    
    public int GetCurrentWeaponIndex()
    {
        return currentWeaponIndex;
    }
    
    public bool IsWeaponUnlocked(int weaponIndex)
    {
        if (weaponIndex < 0 || weaponIndex >= availableWeapons.Count)
            return false;
            
        return availableWeapons[weaponIndex].isUnlocked;
    }
    
    // Method to get weapon by ID (useful for attack system)
    public Weapon GetWeaponByID(int weaponID)
    {
        for (int i = 0; i < availableWeapons.Count; i++)
        {
            if (availableWeapons[i].weaponID == weaponID)
            {
                return availableWeapons[i];
            }
        }
        return null;
    }
}