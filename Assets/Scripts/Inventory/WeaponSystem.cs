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
    
    [HideInInspector]
    public GameObject weaponInstance; 
}

public class WeaponSystem : MonoBehaviour
{
    [Header("Weapon Configuration")]
    [SerializeField] private List<Weapon> availableWeapons = new List<Weapon>();
    [SerializeField] private Transform weaponHolder; 
    [Header("Pre-placed Weapon References")]
    [SerializeField] private GameObject weapon1GameObject; //fakesword"
    [SerializeField] private GameObject weapon2GameObject; //axe (inactive)
    [SerializeField] private GameObject weapon3GameObject; // scythe 
    
    [Header("UI References")]
    [SerializeField] private WeaponInventoryUI inventoryUI;
    
    
    private int currentWeaponIndex = 0;
    private Weapon currentWeapon;
    
  
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
                // Debug.LogError("WeaponHolder not found! Please assign it in the inspector.");
            }
        }
        
       
        AutoFindWeaponsIfNeeded();
        
        // - only weapon 1 is unlocked at start
        InitializeWeapons();
    }
    
    private void Start()
    {
       
        LinkPrePlacedWeapons();
        
       
        if (availableWeapons.Count > 0)
        {
            EquipWeapon(0);
        }
        
       
        if (inventoryUI != null)
        {
            inventoryUI.Initialize(this);
        }
    }
    
    private void AutoFindWeaponsIfNeeded()
    {
       
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
            
            weapon2GameObject = weaponHolder.Find("axe")?.gameObject;
            if (weapon2GameObject == null)
                weapon2GameObject = weaponHolder.Find("Axe")?.gameObject;
            if (weapon2GameObject == null)
                weapon2GameObject = weaponHolder.Find("weapon2")?.gameObject;
        }
        
        if (weapon3GameObject == null && weaponHolder != null)
        {
            
            weapon3GameObject = weaponHolder.Find("scythe")?.gameObject;
            if (weapon3GameObject == null)
                weapon3GameObject = weaponHolder.Find("Scythe")?.gameObject;
            if (weapon3GameObject == null)
                weapon3GameObject = weaponHolder.Find("weapon3")?.gameObject;
        }
    }
    
    private void InitializeWeapons()
    {
        for (int i = 0; i < availableWeapons.Count; i++)
        {
           
            availableWeapons[i].isUnlocked = (i == 0);
        }
    }
    
    private void LinkPrePlacedWeapons()
    {
        
        GameObject[] weaponObjects = { weapon1GameObject, weapon2GameObject, weapon3GameObject };
        
        for (int i = 0; i < availableWeapons.Count && i < weaponObjects.Length; i++)
        {
            if (weaponObjects[i] != null)
            {
                
                availableWeapons[i].weaponInstance = weaponObjects[i];
                
               
                weaponObjects[i].name = $"Weapon_{availableWeapons[i].weaponName}";
                
                
                weaponObjects[i].SetActive(i == 0 && availableWeapons[i].isUnlocked);
                
              
            }
            else
            {
               
            }
        }
    }
    
    public void SwitchToWeapon(int weaponIndex)
    {
      
        if (weaponIndex < 0 || weaponIndex >= availableWeapons.Count)
        {
            Debug.LogWarning($"Weapon index {weaponIndex} is out of range!");
            return;
        }
        
        if (!availableWeapons[weaponIndex].isUnlocked)
        {
            // Debug.Log($"Weapon {availableWeapons[weaponIndex].weaponName} is not unlocked yet!");
            return;
        }
        
        EquipWeapon(weaponIndex);
    }
    
    private void EquipWeapon(int weaponIndex)
    {
       
        if (currentWeapon != null && currentWeapon.weaponInstance != null)
        {
            currentWeapon.weaponInstance.SetActive(false);
            
           
            WeaponBehavior currentBehavior = currentWeapon.weaponInstance.GetComponent<WeaponBehavior>();
            if (currentBehavior != null)
            {
                currentBehavior.OnUnequipped();
            }
        }
        
       
        currentWeaponIndex = weaponIndex;
        currentWeapon = availableWeapons[weaponIndex];
        
        
        if (currentWeapon.weaponInstance != null && currentWeapon.isUnlocked)
        {
            currentWeapon.weaponInstance.SetActive(true);
            
           
            WeaponBehavior newBehavior = currentWeapon.weaponInstance.GetComponent<WeaponBehavior>();
            if (newBehavior != null)
            {
                newBehavior.OnEquipped();
            }
        }
        
      
        if (inventoryUI != null)
        {
            inventoryUI.UpdateSelection(currentWeaponIndex);
        }
        
        
        OnWeaponChanged?.Invoke(currentWeapon);
        
       
    }
    
    public bool UnlockWeapon(int weaponID)
    {
      
        for (int i = 0; i < availableWeapons.Count; i++)
        {
            if (availableWeapons[i].weaponID == weaponID)
            {
                if (!availableWeapons[i].isUnlocked)
                {
                    availableWeapons[i].isUnlocked = true;
                    
                   
                    if (inventoryUI != null)
                    {
                        inventoryUI.UnlockWeapon(i);
                    }
                    
                 
                    EquipWeapon(i);
                    
                  
                    OnWeaponUnlocked?.Invoke(availableWeapons[i]);
                    
                  
                    return true;
                }
                else
                {
                   
                    return false;
                }
            }
        }
        
        Debug.LogWarning($"Weapon with ID {weaponID} not found!");
        return false;
    }
    
   
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