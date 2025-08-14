using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Weapon
{
    public int weaponID;
    public string weaponName;
    public GameObject weaponPrefab;
    public Sprite weaponIcon;
    public int damage;
    public float attackSpeed;
    public float range;
    public bool isUnlocked;
}

public class WeaponSystem : MonoBehaviour
{
    [Header("Weapon Configuration")]
    [SerializeField] private List<Weapon> availableWeapons = new List<Weapon>();
    [SerializeField] private Transform weaponHolder;
    
    [Header("UI References")]
    [SerializeField] private WeaponInventoryUI inventoryUI;
    
    // Current weapon state
    private int currentWeaponIndex = 0;
    private GameObject currentWeaponInstance;
    private Weapon currentWeapon;
    
    // Events
    public System.Action<Weapon> OnWeaponChanged;
    public System.Action<Weapon> OnWeaponUnlocked;
    
    private void Awake()
    {
        // Create weapon holder if it doesn't exist
        if (weaponHolder == null)
        {
            GameObject holderObj = new GameObject("WeaponHolder");
            holderObj.transform.SetParent(transform);
            holderObj.transform.localPosition = Vector3.zero;
            weaponHolder = holderObj.transform;
        }
        
        // Initialize weapons - only weapon 1 is unlocked at start
        InitializeWeapons();
    }
    
    private void Start()
    {
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
    
    private void InitializeWeapons()
    {
        for (int i = 0; i < availableWeapons.Count; i++)
        {
            // Only weapon 1 (index 0) starts unlocked
            availableWeapons[i].isUnlocked = (i == 0);
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
        // Destroy current weapon instance
        if (currentWeaponInstance != null)
        {
            Destroy(currentWeaponInstance);
        }
        
        // Set new current weapon
        currentWeaponIndex = weaponIndex;
        currentWeapon = availableWeapons[weaponIndex];
        
        // Instantiate new weapon
        if (currentWeapon.weaponPrefab != null)
        {
            currentWeaponInstance = Instantiate(currentWeapon.weaponPrefab, weaponHolder);
            currentWeaponInstance.transform.localPosition = Vector3.zero;
            currentWeaponInstance.transform.localRotation = Quaternion.identity;
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
    
    // Public getters
    public Weapon GetCurrentWeapon()
    {
        return currentWeapon;
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
}