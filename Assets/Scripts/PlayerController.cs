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
    
   
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    
    // player variables
    private Vector2 moveInput;
    private bool jumpInput;
    private bool sprintInput;
    private bool attackInput;
    
   
    private bool isGrounded;
    private bool wasGrounded;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool canAttack = true;
    private float lastAttackTime;
    private bool facingRight = true;
    
   
    private PlayerInputActions inputActions;
    
    private void Awake()
    {
    
        inputActions = new PlayerInputActions();
        
   
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        

        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = groundCheckObj.transform;
        }
        
        // Create attack point if it doesn't exist
        if (attackPoint == null)
        {
            GameObject attackPointObj = new GameObject("AttackPoint");
            attackPointObj.transform.SetParent(transform);
            attackPointObj.transform.localPosition = new Vector3(1f, 0, 0);
            attackPoint = attackPointObj.transform;
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
        
        inputActions.Disable();
    }
    
    private void Update()
    {
        CheckGrounded();
        HandleCoyoteTime();
        HandleJumpBuffer();
        HandleAttackCooldown();
        UpdateAnimations();
    }
    
    private void FixedUpdate()
    {
        HandleMovement();
        HandleJump();
    }
    
    #region Input stuff
    
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
        if (canAttack)
        {
            PerformAttack();
        }
    }
    
    #endregion
    
    #region Movement stuff
    
    private void HandleMovement()
    {
        float currentSpeed = sprintInput ? sprintSpeed : walkSpeed;
        float targetVelocity = moveInput.x * currentSpeed;
        
     
        rb.velocity = new Vector2(targetVelocity, rb.velocity.y);
        
    
        if (moveInput.x > 0 && !facingRight)
        {
            Flip();
        }
        else if (moveInput.x < 0 && facingRight)
        {
            Flip();
        }
    }
    
    private void Flip()
    {
        facingRight = !facingRight;
        spriteRenderer.flipX = !facingRight;
        

        Vector3 attackPos = attackPoint.localPosition;
        attackPos.x *= -1;
        attackPoint.localPosition = attackPos;
    }
    
    #endregion
    
    #region Jumping stuff
    
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
    
    private void HandleJump()
    {
        // so u dont jump a lot
        if (jumpBufferCounter > 0 && (isGrounded || coyoteTimeCounter > 0))
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;
        }
        
        
        if (!jumpInput && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }
    }
    
    #endregion
    
    #region Combat
    
    private void HandleAttackCooldown()
    {
        if (!canAttack && Time.time >= lastAttackTime + attackCooldown)
        {
            canAttack = true;
        }
    }
    
    private void PerformAttack()
    {
        canAttack = false;
        lastAttackTime = Time.time;
        
        //  attack animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayerMask);
        
        foreach (Collider2D enemy in hitEnemies)
        {
            //  attack direction for knockback
            Vector2 attackDirection = (enemy.transform.position - transform.position).normalized;
            
            //  damage to enemies
            var enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                bool damageDealt = enemyHealth.TakeDamage(attackDamage, attackDirection);
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

    }
    
    #endregion
    
    #region Gizmos 4 debug visually
    
    private void OnDrawGizmosSelected()
    {
       
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        
      
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
    
    #endregion
}