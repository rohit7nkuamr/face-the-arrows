using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    private static LevelGenerator instance;
    public static LevelGenerator Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<LevelGenerator>();
            }
            return instance;
        }
    }
    
    [Header("Level Segments")]
    [SerializeField] private GameObject[] levelSegmentPrefabs;
    [SerializeField] private GameObject startSegmentPrefab;
    [SerializeField] private float segmentLength = 30f;
    
    [Header("Generation Settings")]
    [SerializeField] private int maxSegmentsActive = 5;
    [SerializeField] private float spawnDistance = 100f;
    [SerializeField] private float despawnDistance = 30f;
    
    [Header("Environment")]
    [SerializeField] private GameObject[] environmentProps;
    [SerializeField] private float propSpawnChance = 0.3f;
    [SerializeField] private float minPropDistance = 5f;
    [SerializeField] private float maxPropDistance = 15f;
    
    private List<GameObject> activeSegments = new List<GameObject>();
    private Transform playerTransform;
    private float nextSpawnZ = 0f;
    private bool isGenerating = false;
    
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
    
    public void StartGenerating()
    {
        if (!isGenerating)
        {
            isGenerating = true;
            
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            
            // Clear any existing segments
            foreach (GameObject segment in activeSegments)
            {
                if (segment != null)
                    Destroy(segment);
            }
            activeSegments.Clear();
            
            // Spawn initial segments
            nextSpawnZ = 0f;
            SpawnInitialSegments();
            
            StartCoroutine(GenerateLevel());
        }
    }
    
    public void StopGenerating()
    {
        isGenerating = false;
        StopAllCoroutines();
    }
    
    void SpawnInitialSegments()
    {
        // Spawn start segment
        if (startSegmentPrefab != null)
        {
            GameObject startSegment = Instantiate(startSegmentPrefab, Vector3.zero, Quaternion.identity);
            activeSegments.Add(startSegment);
            nextSpawnZ += segmentLength;
        }
        
        // Spawn initial segments
        for (int i = 0; i < maxSegmentsActive - 1; i++)
        {
            SpawnSegment();
        }
    }
    
    IEnumerator GenerateLevel()
    {
        while (isGenerating)
        {
            if (playerTransform != null)
            {
                // Check if we need to spawn new segment
                float distanceToNextSpawn = nextSpawnZ - playerTransform.position.z;
                if (distanceToNextSpawn < spawnDistance)
                {
                    SpawnSegment();
                }
                
                // Remove segments that are too far behind
                RemoveOldSegments();
            }
            
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    void SpawnSegment()
    {
        if (levelSegmentPrefabs.Length == 0) return;
        
        // Choose random segment
        GameObject segmentPrefab = levelSegmentPrefabs[Random.Range(0, levelSegmentPrefabs.Length)];
        
        // Spawn segment
        Vector3 spawnPosition = new Vector3(0, 0, nextSpawnZ);
        GameObject newSegment = Instantiate(segmentPrefab, spawnPosition, Quaternion.identity);
        activeSegments.Add(newSegment);
        
        // Add environment props
        SpawnEnvironmentProps(newSegment);
        
        // Update next spawn position
        nextSpawnZ += segmentLength;
    }
    
    void SpawnEnvironmentProps(GameObject segment)
    {
        if (environmentProps.Length == 0) return;
        
        // Spawn props along the sides of the segment
        for (int side = -1; side <= 1; side += 2) // Left and right sides
        {
            if (Random.value < propSpawnChance)
            {
                GameObject propPrefab = environmentProps[Random.Range(0, environmentProps.Length)];
                
                float xOffset = Random.Range(minPropDistance, maxPropDistance) * side;
                float zOffset = Random.Range(-segmentLength / 2f, segmentLength / 2f);
                
                Vector3 propPosition = segment.transform.position + new Vector3(xOffset, 0, zOffset);
                GameObject prop = Instantiate(propPrefab, propPosition, Quaternion.Euler(0, Random.Range(0, 360), 0));
                prop.transform.SetParent(segment.transform);
            }
        }
    }
    
    void RemoveOldSegments()
    {
        if (playerTransform == null) return;
        
        List<GameObject> segmentsToRemove = new List<GameObject>();
        
        foreach (GameObject segment in activeSegments)
        {
            if (segment != null)
            {
                float distance = playerTransform.position.z - segment.transform.position.z;
                if (distance > despawnDistance)
                {
                    segmentsToRemove.Add(segment);
                }
            }
        }
        
        foreach (GameObject segment in segmentsToRemove)
        {
            activeSegments.Remove(segment);
            Destroy(segment);
        }
    }
    
    public void ClearLevel()
    {
        StopGenerating();
        
        foreach (GameObject segment in activeSegments)
        {
            if (segment != null)
                Destroy(segment);
        }
        activeSegments.Clear();
        
        nextSpawnZ = 0f;
    }
}
