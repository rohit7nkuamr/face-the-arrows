using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    private static ObstacleSpawner instance;
    public static ObstacleSpawner Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ObstacleSpawner>();
            }
            return instance;
        }
    }
    
    [Header("Obstacle Prefabs")]
    [SerializeField] private GameObject[] groundObstacles; // Logs, rocks, pits
    [SerializeField] private GameObject[] jumpObstacles;   // Low obstacles to jump over
    [SerializeField] private GameObject[] slideObstacles;  // High obstacles to slide under
    
    [Header("Spawn Settings")]
    [SerializeField] private float minSpawnDistance = 10f;
    [SerializeField] private float maxSpawnDistance = 20f;
    [SerializeField] private float spawnAheadDistance = 50f;
    [SerializeField] private float despawnBehindDistance = 20f;
    
    [Header("Lane Settings")]
    [SerializeField] private float laneDistance = 3f;
    
    [Header("Difficulty")]
    [SerializeField] private float initialObstacleChance = 0.3f;
    [SerializeField] private float maxObstacleChance = 0.7f;
    [SerializeField] private float difficultyIncreaseRate = 0.01f;
    
    private List<GameObject> activeObstacles = new List<GameObject>();
    private Transform playerTransform;
    private float nextSpawnZ = 10f;
    private float currentObstacleChance;
    private bool isSpawning = false;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        currentObstacleChance = initialObstacleChance;
    }
    
    public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            
            currentObstacleChance = initialObstacleChance;
            nextSpawnZ = 10f;
            
            StartCoroutine(SpawnObstacles());
            StartCoroutine(CleanupObstacles());
            StartCoroutine(IncreaseDifficulty());
        }
    }
    
    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }
    
    IEnumerator SpawnObstacles()
    {
        while (isSpawning)
        {
            if (playerTransform != null && GameManager.Instance.IsGameActive())
            {
                float playerZ = playerTransform.position.z;
                
                // Spawn obstacles ahead of player
                while (nextSpawnZ < playerZ + spawnAheadDistance)
                {
                    if (Random.value < currentObstacleChance)
                    {
                        SpawnObstacle();
                    }
                    
                    nextSpawnZ += Random.Range(minSpawnDistance, maxSpawnDistance);
                }
            }
            
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    void SpawnObstacle()
    {
        // Choose obstacle type
        ObstacleType type = (ObstacleType)Random.Range(0, 3);
        GameObject[] obstacleArray = GetObstacleArray(type);
        
        if (obstacleArray.Length == 0) return;
        
        // Choose random obstacle from array
        GameObject obstaclePrefab = obstacleArray[Random.Range(0, obstacleArray.Length)];
        
        // Choose random lane
        int lane = Random.Range(0, 3);
        float xPosition = (lane - 1) * laneDistance;
        
        // Spawn obstacle
        Vector3 spawnPosition = new Vector3(xPosition, 0, nextSpawnZ);
        GameObject obstacle = Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity);
        
        // Add obstacle component if not present
        Obstacle obstacleScript = obstacle.GetComponent<Obstacle>();
        if (obstacleScript == null)
        {
            obstacleScript = obstacle.AddComponent<Obstacle>();
            obstacleScript.obstacleType = type;
        }
        
        activeObstacles.Add(obstacle);
    }
    
    GameObject[] GetObstacleArray(ObstacleType type)
    {
        switch (type)
        {
            case ObstacleType.Ground:
                return groundObstacles;
            case ObstacleType.Jump:
                return jumpObstacles;
            case ObstacleType.Slide:
                return slideObstacles;
            default:
                return groundObstacles;
        }
    }
    
    IEnumerator CleanupObstacles()
    {
        while (isSpawning)
        {
            if (playerTransform != null)
            {
                List<GameObject> toRemove = new List<GameObject>();
                
                foreach (GameObject obstacle in activeObstacles)
                {
                    if (obstacle != null)
                    {
                        float distance = playerTransform.position.z - obstacle.transform.position.z;
                        if (distance > despawnBehindDistance)
                        {
                            toRemove.Add(obstacle);
                        }
                    }
                    else
                    {
                        toRemove.Add(obstacle);
                    }
                }
                
                foreach (GameObject obstacle in toRemove)
                {
                    activeObstacles.Remove(obstacle);
                    if (obstacle != null)
                        Destroy(obstacle);
                }
            }
            
            yield return new WaitForSeconds(1f);
        }
    }
    
    IEnumerator IncreaseDifficulty()
    {
        while (isSpawning)
        {
            yield return new WaitForSeconds(5f);
            
            currentObstacleChance = Mathf.Min(maxObstacleChance, currentObstacleChance + difficultyIncreaseRate);
        }
    }
    
    public void ClearObstacles()
    {
        foreach (GameObject obstacle in activeObstacles)
        {
            if (obstacle != null)
                Destroy(obstacle);
        }
        activeObstacles.Clear();
    }
}

public enum ObstacleType
{
    Ground, // Hit to stumble
    Jump,   // Must jump over
    Slide   // Must slide under
}
