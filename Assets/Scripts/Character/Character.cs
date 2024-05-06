using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{

    private CharacterAnimator animator;
    public float moveSpeed;
    public Vector3 lookingAt;

    public bool IsMoving { get; set; }
    public float OffsetY { get; private set; } = 0.3f;

    private void Awake()
    {

        animator = GetComponent<CharacterAnimator>();
        SetPositionAndSnapToTile(transform.position);
    }

    public void SetPositionAndSnapToTile(Vector2 pos)
    {
        pos.x = Mathf.Floor(pos.x) + 0.5f;
        pos.y = Mathf.Floor(pos.y) + 0.8f;

        transform.position = pos;
    }

    public CharacterAnimator Animator
    {

        get { return animator; }
    }

    public IEnumerator Move(Vector3 moveVector, Action OnMoveOver = null)
    {

        animator.MoveX = Mathf.Clamp(moveVector.x, -1f, 1f);
        animator.MoveY = Mathf.Clamp(moveVector.y, -1f, 1f);

        lookingAt = new Vector3(Mathf.Clamp(moveVector.x, -1f, 1f),  Mathf.Clamp(moveVector.y, -1f, 1f));

        var targetPos = transform.position;
        targetPos.x += moveVector.x;
        targetPos.y += moveVector.y;


        var dir = (targetPos - transform.position).normalized;

        var nextPos = transform.position + dir;
        // this new vector points at the target's feet and not its center/face
        if (!IsWalkable(nextPos))
            yield break;

        IsMoving = true;
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            nextPos = transform.position + dir;
            if (IsWalkable(nextPos))
            {
                while ((nextPos - transform.position).sqrMagnitude > Mathf.Epsilon)
                {
                    transform.position = Vector3.MoveTowards(transform.position, nextPos, moveSpeed * Time.deltaTime);
                    yield return null;
                }
                transform.position = nextPos;
            }
            else
            {
                IsMoving = false;

                OnMoveOver?.Invoke();

                yield break;
            }

        }
        transform.position = targetPos;

        IsMoving = false;

        OnMoveOver?.Invoke();
    }

    public void HandleUpdate()
    {

        animator.IsMoving = IsMoving;
    }

    // Keep this as it might be useful
    private bool IsPathClear(Vector3 targetPos)
    {
        var diff = targetPos - transform.position;
        var dir = diff.normalized;

        return !Physics2D.BoxCast(transform.position + dir, new Vector2(0.1f, 0.1f), 0, dir, diff.magnitude - 1,
        GameLayers.Instance.SolidObjects | GameLayers.Instance.InteractableLayer | GameLayers.Instance.PlayerLayer);
    }

    public void LookTowards(Vector3 targetPos)
    {

        var xdiff = Mathf.Floor(targetPos.x) - Mathf.Floor(transform.position.x);
        var ydiff = Mathf.Floor(targetPos.y) - Mathf.Floor(transform.position.y);

        if (xdiff == 0 || ydiff == 0)
        {
            animator.MoveX = Mathf.Clamp(xdiff, -1f, 1f);
            animator.MoveY = Mathf.Clamp(ydiff, -1f, 1f);
            
        }
        else
            Debug.Log("Error in LookTowards: Can't look diagonally");
    }

    private bool IsWalkable(Vector3 targetPos)
    {

        if (Physics2D.OverlapCircle(targetPos, 0.1f, GameLayers.Instance.Collision | GameLayers.Instance.InteractableLayer | GameLayers.Instance.PlayerLayer) != null)
            return false;

        return true;
    }
}
