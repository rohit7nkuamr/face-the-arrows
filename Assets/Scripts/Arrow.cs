using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    private Vector3 moveDirection;
    private float speed;
    private float lifeTime = 10f;
    private bool hasHit = false;
    
    [Header("Effects")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private AudioClip whooshSound;
    [SerializeField] private AudioClip hitSound;
    
    private AudioSource audioSource;
    private Rigidbody rb;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;
        rb.isKinematic = true;
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Set up collider
        CapsuleCollider collider = GetComponent<CapsuleCollider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<CapsuleCollider>();
            collider.radius = 0.1f;
            collider.height = 1f;
            collider.direction = 2; // Z-axis
        }
        collider.isTrigger = true;
        
        // Auto-destroy after lifetime
        Destroy(gameObject, lifeTime);
    }
    
    public void Initialize(Vector3 direction, float arrowSpeed)
    {
        moveDirection = direction;
        speed = arrowSpeed;
        
        // Orient arrow to face movement direction
        transform.rotation = Quaternion.LookRotation(direction);
        
        // Play whoosh sound
        if (whooshSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(whooshSound);
        }
    }
    
    void Update()
    {
        if (!hasHit)
        {
            // Move the arrow
            transform.position += moveDirection * speed * Time.deltaTime;
            
            // Optional: Add slight gravity effect for realism
            moveDirection.y -= 0.1f * Time.deltaTime;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        
        if (other.CompareTag("Player"))
        {
            hasHit = true;
            
            // Play hit sound
            if (hitSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(hitSound);
            }
            
            // Create hit effect
            if (hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
            
            // Stick arrow to player briefly before destroying
            StartCoroutine(StickToTarget(other.transform));
        }
        else if (other.CompareTag("Ground") || other.CompareTag("Obstacle"))
        {
            hasHit = true;
            
            // Stick arrow in ground/obstacle
            rb.isKinematic = true;
            
            // Destroy after a delay
            Destroy(gameObject, 3f);
        }
    }
    
    IEnumerator StickToTarget(Transform target)
    {
        // Parent to target so arrow moves with player
        transform.SetParent(target);
        
        // Disable further collisions
        GetComponent<Collider>().enabled = false;
        
        yield return new WaitForSeconds(0.5f);
        
        // Fade out and destroy
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            float fadeTime = 0.5f;
            float elapsedTime = 0f;
            Color originalColor = renderer.material.color;
            
            while (elapsedTime < fadeTime)
            {
                float alpha = 1f - (elapsedTime / fadeTime);
                Color newColor = originalColor;
                newColor.a = alpha;
                renderer.material.color = newColor;
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
        
        Destroy(gameObject);
    }
}
