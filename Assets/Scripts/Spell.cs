using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Spell : MonoBehaviour
{
    [SerializeField]
    private MuzzleFlashVFX muzzleFlash = null;
    
    private Rigidbody rb;
    private Collider col;
    
    [SerializeField]
    private float speed = 1f;
    [SerializeField]
    private float maxLifetime = 3f;
    private float currentLifetime = 0f;
    private bool isFired = false;
    
    [SerializeField]
    private List<string> ignoreTags = new List<string>();

    private GameObject specificTarget = null;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public void SetIgnoreTags(List<string> tags)
    {
        ignoreTags = tags;
    }
    
    public void InitParams(float newSpeed, float newMaxLifetime)
    {
        speed = newSpeed;
        maxLifetime = newMaxLifetime;
    }

    public void Fire()
    {
        rb.velocity = transform.forward * speed;
        isFired = true;
    }
    
    public void FireAt(GameObject target, Vector3 targetPoint)
    {
        var direction = targetPoint - transform.position;
        rb.velocity = direction.normalized * speed;
        isFired = true;
        specificTarget = target;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (ignoreTags.Contains(other.gameObject.tag))
            return;
        
        if (specificTarget != null && other.gameObject != specificTarget)
            return;

        Debug.Log($"Hit Object {other.gameObject.name}!");
        if (muzzleFlash != null)
        {
            var muzzleFlashInstance = Instantiate(muzzleFlash, other.GetContact(0).point, Quaternion.identity);
            muzzleFlashInstance.Play();
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (ignoreTags.Contains(other.gameObject.tag))
            return;
        
        if (specificTarget != null && other.gameObject != specificTarget)
            return;

        other.GetComponent<SpellCastTarget>()?.Hit();

        Debug.Log($"Hit Object {other.gameObject.name}!");
        if (muzzleFlash != null)
        {
            var muzzleFlashInstance = Instantiate(muzzleFlash, transform.position, Quaternion.identity);
            muzzleFlashInstance.Play();
        }
        Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        if (!isFired) return;
        
        currentLifetime += Time.fixedDeltaTime;
        if (currentLifetime >= maxLifetime)
        {
            Destroy(gameObject);
        }
    }
}
