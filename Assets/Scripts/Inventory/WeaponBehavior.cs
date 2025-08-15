using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBehavior : MonoBehaviour
{
    [Header("Weapon Visual Settings")]
    [SerializeField] private Vector3 idlePosition = Vector3.zero;
    [SerializeField] private Vector3 idleRotation = Vector3.zero;
    [SerializeField] private Vector3 attackPosition = Vector3.zero;
    [SerializeField] private Vector3 attackRotation = Vector3.zero;
    
    [Header("Animation Settings")]
    [SerializeField] private float animationSpeed = 5f;
    [SerializeField] private AnimationCurve attackCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    // Components
    private SpriteRenderer weaponSprite;
    
    // State
    private bool isAttacking = false;
    private Vector3 targetPosition;
    private Vector3 targetRotation;
    
    private void Awake()
    {
        weaponSprite = GetComponent<SpriteRenderer>();
        
        // Set initial position and rotation
        targetPosition = idlePosition;
        targetRotation = idleRotation;
        
        transform.localPosition = idlePosition;
        transform.localEulerAngles = idleRotation;
    }
    
    private void Update()
    {
        // Smoothly move to target position and rotation
        if (!isAttacking)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * animationSpeed);
            transform.localEulerAngles = Vector3.Lerp(transform.localEulerAngles, targetRotation, Time.deltaTime * animationSpeed);
        }
    }
    
    // Called when this weapon is equipped
    public void OnEquipped()
    {
        targetPosition = idlePosition;
        targetRotation = idleRotation;
        
        Debug.Log($"Weapon {gameObject.name} equipped");
    }
    
    // Called when this weapon is unequipped
    public void OnUnequipped()
    {
        // Reset any attack animations
        StopAttack();
        
        Debug.Log($"Weapon {gameObject.name} unequipped");
    }
    
    // Called when player attacks with this weapon
    public void OnAttack()
    {
        if (!isAttacking)
        {
            StartCoroutine(AttackAnimation());
        }
    }
    
    private System.Collections.IEnumerator AttackAnimation()
    {
        isAttacking = true;
        
        Vector3 startPos = transform.localPosition;
        Vector3 startRot = transform.localEulerAngles;
        
        float duration = 0.3f; // Attack animation duration
        float elapsedTime = 0f;
        
        // Attack animation
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            float curveValue = attackCurve.Evaluate(progress);
            
            // Move to attack position and back
            Vector3 currentTargetPos = Vector3.Lerp(idlePosition, attackPosition, curveValue);
            Vector3 currentTargetRot = Vector3.Lerp(idleRotation, attackRotation, curveValue);
            
            transform.localPosition = Vector3.Lerp(startPos, currentTargetPos, progress * animationSpeed * Time.deltaTime);
            transform.localEulerAngles = Vector3.Lerp(startRot, currentTargetRot, progress * animationSpeed * Time.deltaTime);
            
            yield return null;
        }
        
        // Return to idle
        float returnDuration = 0.2f;
        elapsedTime = 0f;
        
        while (elapsedTime < returnDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / returnDuration;
            
            transform.localPosition = Vector3.Lerp(transform.localPosition, idlePosition, progress);
            transform.localEulerAngles = Vector3.Lerp(transform.localEulerAngles, idleRotation, progress);
            
            yield return null;
        }
        
        isAttacking = false;
    }
    
    public void StopAttack()
    {
        StopAllCoroutines();
        isAttacking = false;
        targetPosition = idlePosition;
        targetRotation = idleRotation;
    }
    
    // Method to set weapon visibility (alternative to SetActive)
    public void SetVisible(bool visible)
    {
        if (weaponSprite != null)
        {
            weaponSprite.enabled = visible;
        }
        
        // If weapon has child objects, toggle them too
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(visible);
        }
    }
}
