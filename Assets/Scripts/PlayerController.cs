using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float jumpBufferTime = 0.2f;

    [Header("Wall Jump Settings")]
    [SerializeField] private Transform wallCheckLeft;
    [SerializeField] private Transform wallCheckRight;
    [SerializeField] private float wallCheckDistance = 0.5f;
    [SerializeField] private float wallJumpForce = 12f;
    [SerializeField] private float wallJumpHorizontalForce = 8f;
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private bool enableWallSliding = true;
    
    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayerMask;
    
    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private LayerMask enemyLayerMask;
    [SerializeField] private int attackDamage = 1;
    
    [Header("Combo Settings")]
    [SerializeField] private float comboWindow = 1.0f; // Time window to continue combo
    [SerializeField] private float[] comboDamageMultipliers = {1f, 1.2f, 1.5f}; // Damage for each combo step
    [SerializeField] private float[] comboRangeMultipliers = {1f, 1.1f, 1.3f}; // Range for each combo step
    
    [Header("Audio & Effects")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip slashSound;
    [SerializeField] private ParticleSystem slashTrail;
    [SerializeField] private Transform spotlight; 
    
    // Components
    private Rigidbody2D rb;
    public Animator animator;
    private SpriteRenderer spriteRenderer;
    private WeaponSystem weaponSystem;
    private StaminaSystem staminaSystem;
    public Transform GrimReaper; //actual one with animations
    
    // Input variables
    private Vector2 moveInput;
    private bool jumpInput;
    private bool sprintInput;
    private bool attackInput;
    
    // State variables
    private bool isGrounded;
    private bool wasGrounded;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool canAttack = true;
    private float lastAttackTime;
    private bool facingRight = true;
    private bool movementDisabled; //for wall jump

    private bool isTouchingWallLeft;
    private bool isTouchingWallRight;
    private bool isTouchingWall;
    private bool isWallSliding;
    public bool isflipped;
    
    // Combo system variables
    private ComboState currentCombo = ComboState.None;
    private int noOfClicks = 0;
    private float lastClickedTime = 0f;
    private bool isAttacking = false;
    
    // Combo states enum
    public enum ComboState
    {
        None = 0,
        Attack1 = 1,
        Attack2 = 2,
        Attack3 = 3
    }
    
    // Input System
    private PlayerInputActions inputActions;

    private void Awake()
    {
        // Initialize input actions
        inputActions = new PlayerInputActions();

        // Get components
        rb = GetComponent<Rigidbody2D>();
       // animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        weaponSystem = GetComponent<WeaponSystem>();
        staminaSystem = GetComponent<StaminaSystem>();

        // Setup audio source if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = groundCheckObj.transform;
        }

        if (attackPoint == null)
        {
            GameObject attackPointObj = new GameObject("AttackPoint");
            attackPointObj.transform.SetParent(transform);
            attackPointObj.transform.localPosition = new Vector3(1f, 0, 0);
            attackPoint = attackPointObj.transform;
        }
        
        if (wallCheckLeft == null)
        {
            GameObject wallCheckLeftObj = new GameObject("WallCheckLeft");
            wallCheckLeftObj.transform.SetParent(transform);
            wallCheckLeftObj.transform.localPosition = new Vector3(-0.5f, 0, 0);
            wallCheckLeft = wallCheckLeftObj.transform;
        }

        if (wallCheckRight == null)
        {
            GameObject wallCheckRightObj = new GameObject("WallCheckRight");
            wallCheckRightObj.transform.SetParent(transform);
            wallCheckRightObj.transform.localPosition = new Vector3(0.5f, 0, 0);
            wallCheckRight = wallCheckRightObj.transform;
        }
    }
    
    private void OnEnable()
    {
        inputActions.Enable();
        
        inputActions.Player.Move.performed += OnMovePerformed;
        inputActions.Player.Move.canceled += OnMoveCanceled;
        inputActions.Player.Jump.performed += OnJumpPerformed;
        inputActions.Player.Jump.canceled += OnJumpCanceled;
        inputActions.Player.Sprint.performed += OnSprintPerformed;
        inputActions.Player.Sprint.canceled += OnSprintCanceled;
        inputActions.Player.Attack.performed += OnAttackPerformed;
        inputActions.Player.Weapon1.performed += OnWeapon1Performed;
        inputActions.Player.Weapon2.performed += OnWeapon2Performed;
        inputActions.Player.Weapon3.performed += OnWeapon3Performed;
    }
    
    private void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMovePerformed;
        inputActions.Player.Move.canceled -= OnMoveCanceled;
        inputActions.Player.Jump.performed -= OnJumpPerformed;
        inputActions.Player.Jump.canceled -= OnJumpCanceled;
        inputActions.Player.Sprint.performed -= OnSprintPerformed;
        inputActions.Player.Sprint.canceled -= OnSprintCanceled;
        inputActions.Player.Attack.performed -= OnAttackPerformed;
        inputActions.Player.Weapon1.performed -= OnWeapon1Performed;
        inputActions.Player.Weapon2.performed -= OnWeapon2Performed;
        inputActions.Player.Weapon3.performed -= OnWeapon3Performed;
        
        inputActions.Disable();
    }

    private void Update()
    {
        CheckGrounded();
        HandleCoyoteTime();
        HandleJumpBuffer();
        HandleAttackCooldown();
        HandleCombo();
        UpdateAnimations();
        
        CheckWallTouch();
        HandleWallSliding();
    }
    
    private void FixedUpdate()
    {
        HandleMovement();
        HandleJump();
    }
    
    #region Input Callbacks
    
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }
    
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        jumpInput = true;
        jumpBufferCounter = jumpBufferTime;
    }
    
    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        jumpInput = false;
    }
    
    private void OnSprintPerformed(InputAction.CallbackContext context)
    {
        sprintInput = true;
    }
    
    private void OnSprintCanceled(InputAction.CallbackContext context)
    {
        sprintInput = false;
    }
    
    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        if (CanAttack())
        {
            Attack();
        }
    }
    
    #endregion
    
    #region Weapon Switching 

    private void OnWeapon1Performed(InputAction.CallbackContext context)
    {
        if (weaponSystem != null)
        {
            weaponSystem.SwitchToWeapon(0);
        }
    }

    private void OnWeapon2Performed(InputAction.CallbackContext context)
    {
        if (weaponSystem != null)
        {
            weaponSystem.SwitchToWeapon(1);
        }
    }

    private void OnWeapon3Performed(InputAction.CallbackContext context)
    {
        if (weaponSystem != null)
        {
            weaponSystem.SwitchToWeapon(2);
        }
    }

    #endregion
    
    #region Movement
    
    private void HandleMovement()
    {
        if (movementDisabled) return;

        bool canSprint = staminaSystem != null ? staminaSystem.CanSprint() : true;
        bool shouldSprint = sprintInput && canSprint && Mathf.Abs(moveInput.x) > 0;
        
        float currentSpeed = shouldSprint ? sprintSpeed : walkSpeed;
        float targetVelocity = moveInput.x * currentSpeed;
        
        if (isAttacking)
        {
            targetVelocity *= 0.3f; // Reduce movement during attack
        }
        
        if (staminaSystem != null)
        {
            if (shouldSprint)
            {
                staminaSystem.StartSprinting();
            }
            else
            {
                staminaSystem.StopSprinting();
            }
        }
        
        rb.velocity = new Vector2(targetVelocity, rb.velocity.y);
       //
        
        if (!isAttacking)
        {
            if (moveInput.x > 0 && !facingRight)
            {
                Flip();
            }
            else if (moveInput.x < 0 && facingRight)
            {
                Flip();
            }
        }

        if (moveInput.x > 0.1f && isGrounded)
        {
            animator.SetBool("IsWalking", true);
        }

        else if(moveInput.x <0.1f && isGrounded)
        {
            animator.SetBool("IsWalking", false);
        }
    }
    
    public void Flip()
    {
        
        facingRight = !facingRight;

    Vector3 scale = transform.localScale;
    scale.x *= -1;
    transform.localScale = scale;
        isflipped = true;

    if (GrimReaper != null)
    {
        Vector3 grimScale = GrimReaper.localScale;
        grimScale.x = Mathf.Abs(grimScale.x); // make sure it stays positive
        GrimReaper.localScale = grimScale;
    }

    if (spotlight != null)
    {
        Vector3 lightScale = spotlight.localScale;
        lightScale.x *= 1; // undo flip
        spotlight.localScale = lightScale;
    }

        // Flip weapons when player changes direction
        if (weaponSystem != null)
        {
           // weaponSystem.FlipWeapons(facingRight);
        }
    }
    
    #endregion
    
    #region Jumping
    
    private void CheckGrounded()
    {
        wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask);
        
        if (!wasGrounded && isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
    }
    
    private void HandleCoyoteTime()
    {
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }
    
    private void HandleJumpBuffer()
    {
        if (jumpBufferCounter > 0)
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }
    
    private void CheckWallTouch()
    {
        isTouchingWallLeft = Physics2D.Raycast(wallCheckLeft.position, Vector2.left, 5, groundLayerMask);
        isTouchingWallRight = Physics2D.Raycast(wallCheckRight.position, Vector2.right, 5, groundLayerMask);
        isTouchingWall = isTouchingWallLeft || isTouchingWallRight;
    }

    private void HandleWallSliding()
    {
        if (enableWallSliding && isTouchingWall && !isGrounded && rb.velocity.y < 0)
        {
            bool movingTowardWall = (isTouchingWallLeft && moveInput.x < 0) || (isTouchingWallRight && moveInput.x > 0);
            
            if (movingTowardWall || moveInput.x == 0)
            {
                isWallSliding = true;
                rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -wallSlideSpeed));
            }
            else
            {
                isWallSliding = false;
            }
        }
        else
        {
            isWallSliding = false;
        }
    }
    
    private void HandleJump()
    {
        if (jumpBufferCounter > 0)
        {
            if (isGrounded || coyoteTimeCounter > 0)
            {
                PerformRegularJump();
                animator.SetTrigger("Jump");
            }
            else if (isTouchingWall && !isGrounded)
            {
                PerformWallJump();
            }
        }

        if (!jumpInput && rb.velocity.y > 0 && !isWallSliding)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }
    }

    private void PerformRegularJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        jumpBufferCounter = 0;
        coyoteTimeCounter = 0;
        isWallSliding = false;
        
        Debug.Log("normal jump");
    }

    private void PerformWallJump()
    {
        float horizontalDirection = 0f;

        if (isTouchingWallLeft)
        {
            horizontalDirection = 1f;
        }
        else if (isTouchingWallRight)
        {
            horizontalDirection = -1f;
        }

        rb.velocity = new Vector2(horizontalDirection * wallJumpHorizontalForce, wallJumpForce);

        jumpBufferCounter = 0;
        coyoteTimeCounter = 0;
        isWallSliding = false;

        StartCoroutine(DisableMovementBriefly(0.1f));

        Debug.Log("wall jumping yay");
    }

    private IEnumerator DisableMovementBriefly(float duration)
    {
        movementDisabled = true;
        yield return new WaitForSeconds(duration);
        movementDisabled = false;
        
    }

    #endregion

    #region Combat & Combo System

    private void HandleAttackCooldown()
    {
        if (!canAttack && Time.time >= lastAttackTime + attackCooldown)
        {
            if (staminaSystem == null || staminaSystem.CanAttack())
            {
                canAttack = true;
            }
        }
    }
    
    private bool CanAttack()
    {
        if (!canAttack) return false;
        
        if (staminaSystem != null && !staminaSystem.CanAttack())
        {
            return false;
        }
        
        return true;
    }
    
    public void Attack()
    {
       
        if (staminaSystem != null && !staminaSystem.CanAttack())
        {
            // Debug.Log("Not enough stamina to attack");
            return;
        }
        
        if (staminaSystem != null && !staminaSystem.TryUseAttackStamina())
        {
            return;
        }
        
        lastClickedTime = Time.time;
        noOfClicks = Mathf.Clamp(noOfClicks + 1, 0, 3);
        
        // Trigger weapon-specific attack animation
        if (weaponSystem != null)
        {
            weaponSystem.TriggerWeaponAttack();
        }
        
        // Play general attack sounds
        if (audioSource != null && slashSound != null)
        {
            audioSource.PlayOneShot(slashSound);
        }
        
        // Instantiate slash particle system at attack point
        if (slashTrail != null && attackPoint != null)
        {
            // Instantiate the particle system at the attack point
            GameObject slashInstance = Instantiate(slashTrail.gameObject, attackPoint.position, attackPoint.rotation);
            
            // Get the particle system component and play it
            ParticleSystem instantiatedParticles = slashInstance.GetComponent<ParticleSystem>();
            if (instantiatedParticles != null)
            {
                instantiatedParticles.Play();
                
                // Destroy the instantiated particle system after it finishes playing
                float particleDuration = instantiatedParticles.main.duration + instantiatedParticles.main.startLifetime.constantMax;
                Destroy(slashInstance, particleDuration);
            }
        }
        
        switch (noOfClicks)
        {
            case 1:
                currentCombo = ComboState.Attack1;
                break;
            case 2:
                currentCombo = ComboState.Attack2;
                break;
            case 3:
                currentCombo = ComboState.Attack3;
                break;
        }
        
        if (animator != null)
        {
            animator.SetInteger("ComboStep", (int)currentCombo);
            animator.SetTrigger("Attack");
        }
        
        isAttacking = true;
        
        // For melee weapons, use traditional collision detection
        // For projectile weapons, the weapon handles its own damage
        if (IsCurrentWeaponMelee())
        {
            StartCoroutine(ColliderSwitch());
        }
        else
        {
            //  projectile weapons, just handle cooldown
            StartCoroutine(HandleProjectileAttackCooldown());
        }
        
       
    }
    
    private bool IsCurrentWeaponMelee()
    {
        if (weaponSystem == null) return true;
        
        GameObject currentWeaponObj = weaponSystem.GetCurrentWeaponGameObject();
        if (currentWeaponObj == null) return true;
        
        WeaponBehavior weaponBehavior = currentWeaponObj.GetComponent<WeaponBehavior>();
        if (weaponBehavior == null) return true;
        
        // Sword and Scythe use traditional melee collision, Axe handles its own projectile damage
        WeaponBehavior.WeaponType weaponType = weaponBehavior.GetWeaponType();
        return weaponType == WeaponBehavior.WeaponType.Sword || weaponType == WeaponBehavior.WeaponType.Scythe;
    }
    
    private IEnumerator HandleProjectileAttackCooldown()
    {
        yield return new WaitForSeconds(0.1f);
        canAttack = false;
        lastAttackTime = Time.time;
    }
    
    private void HandleCombo()
    {
        if (animator == null) return;
        
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float currentAnimLength = stateInfo.length;
        
        if (Time.time - lastClickedTime > comboWindow)
        {
            if (!stateInfo.IsName("Attack3"))
            {
                ResetCombo();
            }
        }
        
        if (stateInfo.normalizedTime > 0.3f && !animator.IsInTransition(0))
        {
            if (stateInfo.IsName("Attack1") && noOfClicks >= 2)
            {
                animator.SetInteger("ComboStep", 2);
                animator.SetTrigger("Attack");
            }
            else if (stateInfo.IsName("Attack2") && noOfClicks >= 3)
            {
                animator.SetInteger("ComboStep", 3);
                animator.SetTrigger("Attack");
            }
            else if (stateInfo.IsName("Attack3"))
            {
                ResetCombo();
            }
            else if (noOfClicks == 1)
            {
                animator.SetInteger("ComboStep", 1);
                animator.SetTrigger("Attack");
            }
        }
        
        if (isAttacking && stateInfo.normalizedTime >= 0.95f && !animator.IsInTransition(0))
        {
            if (stateInfo.IsName("Attack1") || stateInfo.IsName("Attack2") || stateInfo.IsName("Attack3"))
            {
                if (noOfClicks == 0 || (int)currentCombo == noOfClicks)
                {
                    isAttacking = false;
                }
            }
        }
    }
    
    private void ResetCombo()
    {
        noOfClicks = 0;
        currentCombo = ComboState.None;
        isAttacking = false;
        
        if (animator != null)
        {
            animator.SetInteger("ComboStep", 0);
        }
    }
    
    private IEnumerator ColliderSwitch()
    {
        yield return new WaitForSeconds(0.1f);
        
        PerformAttackDamage();
        
        canAttack = false;
        lastAttackTime = Time.time;
    }
    
    private void PerformAttackDamage()
    {
        Weapon currentWeapon = weaponSystem?.GetCurrentWeapon();
        int weaponDamage = currentWeapon?.damage ?? attackDamage;
        float weaponRange = currentWeapon?.range ?? attackRange;
        
        int comboIndex = Mathf.Clamp((int)currentCombo - 1, 0, comboDamageMultipliers.Length - 1);
        float damageMultiplier = comboDamageMultipliers[comboIndex];
        float rangeMultiplier = comboRangeMultipliers[comboIndex];
        
        int finalDamage = Mathf.RoundToInt(weaponDamage * damageMultiplier);
        float finalRange = weaponRange * rangeMultiplier;
        
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, finalRange, enemyLayerMask);
        
        foreach (Collider2D enemy in hitEnemies)
        {
            Vector2 attackDirection = (enemy.transform.position - transform.position).normalized;
            
            var enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                bool damageDealt = enemyHealth.TakeDamage(finalDamage, attackDirection);
                if (damageDealt)
                {
                 


                }
            }
        }
    }
    
    #endregion
    
    #region Animation
    
    private void UpdateAnimations()
    {
        if (animator == null) return;
        
        bool actuallySprintingBasedOnStamina = sprintInput && Mathf.Abs(moveInput.x) > 0 && (staminaSystem == null || staminaSystem.CanSprint());
        
        // Animation parameters can be set here
        // animator.SetFloat("Speed", Mathf.Abs(moveInput.x));
        // animator.SetBool("IsGrounded", isGrounded);
        // animator.SetFloat("VelocityY", rb.velocity.y);
        // animator.SetBool("IsSprinting", actuallySprintingBasedOnStamina);
        // animator.SetBool("IsAttacking", isAttacking);
    }
    
    #endregion
    
    #region Gizmos
    
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        
        if (attackPoint != null)
        {
            float displayRange = attackRange;
            
            if (isAttacking && currentCombo != ComboState.None)
            {
                int comboIndex = Mathf.Clamp((int)currentCombo - 1, 0, comboRangeMultipliers.Length - 1);
                displayRange *= comboRangeMultipliers[comboIndex];
            }
            
            Gizmos.color = isAttacking ? Color.yellow : Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, displayRange);
        }
    }
    
    #endregion
}