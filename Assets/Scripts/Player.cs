using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public class OnSelectedCounterChangedEventArgs : EventArgs
    {
        public ClearCounter selectedCounter;

        public OnSelectedCounterChangedEventArgs()
        {
        }

        public OnSelectedCounterChangedEventArgs(ClearCounter selectedCounter)
        {
            this.selectedCounter = selectedCounter;
        }
    }

    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;

    [SerializeField] private float movementSpeed;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private LayerMask countersLayerMask;

    private float rotateSpeed = 10f;
    private bool isWalking;
    private Vector3 lastInteractDirection;
    private ClearCounter selectedCounter;

    private bool CapsuleCastCheckCollision(Vector3 moveDirection, float moveDistance)
    {
        const float playerRadius = .7f;
        const float playerHeight = 2f;

        bool hasCollision = Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight,
            playerRadius, moveDirection, moveDistance);

        return hasCollision;
    }

    private void Start()
    {
        gameInput.OnInteractAction += GameInputOnInteractAction;
    }

    private void GameInputOnInteractAction(object sender, EventArgs e)
    {
        if (selectedCounter != null)
        {
            selectedCounter.Interact();
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleInteractions();
    }

    public bool IsWalking()
    {
        return isWalking;
    }

    private Vector3 GetMoveDirection()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        return new Vector3(inputVector.x, 0, inputVector.y);
    }

    private void HandleInteractions()
    {
        var moveDirection = GetMoveDirection();
        if (moveDirection != Vector3.zero)
        {
            lastInteractDirection = moveDirection;
        }

        float interactDistance = 2f;
        if (Physics.Raycast(transform.position, lastInteractDirection, out var raycastHit, interactDistance,
                countersLayerMask))
        {
            if (raycastHit.transform.TryGetComponent(out ClearCounter clearCounter))
            {
                if (clearCounter != selectedCounter)
                {
                    selectedCounter = clearCounter;

                    OnSelectedCounterChanged?.Invoke(this,
                        new OnSelectedCounterChangedEventArgs { selectedCounter = selectedCounter });
                }
            }
            else
            {
                selectedCounter = null;
            }
        }
        else
        {
            selectedCounter = null;
        }
    }

    private void HandleMovement()
    {
        float moveDistance = movementSpeed * Time.deltaTime;
        var moveDirection = GetMoveDirection();

        bool canMove = !CapsuleCastCheckCollision(moveDirection, moveDistance);

        if (!canMove)
        {
            // cannot move towards moveDirection 

            // attempt only X movement
            Vector3 moveDirX = new Vector3(moveDirection.x, 0, 0).normalized;
            canMove = !CapsuleCastCheckCollision(moveDirX, moveDistance);

            if (canMove)
            {
                moveDirection = moveDirX;
            }
            else
            {
                // cannot move only on the X
                // attempt only Z movement
                Vector3 moveDirZ = new Vector3(0, 0, moveDirection.z).normalized;
                canMove = !CapsuleCastCheckCollision(moveDirZ, moveDistance);

                if (canMove)
                {
                    moveDirection = moveDirZ;
                }
            }
        }

        if (canMove)
        {
            transform.position += moveDistance * moveDirection;
        }

        isWalking = moveDirection != Vector3.zero;
        transform.forward = Vector3.Slerp(transform.forward, moveDirection, Time.deltaTime * rotateSpeed);
    }
}