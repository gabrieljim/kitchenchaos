using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float movementSpeed;
    [SerializeField] private GameInput gameInput;
    private float rotateSpeed = 10f;
    private bool isWalking;

    private bool CapsuleCastCheckCollision(Vector3 moveDirection, float moveDistance)
    {
        const float playerRadius = .7f;
        const float playerHeight = 2f;
        
        bool hasCollision = Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight,
            playerRadius, moveDirection, moveDistance);

        return hasCollision;
    }

    private void Update()
    {
        float moveDistance = movementSpeed * Time.deltaTime;
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        Vector3 moveDirection = new Vector3(inputVector.x, 0, inputVector.y);

        bool canMove = !CapsuleCastCheckCollision(moveDirection,  moveDistance);

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

    public bool IsWalking()
    {
        return isWalking;
    }
}