using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerIconController : MonoBehaviour
{
    public Image iconImage;
    public RectTransform iconRectTransform;
    public GameObject[] tilePrefabs; // assign your 6 tile prefabs in inspector

    private RectTransform[] boards;
    private int currentIndex = 0;
    private bool hasClaimed = false;
    private bool puzzleSpawned = false;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction undoAction;

    private Vector3 targetPosition;
    private bool isMoving = false;
    private float moveSpeed = 500f;

    private float moveCooldown = 0.25f;
    private float lastMoveTime = 0f;

    private Transform tileParent;

    private void Awake()
    {
        var playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        undoAction = playerInput.actions["Undo"];

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

        targetPosition = boards[currentIndex].position;
        isMoving = true;
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (boards == null || boards.Length == 0)
            return;

        if (!hasClaimed)
        {
            Image boardImage = boards[currentIndex].GetComponent<Image>();
            boardImage.color = iconImage.color;
            hasClaimed = true;

            Debug.Log($"{gameObject.name} claimed board {currentIndex}");
        }
        else if (!puzzleSpawned)
        {
            Transform boardTransform = boards[currentIndex];
            tileParent = boardTransform.Find("TileParent");
            if (tileParent == null)
            {
                Debug.LogError("TileParent not found inside board. Make sure your Board prefab has a child named 'TileParent'");
                return;
            }

            SpawnPuzzleGrid();
            puzzleSpawned = true;

            iconImage.enabled = false; // 👈 Hides the icon after puzzle spawn

            Debug.Log($"{gameObject.name} spawned puzzle on board {currentIndex}");
        }
    }

    private void OnUndo(InputAction.CallbackContext ctx)
    {
        if (!hasClaimed || puzzleSpawned) // 👈 Prevent undo after puzzle is placed
            return;

        Image boardImage = boards[currentIndex].GetComponent<Image>();
        boardImage.color = Color.white;
        hasClaimed = false;

        Debug.Log($"{gameObject.name} unclaimed board {currentIndex}");
    }

    private void SpawnPuzzleGrid()
    {
        int gridSize = 5;
        float tileSize = 100f;
        float spacing = 10f;
        float totalSize = gridSize * tileSize + (gridSize - 1) * spacing;

        Vector2 startPos = new Vector2(-totalSize / 2 + tileSize / 2, totalSize / 2 - tileSize / 2);

        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                int randomIndex = Random.Range(0, tilePrefabs.Length);
                GameObject tile = Instantiate(tilePrefabs[randomIndex], tileParent);

                RectTransform tileRT = tile.GetComponent<RectTransform>();
                if (tileRT == null)
                {
                    Debug.LogError("Tile prefab missing RectTransform component!");
                    continue;
                }

                tileRT.anchoredPosition = startPos + new Vector2(x * (tileSize + spacing), -y * (tileSize + spacing));
                tileRT.sizeDelta = new Vector2(tileSize, tileSize);
            }
        }
    }
}
