using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SpellCaster : MonoBehaviour
{
    [SerializeField]
    private XRGrabInteractable grabInteractable = null;
    [SerializeField]
    private bool isDebugMode;

    [Header("Cast Motion Detection Settings")]
    [SerializeField]
    private int velocityQueueWindow = 10;
    private Queue<float> velocityQueue;
    
    [SerializeField]
    private float spellCastingVelocityStartThreshold = 1f;
    [SerializeField]
    private float spellCastingVelocityEndThreshold = 0.1f;
    
    [Header("Cast Smart Track Settings")]
    [SerializeField]
    private bool isSmartTrackEnabled = true;
    [SerializeField]
    private LayerMask trackingLayerMask;
    [SerializeField]
    private float trackAngle = 10f;
    [SerializeField]
    private float trackDepth = 5f;

    private const int TrackHitBuffer = 100;

    private RaycastHit[] trackHits = new RaycastHit[TrackHitBuffer];

    [Header("Cast General Settings")]
    [SerializeField]
    private float cooldown = 1f;
    [SerializeField]
    private Spell spellPrefab = null;
    
    private float currentCooldown = 0f;
    public bool IsOnCooldown
    {
        get
        {
            return currentCooldown > 0f;
        }
    }
    
    private Vector3 lastPosition;
    private bool isSpellCasting;

    private void Awake()
    {
        if (grabInteractable == null)
        {
            gameObject.SetActive(false);
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        velocityQueue = new Queue<float>(velocityQueueWindow);
        for (var i = 0; i < velocityQueueWindow; i++)
        {
            velocityQueue.Enqueue(0);
        }
        lastPosition = transform.position;
    }

    // FixedUpdate is called once per physics frame
    private void FixedUpdate()
    {
        if (currentCooldown > 0f)
        {
            currentCooldown -= Time.fixedDeltaTime;
        }
        if (grabInteractable.isSelected || isDebugMode)
        {
            var velocity = (transform.position - lastPosition).magnitude;
            velocityQueue.Dequeue();
            velocityQueue.Enqueue(velocity);
            var velocityAverage = GetVelocityAverage();
            // Debug.Log("Velocity Average: " + velocityAverage);
            if (!isSpellCasting && !IsOnCooldown && velocityAverage > spellCastingVelocityStartThreshold)
            {
                isSpellCasting = true;
            }
            else if (isSpellCasting && velocityAverage < spellCastingVelocityEndThreshold)
            {
                Fire();
                currentCooldown = cooldown;
                isSpellCasting = false;
            }
        }
        lastPosition = transform.position;
    }

    private float GetVelocityAverage()
    {
        var average = 0f;
        foreach (var velocity in velocityQueue)
        {
            average += velocity;
        }
        average /= velocityQueueWindow;
        return average;
    }

    private void Fire()
    {
        Debug.Log("Fire!");
        if (isSmartTrackEnabled)
        {
            Array.Clear(trackHits,0, trackHits.Length);
            PhysicsCones.ConeCastNonAlloc(transform.position, transform.forward, trackAngle, trackHits, trackDepth, mask: trackingLayerMask);
            for (int i = 0; i < trackHits.Length; i++)
            {
                var hit = trackHits[i];
                if (hit.collider != null)
                {
                    Debug.Log($"Hit Object {hit.collider.gameObject.name}!");
                    if (spellPrefab != null)
                    {
                        var spellPrefabInstance = Instantiate(spellPrefab, transform.position, transform.rotation);
                        spellPrefabInstance.FireAt(hit.collider.gameObject, hit.point);
                    }
                    return;
                }
            }
        }
        if (spellPrefab != null)
        {
            var spellPrefabInstance = Instantiate(spellPrefab, transform.position, transform.rotation);
            spellPrefabInstance.Fire();
        }
    }
}
