using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public ObstacleType obstacleType = ObstacleType.Ground;
    
    [Header("Effects")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private AudioClip hitSound;
    
    private bool hasBeenHit = false;
    
    void Start()
    {
        // Ensure obstacle has proper tags and colliders
        SetupObstacle();
    }
    
    void SetupObstacle()
    {
        // Set tag
        gameObject.tag = "Obstacle";
        
        // Ensure collider exists
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            
            // Adjust collider based on obstacle type
            switch (obstacleType)
            {
                case ObstacleType.Ground:
                    // Standard collision box
                    break;
                case ObstacleType.Jump:
                    // Lower obstacle - adjust collider height
                    boxCollider.size = new Vector3(1f, 0.5f, 1f);
                    boxCollider.center = new Vector3(0, 0.25f, 0);
                    break;
                case ObstacleType.Slide:
                    // Higher obstacle - adjust collider position
                    boxCollider.size = new Vector3(1f, 1f, 1f);
                    boxCollider.center = new Vector3(0, 1.5f, 0);
                    break;
            }
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (hasBeenHit) return;
        
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            
            if (player != null)
            {
                // Check if player successfully avoided the obstacle
                bool avoided = false;
                
                switch (obstacleType)
                {
                    case ObstacleType.Jump:
                        // Check if player is jumping
                        // This would need to be implemented in PlayerController
                        break;
                    case ObstacleType.Slide:
                        // Check if player is sliding
                        // This would need to be implemented in PlayerController
                        break;
                    case ObstacleType.Ground:
                    default:
                        // Ground obstacles always cause stumble
                        break;
                }
                
                if (!avoided)
                {
                    OnHitPlayer();
                }
            }
        }
    }
    
    void OnHitPlayer()
    {
        hasBeenHit = true;
        
        // Play hit effect
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
        
        // Play hit sound
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position);
        }
    }
}
