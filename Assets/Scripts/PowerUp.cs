using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PowerUpType
{
    Shield,
    Health,
    CoinMagnet,
    SpeedBoost,
    ScoreMultiplier
}

public class PowerUp : MonoBehaviour
{
    [Header("Power-Up Settings")]
    [SerializeField] private PowerUpType powerUpType;
    [SerializeField] private float duration = 5f;
    [SerializeField] private float effectStrength = 1f;
    
    [Header("Visual Effects")]
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobAmount = 0.5f;
    [SerializeField] private GameObject collectEffectPrefab;
    [SerializeField] private Color glowColor = Color.yellow;
    
    [Header("Audio")]
    [SerializeField] private AudioClip collectSound;
    
    private Vector3 startPosition;
    private bool isCollected = false;
    
    void Start()
    {
        startPosition = transform.position;
        
        // Set up trigger collider
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
        
        // Add glow effect
        SetupGlowEffect();
        
        // Set tag
        gameObject.tag = "PowerUp";
    }
    
    void Update()
    {
        if (!isCollected)
        {
            // Rotate
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
            
            // Bob up and down
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
    
    void SetupGlowEffect()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = renderer.material;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", glowColor * 2f);
        }
        
        // Add light component for glow
        Light light = gameObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = glowColor;
        light.intensity = 2f;
        light.range = 5f;
    }
    
    public void Apply(PlayerController player)
    {
        if (isCollected) return;
        isCollected = true;
        
        // Play collection effect
        PlayCollectEffect();
        
        // Apply power-up effect
        switch (powerUpType)
        {
            case PowerUpType.Shield:
                ApplyShield(player);
                break;
            case PowerUpType.Health:
                ApplyHealth(player);
                break;
            case PowerUpType.CoinMagnet:
                ApplyCoinMagnet(player);
                break;
            case PowerUpType.SpeedBoost:
                ApplySpeedBoost(player);
                break;
            case PowerUpType.ScoreMultiplier:
                ApplyScoreMultiplier();
                break;
        }
    }
    
    void ApplyShield(PlayerController player)
    {
        // Create shield effect
        GameObject shield = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        shield.name = "Shield";
        shield.transform.SetParent(player.transform);
        shield.transform.localPosition = Vector3.zero;
        shield.transform.localScale = Vector3.one * 2f;
        
        // Remove collider
        Destroy(shield.GetComponent<Collider>());
        
        // Make transparent
        Renderer renderer = shield.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.SetFloat("_Mode", 3); // Transparent mode
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
        Color shieldColor = new Color(0f, 0.5f, 1f, 0.3f);
        mat.SetColor("_Color", shieldColor);
        renderer.material = mat;
        
        // Add shield component
        ShieldEffect shieldEffect = shield.AddComponent<ShieldEffect>();
        shieldEffect.Initialize(duration, player);
    }
    
    void ApplyHealth(PlayerController player)
    {
        player.Heal(1);
        
        // Visual feedback
        StartCoroutine(HealthEffect(player.gameObject));
    }
    
    IEnumerator HealthEffect(GameObject target)
    {
        // Create healing particles or effect
        float elapsed = 0f;
        while (elapsed < 1f)
        {
            // Green pulse effect
            Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                Color originalColor = renderer.material.color;
                renderer.material.color = Color.Lerp(originalColor, Color.green, Mathf.PingPong(elapsed * 2f, 1f));
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    
    void ApplyCoinMagnet(PlayerController player)
    {
        // Add magnet component to player
        CoinMagnet magnet = player.gameObject.GetComponent<CoinMagnet>();
        if (magnet == null)
        {
            magnet = player.gameObject.AddComponent<CoinMagnet>();
        }
        magnet.Activate(duration);
    }
    
    void ApplySpeedBoost(PlayerController player)
    {
        // This would need to be implemented in PlayerController
        // For now, just increase score multiplier as placeholder
        ApplyScoreMultiplier();
    }
    
    void ApplyScoreMultiplier()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetScoreMultiplier(2f);
            
            // Reset multiplier after duration
            StartCoroutine(ResetMultiplierAfterDuration());
        }
    }
    
    IEnumerator ResetMultiplierAfterDuration()
    {
        yield return new WaitForSeconds(duration);
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetScoreMultiplier(1f);
        }
    }
    
    void PlayCollectEffect()
    {
        // Play sound
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }
        
        // Create particle effect
        if (collectEffectPrefab != null)
        {
            GameObject effect = Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
        
        // Hide and destroy
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        Destroy(gameObject, 0.5f);
    }
}

// Helper component for shield effect
public class ShieldEffect : MonoBehaviour
{
    private float duration;
    private PlayerController player;
    private bool isActive = true;
    
    public void Initialize(float shieldDuration, PlayerController playerController)
    {
        duration = shieldDuration;
        player = playerController;
        StartCoroutine(ShieldRoutine());
    }
    
    IEnumerator ShieldRoutine()
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            // Pulse effect
            float scale = 2f + Mathf.Sin(elapsed * 3f) * 0.2f;
            transform.localScale = Vector3.one * scale;
            
            // Rotate
            transform.Rotate(Vector3.up * 90f * Time.deltaTime);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Fade out
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            float fadeTime = 0.5f;
            float fadeElapsed = 0f;
            Color originalColor = renderer.material.color;
            
            while (fadeElapsed < fadeTime)
            {
                float alpha = Mathf.Lerp(originalColor.a, 0f, fadeElapsed / fadeTime);
                Color newColor = originalColor;
                newColor.a = alpha;
                renderer.material.color = newColor;
                
                fadeElapsed += Time.deltaTime;
                yield return null;
            }
        }
        
        Destroy(gameObject);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (isActive && other.CompareTag("Arrow"))
        {
            // Block one arrow hit
            isActive = false;
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}

// Helper component for coin magnet
public class CoinMagnet : MonoBehaviour
{
    [SerializeField] private float magnetRange = 10f;
    [SerializeField] private float magnetForce = 20f;
    private bool isActive = false;
    
    public void Activate(float duration)
    {
        isActive = true;
        StartCoroutine(MagnetRoutine(duration));
    }
    
    IEnumerator MagnetRoutine(float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            // Find nearby coins
            Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, magnetRange);
            
            foreach (Collider col in nearbyObjects)
            {
                if (col.CompareTag("Coin"))
                {
                    // Attract coin
                    Vector3 direction = (transform.position - col.transform.position).normalized;
                    col.transform.position += direction * magnetForce * Time.deltaTime;
                }
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        isActive = false;
    }
}
