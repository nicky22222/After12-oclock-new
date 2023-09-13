using System;
using System.Collections;
using UnityEngine;

public class GameProgressMoveData : GameProgressData
{
    [SerializeField] private GameObject actor;
    [SerializeField] private GameObject target;
    [SerializeField] private GameProgressMoveSOType moveType;
    [SerializeField] private float actionTime;

    private bool isMoving;
    private bool isRotating;

    public override void Execute()
    {
        base.Execute();
        if (moveType.HasFlag(GameProgressMoveSOType.MoveToTarget))
        {
            GameProgressManager.Instance.StartCoroutine(Move());
        }
        if (moveType.HasFlag(GameProgressMoveSOType.LookAtTarget))
        {
            GameProgressManager.Instance.StartCoroutine(Rotate());
        }
    }

    private IEnumerator Move()
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
        isMoving = false;
    }

    private IEnumerator Rotate()
    {
        isRotating = true;
        var rotateStep = Quaternion.Angle(actor.transform.rotation, target.transform.rotation) / actionTime * Time.fixedDeltaTime;
        var rotateTime = 0f;
        while (rotateTime < actionTime)
        {
            actor.transform.rotation = Quaternion.RotateTowards(actor.transform.rotation, target.transform.rotation, rotateStep);
            rotateTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        isRotating = false;
    }
    
    public override bool IsComplete()
    {
        return !isMoving && !isRotating;
    }
}