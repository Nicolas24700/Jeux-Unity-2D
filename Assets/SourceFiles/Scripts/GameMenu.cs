using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject playRobot;
    private CanvasGroup menuCanvasGroup;

    public bool IsMenuOpen { get; private set; } = true;

    private void Start()
    {
        // Force a consistent startup state so the first Play click is always received.
        SetMenuVisible(true);

        if (playRobot != null)
            playRobot.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OpenMenu();
        }
    }

    public void OnPlayButton()
    {
        // if play, close menu and activate the player robot
        SetMenuVisible(false);

        if (playRobot != null)
            playRobot.SetActive(true);
    }

    public void OpenMenu()
    {
        // open the menu and disable the player robot if echap is pressed
        SetMenuVisible(true);

        if (playRobot != null)
        {
            playRobot.SetActive(false);
        }
    }

    // Kept for compatibility if button OnClick was wired to PlayButton.
    public void PlayButton()
    {
        OnPlayButton();
    }


    public void OnExitButton()
    {
        // if exit, exit the game
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SetMenuVisible(bool isVisible)
    {
        // this Ensure UI can be clicked when menu is open, and lock cursor during gameplay.
        IsMenuOpen = isVisible;

        if (menuPanel == null)
            return;

        Cursor.lockState = isVisible ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isVisible;

        if (menuCanvasGroup == null)
        {
            menuCanvasGroup = menuPanel.GetComponent<CanvasGroup>();
            if (menuCanvasGroup == null)
            {
                menuCanvasGroup = menuPanel.AddComponent<CanvasGroup>();
            }
        }

        //Set the menu panel's CanvasGroup properties to show/hide the menu and enable/disable interaction
        menuCanvasGroup.alpha = isVisible ? 1f : 0f;
        menuCanvasGroup.interactable = isVisible;
        menuCanvasGroup.blocksRaycasts = isVisible;
        return;
    }
}
