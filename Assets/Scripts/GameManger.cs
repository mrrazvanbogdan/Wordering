using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private float elapsedTime;
    private float bestTime = float.MaxValue;
    private bool isTimerRunning;

    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI bestTimeText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Update()
    {
        if (isTimerRunning)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }

    public void StartTimer()
    {
        elapsedTime = 0f;
        isTimerRunning = true;
        UpdateTimerDisplay();
    }

    public void StopTimer()
    {
        isTimerRunning = false;
    }

    public void ResetTimer()
    {
        elapsedTime = 0f;
        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);

        timerText.text = $"Timer: {minutes:00}:{seconds:00}";
    }

    public void RegisterBestTime()
    {
        if (elapsedTime < bestTime)
        {
            bestTime = elapsedTime;
            UpdateBestTimeDisplay();
        }
    }

    private void UpdateBestTimeDisplay()
    {
        int minutes = Mathf.FloorToInt(bestTime / 60);
        int seconds = Mathf.FloorToInt(bestTime % 60);

        bestTimeText.text = $"Best Time: {minutes:00}:{seconds:00}";
    }
}
