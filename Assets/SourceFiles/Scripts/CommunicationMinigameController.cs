using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class CommunicationMinigameController : MonoBehaviour
{

    // all the serialized fields for the minigame configuration and references
    [Header("Jauge de fréquence")]
    [SerializeField] private Slider frequencySlider;
    [SerializeField] private Image sliderFillImage;
    [SerializeField] private float minFrequency = 0f;
    [SerializeField] private float maxFrequency = 100f;
    [SerializeField] private float currentFrequency = 50f;
    [SerializeField] private float continuousSpeed = 40f;

    [Header("Zones de succès")]
    [SerializeField] private float successZoneMin = 40f;
    [SerializeField] private float successZoneMax = 60f;
    [SerializeField] private float successZoneWidth = 5f;
    [SerializeField] private Color successColor = Color.green;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color failColor = Color.red;

    [Header("Feedback visuel")]
    [SerializeField] private List<Image> indicatorLights = new List<Image>(); 
    [SerializeField] private AudioSource feedbackAudio;
    [SerializeField] private AudioClip successClip;
    [SerializeField] private AudioClip warningClip;
    [SerializeField] private AudioClip failClip;

    [Header("Paramètres")]
    //Dwell time equal to how long the player needs to keep the slider in the green zone to complete an indicator.
    [SerializeField] private float requiredDwellTime;

    [Header("Contrôles clavier")]
    [SerializeField] private KeyCode increaseKey = KeyCode.RightArrow;
    [SerializeField] private KeyCode decreaseKey = KeyCode.LeftArrow;

    // internal timer while staying in success zone
    private float dwellTimeCounter = 0f;
    private bool taskCompleted = false;

    // index of the current indicator we are solving (0..5)
    private int currentIndicatorIndex = 0;
    // array of bools to know which indicators are done
    private bool[] indicatorCompleted;
    // when true we are running the advance coroutine
    private bool isAdvancing = false;

    // events for UI and player controller to listen to
    public static event Action<bool> OnTaskStateChanged;
    public static event Action<bool> OnMinigameActiveChanged;

    // Pick a random green zone inside min..max with width = successZoneWidth.
    private void SetRandomSuccessZone()
    {
        // calcul of random succes with clamp to avoid impossible zones
        float w = Mathf.Clamp(successZoneWidth, 1f, Mathf.Max(1f, maxFrequency - minFrequency));
        float minStart = minFrequency;
        float maxStart = maxFrequency - w;
        float start = UnityEngine.Random.Range(minStart, maxStart);
        successZoneMin = start;
        successZoneMax = start + w;
    }

    // Unity start: set slider min/max and show visuals.
    private void Start()
    {
        if (frequencySlider != null)
        {
            frequencySlider.minValue = minFrequency;
            frequencySlider.maxValue = maxFrequency;
            frequencySlider.value = currentFrequency;
        }

        UpdateSliderVisuals();
    }


    private void Update()
    {
        // only run when active and not finished and not already advancing
        if ( taskCompleted || isAdvancing)
            return;

        // read keyboard input
        HandleKeyboardInput();

        // if the slider is in the green zone, increase the dwell counter
        if (IsInSuccessZone())
        {
            dwellTimeCounter += Time.deltaTime;

            if (dwellTimeCounter >= requiredDwellTime)
            {
                // start the coroutine that marks indicator done and moves on
                StartCoroutine(HandleIndicatorComplete());
            }
        }
        else
        {
            // if we left the green zone, reset the counter
            if (dwellTimeCounter > 0f)
            dwellTimeCounter = 0f;
        }
    }

    // Very simple keyboard handling: hold right to increase, left to decrease.
    private void HandleKeyboardInput()
    {
        if (Input.GetKey(increaseKey))
        {
            ContinuousIncrease();
        }

        if (Input.GetKey(decreaseKey))
        {
            ContinuousDecrease();
        }
    }

    // Increase the frequency smoothly when holding key.
    private void ContinuousIncrease()
    {
        currentFrequency = Mathf.Clamp(currentFrequency + continuousSpeed * Time.deltaTime, minFrequency, maxFrequency);
        if (frequencySlider != null) frequencySlider.value = currentFrequency;
        UpdateSliderVisuals();
    }

    // Decrease the frequency smoothly when holding key.
    private void ContinuousDecrease()
    {
        currentFrequency = Mathf.Clamp(currentFrequency - continuousSpeed * Time.deltaTime, minFrequency, maxFrequency);
        if (frequencySlider != null) frequencySlider.value = currentFrequency;
        UpdateSliderVisuals();
    }

    // Coroutine that runs when an indicator is completed.
    // It marks the current indicator green, plays a sound, waits a bit.
    private IEnumerator HandleIndicatorComplete()
    {
        // yield break for stopping immedialty if we are already advancing to avoid double triggers
        if (isAdvancing) yield break;
        isAdvancing = true;

        if (indicatorLights == null || indicatorLights.Count == 0)
        {
            // if no indicators assigned, just complete the task immediately
            CompleteTask();
            isAdvancing = false;
            yield break;
        }

        if (indicatorCompleted == null || indicatorCompleted.Length != indicatorLights.Count)
            indicatorCompleted = new bool[indicatorLights.Count];

        // mark this indicator as done
        indicatorCompleted[currentIndicatorIndex] = true;

        // set the image color to green so player sees it solved
        var completedImg = indicatorLights[currentIndicatorIndex];
        if (completedImg != null) completedImg.color = successColor;

        // play success sound if available
        if (feedbackAudio != null && successClip != null)
            feedbackAudio.PlayOneShot(successClip, 0.8f);

        // reset the dwell timer so we don't retrigger
        dwellTimeCounter = 0f;

        // update visuals (slider and indicators)
        UpdateSliderVisuals();

        // count completed lights
        int completedCount = 0;
        for (int i = 0; i < indicatorCompleted.Length; i++) if (indicatorCompleted[i]) completedCount++;

        // if all done, finish the minigame
        if (completedCount >= indicatorLights.Count)
        {
            CompleteTask();
            isAdvancing = false;
            yield break;
        }

        // go to next indicator
        currentIndicatorIndex = Mathf.Clamp(currentIndicatorIndex + 1, 0, indicatorLights.Count - 1);

        // make sure next indicator is dim so it's not already green
        var nextImg = indicatorLights[currentIndicatorIndex];
        Color dimColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        if (nextImg != null) nextImg.color = dimColor;

        // pick a new random green zone for the next indicator
        SetRandomSuccessZone();
        // put the slider value outside the new green zone so player must adjust
        float outside = successZoneMin - (successZoneMax - successZoneMin) - 5f;
        if (outside < minFrequency) outside = Mathf.Clamp(successZoneMax + (successZoneMax - successZoneMin) + 5f, minFrequency, maxFrequency);
        currentFrequency = outside;
        if (frequencySlider != null) frequencySlider.value = currentFrequency;
        UpdateSliderVisuals();

        isAdvancing = false;
    }


    // Set frequency from outside (UI buttons maybe)
    public void SetFrequency(float value)
    {
        currentFrequency = Mathf.Clamp(value, minFrequency, maxFrequency);
        if (frequencySlider != null) frequencySlider.value = currentFrequency;
        UpdateSliderVisuals();
    }

    // Check if current frequency is inside current green zone
    private bool IsInSuccessZone()
    {
        return currentFrequency >= successZoneMin && currentFrequency <= successZoneMax;
    }

    // Update the slider fill color and indicator lights based on state
    private void UpdateSliderVisuals()
    {
        // decide color: green if in success zone, yellow if near, red if far
        Color baseColor;
        if (IsInSuccessZone()) baseColor = successColor;
        else if (Mathf.Abs(currentFrequency - (successZoneMin + successZoneMax) / 2f) < 15f) baseColor = warningColor;
        else baseColor = failColor;

        // change slider fill color
        if (sliderFillImage != null)
            sliderFillImage.color = baseColor;

        // update the array of indicator lights
        if (indicatorLights != null && indicatorLights.Count > 0)
        {
            // ensure array exists
            if (indicatorCompleted == null || indicatorCompleted.Length != indicatorLights.Count)
                indicatorCompleted = new bool[indicatorLights.Count];

            for (int i = 0; i < indicatorLights.Count; i++)
            {
                var img = indicatorLights[i];
                if (img == null) continue;

                if (indicatorCompleted[i])
                {
                    // already solved -> green
                    img.color = successColor;
                }
                else if (i == currentIndicatorIndex)
                {
                    // the active indicator shows the slider color
                    img.color = baseColor;
                }
                else
                {
                    // other indicators are dim
                    img.color = new Color(0.2f, 0.2f, 0.2f, 1f);
                }
            }
        }
    }

    // Play audio feedback based on slider distance to success zone
    private void PlayFeedback()
    {
        if (feedbackAudio == null)
            return;

        if (IsInSuccessZone())
        {
            feedbackAudio.PlayOneShot(successClip, 0.5f);
        }
        else if (Mathf.Abs(currentFrequency - (successZoneMin + successZoneMax) / 2f) < 15f)
        {
            feedbackAudio.PlayOneShot(warningClip, 0.3f);
        }
        else
        {
            feedbackAudio.PlayOneShot(failClip, 0.2f);
        }
    }

    // Called when all indicators are completed
    private void CompleteTask()
    {
        taskCompleted = true;
        if (feedbackAudio != null && successClip != null) feedbackAudio.PlayOneShot(successClip, 1f);
        OnTaskStateChanged?.Invoke(true);
        OnMinigameActiveChanged?.Invoke(false);

        // set all lights green just in case
        if (indicatorLights != null)
        {
            for (int i = 0; i < indicatorLights.Count; i++)
            {
                if (indicatorLights[i] != null) indicatorLights[i].color = successColor;
            }
        }
    }

    // Start the minigame, initialize values and randomize first success zone
    public void StartTask()
    {
        taskCompleted = false;
        dwellTimeCounter = 0f;

        // initialize indicators
        if (indicatorLights != null && indicatorLights.Count > 0)
        {
            indicatorCompleted = new bool[indicatorLights.Count];
            currentIndicatorIndex = 0;
            // ensure all indicator images start dim
            for (int i = 0; i < indicatorLights.Count; i++)
            {
                if (indicatorLights[i] != null) indicatorLights[i].color = new Color(0.2f, 0.2f, 0.2f, 1f);
            }
        }

        // pick a random green zone and set slider outside it so player must move
        SetRandomSuccessZone();
        currentFrequency = Mathf.Clamp(successZoneMin - successZoneWidth - 5f, minFrequency, maxFrequency);
        if (frequencySlider != null) frequencySlider.value = currentFrequency;
        UpdateSliderVisuals();
        OnMinigameActiveChanged?.Invoke(true);

    }
}