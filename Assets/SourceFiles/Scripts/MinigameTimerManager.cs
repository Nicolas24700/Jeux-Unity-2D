using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MinigameTimerManager : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] private float timeLimitSeconds = 180f;
    [SerializeField] private TMP_Text timerText;

    [Header("Panels")]
    [SerializeField] private GameObject timerPanel;
    [SerializeField] private GameObject successPanel;
    [SerializeField] private GameObject failPanel;

    [Header("Dependencies")]
    [SerializeField] private GameMenu gameMenu;
    [SerializeField] private GameObject playerRobot;

    private float remainingTime;
    private bool isRunFinished;
    private bool gazoleSolved;
    private bool communicationSolved;
    private bool fireSolved;

    private void Awake()
    {
        // initialize the timer and set up the initial UI state
        remainingTime = Mathf.Max(1f, timeLimitSeconds);

        UpdateTimerText();
    }

    private void OnEnable()
    {
        // take note of task state changes and fire extinguishing events to track progress and determine when to show success or failure panels
        GazoleMinigameController.OnTaskStateChanged += HandleGazoleTaskState;
        CommunicationMinigameController.OnTaskStateChanged += HandleCommunicationTaskState;
        FireExtinguishable.OnFireExtinguished += HandleFireExtinguished;
    }

    private void Update()
    {
        if (isRunFinished)
        {
            return;
        }

        if (IsTimerPaused())
        {
            return;
        }

        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            UpdateTimerText();
            ShowFailPanel();
            return;
        }

        UpdateTimerText();
    }

    private bool IsTimerPaused()
    {
        // if the game menu is open, stop the timer
        return gameMenu != null && gameMenu.IsMenuOpen;
    }

    private void HandleGazoleTaskState(bool isCompleted)
    {
        // if gazole task is completed, mark it as solved
        if (!isCompleted || gazoleSolved || isRunFinished)
        {
            return;
        }

        gazoleSolved = true;
        TryShowSuccessPanel();
    }

    private void HandleCommunicationTaskState(bool isCompleted)
    {
        // if communication task is completed, mark it as solved
        if (!isCompleted || communicationSolved || isRunFinished)
        {
            return;
        }

        communicationSolved = true;
        TryShowSuccessPanel();
    }

    private void HandleFireExtinguished()
    {
        // if fire task is completed, mark it as solved
        if (fireSolved || isRunFinished)
        {
            return;
        }

        fireSolved = true;
        TryShowSuccessPanel();
    }

    private void TryShowSuccessPanel()
    {
        // if all tasks are solved and there's still time remaining, show the success panel
        if (gazoleSolved && communicationSolved && fireSolved && remainingTime > 0f)
        {
            ShowSuccessPanel();
        }
    }

    private void ShowSuccessPanel()
    {
        isRunFinished = true;

        // unlock the cursor and make it visible for interacting with the success panel
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (timerPanel != null)
        {
            timerPanel.SetActive(false);
        }

        if (failPanel != null)
        {
            failPanel.SetActive(false);
        }

        if (successPanel != null)
        {
            successPanel.SetActive(true);
        }
        if (playerRobot != null)
        {
            playerRobot.SetActive(false);
        }
    }

    private void ShowFailPanel()
    {
        isRunFinished = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (timerPanel != null)
        {
            timerPanel.SetActive(false);
        }

        if (successPanel != null)
        {
            successPanel.SetActive(false);
        }

        if (failPanel != null)
        {
            failPanel.SetActive(true);
        }

        if (playerRobot != null)
        {
            playerRobot.SetActive(false);
        }
    }

    public void OnReplayButton()
    {
        // reset the game by reloading the current scene
        Time.timeScale = 1f;
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.buildIndex);
    }

    private void UpdateTimerText()
    {
        // update timer text
        if (timerText == null)
        {
            return;
        }

        int totalSeconds = Mathf.CeilToInt(remainingTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        timerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
    }
}
