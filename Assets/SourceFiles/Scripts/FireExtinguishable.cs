using System.Collections.Generic;
using UnityEngine;

public class FireExtinguishable : MonoBehaviour
{
    [Header("Fire Health")]
    [SerializeField] private float maxFireStrength = 100f;
    [SerializeField] private float extinguishPerParticleHit = 1f;

    [Header("Destroy")]
    [SerializeField] private GameObject objectToDestroy;
    [SerializeField] private float destroyDelay = 0.1f;

    private float currentFireStrength;
    private bool isExtinguished;
    private readonly List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

    private void Awake()
    {
        // fire = max - current so when current is 0, fire is max, and when current is max, fire is 0
        currentFireStrength = Mathf.Max(1f, maxFireStrength);
    }


// ---------------------------
    private void OnParticleCollision(GameObject other)
    {
        if (isExtinguished)
            return;

        int hitCount = 1;
        // create a list of particle collision events and get the number of hits from the particle system
        ParticleSystem particleSystem = other.GetComponent<ParticleSystem>();
        if (particleSystem != null)
        {
            // for each particle collision event, check if the collided object is this fire and count the hits
            hitCount = particleSystem.GetCollisionEvents(gameObject, collisionEvents);
            if (hitCount <= 0)
                hitCount = 1;
        }

        float extinguishAmount = hitCount * extinguishPerParticleHit;
        ApplyExtinguish(extinguishAmount);
    }

    public void ApplyExtinguish(float amount)
    {
        // apply extinguishing to the fire, if the fire is already extinguished or the amount is 0.
        if (isExtinguished || amount <= 0f)
            return;

        currentFireStrength = Mathf.Max(0f, currentFireStrength - amount);

        if (currentFireStrength <= 0f)
            ExtinguishCompletely();
    }

    private void ExtinguishCompletely()
    {
        isExtinguished = true;

        // Remove extincteur from player if fire is extinguished.
        UniqueItemInteractionTrigger trigger = Object.FindFirstObjectByType<UniqueItemInteractionTrigger>();
        if (trigger != null && trigger.EquippedItemInstance != null)
            trigger.RemoveEquippedItem();

        Destroy(objectToDestroy, destroyDelay);
    }

    private void MakeObjectInvisible()
    {
        // Disable all renderers and colliders of the object to make it invisible and non-interactable
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer rendererComponent in renderers)
            rendererComponent.enabled = false;

        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        foreach (Collider colliderComponent in colliders)
            colliderComponent.enabled = false;
    }
}
