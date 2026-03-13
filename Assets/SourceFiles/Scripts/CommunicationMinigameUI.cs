using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CommunicationMinigameUI : MonoBehaviour
{
    [SerializeField] private GameObject panel; 
    [SerializeField] private CommunicationMinigameController controller;
    [SerializeField] private MonoBehaviour playerController; 
    [SerializeField] private GameObject successPanel;

    private Coroutine hideSuccessCoroutine;

    private void Awake()
    {
        if (panel != null)
            panel.SetActive(false);
        if (successPanel != null)
            successPanel.SetActive(false);

        if (playerController == null)
        {
            var found = FindObjectOfType(System.Type.GetType("StarterAssets.ThirdPersonController, Assembly-CSharp")) as MonoBehaviour;
            if (found != null) playerController = found;
        }
    }

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
        if (completed)
        {
            if (successPanel != null)
            {
                successPanel.SetActive(true);

                if (hideSuccessCoroutine != null)
                {
                    hideSuccessCoroutine = null;
                }
                CoroutineRunner.Run(HideSuccessAfterDelayRealtime(2f));
            }

            if (panel != null && panel.activeSelf)
                panel.SetActive(false);

            if (playerController != null)
                playerController.enabled = true;
        }
    }

    private System.Collections.IEnumerator HideSuccessAfterDelayRealtime(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        if (successPanel != null)
            successPanel.SetActive(false);
        hideSuccessCoroutine = null;
    }

    public void Show()
    {
        if (panel == null || controller == null) return;
        panel.SetActive(true);

        if (playerController != null)
        {
            playerController.enabled = false;
            Debug.Log("Player controller frozen");
        }

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.sendNavigationEvents = false;
        }

        controller.StartTask();
        Debug.Log("Communication UI shown");
    }

    public void Hide()
    {
        if (panel == null || controller == null) return;
        panel.SetActive(false);

        if (playerController != null)
        {
            playerController.enabled = true;
            Debug.Log("Player controller unfrozen");
        }

        controller.StopTask();

        if (EventSystem.current != null)
        {
            EventSystem.current.sendNavigationEvents = true;
        }

        Debug.Log("Communication UI hidden");
    }

    public void Toggle()
    {
        if (panel == null || controller == null) return;
        if (panel.activeSelf) Hide(); else Show();
    }

    public bool IsVisible => panel != null && panel.activeSelf;
}
