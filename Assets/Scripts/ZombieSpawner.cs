using UnityEngine;
using System.Collections;

public class ZombieSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject zombiePrefab;
    public int maxZombies = 20;
    public float startSpawnRate = 5f;
    public float minSpawnRate = 1f;
    public float spawnRateDecrease = 0.1f;
    
    [Header("Spawn Area")]
    public Vector3 spawnAreaCenter = Vector3.zero;
    public float spawnAreaRadius = 50f;
    public float minDistanceFromPlayer = 15f;
    public float spawnHeight = 1f;
    
    [Header("References")]
    public Transform player;
    
    // Private variables
    private float currentSpawnRate;
    private int zombiesSpawned = 0;
    private int currentZombieCount = 0;
    
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
            // Random position in circle
            Vector2 randomCircle = Random.insideUnitCircle * spawnAreaRadius;
            Vector3 randomPosition = spawnAreaCenter + new Vector3(randomCircle.x, 0, randomCircle.y);
            
            // Check distance from player
            if (player != null)
            {
                float distanceToPlayer = Vector3.Distance(randomPosition, player.position);
                
                if (distanceToPlayer < minDistanceFromPlayer)
                {
                    continue;
                }
            }
            
            // Raycast to find ground
            RaycastHit hit;
            if (Physics.Raycast(randomPosition + Vector3.up * 100f, Vector3.down, out hit, 200f))
            {
                return hit.point + Vector3.up * spawnHeight;
            }
        }
        
        // Fallback position
        return spawnAreaCenter + new Vector3(Random.Range(-20f, 20f), spawnHeight, Random.Range(-20f, 20f));
    }
    
    void IncreaseDifficulty()
    {
        // Every 5 zombies, decrease spawn rate
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
}