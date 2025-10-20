using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    
    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float runDistance = 30f; // Player bu mesafede ise koş
    public float attackRange = 2f;
    public float rotationSpeed = 5f;
    
    [Header("Attack")]
    public int attackDamage = 10;
    public float attackCooldown = 1.5f;
    
    // Private variables
    private Transform player;
    private float lastAttackTime;
    private Animator animator;
    private AudioSource audioSource;
    
    void Start()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(ScreamRoutine());
        animator = GetComponent<Animator>();
        
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }
    
    void Update()
    {
        if (player == null)
            return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Always chase player
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        
        bool isAttacking = distanceToPlayer <= attackRange;
        bool shouldRun = distanceToPlayer <= runDistance && distanceToPlayer > attackRange;
        
        // Determine current speed
        float currentMoveSpeed = shouldRun ? runSpeed : walkSpeed;
        
        // Move towards player (only if not attacking)
        if (!isAttacking)
        {
            transform.position += direction * currentMoveSpeed * Time.deltaTime;
        }
        
        // Rotate towards player
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        // Update animator with movement speed
        if (animator != null)
        {
            // Set Speed parameter based on movement
            float animSpeed = isAttacking ? 0f : currentMoveSpeed;
            animator.SetFloat("Speed", animSpeed);
            
            // Optional: Set IsRunning parameter for better control
            animator.SetBool("IsRunning", shouldRun);
            animator.SetBool("IsChasing", !isAttacking);
        }

        // Attack if in range
        if (isAttacking && Time.time >= lastAttackTime + attackCooldown)
        {
            AttackPlayer();
        }
    }
    
    void AttackPlayer()
{
    lastAttackTime = Time.time;

        Debug.Log("ATTACK TRIGGERED");

    AudioManager audioManager = AudioManager.GetInstance();
    
    if (animator != null)
    {
        Debug.Log("Animator found, setting Attack trigger");
        animator.SetTrigger("Attack");
        audioManager.PlayZombieAttack();
    }
    else
    {
        Debug.Log("Animator is NULL!");
    }
    
    PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
    if (playerHealth != null)
    {
        playerHealth.TakeDamage(attackDamage);
    }
}
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        AudioManager audioManager = AudioManager.GetInstance();
        audioManager.PlayZombieHit();
        
        Debug.Log(gameObject.name + " took " + damage + " damage. Health: " + currentHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " died!");
        Destroy(gameObject);
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