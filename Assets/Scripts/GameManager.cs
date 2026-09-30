using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Настройки игры")]
    [SerializeField] private int winScore = 5;
    [SerializeField] private int maxLives = 3;
    [SerializeField] private float fadeDuration = 1f;

    [Header("UI")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    private Text scoreText;
    private Text livesText;

    private int score;
    private int lives;
    private bool isPaused;
    private bool isGameOver;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        GameObject scoreObj = GameObject.Find("ScoreText");
        GameObject livesObj = GameObject.Find("LivesText");

        if (scoreObj != null)
            scoreText = scoreObj.GetComponent<Text>();
        if (livesObj != null)
            livesText = livesObj.GetComponent<Text>();

        if (scoreText == null || livesText == null)
        {
            Debug.LogWarning("Не найдены Text объекты! Убедись, что они названы ScoreText и LivesText");
        }

        score = 0;
        lives = maxLives;
        isPaused = false;
        isGameOver = false;

        UpdateUI();
        pauseMenu.SetActive(false);
        winPanel.SetActive(false);
        losePanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isGameOver) return;
            TogglePause();
        }
    }

    public void OnPickup()
    {
        if (isGameOver) return;

        score++;
        UpdateUI();
        Debug.Log("Подобрал! Очки: " + score);

        if (score >= winScore)
            StartCoroutine(FadeInPanel(winPanel));
    }

    public void OnDeath()
    {
        if (isGameOver) return;

        lives--;
        UpdateUI();
        Debug.Log("Потерял жизнь! Осталось: " + lives);

        if (lives <= 0)
            StartCoroutine(FadeInPanel(losePanel));
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        pauseMenu.SetActive(isPaused);
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
        isGameOver = false;
        pauseMenu.SetActive(false);
        winPanel.SetActive(false);
        losePanel.SetActive(false);

        // Показываем текст очков и жизней
        if (scoreText != null)
            scoreText.enabled = true;
        if (livesText != null)
            livesText.enabled = true;

        // Перезагрузка сцены
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    IEnumerator FadeInPanel(GameObject panel)
    {
        // Останавливаем игру
        Time.timeScale = 0f;
        isGameOver = true;

        // Скрываем текст очков и жизней
        if (scoreText != null)
            scoreText.enabled = false;
        if (livesText != null)
            livesText.enabled = false;

        // Включаем панель
        panel.SetActive(true);

        // Получаем CanvasGroup
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = panel.AddComponent<CanvasGroup>();
        }

        // Устанавливаем начальную прозрачность
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // Плавно увеличиваем прозрачность
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        // Устанавливаем финальное значение
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Очки: " + score + " / " + winScore;

        if (livesText != null)
            livesText.text = "Жизни: " + lives + " / " + maxLives;
    }
}
