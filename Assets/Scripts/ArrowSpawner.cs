using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowSpawner : MonoBehaviour
{
    private static ArrowSpawner instance;
    public static ArrowSpawner Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ArrowSpawner>();
            }
            return instance;
        }
    }
    
    [Header("Arrow Settings")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private GameObject warningIndicatorPrefab;
    [SerializeField] private float arrowSpeed = 20f;
    [SerializeField] private float arrowSpawnDistance = 50f;
    [SerializeField] private float arrowHeight = 5f;
    
    [Header("Spawn Settings")]
    [SerializeField] private float minSpawnInterval = 2f;
    [SerializeField] private float maxSpawnInterval = 5f;
    [SerializeField] private float difficultyIncreaseRate = 0.95f;
    [SerializeField] private float minIntervalLimit = 0.5f;
    
    [Header("Warning System")]
    [SerializeField] private float warningDuration = 0.5f;
    [SerializeField] private Color warningColor = Color.red;
    
    [Header("Lane Settings")]
    [SerializeField] private float laneDistance = 3f;
    
    private bool isSpawning = false;
    private Transform playerTransform;
    private float currentMinInterval;
    private float currentMaxInterval;
    
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
        currentMinInterval = minSpawnInterval;
        currentMaxInterval = maxSpawnInterval;
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
            StartCoroutine(SpawnArrows());
            StartCoroutine(IncreaseDifficulty());
        }
    }
    
    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }
    
    IEnumerator SpawnArrows()
    {
        while (isSpawning)
        {
            float waitTime = Random.Range(currentMinInterval, currentMaxInterval);
            yield return new WaitForSeconds(waitTime);
            
            if (playerTransform != null && GameManager.Instance.IsGameActive())
            {
                SpawnArrow();
            }
        }
    }
    
    void SpawnArrow()
    {
        // Choose random lane
        int targetLane = Random.Range(0, 3);
        float xPosition = (targetLane - 1) * laneDistance;
        
        // Calculate spawn position ahead of player
        Vector3 spawnPosition = playerTransform.position + Vector3.forward * arrowSpawnDistance;
        spawnPosition.x = xPosition;
        spawnPosition.y = arrowHeight;
        
        // Show warning indicator
        StartCoroutine(ShowWarningAndFireArrow(targetLane, spawnPosition));
    }
    
    IEnumerator ShowWarningAndFireArrow(int lane, Vector3 arrowSpawnPos)
    {
        // Create warning indicator on the ground
        if (warningIndicatorPrefab != null && playerTransform != null)
        {
            float xPos = (lane - 1) * laneDistance;
            Vector3 warningPos = playerTransform.position + Vector3.forward * 5f;
            warningPos.x = xPos;
            warningPos.y = 0.1f;
            
            GameObject warning = Instantiate(warningIndicatorPrefab, warningPos, Quaternion.Euler(90, 0, 0));
            
            // Animate warning (pulsing effect)
            StartCoroutine(AnimateWarning(warning));
            
            // Wait for warning duration
            yield return new WaitForSeconds(warningDuration);
            
            // Destroy warning
            Destroy(warning);
        }
        else
        {
            yield return new WaitForSeconds(warningDuration);
        }
        
        // Fire the arrow
        if (arrowPrefab != null && isSpawning)
        {
            GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPos, Quaternion.identity);
            Arrow arrowScript = arrow.GetComponent<Arrow>();
            if (arrowScript == null)
            {
                arrowScript = arrow.AddComponent<Arrow>();
            }
            
            // Calculate direction to player's lane
            Vector3 targetPos = playerTransform.position + Vector3.forward * 2f;
            targetPos.x = (lane - 1) * laneDistance;
            
            Vector3 direction = (targetPos - arrowSpawnPos).normalized;
            arrowScript.Initialize(direction, arrowSpeed);
        }
    }
    
    IEnumerator AnimateWarning(GameObject warning)
    {
        if (warning == null) yield break;
        
        Renderer renderer = warning.GetComponent<Renderer>();
        if (renderer == null) yield break;
        
        float elapsedTime = 0f;
        Color originalColor = renderer.material.color;
        
        while (elapsedTime < warningDuration && warning != null)
        {
            float alpha = Mathf.PingPong(elapsedTime * 4f, 1f);
            Color newColor = warningColor;
            newColor.a = alpha;
            renderer.material.color = newColor;
            
            float scale = 1f + Mathf.PingPong(elapsedTime * 2f, 0.2f);
            warning.transform.localScale = Vector3.one * scale;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
    
    IEnumerator IncreaseDifficulty()
    {
        while (isSpawning)
        {
            yield return new WaitForSeconds(10f);
            
            // Gradually decrease spawn intervals
            currentMinInterval = Mathf.Max(minIntervalLimit, currentMinInterval * difficultyIncreaseRate);
            currentMaxInterval = Mathf.Max(minIntervalLimit * 2f, currentMaxInterval * difficultyIncreaseRate);
            
            Debug.Log($"Difficulty increased! Min interval: {currentMinInterval}, Max interval: {currentMaxInterval}");
        }
    }
    
    public void ResetDifficulty()
    {
        currentMinInterval = minSpawnInterval;
        currentMaxInterval = maxSpawnInterval;
    }
}
