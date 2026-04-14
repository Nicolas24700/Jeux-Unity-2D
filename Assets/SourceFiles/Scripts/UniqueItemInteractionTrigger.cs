using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniqueItemInteractionTrigger : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private KeyCode sprayKey = KeyCode.F;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private GameObject interactionPrompt;

    [Header("the Item")]
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Transform handSocket;
    [SerializeField] private Vector3 localPositionOffset = Vector3.zero;
    [SerializeField] private Vector3 localRotationOffset = Vector3.zero;
    [SerializeField] private Vector3 localScale = Vector3.one;
    // private string sprayParticleObjectName = "SprayFX";

    private bool playerNearby;
    private bool hasGivenItem;
    private GameObject equippedItemInstance;
    private ParticleSystem sprayParticles;

    private void Update()
    {
        HandleSprayInput();

        // check if the player is nearby, hasn't already received the item, and has pressed the interact key to give the item
        if (!playerNearby || hasGivenItem || !Input.GetKeyDown(interactKey))
            return;

        TryGiveItem();
    }

    private void TryGiveItem()
    {
        
        //equip the item and set position/scale and rotation
        equippedItemInstance = Instantiate(itemPrefab, handSocket);
        equippedItemInstance.transform.localPosition = localPositionOffset;
        equippedItemInstance.transform.localRotation = Quaternion.Euler(localRotationOffset);
        equippedItemInstance.transform.localScale = localScale;

        CacheSprayParticles();

        hasGivenItem = true;

    }

    private void CacheSprayParticles()
    {
        // script for hide the spray particles
        //sprayParticles = the child particle system of the equipped item with the name "SprayFX" 
        sprayParticles = equippedItemInstance.GetComponentInChildren<ParticleSystem>(true);

        // Force World space so particles don't follow the player's movement
        var main = sprayParticles.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        // force stop the particles from playing when the item is given to the player
        sprayParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void HandleSprayInput()
    {
        // if the player hasn't received the item yet or if the spray particles haven't been cached, do nothing
        if (!hasGivenItem || sprayParticles == null)
            return;

        bool spraying = Input.GetKey(sprayKey);

        // play the spray particles if the player is holding the spray key, and stop them if they are not
        if (spraying && !sprayParticles.isPlaying)
            sprayParticles.Play();

        // stop the spray particles if the player is not holding the spray key, and apply the spray effect to any fires in front of the player
        if (!spraying && sprayParticles.isPlaying)
            sprayParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        if (spraying)
            ApplySprayToFires();
    }

    private void ApplySprayToFires()
    {

        Transform sprayOrigin = sprayParticles.transform;
        Vector3 origin = sprayOrigin.position;
        Vector3 direction = sprayOrigin.forward;

        // SphereCast in front of the player to find any fires within range of the spray, and apply the extinguish effect to them
        Ray ray = new Ray(origin, direction);
        RaycastHit[] hits = Physics.SphereCastAll(ray, 0.75f, 5f);

        foreach (RaycastHit hit in hits)
        {
            // check if the hit object has a FireExtinguishable component in its parent objects and if so apply the extinguish effect to it
            FireExtinguishable fire = hit.collider.GetComponentInParent<FireExtinguishable>();
            if (fire != null)
            // apply the extinguish effect to the fire, using a value of 40f multiplied by Time.deltaTime to ensure consistent extinguishing regardless of frame rate
            fire.ApplyExtinguish(40f * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag) || hasGivenItem)
            return;

        playerNearby = true;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerNearby = false;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

// save the state of the unique item interaction trigger so that it can be loaded later
    public bool HasGivenItem => hasGivenItem;
    public GameObject EquippedItemInstance => equippedItemInstance;

    public void RemoveEquippedItem()
    {
        //if the player finish to extinguish the fire, we remove the equipped item from the player
        if (equippedItemInstance != null)
            Destroy(equippedItemInstance);

        equippedItemInstance = null;
        sprayParticles = null;
        hasGivenItem = false;
    }
}
