using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WeaponInventoryUI : MonoBehaviour
{
    [Header("Weapon Slot References")]
    [SerializeField] private WeaponSlot slot1;
    [SerializeField] private WeaponSlot slot2;
    [SerializeField] private WeaponSlot slot3;
    
    [Header("Visual Settings")]
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private float selectedScale = 1.2f;
    [SerializeField] private float normalScale = 1f;
    
    private WeaponSystem weaponSystem;
    private List<WeaponSlot> weaponSlots = new List<WeaponSlot>();
    private int currentSelectedIndex = 0;
    
    [System.Serializable]
    public class WeaponSlot
    {
        public GameObject slotContainer;
        public Image weaponIcon;
        public Image slotBackground;
        public GameObject lockedOverlay; 
    }
    
    public void Initialize(WeaponSystem weaponSystem)
    {
        this.weaponSystem = weaponSystem;
        
      
        weaponSlots.Clear();
        weaponSlots.Add(slot1);
        weaponSlots.Add(slot2);
        weaponSlots.Add(slot3);
        
        
        InitializeSlots();
        UpdateAllSlots();
    }
    
    private void InitializeSlots()
    {
        List<Weapon> weapons = weaponSystem.GetAllWeapons();
        
        for (int i = 0; i < weaponSlots.Count && i < weapons.Count; i++)
        {
            WeaponSlot slot = weaponSlots[i];
            Weapon weapon = weapons[i];
            
           
            if (slot.weaponIcon != null && weapon.weaponIcon != null)
            {
                slot.weaponIcon.sprite = weapon.weaponIcon;
            }
            
          
            if (slot.weaponIcon != null)
            {
                slot.weaponIcon.gameObject.SetActive(weapon.isUnlocked);
            }
            
           
            if (slot.lockedOverlay != null)
            {
                slot.lockedOverlay.SetActive(!weapon.isUnlocked);
            }
        }
    }
    
    public void UpdateSelection(int selectedIndex)
    {
        
        if (currentSelectedIndex < weaponSlots.Count)
        {
            SetSlotSelected(weaponSlots[currentSelectedIndex], false);
        }
        
       
        currentSelectedIndex = selectedIndex;
        if (currentSelectedIndex < weaponSlots.Count)
        {
            SetSlotSelected(weaponSlots[currentSelectedIndex], true);
        }
    }
    
    public void UnlockWeapon(int weaponIndex)
    {
        if (weaponIndex < weaponSlots.Count)
        {
            WeaponSlot slot = weaponSlots[weaponIndex];
            
           
            if (slot.weaponIcon != null)
            {
                slot.weaponIcon.gameObject.SetActive(true);
            }
            
           
            if (slot.lockedOverlay != null)
            {
                slot.lockedOverlay.SetActive(false);
            }
            
           
        }
    }
    
    private void UpdateAllSlots()
    {
        List<Weapon> weapons = weaponSystem.GetAllWeapons();
        
        for (int i = 0; i < weaponSlots.Count && i < weapons.Count; i++)
        {
            WeaponSlot slot = weaponSlots[i];
            Weapon weapon = weapons[i];
            
            
            if (slot.weaponIcon != null)
            {
                slot.weaponIcon.gameObject.SetActive(weapon.isUnlocked);
            }
            
           
            if (slot.lockedOverlay != null)
            {
                slot.lockedOverlay.SetActive(!weapon.isUnlocked);
            }
            
            
            SetSlotSelected(slot, i == currentSelectedIndex);
        }
    }
    
    private void SetSlotSelected(WeaponSlot slot, bool selected)
    {
        if (slot.slotContainer != null)
        {
            
            float targetScale = selected ? selectedScale : normalScale;
            slot.slotContainer.transform.localScale = Vector3.one * targetScale;
        }
        
        if (slot.slotBackground != null)
        {
            
            slot.slotBackground.color = selected ? selectedColor : normalColor;
        }
    }
    
  
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}