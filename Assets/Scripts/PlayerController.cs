using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float forwardSpeed = 10f;
    [SerializeField] private float speedIncreaseRate = 0.1f;
    [SerializeField] private float maxSpeed = 30f;
    [SerializeField] private float laneDistance = 3f;
    [SerializeField] private float laneSwitchSpeed = 10f;
    [SerializeField] private float jumpForce = 10f;
    
    [Header("Lane Settings")]
    private int currentLane = 1; // 0 = left, 1 = middle, 2 = right
    private Vector3 targetPosition;
    
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;
    
    [Header("Animation & Effects")]
    private Animator animator;
    private bool isJumping = false;
    private bool isSliding = false;
    
    private Rigidbody rb;
    private bool isGrounded = true;
    private bool isDead = false;
    
    // Events
    public delegate void OnHealthChanged(int health);
    public static event OnHealthChanged onHealthChanged;
    
    public delegate void OnPlayerDeath();
    public static event OnPlayerDeath onPlayerDeath;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        targetPosition = transform.position;
        
        // Start in middle lane
        currentLane = 1;
    }
    
    void Update()
    {
        if (isDead) return;
        
        // Handle input
        HandleInput();
        
        // Increase speed over time
        if (forwardSpeed < maxSpeed)
        {
            forwardSpeed += speedIncreaseRate * Time.deltaTime;
        }
    }
    
    void FixedUpdate()
    {
        if (isDead) return;
        
        // Move forward
        Vector3 forwardMove = transform.forward * forwardSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + forwardMove);
        
        // Lane switching
        Vector3 newPosition = rb.position;
        newPosition.x = Mathf.Lerp(newPosition.x, targetPosition.x, laneSwitchSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);
    }
    
    void HandleInput()
    {
        // Mobile swipe detection would go here
        // For testing with keyboard
        if (Input.GetKeyDown(KeyCode.LeftArrow) || SwipeManager.IsSwipingLeft())
        {
            ChangeLane(-1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || SwipeManager.IsSwipingRight())
        {
            ChangeLane(1);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || SwipeManager.IsSwipingUp())
        {
            Jump();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || SwipeManager.IsSwipingDown())
        {
            Slide();
        }
    }
    
    void ChangeLane(int direction)
    {
        int targetLane = currentLane + direction;
        
        if (targetLane < 0 || targetLane > 2)
            return;
            
        currentLane = targetLane;
        targetPosition = new Vector3((currentLane - 1) * laneDistance, transform.position.y, transform.position.z);
    }
    
    void Jump()
    {
        if (!isGrounded || isJumping) return;
        
        isJumping = true;
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        
        if (animator != null)
            animator.SetTrigger("Jump");
    }
    
    void Slide()
    {
        if (!isGrounded || isSliding) return;
        
        StartCoroutine(SlideCoroutine());
    }
    
    IEnumerator SlideCoroutine()
    {
        isSliding = true;
        
        if (animator != null)
            animator.SetBool("IsSliding", true);
            
        // Reduce collider height
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        if (capsule != null)
        {
            float originalHeight = capsule.height;
            float originalCenter = capsule.center.y;
            
            capsule.height = originalHeight * 0.5f;
            capsule.center = new Vector3(capsule.center.x, originalCenter * 0.5f, capsule.center.z);
            
            yield return new WaitForSeconds(1f);
            
            capsule.height = originalHeight;
            capsule.center = new Vector3(capsule.center.x, originalCenter, capsule.center.z);
        }
        
        if (animator != null)
            animator.SetBool("IsSliding", false);
            
        isSliding = false;
    }
    
    public void TakeDamage(int damage = 1)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        onHealthChanged?.Invoke(currentHealth);
        
        // Visual feedback
        StartCoroutine(DamageFlash());
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    IEnumerator DamageFlash()
    {
        // Flash red effect
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.material.color = Color.red;
        }
        
        yield return new WaitForSeconds(0.2f);
        
        foreach (var renderer in renderers)
        {
            renderer.material.color = Color.white;
        }
    }
    
    void Die()
    {
        isDead = true;
        onPlayerDeath?.Invoke();
        
        if (animator != null)
            animator.SetTrigger("Death");
            
        // Stop movement
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            isJumping = false;
        }
        else if (collision.gameObject.CompareTag("Obstacle"))
        {
            // Stumble and get caught by animals
            Die();
        }
    }
    
    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Arrow"))
        {
            TakeDamage(1);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("PowerUp"))
        {
            // Handle power-up collection
            PowerUp powerUp = other.GetComponent<PowerUp>();
            if (powerUp != null)
            {
                powerUp.Apply(this);
            }
            Destroy(other.gameObject);
        }
    }
    
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        onHealthChanged?.Invoke(currentHealth);
    }
    
    public float GetCurrentSpeed()
    {
        return forwardSpeed;
    }
    
    public int GetCurrentLane()
    {
        return currentLane;
    }
}
