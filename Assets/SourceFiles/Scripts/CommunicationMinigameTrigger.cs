using UnityEngine;

public class CommunicationMinigameTrigger : MonoBehaviour
{
    [SerializeField] private GameObject uiGameObject;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private GameObject interactionPrompt;

    private bool playerNearby = false;
    private IMinigameUI minigameUI;

    private void Awake()
    {
        // Automatically find IMinigameUI component on the assigned GameObject
        if (uiGameObject != null)
        {
            minigameUI = uiGameObject.GetComponent<IMinigameUI>();
        }

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    private void Update()
    {
        if (playerNearby && minigameUI != null && Input.GetKeyDown(interactKey))
        {
            minigameUI.Toggle();
        }
    }

    // 3D trigger callbacks
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerNearby = true;
            if (interactionPrompt != null) interactionPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerNearby = false;
            if (interactionPrompt != null) interactionPrompt.SetActive(false);
        }
    }
}
