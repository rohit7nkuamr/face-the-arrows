using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
            }
            return instance;
        }
    }
    
    [Header("Game State")]
    private bool isGameActive = false;
    private bool isPaused = false;
    private float gameTime = 0f;
    
    [Header("Score")]
    private float score = 0f;
    private float scoreMultiplier = 1f;
    private float distanceTraveled = 0f;
    [SerializeField] private float scorePerMeter = 10f;
    
    [Header("UI References")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text distanceText;
    [SerializeField] private Text highScoreText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gameplayUI;
    
    [Header("Player Reference")]
    [SerializeField] private GameObject playerPrefab;
    private GameObject currentPlayer;
    private PlayerController playerController;
    private Vector3 playerStartPosition;
    
    [Header("High Score")]
    private float highScore;
    private const string HIGH_SCORE_KEY = "FaceTheArrowsHighScore";
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        LoadHighScore();
        ShowMainMenu();
        
        // Subscribe to player events
        PlayerController.onPlayerDeath += OnPlayerDeath;
    }
    
    void OnDestroy()
    {
        PlayerController.onPlayerDeath -= OnPlayerDeath;
    }
    
    void Update()
    {
        if (isGameActive && !isPaused)
        {
            gameTime += Time.deltaTime;
            UpdateScore();
            UpdateUI();
        }
        
        // Pause game
        if (Input.GetKeyDown(KeyCode.Escape) && isGameActive)
        {
            TogglePause();
        }
    }
    
    void UpdateScore()
    {
        if (playerController != null)
        {
            distanceTraveled = Vector3.Distance(playerStartPosition, currentPlayer.transform.position);
            score = distanceTraveled * scorePerMeter * scoreMultiplier;
        }
    }
    
    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + Mathf.FloorToInt(score).ToString();
            
        if (distanceText != null)
            distanceText.text = "Distance: " + Mathf.FloorToInt(distanceTraveled).ToString() + "m";
    }
    
    public void StartGame()
    {
        // Hide menus
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (pausePanel != null)
            pausePanel.SetActive(false);
            
        // Show gameplay UI
        if (gameplayUI != null)
            gameplayUI.SetActive(true);
        
        // Reset game state
        score = 0f;
        distanceTraveled = 0f;
        gameTime = 0f;
        scoreMultiplier = 1f;
        
        // Spawn player
        if (playerPrefab != null)
        {
            playerStartPosition = new Vector3(0, 1, 0);
            currentPlayer = Instantiate(playerPrefab, playerStartPosition, Quaternion.identity);
            playerController = currentPlayer.GetComponent<PlayerController>();
        }
        
        // Start spawning systems
        if (LevelGenerator.Instance != null)
            LevelGenerator.Instance.StartGenerating();
        if (ArrowSpawner.Instance != null)
            ArrowSpawner.Instance.StartSpawning();
        if (ObstacleSpawner.Instance != null)
            ObstacleSpawner.Instance.StartSpawning();
        
        isGameActive = true;
        isPaused = false;
        Time.timeScale = 1f;
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void ShowMainMenu()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (pausePanel != null)
            pausePanel.SetActive(false);
        if (gameplayUI != null)
            gameplayUI.SetActive(false);
            
        isGameActive = false;
        Time.timeScale = 1f;
    }
    
    void OnPlayerDeath()
    {
        isGameActive = false;
        
        // Stop spawning systems
        if (LevelGenerator.Instance != null)
            LevelGenerator.Instance.StopGenerating();
        if (ArrowSpawner.Instance != null)
            ArrowSpawner.Instance.StopSpawning();
        if (ObstacleSpawner.Instance != null)
            ObstacleSpawner.Instance.StopSpawning();
        
        // Check for high score
        if (score > highScore)
        {
            highScore = score;
            SaveHighScore();
        }
        
        // Show game over screen
        StartCoroutine(ShowGameOverAfterDelay(2f));
    }
    
    IEnumerator ShowGameOverAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            // Update game over UI
            Text finalScoreText = gameOverPanel.transform.Find("FinalScoreText")?.GetComponent<Text>();
            if (finalScoreText != null)
                finalScoreText.text = "Final Score: " + Mathf.FloorToInt(score).ToString();
                
            Text finalDistanceText = gameOverPanel.transform.Find("FinalDistanceText")?.GetComponent<Text>();
            if (finalDistanceText != null)
                finalDistanceText.text = "Distance: " + Mathf.FloorToInt(distanceTraveled).ToString() + "m";
                
            Text gameOverHighScoreText = gameOverPanel.transform.Find("HighScoreText")?.GetComponent<Text>();
            if (gameOverHighScoreText != null)
                gameOverHighScoreText.text = "High Score: " + Mathf.FloorToInt(highScore).ToString();
        }
    }
    
    public void TogglePause()
    {
        isPaused = !isPaused;
        
        if (pausePanel != null)
            pausePanel.SetActive(isPaused);
            
        Time.timeScale = isPaused ? 0f : 1f;
    }
    
    public void ResumeGame()
    {
        isPaused = false;
        if (pausePanel != null)
            pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    void LoadHighScore()
    {
        highScore = PlayerPrefs.GetFloat(HIGH_SCORE_KEY, 0f);
        if (highScoreText != null)
            highScoreText.text = "High Score: " + Mathf.FloorToInt(highScore).ToString();
    }
    
    void SaveHighScore()
    {
        PlayerPrefs.SetFloat(HIGH_SCORE_KEY, highScore);
        PlayerPrefs.Save();
    }
    
    public void AddScore(float points)
    {
        score += points * scoreMultiplier;
    }
    
    public void SetScoreMultiplier(float multiplier)
    {
        scoreMultiplier = multiplier;
    }
    
    public bool IsGameActive()
    {
        return isGameActive && !isPaused;
    }
    
    public float GetGameTime()
    {
        return gameTime;
    }
    
    public float GetScore()
    {
        return score;
    }
}
