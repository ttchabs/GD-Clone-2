using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WeaponCollectible : MonoBehaviour
{
    [Header("Weapon Configuration")]
    [SerializeField] private int weaponID;
    [SerializeField] private string weaponName;
    [SerializeField] private Sprite weaponSprite;
    
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private KeyCode interactKey = KeyCode.I;
    
    [Header("UI References")]
    [SerializeField] private GameObject interactionUI;
    [SerializeField] private Text interactionText;
    [SerializeField] private Canvas worldCanvas;
    
    [Header("Collection Animation")]
    [SerializeField] private float floatSpeed = 5f;
    [SerializeField] private float floatHeight = 1f;
    [SerializeField] private float collectionDuration = 1f;
    [SerializeField] private AnimationCurve collectionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
   
    private SpriteRenderer spriteRenderer;
    private Collider2D weaponCollider;
    
   
    private Transform player;
    private bool playerInRange = false;
    private bool isCollected = false;
    private Vector3 originalPosition;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        weaponCollider = GetComponent<Collider2D>();
        originalPosition = transform.position;
        
      
        if (spriteRenderer != null && weaponSprite != null)
        {
            spriteRenderer.sprite = weaponSprite;
        }
        
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        
        SetupInteractionUI();
    }
    
    private void SetupInteractionUI()
    {
        if (interactionUI == null)
        {
            CreateDefaultInteractionUI();
        }
        
        if (interactionText != null)
        {
            interactionText.text = $"Press {interactKey} to collect {weaponName}";
        }
        
       
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
    }
    
    private void CreateDefaultInteractionUI()
    {
        
        if (worldCanvas == null)
        {
            GameObject canvasObj = new GameObject("WeaponUI_Canvas");
            canvasObj.transform.SetParent(transform);
            canvasObj.transform.localPosition = Vector3.up * 1.5f;
            
            worldCanvas = canvasObj.AddComponent<Canvas>();
            worldCanvas.renderMode = RenderMode.WorldSpace;
            worldCanvas.worldCamera = Camera.main;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 100f;
            
            RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(3f, 1f);
        }
        
       
        GameObject uiObj = new GameObject("InteractionUI");
        uiObj.transform.SetParent(worldCanvas.transform);
        uiObj.transform.localPosition = Vector3.zero;
        uiObj.transform.localScale = Vector3.one;
        
        
        Image background = uiObj.AddComponent<Image>();
        background.color = new Color(0, 0, 0, 0.7f);
        
        RectTransform uiRect = uiObj.GetComponent<RectTransform>();
        uiRect.anchorMin = Vector2.zero;
        uiRect.anchorMax = Vector2.one;
        uiRect.offsetMin = Vector2.zero;
        uiRect.offsetMax = Vector2.zero;
        
       
        GameObject textObj = new GameObject("InteractionText");
        textObj.transform.SetParent(uiObj.transform);
        
        Text text = textObj.AddComponent<Text>();
        text.text = $"Press {interactKey} to collect {weaponName}";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 12;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 5);
        textRect.offsetMax = new Vector2(-10, -5);
        
        interactionUI = uiObj;
        interactionText = text;
    }
    
    private void Update()
    {
        if (isCollected) return;
        
        
        FloatingAnimation(); //maybe weapon is floating
        
      
        CheckPlayerDistance();
        
        
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            CollectWeapon();
        }
    }
    
    private void FloatingAnimation()
    {
        float newY = originalPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(originalPosition.x, newY, originalPosition.z);
    }
    
    private void CheckPlayerDistance()
    {
        if (player == null) return;
        
        float distance = Vector2.Distance(transform.position, player.position);
        bool inRange = distance <= interactionRange;
        
        if (inRange != playerInRange)
        {
            playerInRange = inRange;
            
            if (interactionUI != null)
            {
                interactionUI.SetActive(playerInRange);
            }
        }
    }
    
    private void CollectWeapon()
    {
        if (isCollected) return;
        
       
        WeaponSystem weaponSystem = player.GetComponent<WeaponSystem>();
        if (weaponSystem == null)
        {
          
            return;
        }
        
      
        bool unlocked = weaponSystem.UnlockWeapon(weaponID);
        if (unlocked)
        {
            isCollected = true;
            StartCoroutine(CollectionAnimation());
        }
    }
    
    private IEnumerator CollectionAnimation()
    {
        
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
        
        
        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
        }
        
       
        Vector3 startPos = transform.position;
        Vector3 targetPos = player.position + Vector3.up * 0.5f;
        
        float elapsedTime = 0f;
        
        //  weapon float to player
        while (elapsedTime < collectionDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / collectionDuration;
            float curveValue = collectionCurve.Evaluate(progress);
            
            // move towards player
            transform.position = Vector3.Lerp(startPos, targetPos, curveValue);
            
            
            float scale = Mathf.Lerp(1f, 0.2f, curveValue);
            transform.localScale = Vector3.one * scale;
            
           
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = Mathf.Lerp(1f, 0f, curveValue);
                spriteRenderer.color = color;
            }
            
            yield return null;
        }
        
        
        Destroy(gameObject);
    }
    
   
    public void SetWeaponData(int id, string name, Sprite sprite)
    {
        weaponID = id;
        weaponName = name;
        weaponSprite = sprite;
        
        if (spriteRenderer != null && weaponSprite != null)
        {
            spriteRenderer.sprite = weaponSprite;
        }
        
        if (interactionText != null)
        {
            interactionText.text = $"Press {interactKey} to collect {weaponName}";
        }
    }
    
    private void OnDrawGizmosSelected()
    {
       
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
       
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(originalPosition, Vector3.one * 0.2f);
    }
}