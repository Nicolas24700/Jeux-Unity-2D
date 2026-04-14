using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CommunicationMinigameUI : MonoBehaviour, IMinigameUI
{
    // ui references
    [SerializeField] private GameObject panel;
    [SerializeField] private CommunicationMinigameController controller;
    [SerializeField] private MonoBehaviour playerController;
    [SerializeField] private GameObject successPanel;

    [SerializeField] private Animator firstDoorAnimator;

    private void OnEnable()
    {
        CommunicationMinigameController.OnTaskStateChanged += OnTaskStateChanged;
    }

    private void OnDisable()
    {
        CommunicationMinigameController.OnTaskStateChanged -= OnTaskStateChanged;
    }

    private void OnTaskStateChanged(bool completed)
    {
        // when the task state changes, if it's completed, show the success panel and hide the main panel, then re-enable player control
        if (completed)
        {
            if (successPanel != null)
            {
                successPanel.SetActive(true);
                CoroutineRunner.Run(HideSuccessAfterDelayRealtime(2f));
            }

            if (panel != null && panel.activeSelf)
                panel.SetActive(false);

            if (playerController != null)
                playerController.enabled = true;

            // open the door
            OpenFirstDoor();
        }
    }

    private System.Collections.IEnumerator HideSuccessAfterDelayRealtime(float seconds)
    {
        // fonction for hiding the success panel after a delay, using unscaled time to ensure it works even if the game is paused
        yield return new WaitForSecondsRealtime(seconds);
        if (successPanel != null)
            successPanel.SetActive(false);
    }

    public void Show()
    {
        // when showing the UI, disable player control and navigation events to prevent interference with the minigame
        if (panel == null || controller == null) return;
        panel.SetActive(true);

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.sendNavigationEvents = false;
        }

        controller.StartTask();
    }

    public void Hide()
    {
        // when hiding the UI, re-enable player control and navigation events
        if (panel == null || controller == null) return;
        panel.SetActive(false);

        if (playerController != null)
        {
            playerController.enabled = true;
        }

        if (EventSystem.current != null)
        {
            EventSystem.current.sendNavigationEvents = true;
        }
    }

    public void Toggle()
    {
        if (panel == null || controller == null) return;
        if (panel.activeSelf) Hide(); else Show();
    }

    public bool IsVisible => panel != null && panel.activeSelf;

    // -------------------------
    // door Open function
    // -------------------------
    private void OpenFirstDoor()
    {
        const string IS_OPEN_PARAM = "IsOpen";

        firstDoorAnimator.SetBool(IS_OPEN_PARAM, true);
        return;
    }
}
