using UnityEngine;

public class CommunicationMinigameTrigger : MonoBehaviour
{
    [SerializeField] private CommunicationMinigameUI uiController;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string playerTag = "Player";
    [Tooltip("Optional: small UI GameObject (e.g. Text) to show when player is near, like 'Press E'.")]
    [SerializeField] private GameObject interactionPrompt;

    private bool playerNearby = false;

    private void Awake()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    private void Update()
    {
        if (playerNearby && uiController != null && Input.GetKeyDown(interactKey))
        {
            uiController.Toggle();
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
