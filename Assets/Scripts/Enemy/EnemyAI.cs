using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyAI : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float waitTime = 1f;
    [SerializeField] private float flipCooldown = 1f;
    [SerializeField] private float patrolBuffer = 0.1f;
    
    [Header("Chase Settings")]
    [SerializeField] private float chaseRange = 8f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float losePlayerRange = 12f;
    
    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckDistance = 1f;
    [SerializeField] private LayerMask groundLayerMask;
    
    [Header("Edge Detection")]
    [SerializeField] private Transform edgeCheck;
    [SerializeField] private float edgeCheckDistance = 0.5f;

    [Header("Attack Parameters")] 
    [SerializeField] private AttackScript attack;
    [SerializeField] private float attackCooldown;
    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    
 
    private EnemyState currentState;
    private Transform player;
    private Transform currentTarget;
    private bool facingRight = true;
    private float waitTimer;
    private Vector3 lastKnownPlayerPosition;
    private float flipTime;
    private float attackTargetingRange;
    private float attackCooldownTime;
    private Vector2 resultv2;
    
    
 
    public enum EnemyState
    {
        Patrolling,
        Waiting,
        Chasing,
        Returning,
        Attacking
    }
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        attackTargetingRange = attack.targetingRange;
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
   
        SetupCheckPoints();
        

        currentState = EnemyState.Patrolling;
        currentTarget = pointA;
    }
    
    private void SetupCheckPoints()
    {
   
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = groundCheckObj.transform;
        }
        
      
        if (edgeCheck == null)
        {
            GameObject edgeCheckObj = new GameObject("EdgeCheck");
            edgeCheckObj.transform.SetParent(transform);
            edgeCheckObj.transform.localPosition = new Vector3(0.5f, -0.5f, 0);
            edgeCheck = edgeCheckObj.transform;
        }
        
   
        if (pointA == null)
        {
            GameObject pointAObj = new GameObject("PatrolPointA");
            pointAObj.transform.position = transform.position + Vector3.left * 3f;
            pointA = pointAObj.transform;
        }
        
        if (pointB == null)
        {
            GameObject pointBObj = new GameObject("PatrolPointB");
            pointBObj.transform.position = transform.position + Vector3.right * 3f;
            pointB = pointBObj.transform;
        }
    }
    
    private void Update()
    {
        UpdateState();
        UpdateAnimations();
        
        UpdateEdgeCheckPosition();
    }
    
    private void FixedUpdate()
    {
        HandleMovement();
    }
    
    private void UpdateState()
    {
        float distanceToPlayer = GetHorizontalDistanceToPlayer();
        
        switch (currentState)
        {
            case EnemyState.Patrolling:
                HandlePatrolling();
                
                if (player != null && distanceToPlayer <= chaseRange && chaseRange > attackTargetingRange)
                {
                    SetState(EnemyState.Chasing);
                    lastKnownPlayerPosition = player.position;
                }
                
                if (player != null && distanceToPlayer < attackTargetingRange && chaseRange < attackTargetingRange)
                {
                    SetState(EnemyState.Attacking);
                }
                break;
                
            case EnemyState.Waiting:
                HandleWaiting();
                
                if (player != null && distanceToPlayer <= chaseRange)
                {
                    SetState(EnemyState.Chasing);
                    lastKnownPlayerPosition = player.position;
                }
                if (player != null && distanceToPlayer < attackTargetingRange && chaseRange < attackTargetingRange)
                {
                    SetState(EnemyState.Attacking);
                }
                break;
                
            case EnemyState.Chasing:
                HandleChasing();
                if (player != null && distanceToPlayer < attackTargetingRange)
                {
                    SetState(EnemyState.Attacking);
                }
                if (player == null || distanceToPlayer > losePlayerRange)
                {
                    SetState(EnemyState.Returning);
                }
                break;
                
            case EnemyState.Returning:
                HandleReturning();
                break;
            
            case EnemyState.Attacking:
                HandleAttacking();
                break;
        }
    }
    
    private void HandlePatrolling()
    {

        if (Vector2.Distance(transform.position, currentTarget.position) < 0.2f)
        {
            SetState(EnemyState.Waiting);
            waitTimer = waitTime;
        }
    }
    
    private void HandleWaiting()
    {
        waitTimer -= Time.deltaTime;
        
        if (waitTimer <= 0)
        {
            print("WAITING, SWITCHING");
                currentTarget = currentTarget == pointA ? pointB : pointA; 
                SetState(EnemyState.Patrolling);
        }
    }
    
    private void HandleChasing()
    {
        if (player != null)
        {
            lastKnownPlayerPosition = player.position;
            
            FaceTarget(player.position);
        }
    }
    
    private void HandleReturning()
    {
        float distanceToA = Vector2.Distance(transform.position, pointA.position);
        float distanceToB = Vector2.Distance(transform.position, pointB.position);
        //if (this.transform.position.x < pointA.position.x || this.transform.position.x > pointB.position.x)
        // {
        //     currentTarget = (distanceToA > distanceToB) ? pointA : pointB;
        // }
        // else
        // {
            currentTarget = (distanceToA < distanceToB) ? pointA : pointB;
        //}
        print("RETURNING");

        if (Vector2.Distance( transform.position,currentTarget.position) >= patrolBuffer)
        {
            SetState(EnemyState.Waiting);
        }
        
        if (Vector2.Distance(transform.position, currentTarget.position) >= patrolBuffer)
        {
            SetState(EnemyState.Waiting);
        }
    }
    
private void HandleAttacking()
{
    if (player == null) 
    {
        SetState(EnemyState.Chasing);
        return;
    }
    
    // Face the player
    float directionToPlayer = player.position.x - transform.position.x;
    FaceDirection(directionToPlayer);
    
    // Check if player is still in attack range
    float distanceToPlayer = GetHorizontalDistanceToPlayer();
    if (distanceToPlayer > attackTargetingRange)
    {
        SetState(EnemyState.Chasing);
        return;
    }
    
    //  attack cooldown and firing
    attackCooldownTime -= Time.deltaTime;
    if (attackCooldownTime <= 0f && attack.CanFire())
    {
   
        StartCoroutine(attack.Fire(directionToPlayer));
        attackCooldownTime = attackCooldown;
    }
}
    
    private void HandleMovement()
    {
        Vector2 targetPosition = Vector2.zero;
        float moveSpeed = 0f;
        
        switch (currentState)
        {
            case EnemyState.Patrolling:
            case EnemyState.Returning:
                targetPosition = currentTarget.position;
                moveSpeed = patrolSpeed;
                break;
                
            case EnemyState.Chasing:
                if (player != null)
                {
                    targetPosition = player.position;
                }
                else
                {
                    targetPosition = lastKnownPlayerPosition;
                }
                moveSpeed = chaseSpeed;
                break;
                
            case EnemyState.Waiting:
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
            
            case EnemyState.Attacking:
                rb.velocity = new Vector2(0, rb.velocity.y);
                SetState(EnemyState.Chasing);
                return;
        }
        
        float direction = Mathf.Sign(targetPosition.x - transform.position.x);
        
        if (CanMove(direction))
        {
            rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);
            FaceDirection(direction);
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            
            if (currentState == EnemyState.Patrolling)
            {
                SetState(EnemyState.Waiting);
                waitTimer = waitTime;
            }
        }
    }
    
    private bool CanMove(float direction)
    {
        bool patrolCheck = true;
        RaycastHit2D groundHit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayerMask);
        
        //patrol check
        if (currentState == EnemyState.Patrolling)
        {
            if (GetHorizontalDistanceToTarget() > patrolBuffer)
            {
                patrolCheck = true;
            }
            else
            {
                patrolCheck = false;
            }
        }
        
        Vector3 edgeCheckPos = edgeCheck.position;
        if (direction > 0) 
        {
            edgeCheckPos.x = transform.position.x + edgeCheckDistance;
        }
        else 
        {
            edgeCheckPos.x = transform.position.x - edgeCheckDistance;
        }
        
        RaycastHit2D edgeHit = Physics2D.Raycast(edgeCheckPos, Vector2.down, groundCheckDistance, groundLayerMask);
        
        return groundHit.collider != null && edgeHit.collider != null && patrolCheck;
    }
    
    private void FaceTarget(Vector3 targetPosition)
    {
        float direction = Mathf.Sign(targetPosition.x - transform.position.x);
        FaceDirection(direction);
    }
    
    private void FaceDirection(float direction)
    {
        flipTime = flipCooldown;
        flipTime -= Time.deltaTime;
        if (direction > 0 && !facingRight )//&& flipTime == 0
        {
            Flip();
            //flipTime = flipCooldown;
        }
        else if (direction < 0 && facingRight )//&& flipTime == 0
        {
            Flip();
            //flipTime = flipCooldown;
        }
    }
    
    
    private void Flip()
    {
        facingRight = !facingRight;
        spriteRenderer.flipX = !facingRight;
    }
    
    private void UpdateEdgeCheckPosition()
    {
        Vector3 edgePos = edgeCheck.localPosition;
        edgePos.x = facingRight ? 0.5f : -0.5f;
        edgeCheck.localPosition = edgePos;
    }
    
    private float GetHorizontalDistanceToPlayer()
    {
        if (player == null) return float.MaxValue;
        return Mathf.Abs(player.position.x - transform.position.x);
    }
    
    private float GetHorizontalDistanceToTarget()
    {
        if (currentTarget == null) {return float.MaxValue;}
        else
        {
            return Math.Abs(currentTarget.position.x - transform.position.x);
        }
        
    }
    
    private void SetState(EnemyState newState)
    {
        currentState = newState;
        
        
    }
    
    private void UpdateAnimations()
    {
        if (animator == null) return;
        
        animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
        animator.SetBool("IsChasing", currentState == EnemyState.Chasing);
        animator.SetInteger("State", (int)currentState);
    }
    
   
    public EnemyState GetCurrentState()
    {
        return currentState;
    }
    
    public void SetPatrolPoints(Transform newPointA, Transform newPointB)
    {
        pointA = newPointA;
        pointB = newPointB;
        currentTarget = pointA;
    }
    
    private void OnDrawGizmosSelected()
    {
        
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(pointA.position, 0.3f);
            Gizmos.DrawWireSphere(pointB.position, 0.3f);
            Gizmos.DrawLine(pointA.position, pointB.position);
        }
        
        // for chase range
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(chaseRange * 2, 1f, 0));
        
        //lose player range when u outrun
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(losePlayerRange * 2, 1f, 0));
        
        
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(groundCheck.position, Vector2.down * groundCheckDistance);
        }
        
       
        if (edgeCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(edgeCheck.position, Vector2.down * groundCheckDistance);
        }
    }
}
