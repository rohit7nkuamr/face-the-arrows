using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WildAnimalChase : MonoBehaviour
{
    [Header("Chase Settings")]
    [SerializeField] private float chaseDistance = 5f;
    [SerializeField] private float catchUpSpeed = 15f;
    [SerializeField] private float normalSpeed = 10f;
    [SerializeField] private float maxDistance = 10f;
    
    [Header("Animal Pack")]
    [SerializeField] private GameObject[] animalPrefabs;
    [SerializeField] private int packSize = 3;
    [SerializeField] private float packSpread = 2f;
    
    [Header("Visual Effects")]
    [SerializeField] private bool useGlowingEyes = true;
    [SerializeField] private Color eyeGlowColor = Color.red;
    [SerializeField] private float eyeGlowIntensity = 2f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip[] growlSounds;
    [SerializeField] private AudioClip[] roarSounds;
    [SerializeField] private float growlInterval = 3f;
    
    private List<GameObject> animalPack = new List<GameObject>();
    private Transform playerTransform;
    private PlayerController playerController;
    private AudioSource audioSource;
    private bool isChasing = false;
    private float currentSpeed;
    private float lastGrowlTime;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f; // 3D sound
            audioSource.minDistance = 5f;
            audioSource.maxDistance = 20f;
        }
        
        currentSpeed = normalSpeed;
    }
    
    public void StartChase(Transform player)
    {
        if (!isChasing)
        {
            playerTransform = player;
            playerController = player.GetComponent<PlayerController>();
            isChasing = true;
            
            SpawnAnimalPack();
            StartCoroutine(ChaseRoutine());
            StartCoroutine(PlayGrowlSounds());
        }
    }
    
    public void StopChase()
    {
        isChasing = false;
        StopAllCoroutines();
        
        // Remove animal pack
        foreach (GameObject animal in animalPack)
        {
            if (animal != null)
                Destroy(animal);
        }
        animalPack.Clear();
    }
    
    void SpawnAnimalPack()
    {
        if (animalPrefabs.Length == 0) return;
        
        Vector3 spawnPosition = transform.position;
        
        for (int i = 0; i < packSize; i++)
        {
            GameObject animalPrefab = animalPrefabs[Random.Range(0, animalPrefabs.Length)];
            
            // Spread animals horizontally
            float xOffset = (i - packSize / 2) * packSpread;
            Vector3 animalPos = spawnPosition + new Vector3(xOffset, 0, Random.Range(-1f, 1f));
            
            GameObject animal = Instantiate(animalPrefab, animalPos, Quaternion.identity);
            animal.transform.SetParent(transform);
            
            // Setup glowing eyes
            if (useGlowingEyes)
            {
                SetupGlowingEyes(animal);
            }
            
            // Add movement variation
            AnimalMovement movement = animal.AddComponent<AnimalMovement>();
            movement.Initialize(Random.Range(0.8f, 1.2f));
            
            animalPack.Add(animal);
        }
    }
    
    void SetupGlowingEyes(GameObject animal)
    {
        // Find eye objects or create glowing spheres as eyes
        Transform[] eyes = new Transform[2];
        
        // Create simple glowing eye effect
        for (int i = 0; i < 2; i++)
        {
            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.transform.SetParent(animal.transform);
            eye.transform.localScale = Vector3.one * 0.1f;
            eye.transform.localPosition = new Vector3((i == 0 ? -0.2f : 0.2f), 0.5f, 0.3f);
            
            // Remove collider
            Destroy(eye.GetComponent<Collider>());
            
            // Setup emissive material
            Renderer renderer = eye.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", eyeGlowColor * eyeGlowIntensity);
            renderer.material = mat;
            
            eyes[i] = eye.transform;
        }
    }
    
    IEnumerator ChaseRoutine()
    {
        while (isChasing)
        {
            if (playerTransform != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
                
                // Adjust speed based on distance
                if (distanceToPlayer > chaseDistance)
                {
                    // Player is getting away, speed up
                    currentSpeed = catchUpSpeed;
                    
                    // Play roar sound occasionally
                    if (Random.value < 0.1f && roarSounds.Length > 0)
                    {
                        audioSource.PlayOneShot(roarSounds[Random.Range(0, roarSounds.Length)]);
                    }
                }
                else
                {
                    // Maintain chase distance
                    currentSpeed = playerController != null ? playerController.GetCurrentSpeed() : normalSpeed;
                }
                
                // Move pack forward
                Vector3 targetPosition = playerTransform.position - playerTransform.forward * chaseDistance;
                targetPosition.y = transform.position.y;
                
                transform.position = Vector3.Lerp(transform.position, targetPosition, currentSpeed * Time.deltaTime * 0.5f);
                
                // Keep animals facing player
                Vector3 lookDirection = (playerTransform.position - transform.position).normalized;
                lookDirection.y = 0;
                transform.rotation = Quaternion.LookRotation(lookDirection);
                
                // Check if animals caught the player
                if (distanceToPlayer < 1f)
                {
                    CatchPlayer();
                }
            }
            
            yield return null;
        }
    }
    
    IEnumerator PlayGrowlSounds()
    {
        while (isChasing)
        {
            if (growlSounds.Length > 0 && Time.time - lastGrowlTime > growlInterval)
            {
                audioSource.PlayOneShot(growlSounds[Random.Range(0, growlSounds.Length)]);
                lastGrowlTime = Time.time;
            }
            
            yield return new WaitForSeconds(growlInterval + Random.Range(-1f, 1f));
        }
    }
    
    void CatchPlayer()
    {
        if (playerController != null)
        {
            // Trigger player death
            playerController.SendMessage("Die", SendMessageOptions.DontRequireReceiver);
        }
        
        // Play catch animation/sound
        if (roarSounds.Length > 0)
        {
            audioSource.PlayOneShot(roarSounds[Random.Range(0, roarSounds.Length)]);
        }
        
        StopChase();
    }
}

// Helper class for individual animal movement
public class AnimalMovement : MonoBehaviour
{
    private float speedVariation = 1f;
    private float bobAmount = 0.1f;
    private float bobSpeed = 2f;
    private Vector3 startPosition;
    
    public void Initialize(float variation)
    {
        speedVariation = variation;
        startPosition = transform.localPosition;
    }
    
    void Update()
    {
        // Add bobbing motion for running effect
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed * speedVariation) * bobAmount;
        transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
        
        // Add slight rotation for more dynamic movement
        transform.localRotation = Quaternion.Euler(0, Mathf.Sin(Time.time * speedVariation) * 5f, 0);
    }
}
