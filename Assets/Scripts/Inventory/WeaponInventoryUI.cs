using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WeaponInventoryUI : MonoBehaviour
{
    [Header("UI Configuration")]
    [SerializeField] private GameObject weaponSlotPrefab;
    [SerializeField] private Transform weaponContainer;
    [SerializeField] private float slotSpacing = 80f;
    
    [Header("Visual Settings")]
    [SerializeField] private Color lockedColor = Color.gray;
    [SerializeField] private Color unlockedColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private float selectedScale = 1.2f;
    [SerializeField] private float normalScale = 1f;
    
  
    private WeaponSystem weaponSystem;
    private List<WeaponSlotUI> weaponSlots = new List<WeaponSlotUI>();
    private int currentSelectedIndex = 0;
    
    [System.Serializable]
    private class WeaponSlotUI
    {
        public GameObject slotObject;
        public Image weaponIcon;
        public Image background;
        public Image border;
        public bool isUnlocked;
        public int weaponIndex;
    }
    
    public void Initialize(WeaponSystem weaponSystem)
    {
        this.weaponSystem = weaponSystem;
        CreateWeaponSlots();
        UpdateAllSlots();
    }
    
    private void CreateWeaponSlots()
    {
        if (weaponSystem == null) return;
        
        List<Weapon> weapons = weaponSystem.GetAllWeapons();
        
      
        foreach (Transform child in weaponContainer)
        {
            Destroy(child.gameObject);
        }
        weaponSlots.Clear();
        
        // Create slots in diamond/circular pattern
        for (int i = 0; i < weapons.Count; i++)
        {
            GameObject slotObj = CreateWeaponSlot(i, weapons[i]);
            weaponSlots.Add(GetSlotUIFromObject(slotObj, i));
        }
    }
    
    private GameObject CreateWeaponSlot(int index, Weapon weapon)
    {
        GameObject slotObj;
        
        if (weaponSlotPrefab != null)
        {
            slotObj = Instantiate(weaponSlotPrefab, weaponContainer);
        }
        else
        {
            // Create default slot if no prefab provided
            slotObj = CreateDefaultSlot();
            slotObj.transform.SetParent(weaponContainer);
        }
        
        // Position slots in diamond pattern (similar to spell diamond)
        Vector2 position = GetSlotPosition(index);
        slotObj.GetComponent<RectTransform>().anchoredPosition = position;
        
        // Set weapon icon
        Image iconImage = slotObj.transform.Find("WeaponIcon")?.GetComponent<Image>();
        if (iconImage != null && weapon.weaponIcon != null)
        {
            iconImage.sprite = weapon.weaponIcon;
        }
        
        return slotObj;
    }
    
    private GameObject CreateDefaultSlot()
    {
        GameObject slotObj = new GameObject("WeaponSlot");
        RectTransform slotRect = slotObj.AddComponent<RectTransform>();
        slotRect.sizeDelta = new Vector2(60, 60);
        
        // Background
        GameObject backgroundObj = new GameObject("Background");
        backgroundObj.transform.SetParent(slotObj.transform);
        Image backgroundImg = backgroundObj.AddComponent<Image>();
        backgroundImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        RectTransform bgRect = backgroundObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        // Border
        GameObject borderObj = new GameObject("Border");
        borderObj.transform.SetParent(slotObj.transform);
        Image borderImg = borderObj.AddComponent<Image>();
        borderImg.color = Color.white;
        RectTransform borderRect = borderObj.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = new Vector2(-2, -2);
        borderRect.offsetMax = new Vector2(2, 2);
        
        // Weapon Icon
        GameObject iconObj = new GameObject("WeaponIcon");
        iconObj.transform.SetParent(slotObj.transform);
        Image iconImg = iconObj.AddComponent<Image>();
        iconImg.color = Color.white;
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.1f, 0.1f);
        iconRect.anchorMax = new Vector2(0.9f, 0.9f);
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;
        
        return slotObj;
    }
    
    private WeaponSlotUI GetSlotUIFromObject(GameObject slotObj, int index)
    {
        WeaponSlotUI slot = new WeaponSlotUI();
        slot.slotObject = slotObj;
        slot.weaponIndex = index;
        slot.weaponIcon = slotObj.transform.Find("WeaponIcon")?.GetComponent<Image>();
        slot.background = slotObj.transform.Find("Background")?.GetComponent<Image>();
        slot.border = slotObj.transform.Find("Border")?.GetComponent<Image>();
        slot.isUnlocked = weaponSystem.IsWeaponUnlocked(index);
        
        return slot;
    }
    
    private Vector2 GetSlotPosition(int index)
    {
        // Create diamond pattern for 3 weapons
        switch (index)
        {
            case 0: // Center-bottom (Weapon 1)
                return new Vector2(0, -slotSpacing);
            case 1: // Left (Weapon 2)
                return new Vector2(-slotSpacing, 0);
            case 2: // Right (Weapon 3)
                return new Vector2(slotSpacing, 0);
            default:
                // For more weapons, arrange in circle
                float angle = (360f / weaponSystem.GetAllWeapons().Count) * index * Mathf.Deg2Rad;
                return new Vector2(
                    Mathf.Sin(angle) * slotSpacing,
                    Mathf.Cos(angle) * slotSpacing
                );
        }
    }
    
    public void UpdateSelection(int selectedIndex)
    {
        currentSelectedIndex = selectedIndex;
        UpdateAllSlots();
    }
    
    public void UnlockWeapon(int weaponIndex)
    {
        if (weaponIndex < weaponSlots.Count)
        {
            weaponSlots[weaponIndex].isUnlocked = true;
            UpdateSlotVisual(weaponSlots[weaponIndex]);
        }
    }
    
    private void UpdateAllSlots()
    {
        for (int i = 0; i < weaponSlots.Count; i++)
        {
            weaponSlots[i].isUnlocked = weaponSystem.IsWeaponUnlocked(i);
            UpdateSlotVisual(weaponSlots[i]);
        }
    }
    
    private void UpdateSlotVisual(WeaponSlotUI slot)
    {
        if (slot.slotObject == null) return;
        
        bool isSelected = (slot.weaponIndex == currentSelectedIndex);
        
        // Update scale
        float targetScale = isSelected ? selectedScale : normalScale;
        slot.slotObject.transform.localScale = Vector3.one * targetScale;
        
        // Update colors
        Color iconColor = slot.isUnlocked ? unlockedColor : lockedColor;
        Color borderColor = isSelected ? selectedColor : unlockedColor;
        
        if (slot.weaponIcon != null)
        {
            slot.weaponIcon.color = iconColor;
        }
        
        if (slot.border != null)
        {
            slot.border.color = borderColor;
        }
        
        // Update alpha for locked weapons
        CanvasGroup canvasGroup = slot.slotObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = slot.slotObject.AddComponent<CanvasGroup>();
        }
        
        canvasGroup.alpha = slot.isUnlocked ? 1f : 0.5f;
    }
    
    // Public method to show/hide inventory
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}