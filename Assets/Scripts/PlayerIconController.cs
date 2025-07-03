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
    private bool inPuzzleMode = false;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction undoAction;

    private Vector3 targetPosition;
    private bool isMoving = false;
    private float moveSpeed = 500f;

    private float moveCooldown = 0.25f;
    private float lastMoveTime = 0f;

    private Transform tileParent;
    private GameObject[,] tileGrid = new GameObject[5, 5];
    private int cursorX = 0;
    private int cursorY = 0;

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
        {
            iconRectTransform.position = boards[0].position;
            targetPosition = boards[0].position;
        }
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        if (Time.time - lastMoveTime < moveCooldown)
            return;

        Vector2 direction = ctx.ReadValue<Vector2>();
        lastMoveTime = Time.time;

        if (!hasClaimed || !puzzleSpawned)
        {
            if (boards == null || boards.Length == 0) return;

            if (direction.x > 0.5f)
                currentIndex = (currentIndex + 1) % boards.Length;
            else if (direction.x < -0.5f)
                currentIndex = (currentIndex - 1 + boards.Length) % boards.Length;

            targetPosition = boards[currentIndex].position;
            isMoving = true;
        }
        else if (inPuzzleMode)
        {
            if (direction.x > 0.5f)
                cursorX = (cursorX + 1) % 5;
            else if (direction.x < -0.5f)
                cursorX = (cursorX - 1 + 5) % 5;

            if (direction.y > 0.5f)
                cursorY = (cursorY - 1 + 5) % 5;
            else if (direction.y < -0.5f)
                cursorY = (cursorY + 1) % 5;

            UpdateSelector();
        }
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
            inPuzzleMode = true;

            iconImage.enabled = false;

            Debug.Log($"{gameObject.name} spawned puzzle on board {currentIndex}");
        }
    }

    private void OnUndo(InputAction.CallbackContext ctx)
    {
        if (!hasClaimed || puzzleSpawned)
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

                tileGrid[x, y] = tile;
            }
        }

        cursorX = 0;
        cursorY = 0;
        UpdateSelector();
    }

    private void UpdateSelector()
    {
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                GameObject tile = tileGrid[x, y];
                if (tile == null) continue;

                Transform selector = tile.transform.Find("Selector");
                if (selector != null)
                {
                    selector.gameObject.SetActive(x == cursorX && y == cursorY);
                }
            }
        }
    }
}
