using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GazoleMinigameUI : MonoBehaviour, IMinigameUI
{
    // ui references
    [SerializeField] private GameObject panel;
    [SerializeField] private GazoleMinigameController controller;
    [SerializeField] private MonoBehaviour playerController;
    [SerializeField] private GameObject successPanel;

    [SerializeField] private Animator firstDoorAnimator;
    private void OnEnable()
    {
        // subscribe to the task state change event to react when the player completes the task
        GazoleMinigameController.OnTaskStateChanged += OnTaskStateChanged;
    }

    private void OnDisable()
    {
        // unsubscribe from the event to avoid memory leaks and unintended behavior when the object is disabled or destroyed
        GazoleMinigameController.OnTaskStateChanged -= OnTaskStateChanged;
    }

    private void OnTaskStateChanged(bool completed)
    {
        if (completed)
        {
            if (successPanel != null)
            {
                successPanel.SetActive(true);
                // start a coroutine to hide the success panel after a delay, using unscaled time to ensure it works even if the game is paused
                CoroutineRunner.Run(HideSuccessAfterDelayRealtime(2f));
            }

            if (panel != null && panel.activeSelf)
                panel.SetActive(false);

            if (playerController != null)
                playerController.enabled = true;

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
        // when showing the UI, we activate the panel, disable player control, and disable navigation events to prevent unintended interactions with other UI elements
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
        if (firstDoorAnimator != null)
            firstDoorAnimator.SetBool(IS_OPEN_PARAM, true);
    }
}