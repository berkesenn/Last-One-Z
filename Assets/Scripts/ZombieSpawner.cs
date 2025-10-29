using UnityEngine;
using System.Collections;

public class ZombieSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject zombiePrefab;
    public int maxZombies = 20;
    public float startSpawnRate = 5f;
    public float minSpawnRate = 1f; // En hızlı: 1 saniyede 1 zombi
    public float spawnRateDecrease = 1f; // Her aşamada 1 saniye azalır (5→4→3→2→1)
    
    [Header("Spawn Area")]
    public Vector3 spawnAreaCenter = Vector3.zero;
    public float spawnAreaRadius = 50f;
    public float minDistanceFromPlayer = 25f; // Minimum 25 metre uzakta spawn olsun
    public float maxDistanceFromPlayer = 60f; // Maximum 60 metre uzakta (çok uzak olmasın)
    public float spawnHeight = 1f;
    
    [Header("References")]
    public Transform player;
    
    // Private variables
    private float currentSpawnRate;
    private int zombiesSpawned = 0;
    private int currentZombieCount = 0;
    private bool hasStoppedSpawning = false;
    
    void Start()
    {
        currentSpawnRate = startSpawnRate;
        
        // If player not assigned, find it
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
        
        StartCoroutine(SpawnZombies());
    }
    
    void Update()
    {
        if (!hasStoppedSpawning)
        {
            StopSpawningIfDead();
        }
    }
    
    IEnumerator SpawnZombies()
    {
        while (true)
        {
            yield return new WaitForSeconds(currentSpawnRate);
            
            if (currentZombieCount < maxZombies)
            {
                SpawnZombie();
            }
        }
    }
    
    void SpawnZombie()
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();
        
        if (spawnPosition != Vector3.zero)
        {
            GameObject zombie = Instantiate(zombiePrefab, spawnPosition, Quaternion.identity);
            currentZombieCount++;
            zombiesSpawned++;
            
            // Subscribe to zombie death to decrease count
            Enemy enemyScript = zombie.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                StartCoroutine(WaitForZombieDeath(zombie));
            }
            
            // Increase difficulty over time
            IncreaseDifficulty();
        }
    }
    
    IEnumerator WaitForZombieDeath(GameObject zombie)
    {
        while (zombie != null)
        {
            yield return new WaitForSeconds(0.5f);
        }
        
        currentZombieCount--;
    }
    
    Vector3 GetRandomSpawnPosition()
    {
        int maxAttempts = 30;
        
        for (int i = 0; i < maxAttempts; i++)
        {
            // Random açı (360 derece) ve mesafe
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float randomDistance = Random.Range(minDistanceFromPlayer, maxDistanceFromPlayer);
            
            // Player'ın etrafında dairesel spawn pozisyonu
            Vector3 randomPosition = Vector3.zero;
            if (player != null)
            {
                randomPosition = player.position + new Vector3(
                    Mathf.Cos(randomAngle) * randomDistance,
                    0,
                    Mathf.Sin(randomAngle) * randomDistance
                );
            }
            else
            {
                // Player yoksa merkeze göre spawn
                Vector2 randomCircle = Random.insideUnitCircle * spawnAreaRadius;
                randomPosition = spawnAreaCenter + new Vector3(randomCircle.x, 0, randomCircle.y);
            }
            
            // Raycast to find ground
            RaycastHit hit;
            if (Physics.Raycast(randomPosition + Vector3.up * 100f, Vector3.down, out hit, 200f))
            {
                Vector3 spawnPos = hit.point + Vector3.up * spawnHeight;
                
                // Debug: Spawn pozisyonunu göster
                Debug.Log($"Zombie spawn: {spawnPos}, Player: {player.position}, Açı: {randomAngle * Mathf.Rad2Deg}°, Mesafe: {randomDistance}m");
                
                return spawnPos;
            }
        }
        
        // Fallback position
        return spawnAreaCenter + new Vector3(Random.Range(-20f, 20f), spawnHeight, Random.Range(-20f, 20f));
    }
    
    void IncreaseDifficulty()
    {
        // Her 5 zombide bir aşama ilerle (5→4→3→2→1 saniye)
        if (zombiesSpawned % 5 == 0)
        {
            currentSpawnRate = Mathf.Max(minSpawnRate, currentSpawnRate - spawnRateDecrease);
            Debug.Log("Spawn rate increased! New rate: " + currentSpawnRate + "s");
        }
    }

    // Visualize spawn area in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(spawnAreaCenter, spawnAreaRadius);

        Gizmos.color = Color.yellow;
        if (player != null)
        {
            Gizmos.DrawWireSphere(player.position, minDistanceFromPlayer);
        }
    }
    
    void StopSpawningIfDead()
    {
        if (player == null)
            return;
        
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null && playerHealth.IsDead)
        {
            StopAllCoroutines();
            hasStoppedSpawning = true;
            Debug.Log("Player is dead. Stopping zombie spawning.");
        }
    }
}