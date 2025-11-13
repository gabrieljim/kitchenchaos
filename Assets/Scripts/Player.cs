using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float movementSpeed;
    private float rotateSpeed = 10f;
    private bool isWalking;
    
    private void Update()
    {
        Vector2 inputVector = new Vector2(0, 0);
        if (Input.GetKey(KeyCode.W))
        {
            inputVector.y += 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            inputVector.y -= 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            inputVector.x -= 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            inputVector.x += 1;
        }
        
        inputVector = inputVector.normalized;
        Vector3 moveDirection = new Vector3(inputVector.x, 0, inputVector.y);
        isWalking = moveDirection != Vector3.zero;
        transform.forward = Vector3.Slerp(transform.forward, moveDirection, Time.deltaTime * rotateSpeed);
        transform.position += movementSpeed * Time.deltaTime * moveDirection;
    }

    public bool IsWalking()
    {
        return isWalking;
    }
}
