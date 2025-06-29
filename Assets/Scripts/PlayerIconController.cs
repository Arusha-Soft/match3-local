using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerIconController : MonoBehaviour
{
    public Image iconImage;
    public RectTransform iconRectTransform;

    private RectTransform[] boards;
    private int currentIndex = 0;
    private bool hasClaimed = false;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction undoAction;

    //Smooth Move
    private Vector3 targetPosition;
    private bool isMoving = false;
    private float moveSpeed = 500f; // pixels per second, adjust as needed

    private float moveCooldown = 0.25f;  // seconds between moves
    private float lastMoveTime = 0f;

    private void Awake()
    {
        var playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];

        // Get Undo action if it exists in your input asset, else create dynamically
        undoAction = playerInput.actions["Undo"];
        if (undoAction == null)
        {
            // Dynamically create Undo action bound to B button (buttonEast)
            undoAction = playerInput.actions["Jump"].actionMap.AddAction("Undo", binding: "<Gamepad>/buttonEast");
            undoAction.Enable();
        }

        moveAction.performed += OnMove;
        jumpAction.performed += OnJump;
        undoAction.performed += OnUndo;

        moveAction.Enable();
        jumpAction.Enable();
        undoAction.Enable();
    }

    private void OnDestroy()
    {
        moveAction.performed -= OnMove;
        jumpAction.performed -= OnJump;
        undoAction.performed -= OnUndo;
    }
    private void Update()
    {
        if (isMoving)
        {
            iconRectTransform.position = Vector3.MoveTowards(iconRectTransform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(iconRectTransform.position, targetPosition) < 0.1f)
            {
                iconRectTransform.position = targetPosition;
                isMoving = false;
            }
        }
    }
    public void SetColor(Color color)
    {
        iconImage.color = color;
    }

    public void SetBoards(RectTransform[] boardRects)
    {
        boards = boardRects;
        if (boards.Length > 0)
            iconRectTransform.position = boards[0].position;

        //smooth move
        targetPosition = boards[0].position;
        iconRectTransform.position = targetPosition;
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        if (hasClaimed || boards == null || boards.Length == 0)
            return;

        if (Time.time - lastMoveTime < moveCooldown)
            return;

        Vector2 direction = ctx.ReadValue<Vector2>();

        if (direction.x > 0.5f)
        {
            currentIndex = (currentIndex + 1) % boards.Length;
            lastMoveTime = Time.time;
        }
        else if (direction.x < -0.5f)
        {
            currentIndex = (currentIndex - 1 + boards.Length) % boards.Length;
            lastMoveTime = Time.time;
        }

        // iconRectTransform.position = boards[currentIndex].position;
        targetPosition = boards[currentIndex].position;
        isMoving = true;
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (hasClaimed || boards == null || boards.Length == 0)
            return;

        Image boardImage = boards[currentIndex].GetComponent<Image>();
        boardImage.color = iconImage.color;
        hasClaimed = true;

        Debug.Log($"{gameObject.name} claimed board {currentIndex}");
    }

    private void OnUndo(InputAction.CallbackContext ctx)
    {
        if (!hasClaimed)
            return;

        Image boardImage = boards[currentIndex].GetComponent<Image>();
        boardImage.color = Color.white; // Reset to default color (adjust if needed)
        hasClaimed = false;

        Debug.Log($"{gameObject.name} unclaimed board {currentIndex}");
    }
}
