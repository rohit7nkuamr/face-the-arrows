using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Health Display")]
    [SerializeField] private GameObject healthContainer;
    [SerializeField] private GameObject healthIconPrefab;
    [SerializeField] private Sprite fullHealthSprite;
    [SerializeField] private Sprite emptyHealthSprite;
    private List<Image> healthIcons = new List<Image>();
    
    [Header("Score Display")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text distanceText;
    [SerializeField] private Text multiplierText;
    
    [Header("Warning Indicators")]
    [SerializeField] private Image screenFlashImage;
    [SerializeField] private Color damageFlashColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private float flashDuration = 0.2f;
    
    [Header("Game Over Screen")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text finalScoreText;
    [SerializeField] private Text finalDistanceText;
    [SerializeField] private Text highScoreText;
    [SerializeField] private Text newHighScoreText;
    
    [Header("Pause Menu")]
    [SerializeField] private GameObject pausePanel;
    
    [Header("Main Menu")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private Text mainMenuHighScoreText;
    
    [Header("Tutorial")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private float tutorialDisplayTime = 5f;
    
    void Start()
    {
        // Subscribe to events
        PlayerController.onHealthChanged += UpdateHealthDisplay;
        PlayerController.onPlayerDeath += OnPlayerDeath;
        
        // Initialize UI
        InitializeHealthDisplay();
        HideAllPanels();
    }
    
    void OnDestroy()
    {
        PlayerController.onHealthChanged -= UpdateHealthDisplay;
        PlayerController.onPlayerDeath -= OnPlayerDeath;
    }
    
    void InitializeHealthDisplay()
    {
        if (healthContainer == null || healthIconPrefab == null) return;
        
        // Clear existing icons
        foreach (Transform child in healthContainer.transform)
        {
            Destroy(child.gameObject);
        }
        healthIcons.Clear();
        
        // Create health icons
        for (int i = 0; i < 3; i++)
        {
            GameObject icon = Instantiate(healthIconPrefab, healthContainer.transform);
            Image img = icon.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = fullHealthSprite;
                healthIcons.Add(img);
            }
        }
    }
    
    void UpdateHealthDisplay(int currentHealth)
    {
        for (int i = 0; i < healthIcons.Count; i++)
        {
            if (i < currentHealth)
            {
                healthIcons[i].sprite = fullHealthSprite;
                healthIcons[i].color = Color.white;
            }
            else
            {
                healthIcons[i].sprite = emptyHealthSprite;
                healthIcons[i].color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            }
        }
        
        // Flash screen on damage
        if (currentHealth < healthIcons.Count)
        {
            StartCoroutine(FlashScreen());
        }
    }
    
    IEnumerator FlashScreen()
    {
        if (screenFlashImage == null) yield break;
        
        screenFlashImage.gameObject.SetActive(true);
        screenFlashImage.color = damageFlashColor;
        
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            float alpha = Mathf.Lerp(damageFlashColor.a, 0f, elapsed / flashDuration);
            Color newColor = damageFlashColor;
            newColor.a = alpha;
            screenFlashImage.color = newColor;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        screenFlashImage.gameObject.SetActive(false);
    }
    
    public void UpdateScore(float score, float distance, float multiplier = 1f)
    {
        if (scoreText != null)
            scoreText.text = "Score: " + Mathf.FloorToInt(score).ToString("N0");
            
        if (distanceText != null)
            distanceText.text = Mathf.FloorToInt(distance).ToString() + "m";
            
        if (multiplierText != null)
        {
            if (multiplier > 1f)
            {
                multiplierText.gameObject.SetActive(true);
                multiplierText.text = "x" + multiplier.ToString("F1");
            }
            else
            {
                multiplierText.gameObject.SetActive(false);
            }
        }
    }
    
    void OnPlayerDeath()
    {
        // Death is handled by GameManager, but we can add UI effects here
        StartCoroutine(DeathSequence());
    }
    
    IEnumerator DeathSequence()
    {
        // Dramatic death effect
        if (screenFlashImage != null)
        {
            screenFlashImage.gameObject.SetActive(true);
            
            float duration = 1f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                float alpha = Mathf.Lerp(0f, 0.8f, elapsed / duration);
                screenFlashImage.color = new Color(0f, 0f, 0f, alpha);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
    }
    
    public void ShowGameOverScreen(float score, float distance, float highScore, bool isNewHighScore)
    {
        if (gameOverPanel == null) return;
        
        gameOverPanel.SetActive(true);
        
        if (finalScoreText != null)
            finalScoreText.text = "Score: " + Mathf.FloorToInt(score).ToString("N0");
            
        if (finalDistanceText != null)
            finalDistanceText.text = "Distance: " + Mathf.FloorToInt(distance).ToString() + "m";
            
        if (highScoreText != null)
            highScoreText.text = "Best: " + Mathf.FloorToInt(highScore).ToString("N0");
            
        if (newHighScoreText != null)
        {
            newHighScoreText.gameObject.SetActive(isNewHighScore);
            if (isNewHighScore)
            {
                StartCoroutine(AnimateNewHighScore());
            }
        }
    }
    
    IEnumerator AnimateNewHighScore()
    {
        if (newHighScoreText == null) yield break;
        
        float time = 0f;
        while (true)
        {
            float scale = 1f + Mathf.Sin(time * 5f) * 0.1f;
            newHighScoreText.transform.localScale = Vector3.one * scale;
            
            time += Time.deltaTime;
            yield return null;
        }
    }
    
    public void ShowMainMenu()
    {
        HideAllPanels();
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }
    
    public void ShowPauseMenu()
    {
        if (pausePanel != null)
            pausePanel.SetActive(true);
    }
    
    public void HidePauseMenu()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }
    
    public void ShowTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
            StartCoroutine(HideTutorialAfterDelay());
        }
    }
    
    IEnumerator HideTutorialAfterDelay()
    {
        yield return new WaitForSeconds(tutorialDisplayTime);
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
    }
    
    void HideAllPanels()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
    }
    
    // Button callbacks
    public void OnPlayButtonPressed()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
    }
    
    public void OnRestartButtonPressed()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }
    
    public void OnMainMenuButtonPressed()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ShowMainMenu();
        }
    }
    
    public void OnPauseButtonPressed()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TogglePause();
        }
    }
    
    public void OnResumeButtonPressed()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }
    }
    
    public void OnQuitButtonPressed()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
    }
}
