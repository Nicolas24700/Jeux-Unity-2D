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
            if (minigameUI == null)
            {
                Debug.LogError($"{name}: {uiGameObject.name} does not have any component implementing IMinigameUI interface.");
            }
        }
        else
        {
            Debug.LogError($"{name}: uiGameObject is not assigned.");
        }

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    private void Update()
    {
        if (playerNearby && minigameUI != null && Input.GetKeyDown(interactKey))
        {
            minigameUI.Toggle();
            Debug.Log("Interaction E: toggled UI.");
        }
    }

    // 3D trigger callbacks
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerNearby = true;
            if (interactionPrompt != null) interactionPrompt.SetActive(true);
            Debug.Log("Player entered communication trigger (3D).");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerNearby = false;
            if (interactionPrompt != null) interactionPrompt.SetActive(false);
            Debug.Log("Player exited communication trigger (3D).");
        }
    }
}
