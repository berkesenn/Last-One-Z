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
    
    [Header("Hit Reaction")]
    public float hitStunDuration = 2f; // Vurulunca ne kadar süre durmalı
    
    [Header("Movement Boundaries")]
    public float boundaryMargin = 10f; // Terrain kenarından ne kadar uzakta dur
    
    // Private variables
    private Transform player;
    private float lastAttackTime;
    private Animator animator;
    private AudioSource audioSource;
    private bool isHit = false; // Vurulma durumu
    private Terrain terrain;
    private Vector3 terrainSize;
    private Vector3 terrainPosition;
    
    void Start()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(ScreamRoutine());
        animator = GetComponent<Animator>();
        
        // Terrain bilgilerini al
        terrain = Terrain.activeTerrain;
        if (terrain != null)
        {
            terrainSize = terrain.terrainData.size;
            terrainPosition = terrain.transform.position;
        }
        
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
        
        // Eğer vurulma animasyonu oynuyorsa hareket etme
        if (isHit)
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
            Vector3 movement = direction * currentMoveSpeed * Time.deltaTime;
            Vector3 nextPosition = transform.position + movement;
            
            // Sınır kontrolü
            if (terrain != null)
            {
                nextPosition = ClampPositionToBoundaries(nextPosition);
            }
            
            transform.position = nextPosition;
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
        
        // Vurulma animasyonunu tetikle
        if (animator != null)
        {
            animator.SetTrigger("GetHit");
        }
        
        // Vurulma durumunu başlat (hareket durdurmak için)
        StartCoroutine(HitStun());
        
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
    
    IEnumerator HitStun()
    {
        // Vurulma durumunu aktif et (hareket durur)
        isHit = true;
        
        // Animatördeki Speed'i sıfırla (koşma/yürüme animasyonu dursun)
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }
        
        // Animasyon uzunluğunu dinamik olarak al
        float animationLength = hitStunDuration;
        if (animator != null)
        {
            // "Zombie Reaction Hit" state'inin uzunluğunu al
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
            // Eğer şu anda GetHit state'indeyse, animasyon uzunluğunu kullan
            if (stateInfo.IsName("Zombie Reaction Hit"))
            {
                animationLength = stateInfo.length;
            }
            // Değilse biraz bekle ve tekrar kontrol et (transition süresi için)
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
        
        // Animasyon bitene kadar bekle
        yield return new WaitForSeconds(animationLength);
        
        // Vurulma durumunu kapat (tekrar hareket edebilir)
        isHit = false;
    }
    
    Vector3 ClampPositionToBoundaries(Vector3 position)
    {
        // Terrain sınırlarını hesapla (margin dahil)
        float minX = terrainPosition.x + boundaryMargin;
        float maxX = terrainPosition.x + terrainSize.x - boundaryMargin;
        float minZ = terrainPosition.z + boundaryMargin;
        float maxZ = terrainPosition.z + terrainSize.z - boundaryMargin;
        
        // Pozisyonu sınırla
        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.z = Mathf.Clamp(position.z, minZ, maxZ);
        
        return position;
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