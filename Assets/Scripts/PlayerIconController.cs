using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerIconController : MonoBehaviour
{
    public Image iconImage;
    public RectTransform iconRectTransform;
    public GameObject[] tilePrefabs;

    private RectTransform[] boards;
    private int currentIndex = 0;
    private bool hasClaimed = false;
    private bool puzzleSpawned = false;
    private bool inPuzzleMode = false;
    private bool inEditMode = false;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction undoAction;

    private Vector3 targetPosition;
    private bool isMoving = false;
    private float moveSpeed = 500f;

    private float moveCooldown = 0.2f;
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
        if (Time.time - lastMoveTime < moveCooldown) return;
        lastMoveTime = Time.time;

        Vector2 direction = ctx.ReadValue<Vector2>();

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
            if (!inEditMode)
            {
                // Just move selector cursor
                if (direction.x > 0.5f)
                    cursorX = (cursorX + 1) % 5;
                else if (direction.x < -0.5f)
                    cursorX = (cursorX - 1 + 5) % 5;
                else if (direction.y > 0.5f)
                    cursorY = (cursorY - 1 + 5) % 5;
                else if (direction.y < -0.5f)
                    cursorY = (cursorY + 1) % 5;

                UpdateSelector();
            }
            else
            {
                // In edit mode: roll row/column of current cursor position
                if (direction.x > 0.5f)
                    RollRow(cursorY, true);
                else if (direction.x < -0.5f)
                    RollRow(cursorY, false);
                else if (direction.y > 0.5f)
                    RollColumn(cursorX, false);
                else if (direction.y < -0.5f)
                    RollColumn(cursorX, true);
            }
        }
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (boards == null || boards.Length == 0) return;

        if (!hasClaimed)
        {
            Image boardImage = boards[currentIndex].GetComponent<Image>();
            boardImage.color = iconImage.color;
            hasClaimed = true;
        }
        else if (!puzzleSpawned)
        {
            Transform boardTransform = boards[currentIndex];
            tileParent = boardTransform.Find("TileParent");
            if (tileParent == null)
            {
                Debug.LogError("TileParent not found");
                return;
            }

            SpawnPuzzleGrid();
            puzzleSpawned = true;
            inPuzzleMode = true;
            iconImage.enabled = false;
        }
        else if (inPuzzleMode && !inEditMode)
        {
            // Enter edit mode for current tile
            inEditMode = true;
        }
    }

    private void OnUndo(InputAction.CallbackContext ctx)
    {
        if (inPuzzleMode && inEditMode)
        {
            // Exit edit mode
            inEditMode = false;
            Debug.Log("Exited edit mode");
            return;
        }

        if (!hasClaimed || puzzleSpawned)
            return;

        Image boardImage = boards[currentIndex].GetComponent<Image>();
        boardImage.color = Color.white;
        hasClaimed = false;
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
                var tile = tileGrid[x, y];
                Transform selector = tile.transform.Find("Selector");
                if (selector)
                    selector.gameObject.SetActive(x == cursorX && y == cursorY);
            }
        }
    }

    private void RollRow(int rowIndex, bool toRight)
    {
        Sprite[] rowSprites = new Sprite[5];
        for (int x = 0; x < 5; x++)
            rowSprites[x] = tileGrid[x, rowIndex].GetComponent<Image>().sprite;

        for (int x = 0; x < 5; x++)
        {
            int from = toRight ? (x + 4) % 5 : (x + 1) % 5;
            tileGrid[x, rowIndex].GetComponent<Image>().sprite = rowSprites[from];
        }
    }

    private void RollColumn(int colIndex, bool toDown)
    {
        Sprite[] colSprites = new Sprite[5];
        for (int y = 0; y < 5; y++)
            colSprites[y] = tileGrid[colIndex, y].GetComponent<Image>().sprite;

        for (int y = 0; y < 5; y++)
        {
            int from = toDown ? (y + 4) % 5 : (y + 1) % 5;
            tileGrid[colIndex, y].GetComponent<Image>().sprite = colSprites[from];
        }
    }
}
