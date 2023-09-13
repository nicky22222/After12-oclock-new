using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpellCastTarget : MonoBehaviour
{
    [SerializeField] private HitAction hitAction = HitAction.None;
    [SerializeField] private string broadcastMsg;
    [Header("Move To Anchor Options")]
    [SerializeField] private GameObject anchor;
    [SerializeField] private float moveTime;
    [SerializeField] private string moveEndBroadcastMsg;

    private Rigidbody rb;
    public bool IsHit { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        IsHit = false;
    }

    public void Hit()
    {
        IsHit = true;
        if (broadcastMsg != "")
        {
            GameProgressManager.Instance.BroadcastMsg(broadcastMsg);
        }
        if (hitAction == HitAction.Drop)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
        else if (hitAction == HitAction.MoveToAnchor)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            StartCoroutine(Snap());
        }
    }

    private IEnumerator Snap()
    {
        var moveStep = (anchor.transform.position - transform.position) / moveTime * Time.fixedDeltaTime;
        var rotateStep = Quaternion.Angle(transform.rotation, anchor.transform.rotation) / moveTime * Time.fixedDeltaTime;
        var snapTime = 0f;
        while (snapTime < moveTime)
        {
            transform.position += moveStep;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, anchor.transform.rotation, rotateStep);
            snapTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        transform.position = anchor.transform.position;
        transform.rotation = anchor.transform.rotation;
        if (moveEndBroadcastMsg != "")
        {
            GameProgressManager.Instance.BroadcastMsg(moveEndBroadcastMsg);
        }
    }
}

public enum HitAction
{
    None,
    Drop,
    MoveToAnchor
}