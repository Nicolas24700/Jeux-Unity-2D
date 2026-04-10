using System;
using UnityEngine;
using UnityEngine.UI;

public class GazoleMinigameController : MonoBehaviour
{
    [SerializeField] private Image fuelImage;

    [Header("Gameplay")]
    [SerializeField] private float increasePerPress;
    [SerializeField] private float drainPerSecond;
    [SerializeField] private float maxFuel = 100f;

    [Header("Contrôles")]
    [SerializeField] private KeyCode interactKey = KeyCode.F;

    // events for communicating task state and minigame active state to the UI and other systems
    public static event Action<bool> OnTaskStateChanged;
    public static event Action<bool> OnMinigameActiveChanged;

    //state variables
    private float currentFuel = 0f;
    private bool taskCompleted = false;

    private void Start()
    {
        currentFuel = 0f;
        taskCompleted = false;
        fuelImage.fillAmount = 0f;
    }

    private void Update()
    {

        // fuel drain
        if (drainPerSecond > 0f)
        {
            SetFuel(currentFuel - drainPerSecond * Time.deltaTime);
        }

        // add fuel on key press
        if (Input.GetKeyDown(interactKey))
        {
            AddFuel(increasePerPress);
        }
    }

    public void AddFuel(float amount)
    {
        // change the value of the fuel
        SetFuel(currentFuel + amount);
    }

    public void SetFuel(float value)
    {

        // rounding the fuel value and update the UI
        currentFuel = Mathf.Clamp(value, 0f, maxFuel);

        if (fuelImage != null)
            fuelImage.fillAmount = (maxFuel > 0f) ? (currentFuel / maxFuel) : 0f;

        if (currentFuel >= maxFuel)
        {
            taskCompleted = true;
            OnTaskStateChanged?.Invoke(true);
            OnMinigameActiveChanged?.Invoke(false);
            // with invoke , other systems can react to the task completion and minigame deactivation without needing a direct reference to this controller, allowing for better decoupling and modularity in the codebase.
            Debug.Log("✅ Gazole : réservoir plein, tâche terminée.");
        }
    }

    public void StartTask()
    {
        SetFuel(0f);
        OnMinigameActiveChanged?.Invoke(true);
    }
}