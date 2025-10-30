using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    public float headshotMultiplier = 2f; // 25 * 2 = 50 damage
    
    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float runDistance = 30f;
    public float attackRange = 2f;
    public float rotationSpeed = 5f;
    
    [Header("Attack")]
    public int attackDamage = 10;
    public float attackCooldown = 1.5f;
    
    [Header("Hit Reaction")]
    public float hitStunDuration = 2f;
    
    [Header("Movement Boundaries")]
    public float boundaryMargin = 10f;
    
    // Private variables
    private Transform player;
    private float lastAttackTime;
    private Animator animator;
    private AudioSource audioSource;
    private bool isHit = false;
    private NavMeshAgent agent;
    
    void Start()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(ScreamRoutine());
        animator = GetComponent<Animator>();
        
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
        }
        
        agent.speed = walkSpeed;
        agent.acceleration = 8f;
        agent.angularSpeed = rotationSpeed * 60f;
        agent.stoppingDistance = attackRange;
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }
    
    void Update()
    {
        if (player == null || agent == null)
            return;
        
        if (isHit)
        {
            agent.isStopped = true;
            return;
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        bool isAttacking = distanceToPlayer <= attackRange;
        bool shouldRun = distanceToPlayer <= runDistance && distanceToPlayer > attackRange;
        
        float currentMoveSpeed = shouldRun ? runSpeed : walkSpeed;
        agent.speed = currentMoveSpeed;
        
        if (!isAttacking)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else
        {
            agent.isStopped = true;
            
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        
        if (animator != null)
        {
            float animSpeed = agent.velocity.magnitude;
            animator.SetFloat("Speed", animSpeed);
            animator.SetBool("IsRunning", shouldRun);
            animator.SetBool("IsChasing", !isAttacking);
        }

        if (isAttacking && Time.time >= lastAttackTime + attackCooldown)
        {
            AttackPlayer();
        }
    }
    
    void AttackPlayer()
    {
        lastAttackTime = Time.time;

        AudioManager audioManager = AudioManager.GetInstance();
        
        if (animator != null)
        {
            animator.SetTrigger("Attack");
            audioManager.PlayZombieAttack();
        }
        
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
        }
    }
    
    public void TakeDamage(int damage, bool isHeadshot = false)
    {
        int finalDamage = damage;
        
        if (isHeadshot)
        {
            finalDamage = Mathf.RoundToInt(damage * headshotMultiplier);
        }
        
        currentHealth -= finalDamage;
        AudioManager audioManager = AudioManager.GetInstance();
        audioManager.PlayZombieHit();
        
        if (animator != null)
        {
            animator.SetTrigger("GetHit");
        }
        
        StartCoroutine(HitStun());
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
    
    IEnumerator HitStun()
    {
        isHit = true;
        
        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.ResetPath();
        }
        
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }
        
        float animationLength = hitStunDuration;
        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
            if (stateInfo.IsName("Zombie Reaction Hit"))
            {
                animationLength = stateInfo.length;
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
                stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.IsName("Zombie Reaction Hit"))
                {
                    animationLength = stateInfo.length;
                }
            }
        }
        
        yield return new WaitForSeconds(animationLength);
        
        isHit = false;
        
        if (agent != null)
        {
            agent.isStopped = false;
        }
    }
    
    private IEnumerator ScreamRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(3f, 8f);
            yield return new WaitForSeconds(waitTime);

            if (audioSource != null)
            {
                audioSource.Play();
            }
        }
    }
}