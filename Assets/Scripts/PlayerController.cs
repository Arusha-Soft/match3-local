using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //private CharacterController controller;

    private Vector2 moveInput;
    private Vector3 test;

    //private void Awake()
    //{
    //    controller = GetComponent<CharacterController>();
    //}

    public void Move(InputAction.CallbackContext context)
    {
        moveInput=context.ReadValue<Vector2>();
        Debug.Log("Move");
    }
    public void Jump(InputAction.CallbackContext context)
    {
        if(context.performed)// && controller.isGrounded)
        {
            Debug.Log("Jump");
        }
    }

}
