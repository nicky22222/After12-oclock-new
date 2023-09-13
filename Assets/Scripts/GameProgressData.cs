using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct KeyObjectPair
{
    public string key;
    public GameObject target;
}

[Serializable]
public class GameProgressData
{
    [SerializeField] private Animator animator;
    [SerializeField] private string animationTriggerName;
    [SerializeField] private GameObject actor;
    [SerializeField] private bool isActorActive = true;
    [SerializeField] private GameProgressMoveSOType moveType;
    [SerializeField] private float actionTime;
    
    [Header("General")]
    [SerializeField] private GameObject target;

    [Header("WaitForMsgToShowObject")]
    [SerializeField] private List<KeyObjectPair> waitMsgs;

    [Header("SetSkybox")]
    [SerializeField] private Material skybox;

    private bool isMoving;
    private bool isRotating;
    private bool isWaiting;

    private Dictionary<string, GameObject> _waitMsgs;

    public virtual void Execute()
    {
        actor.SetActive(isActorActive);
        if (animator != null && !string.IsNullOrEmpty(animationTriggerName))
        {
            Debug.Log($"GameProgressSO set animator trigger {animationTriggerName}");
            animator.SetTrigger(animationTriggerName);
        }
        if (moveType.HasFlag(GameProgressMoveSOType.MoveToTarget))
        {
            var isSettingParent = moveType.HasFlag(GameProgressMoveSOType.SetParent);
            GameProgressManager.Instance.StartCoroutine(Move(isSettingParent));
        }
        if (moveType.HasFlag(GameProgressMoveSOType.LookAtTarget))
        {
            var lookAngle = Quaternion.LookRotation(target.transform.position - actor.transform.position);
            GameProgressManager.Instance.StartCoroutine(Rotate(lookAngle));
        }
        else if (moveType.HasFlag(GameProgressMoveSOType.SetCopyRotation))
        {
            GameProgressManager.Instance.StartCoroutine(Rotate(target.transform.rotation));
        }
        if (moveType.HasFlag(GameProgressMoveSOType.WaitForTargetHit))
        {
            GameProgressManager.Instance.StartCoroutine(WaitTargetHit());
        }
        if (moveType.HasFlag(GameProgressMoveSOType.SetSkybox))
        {
            GameProgressManager.Instance.SetSkybox(skybox);
        }
        if (moveType.HasFlag(GameProgressMoveSOType.WaitForMsgToShowObject))
        {
            WaitMsgs(waitMsgs);
        }
        if (moveType.Equals(GameProgressMoveSOType.None))
        {
            GameProgressManager.Instance.StartCoroutine(Wait());
        }
    }

    public virtual bool IsComplete()
    {
        return !isMoving && !isRotating && !isWaiting;
    }

    private IEnumerator Move(bool isSettingParent = false)
    {
        isMoving = true;
        var moveStep = (target.transform.position - actor.transform.position) / actionTime * Time.fixedDeltaTime;
        var moveTime = 0f;
        while (moveTime < actionTime)
        {
            actor.transform.position += moveStep;
            moveTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        actor.transform.position = target.transform.position;
        if (isSettingParent)
        {
            actor.transform.SetParent(target.transform);
        }
        isMoving = false;
    }

    private IEnumerator Rotate(Quaternion finalRotation)
    {
        isRotating = true;
        var rotateStep = Quaternion.Angle(actor.transform.rotation, finalRotation) / actionTime * Time.fixedDeltaTime;
        var rotateTime = 0f;
        while (rotateTime < actionTime)
        {
            actor.transform.rotation = Quaternion.RotateTowards(actor.transform.rotation, finalRotation, rotateStep);
            rotateTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        actor.transform.rotation = finalRotation;
        isRotating = false;
    }

    private IEnumerator WaitTargetHit()
    {
        isWaiting = true;
        if (target.TryGetComponent<SpellCastTarget>(out var spellCastTarget))
        {
            while (true)
            {
                if (spellCastTarget.IsHit)
                {
                    break;
                }
                yield return new WaitForFixedUpdate();
            }
        }
        else
        {
            Debug.LogError($"Target {target} has no spell cast component!");
        }
        yield return new WaitForSeconds(actionTime);
        isWaiting = false;
    }

    private void WaitMsgs(List<KeyObjectPair> msgs)
    {
        if (msgs.Count == 0) return;
        isWaiting = true;
        _waitMsgs = new Dictionary<string, GameObject>();
        foreach (var m in msgs)
        {
            _waitMsgs[m.key] = m.target;
        }
    }

    public void BroadcastMsg(string msg)
    {
        if (_waitMsgs != null && _waitMsgs.ContainsKey(msg))
        {
            _waitMsgs[msg].SetActive(true);
            _waitMsgs.Remove(msg);
            if (_waitMsgs.Count == 0)
            {
                isWaiting = false;
            }
        }
    }

    private IEnumerator Wait()
    {
        isWaiting = true;
        yield return new WaitForSeconds(actionTime);
        isWaiting = false;
    }
}

[Flags]
public enum GameProgressMoveSOType // In binary
{
    None = 0,
    MoveToTarget = 1,
    LookAtTarget = 2,
    WaitForTargetHit = 4,
    WaitForMsgToShowObject = 8,
    SetParent = 16,
    SetSkybox = 32,
    SetCopyRotation = 64,
}