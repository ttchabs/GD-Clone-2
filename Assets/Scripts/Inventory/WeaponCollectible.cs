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
    
    [Header("Interaction UI")]
    [SerializeField] private GameObject interactionPrompt; // Assign in inspector
    [SerializeField] private Text promptText; // Optional: to customize text
    
    [Header("Collection Effects")]
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private ParticleSystem collectParticles;
    [SerializeField] private float floatSpeed = 5f;
    [SerializeField] private float floatHeight = 1f;
    [SerializeField] private float collectionDuration = 1f;
    [SerializeField] private AnimationCurve collectionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    // Components
    private SpriteRenderer spriteRenderer;
    private Collider2D weaponCollider;
    private AudioSource audioSource;
    
    // State
    private Transform player;
    private bool playerInRange = false;
    private bool isCollected = false;
    private Vector3 originalPosition;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        weaponCollider = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();
        originalPosition = transform.position;
        
        // Set weapon sprite
        if (spriteRenderer != null && weaponSprite != null)
        {
            spriteRenderer.sprite = weaponSprite;
        }
        
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        // Setup audio source if needed
        if (audioSource == null && collectSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        // Setup interaction prompt
        SetupInteractionPrompt();
        
        // Hide interaction prompt initially
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }
    
    private void SetupInteractionPrompt()
    {
        if (promptText != null)
        {
            promptText.text = $"Press {interactKey} to collect {weaponName}";
        }
    }
    
    private void Update()
    {
        if (isCollected) return;
        
        // Floating animation
        FloatingAnimation();
        
        // Check player distance
        CheckPlayerDistance();
        
        // Handle interaction input
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
            
            // Show/hide interaction prompt
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(playerInRange);
            }
        }
    }
    
    private void CollectWeapon()
    {
        if (isCollected) return;
        
        // Find weapon system
        WeaponSystem weaponSystem = player.GetComponent<WeaponSystem>();
        if (weaponSystem == null)
        {
            Debug.LogError("Player doesn't have a WeaponSystem component!");
            return;
        }
        
        // Try to unlock weapon
        bool unlocked = weaponSystem.UnlockWeapon(weaponID);
        if (unlocked)
        {
            isCollected = true;
            StartCoroutine(CollectionSequence());
        }
    }
    
    private IEnumerator CollectionSequence()
    {
        // Hide interaction prompt
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        
        // Disable collider
        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
        }
        
        // Play collect sound
        if (audioSource != null && collectSound != null)
        {
            audioSource.PlayOneShot(collectSound);
        }
        
        // Play particles
        if (collectParticles != null)
        {
            collectParticles.Play();
        }
        
        // Float to player animation
        Vector3 startPos = transform.position;
        Vector3 targetPos = player.position + Vector3.up * 0.5f;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < collectionDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / collectionDuration;
            float curveValue = collectionCurve.Evaluate(progress);
            
            // Move towards player
            transform.position = Vector3.Lerp(startPos, targetPos, curveValue);
            
            // Scale down
            float scale = Mathf.Lerp(1f, 0.2f, curveValue);
            transform.localScale = Vector3.one * scale;
            
            // Fade out
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = Mathf.Lerp(1f, 0f, curveValue);
                spriteRenderer.color = color;
            }
            
            yield return null;
        }
        
        // Wait a bit for sound to finish
        yield return new WaitForSeconds(0.2f);
        
        // Destroy the object
        Destroy(gameObject);
    }
    
    // Public method to set weapon data (useful for spawning weapons dynamically)
    public void SetWeaponData(int id, string name, Sprite sprite)
    {
        weaponID = id;
        weaponName = name;
        weaponSprite = sprite;
        
        if (spriteRenderer != null && weaponSprite != null)
        {
            spriteRenderer.sprite = weaponSprite;
        }
        
        SetupInteractionPrompt();
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw interaction range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        // Draw original position
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(originalPosition, Vector3.one * 0.2f);
    }
}