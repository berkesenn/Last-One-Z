using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float chaseRange = 20f;
    public float attackRange = 2f;
    public float rotationSpeed = 5f;
    
    [Header("Attack")]
    public int attackDamage = 10;
    public float attackCooldown = 1.5f;
    
    // Private variables
    private Transform player;
    private float lastAttackTime;
    
    void Start()
    {
        currentHealth = maxHealth;
        
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        // CharacterController is NOT needed, we keep Capsule Collider for bullets
        // Just use simple transform movement
    }
    
    void Update()
    {
        if (player == null)
            return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Chase player if in range
        if (distanceToPlayer <= chaseRange)
        {
            // Move towards player (simple transform movement)
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0; // Keep on same Y level
            
            // Move using transform
            transform.position += direction * moveSpeed * Time.deltaTime;
            
            // Rotate towards player
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            
            // Attack if in range
            if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                AttackPlayer();
            }
        }
    }
    
    void AttackPlayer()
    {
        lastAttackTime = Time.time;
        
        // Deal damage to player
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
            Debug.Log("Zombie attacked player for " + attackDamage + " damage!");
        }
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
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
}