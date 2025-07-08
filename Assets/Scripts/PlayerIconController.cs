using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;


public class PlayerIconController : MonoBehaviour
{
    public Image iconImage;
    public RectTransform iconRectTransform;
    public GameObject[] tilePrefabs;

    public int playerIndex;
    private PlayerJoinManager joinManager;

    private RectTransform[] boards;
    private int currentIndex = 0;
    private bool hasClaimed = false;
    private bool puzzleSpawned = false;
    private bool inPuzzleMode = false;
    private bool inEditMode = false;
    private bool puzzleSpawnAllowed = false;
    private bool isBoardLocked = false;
    private bool inputLocked = false;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction undoAction;

    private Vector3 targetPosition;
    private bool isMoving = false;
    private float moveSpeed = 500f;
    private float moveCooldown = 0.065f;
    private float lastMoveTime = 0f;

    private Transform tileParent;
    private GameObject[,] tileGrid = new GameObject[5, 5];
    private int cursorX = 0;
    private int cursorY = 0;

    private Image healthBar;
    private bool gameWon = false;
    private bool hasFiveMatchOnBoard = false;
    public bool HasClaimed => hasClaimed;
    private bool isShifting = false;


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

        if (gameWon && Keyboard.current.rKey.wasPressedThisFrame)
        {
            Debug.Log("🔄 Restarting game...");
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void SetColor(Color color) => iconImage.color = color;

    public void SetBoards(RectTransform[] boardRects)
    {
        boards = boardRects;
        if (boards.Length > 0)
        {
            iconRectTransform.position = boards[0].position;
            targetPosition = boards[0].position;
        }
    }

    public void SetJoinManager(PlayerJoinManager manager) => joinManager = manager;

    private void OnMove(InputAction.CallbackContext ctx)
    {
        if (inputLocked) return;
        if (Time.time - lastMoveTime < moveCooldown) return;

        lastMoveTime = Time.time;
        Vector2 direction = ctx.ReadValue<Vector2>();

        if (!hasClaimed || !puzzleSpawned)
        {
            if (boards == null || boards.Length == 0) return;

            if (direction.x > 0.5f) currentIndex = (currentIndex + 1) % boards.Length;
            else if (direction.x < -0.5f) currentIndex = (currentIndex - 1 + boards.Length) % boards.Length;

            int attempts = 0;
            while (!joinManager.CanClaimBoard(currentIndex, playerIndex) && attempts < boards.Length)
            {
                currentIndex = (currentIndex + 1) % boards.Length;
                attempts++;
            }

            targetPosition = boards[currentIndex].position;
            isMoving = true;
        }
        else if (inPuzzleMode)
        {
            if (!inEditMode)
            {
                if (direction.x > 0.5f) cursorX = (cursorX + 1) % 5;
                else if (direction.x < -0.5f) cursorX = (cursorX - 1 + 5) % 5;
                else if (direction.y > 0.5f) cursorY = (cursorY - 1 + 5) % 5;
                else if (direction.y < -0.5f) cursorY = (cursorY + 1) % 5;

                UpdateSelector();
            }
            else
            {
                if (direction.x > 0.5f) RollRow(cursorY, true);
                else if (direction.x < -0.5f) RollRow(cursorY, false);
                else if (direction.y > 0.5f) RollColumn(cursorX, false);
                else if (direction.y < -0.5f) RollColumn(cursorX, true);
            }
        }
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (boards == null || boards.Length == 0) return;

        if (!hasClaimed)
        {
            if (joinManager.CanClaimBoard(currentIndex, playerIndex))
            {
                bool success = joinManager.ClaimBoard(currentIndex, playerIndex);
                if (success)
                {
                    hasClaimed = true;
                    inputLocked = true;
                    Debug.Log($"{gameObject.name} initially claimed board {currentIndex}.");
                }
            }
            else
            {
                UIManager.Instance.SetText($"{gameObject.name} cannot claim board {currentIndex} - already claimed.");
            }
        }
        else if (hasClaimed && !isBoardLocked)
        {
            isBoardLocked = true;
            inputLocked = true;
            Debug.Log($"{gameObject.name} fully claimed board {currentIndex}.");

            var board = boards[currentIndex];
            var hb = board.Find("HealthBar");
            if (hb != null) healthBar = hb.GetComponent<Image>();

            joinManager.OnPlayerClaimed(currentIndex, playerIndex);
        }
        else if (inPuzzleMode && !inEditMode)
        {
            inEditMode = true;
            SetSelectorHighlight(true);
        }
    }

    private void OnUndo(InputAction.CallbackContext ctx)
    {
        if (inPuzzleMode && inEditMode)
        {
            inEditMode = false;
            SetSelectorHighlight(false);
            return;
        }

        if (hasClaimed && !isBoardLocked)
        {
            joinManager.ResetBoard(currentIndex);
            hasClaimed = false;
            inputLocked = false;
        }
    }

    public void SpawnPuzzleAfterCountdown()
    {
        if (puzzleSpawned) return;

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
        inputLocked = false;
        puzzleSpawnAllowed = true;
        iconImage.enabled = false;
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
                tileRT.localScale = Vector3.zero;
                tileRT.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

                var tileComp = tile.GetComponent<Tile>();
                tileComp.SetTile(randomIndex, tilePrefabs[randomIndex].GetComponent<Tile>().tileImage.sprite);

                tileGrid[x, y] = tile;
            }
        }

        cursorX = 0;
        cursorY = 0;
        UpdateSelector();

        hasFiveMatchOnBoard = CheckForAnyFiveMatchesOnBoard();

        if (hasFiveMatchOnBoard)
        {
            // Immediately clear those matches if you want to start clean
            StartCoroutine(ClearMatchesAndRespawnRoutine());
        }

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
                {
                    bool isSelected = (x == cursorX && y == cursorY);
                    selector.gameObject.SetActive(isSelected);
                    var highlight = selector.Find("Highlight");
                    if (highlight != null)
                    {
                        highlight.gameObject.SetActive(isSelected && inEditMode);
                    }
                }
            }
        }
    }

    private void SetSelectorHighlight(bool on)
    {
        if (boards == null || !puzzleSpawned) return;
        var selector = tileGrid[cursorX, cursorY].transform.Find("Selector");
        if (selector == null) return;
        var highlight = selector.Find("Highlight");
        if (highlight == null) return;
        highlight.gameObject.SetActive(on);
    }

    //private void RollRow(int rowIndex, bool toRight)
    //{
    //    Sprite[] rowSprites = new Sprite[5];
    //    int[] rowTileIDs = new int[5];

    //    for (int x = 0; x < 5; x++)
    //    {
    //        var tileComp = tileGrid[x, rowIndex].GetComponent<Tile>();
    //        rowSprites[x] = tileComp.tileImage.sprite;
    //        rowTileIDs[x] = tileComp.tileID;
    //    }

    //    for (int x = 0; x < 5; x++)
    //    {
    //        int from = toRight ? (x + 4) % 5 : (x + 1) % 5;
    //        var tileComp = tileGrid[x, rowIndex].GetComponent<Tile>();
    //        tileComp.SetTile(rowTileIDs[from], rowSprites[from]);
    //    }

    //    StartCoroutine(ClearMatchesAndRespawnRoutine());
    //}
    //private void RollRow(int rowIndex, bool toRight)
    //{
    //    StartCoroutine(RollRowRoutine(rowIndex, toRight));
    //}

    //private IEnumerator RollRowRoutine(int rowIndex, bool toRight)
    //{
    //    GridLayoutGroup grid = tileGrid[0, rowIndex].transform.parent.GetComponent<GridLayoutGroup>();
    //    RectTransform parentRect = grid.GetComponent<RectTransform>();

    //    // 1. Disable layout group completely before animation
    //    grid.enabled = false;

    //    // Also disable any LayoutElement components on tiles (optional but safer)
    //    foreach (var tile in tileGrid)
    //    {
    //        var layoutElement = tile.GetComponent<LayoutElement>();
    //        if (layoutElement != null) layoutElement.enabled = false;
    //    }

    //    // 2. Cache start positions (as you do now)
    //    RectTransform[] tileRects = new RectTransform[5];
    //    GameObject[] originalTiles = new GameObject[5];
    //    Vector2[] startPositions = new Vector2[5];
    //    Vector2[] targetPositions = new Vector2[5];

    //    //float tileSize = grid.cellSize.x;

    //    float cellWidth = grid.cellSize.x;
    //    float spacingX = grid.spacing.x;
    //    RectOffset padding = grid.padding;

    //    for (int x = 0; x < 5; x++)
    //    {
    //        originalTiles[x] = tileGrid[x, rowIndex];
    //        tileRects[x] = originalTiles[x].GetComponent<RectTransform>();

    //        float anchoredY = tileRects[x].anchoredPosition.y; // Preserve Y
    //        float anchoredX = padding.left + x * (cellWidth + spacingX);
    //        startPositions[x] = new Vector2(anchoredX, anchoredY);
    //    }

    //    // 3. Set target positions based on direction
    //    for (int x = 0; x < 5; x++)
    //    {
    //        int to = toRight ? (x + 1) % 5 : (x + 4) % 5;
    //        targetPositions[x] = startPositions[to];
    //    }

    //    // 4. Animate tiles to target positions
    //    float duration = 0.2f;
    //    for (int x = 0; x < 5; x++)
    //    {
    //        tileRects[x].DOAnchorPos(targetPositions[x], duration).SetEase(Ease.InOutQuad);
    //    }

    //    // 5. Wait for animation to finish
    //    yield return new WaitForSeconds(duration);

    //    // 6. Update tileGrid references to match new positions
    //    GameObject[] newTiles = new GameObject[5];
    //    for (int x = 0; x < 5; x++)
    //    {
    //        int from = toRight ? (x + 4) % 5 : (x + 1) % 5;
    //        newTiles[x] = tileGrid[from, rowIndex];
    //    }
    //    for (int x = 0; x < 5; x++)
    //    {
    //        tileGrid[x, rowIndex] = newTiles[x];
    //        // Fix final anchoredPosition explicitly
    //        RectTransform rt = tileGrid[x, rowIndex].GetComponent<RectTransform>();
    //        rt.anchoredPosition = startPositions[x];
    //    }

    //    StartCoroutine(ClearMatchesAndRespawnRoutine());
    //}
    //private void RollRow(int rowIndex, bool toRight)
    //{
    //    StartCoroutine(RollRowAnimated(rowIndex, toRight));
    //}



    //private void RollRow(int rowIndex, bool toRight)
    //{
    //    StartCoroutine(RollRowRoutine(rowIndex, toRight));
    //}

    //private IEnumerator RollRowRoutine(int rowIndex, bool toRight)
    //{
    //    GridLayoutGroup grid = tileGrid[0, rowIndex].transform.parent.GetComponent<GridLayoutGroup>();
    //    RectOffset padding = grid.padding;
    //    float cellWidth = grid.cellSize.x;
    //    float spacing = grid.spacing.x;

    //    // 1. Cache sprite + tileID + rect
    //    Sprite[] rowSprites = new Sprite[5];
    //    int[] rowTileIDs = new int[5];
    //    RectTransform[] tileRects = new RectTransform[5];

    //    for (int x = 0; x < 5; x++)
    //    {
    //        var tile = tileGrid[x, rowIndex];
    //        var tileComp = tile.GetComponent<Tile>();
    //        rowSprites[x] = tileComp.tileImage.sprite;
    //        rowTileIDs[x] = tileComp.tileID;
    //        tileRects[x] = tile.GetComponent<RectTransform>();
    //    }

    //    // 2. Animate tiles
    //    float duration = 0.2f;
    //    float dir = toRight ? 1f : -1f;
    //    float offset = (cellWidth + spacing) * dir;

    //    for (int x = 0; x < 5; x++)
    //    {
    //        tileRects[x].DOAnchorPosX(tileRects[x].anchoredPosition.x + offset, duration)
    //            .SetEase(Ease.InOutQuad);
    //    }

    //    // 3. Wait for tween to complete
    //    yield return new WaitForSeconds(duration);

    //    // 4. Apply logic shift
    //    for (int x = 0; x < 5; x++)
    //    {
    //        int from = toRight ? (x + 4) % 5 : (x + 1) % 5;
    //        tileGrid[x, rowIndex].GetComponent<Tile>().SetTile(rowTileIDs[from], rowSprites[from]);
    //    }

    //    // ✅ 5. Snap each tile back to exact GridLayoutGroup position
    //    for (int x = 0; x < 5; x++)
    //    {
    //        RectTransform rt = tileGrid[x, rowIndex].GetComponent<RectTransform>();
    //        float anchoredX = padding.left + x * (cellWidth + spacing);
    //        rt.anchoredPosition = new Vector2(anchoredX, rt.anchoredPosition.y);
    //    }
    //    grid.enabled = true;
    //    LayoutRebuilder.ForceRebuildLayoutImmediate(grid.GetComponent<RectTransform>());
    //    // 6. Match logic
    //    StartCoroutine(ClearMatchesAndRespawnRoutine());
    //}
    //fully best Working Code
    //private void RollRow(int rowIndex, bool toRight)
    //{
    //    if (isShifting) return;
    //    StartCoroutine(RollRowRoutine(rowIndex, toRight));
    //}

    //private IEnumerator RollRowRoutine(int rowIndex, bool toRight)
    //{
    //    isShifting = true;

    //    GridLayoutGroup grid = tileGrid[0, rowIndex].transform.parent.GetComponent<GridLayoutGroup>();
    //    RectOffset padding = grid.padding;
    //    float cellWidth = grid.cellSize.x;
    //    float spacing = grid.spacing.x;

    //    Sprite[] rowSprites = new Sprite[5];
    //    int[] rowTileIDs = new int[5];
    //    RectTransform[] tileRects = new RectTransform[5];

    //    for (int x = 0; x < 5; x++)
    //    {
    //        var tile = tileGrid[x, rowIndex];
    //        var tileComp = tile.GetComponent<Tile>();
    //        rowSprites[x] = tileComp.tileImage.sprite;
    //        rowTileIDs[x] = tileComp.tileID;
    //        tileRects[x] = tile.GetComponent<RectTransform>();
    //    }

    //    float duration = 0.2f;
    //    float dir = toRight ? 1f : -1f;
    //    float offset = (cellWidth + spacing) * dir;

    //    for (int x = 0; x < 5; x++)
    //    {
    //        tileRects[x].DOKill(); // Stop any previous animation
    //        tileRects[x].DOAnchorPosX(tileRects[x].anchoredPosition.x + offset, duration)
    //            .SetEase(Ease.InOutQuad);
    //    }

    //    yield return new WaitForSeconds(duration);

    //    for (int x = 0; x < 5; x++)
    //    {
    //        int from = toRight ? (x + 4) % 5 : (x + 1) % 5;
    //        tileGrid[x, rowIndex].GetComponent<Tile>().SetTile(rowTileIDs[from], rowSprites[from]);
    //    }

    //    for (int x = 0; x < 5; x++)
    //    {
    //        RectTransform rt = tileGrid[x, rowIndex].GetComponent<RectTransform>();
    //        float anchoredX = padding.left + x * (cellWidth + spacing);
    //        rt.anchoredPosition = new Vector2(anchoredX, rt.anchoredPosition.y);
    //    }

    //    grid.enabled = true;
    //    LayoutRebuilder.ForceRebuildLayoutImmediate(grid.GetComponent<RectTransform>());

    //    StartCoroutine(ClearMatchesAndRespawnRoutine());

    //    isShifting = false;
    //}


    private void RollRow(int rowIndex, bool toRight)
    {
        if (isShifting) return;
        StartCoroutine(RollRowRoutine(rowIndex, toRight));
    }

    private IEnumerator RollRowRoutine(int rowIndex, bool toRight)
    {
        isShifting = true;

        GridLayoutGroup grid = tileGrid[0, rowIndex].transform.parent.GetComponent<GridLayoutGroup>();
        RectOffset padding = grid.padding;
        float cellWidth = grid.cellSize.x;
        float spacing = grid.spacing.x;

        Sprite[] rowSprites = new Sprite[5];
        int[] rowTileIDs = new int[5];
        RectTransform[] tileRects = new RectTransform[5];

        for (int x = 0; x < 5; x++)
        {
            var tile = tileGrid[x, rowIndex];
            var tileComp = tile.GetComponent<Tile>();
            rowSprites[x] = tileComp.tileImage.sprite;
            rowTileIDs[x] = tileComp.tileID;
            tileRects[x] = tile.GetComponent<RectTransform>();
            tileRects[x].DOKill(); // Kill any running tween
        }

        float duration = 0.2f;

        if (toRight)
        {
            // Move tiles right, temp tile appears from outside-left moving in

            float outsideLeftX = padding.left - (cellWidth + spacing);
            float anchoredY = tileRects[0].anchoredPosition.y;

            // Create temp tile off-screen left with last tile data
            GameObject tempTile = Instantiate(tilePrefabs[0], tileGrid[0, rowIndex].transform.parent);
            RectTransform tempRT = tempTile.GetComponent<RectTransform>();
            tempRT.SetAsLastSibling();
            tempRT.anchoredPosition = new Vector2(outsideLeftX, anchoredY);

            Tile tempTileComp = tempTile.GetComponent<Tile>();
            tempTileComp.SetTile(rowTileIDs[4], rowSprites[4]); // last tile's data

            // Animate existing tiles right (+offset)
            float offset = cellWidth + spacing;
            for (int x = 0; x < 5; x++)
            {
                tileRects[x].DOAnchorPosX(tileRects[x].anchoredPosition.x + offset, duration)
                    .SetEase(Ease.InOutQuad);
            }

            // Animate temp tile from outside-left to first tile position
            Vector2 firstPos = new Vector2(padding.left, anchoredY);
            tempRT.DOAnchorPosX(firstPos.x, duration).SetEase(Ease.InOutQuad);

            yield return new WaitForSeconds(duration);

            // Logical shift right
            for (int x = 0; x < 5; x++)
            {
                int from = (x + 4) % 5;
                tileGrid[x, rowIndex].GetComponent<Tile>().SetTile(rowTileIDs[from], rowSprites[from]);
            }

            // Snap tiles exactly into grid positions
            for (int x = 0; x < 5; x++)
            {
                RectTransform rt = tileGrid[x, rowIndex].GetComponent<RectTransform>();
                float anchoredX = padding.left + x * (cellWidth + spacing);
                rt.anchoredPosition = new Vector2(anchoredX, rt.anchoredPosition.y);
            }

            Destroy(tempTile);
        }
        else
        {
            // Move tiles left, temp tile appears from outside-right moving in

            float outsideRightX = padding.left + 5 * (cellWidth + spacing);
            float anchoredY = tileRects[0].anchoredPosition.y;

            // Create temp tile off-screen right with first tile data
            GameObject tempTile = Instantiate(tilePrefabs[0], tileGrid[0, rowIndex].transform.parent);
            RectTransform tempRT = tempTile.GetComponent<RectTransform>();
            tempRT.SetAsLastSibling();
            tempRT.anchoredPosition = new Vector2(outsideRightX, anchoredY);

            Tile tempTileComp = tempTile.GetComponent<Tile>();
            tempTileComp.SetTile(rowTileIDs[0], rowSprites[0]); // first tile's data

            // Animate existing tiles left (-offset)
            float offset = -(cellWidth + spacing);
            for (int x = 0; x < 5; x++)
            {
                tileRects[x].DOAnchorPosX(tileRects[x].anchoredPosition.x + offset, duration)
                    .SetEase(Ease.InOutQuad);
            }

            // Animate temp tile from outside-right to last tile position
            Vector2 lastPos = new Vector2(padding.left + 4 * (cellWidth + spacing), anchoredY);
            tempRT.DOAnchorPosX(lastPos.x, duration).SetEase(Ease.InOutQuad);

            yield return new WaitForSeconds(duration);

            // Logical shift left
            for (int x = 0; x < 5; x++)
            {
                int from = (x + 1) % 5;
                tileGrid[x, rowIndex].GetComponent<Tile>().SetTile(rowTileIDs[from], rowSprites[from]);
            }

            // Snap tiles exactly into grid positions
            for (int x = 0; x < 5; x++)
            {
                RectTransform rt = tileGrid[x, rowIndex].GetComponent<RectTransform>();
                float anchoredX = padding.left + x * (cellWidth + spacing);
                rt.anchoredPosition = new Vector2(anchoredX, rt.anchoredPosition.y);
            }

            Destroy(tempTile);
        }

        grid.enabled = true;
        LayoutRebuilder.ForceRebuildLayoutImmediate(grid.GetComponent<RectTransform>());

        StartCoroutine(ClearMatchesAndRespawnRoutine());

        isShifting = false;
    }




    //private void RollColumn(int colIndex, bool toDown)
    //{
    //    StartCoroutine(RollColumnRoutine(colIndex, toDown));
    //}

    //private IEnumerator RollColumnRoutine(int colIndex, bool toDown)
    //{
    //    GridLayoutGroup grid = tileGrid[colIndex, 0].transform.parent.GetComponent<GridLayoutGroup>();
    //    RectOffset padding = grid.padding;
    //    float cellHeight = grid.cellSize.y;
    //    float spacingY = grid.spacing.y;

    //    // 1. Cache sprite + tileID + rect
    //    Sprite[] colSprites = new Sprite[5];
    //    int[] colTileIDs = new int[5];
    //    RectTransform[] tileRects = new RectTransform[5];

    //    for (int y = 0; y < 5; y++)
    //    {
    //        var tile = tileGrid[colIndex, y];
    //        var tileComp = tile.GetComponent<Tile>();
    //        colSprites[y] = tileComp.tileImage.sprite;
    //        colTileIDs[y] = tileComp.tileID;
    //        tileRects[y] = tile.GetComponent<RectTransform>();
    //    }

    //    // ✅ 2. Animate tiles in correct Y direction
    //    float duration = 0.2f;
    //    float dir = toDown ? 1f : -1f; // ✅ Fix: pressing down should move tiles visually down (y decreases)
    //    float offset = (cellHeight + spacingY) * dir;

    //    for (int y = 0; y < 5; y++)
    //    {
    //        tileRects[y].DOAnchorPosY(tileRects[y].anchoredPosition.y - offset, duration)
    //            .SetEase(Ease.InOutQuad);
    //    }

    //    // 3. Wait for tween
    //    yield return new WaitForSeconds(duration);

    //    // 4. Apply logic shift
    //    for (int y = 0; y < 5; y++)
    //    {
    //        int from = toDown ? (y + 4) % 5 : (y + 1) % 5;
    //        tileGrid[colIndex, y].GetComponent<Tile>().SetTile(colTileIDs[from], colSprites[from]);
    //    }

    //    // 5. Snap tiles back to their exact GridLayoutGroup position
    //    for (int y = 0; y < 5; y++)
    //    {
    //        RectTransform rt = tileGrid[colIndex, y].GetComponent<RectTransform>();
    //        float anchoredY = -(padding.top + y * (cellHeight + spacingY));
    //        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, anchoredY);
    //    }
    //    grid.enabled = true;
    //    LayoutRebuilder.ForceRebuildLayoutImmediate(grid.GetComponent<RectTransform>());
    //    // 6. Match logic
    //    StartCoroutine(ClearMatchesAndRespawnRoutine());
    //}

    private void RollColumn(int colIndex, bool toDown)
    {
        if (isShifting) return;
        StartCoroutine(RollColumnRoutine(colIndex, toDown));
    }

    private IEnumerator RollColumnRoutine(int colIndex, bool toDown)
    {
        isShifting = true;

        GridLayoutGroup grid = tileGrid[colIndex, 0].transform.parent.GetComponent<GridLayoutGroup>();
        RectOffset padding = grid.padding;
        float cellHeight = grid.cellSize.y;
        float spacing = grid.spacing.y;
        float offset = cellHeight + spacing;

        Sprite[] colSprites = new Sprite[5];
        int[] colTileIDs = new int[5];
        RectTransform[] tileRects = new RectTransform[5];

        for (int y = 0; y < 5; y++)
        {
            var tile = tileGrid[colIndex, y];
            var tileComp = tile.GetComponent<Tile>();
            colSprites[y] = tileComp.tileImage.sprite;
            colTileIDs[y] = tileComp.tileID;
            tileRects[y] = tile.GetComponent<RectTransform>();
            tileRects[y].DOKill();
        }

        float duration = 0.2f;
        float anchoredX = tileRects[0].anchoredPosition.x;

        if (toDown)
        {
            // Move tiles down (new tile appears from top)
            float outsideTopY = -(padding.top - offset);

            GameObject tempTile = Instantiate(tilePrefabs[0], tileGrid[colIndex, 0].transform.parent);
            RectTransform tempRT = tempTile.GetComponent<RectTransform>();
            tempRT.anchoredPosition = new Vector2(anchoredX, outsideTopY);

            Tile tempTileComp = tempTile.GetComponent<Tile>();
            tempTileComp.SetTile(colTileIDs[4], colSprites[4]);

            Canvas tempCanvas = tempTile.AddComponent<Canvas>();
            tempCanvas.overrideSorting = true;
            tempCanvas.sortingOrder = -1;

            CanvasGroup tempGroup = tempTile.AddComponent<CanvasGroup>();
            tempGroup.alpha = 0;

            tempRT.SetAsLastSibling();

            for (int y = 0; y < 5; y++)
            {
                tileRects[y].DOAnchorPosY(tileRects[y].anchoredPosition.y - offset, duration)
                    .SetEase(Ease.InOutQuad);
            }

            float targetY = -(padding.top);
            tempRT.DOAnchorPosY(targetY, duration).SetEase(Ease.InOutQuad);
            DOVirtual.DelayedCall(0.05f, () => tempGroup.DOFade(1f, 0.15f));

            yield return new WaitForSeconds(duration);

            // Logical shift down
            for (int y = 0; y < 5; y++)
            {
                int from = (y + 4) % 5;
                tileGrid[colIndex, y].GetComponent<Tile>().SetTile(colTileIDs[from], colSprites[from]);
            }

            for (int y = 0; y < 5; y++)
            {
                RectTransform rt = tileGrid[colIndex, y].GetComponent<RectTransform>();
                float anchoredY = -(padding.top + y * offset);
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, anchoredY);
            }

            Destroy(tempTile);
        }
        else
        {
            // Move tiles up (new tile appears from bottom)
            float outsideBottomY = -(padding.top + 5 * offset);

            GameObject tempTile = Instantiate(tilePrefabs[0], tileGrid[colIndex, 0].transform.parent);
            RectTransform tempRT = tempTile.GetComponent<RectTransform>();
            tempRT.anchoredPosition = new Vector2(anchoredX, outsideBottomY);

            Tile tempTileComp = tempTile.GetComponent<Tile>();
            tempTileComp.SetTile(colTileIDs[0], colSprites[0]);

            Canvas tempCanvas = tempTile.AddComponent<Canvas>();
            tempCanvas.overrideSorting = true;
            tempCanvas.sortingOrder = -1;

            CanvasGroup tempGroup = tempTile.AddComponent<CanvasGroup>();
            tempGroup.alpha = 0;

            tempRT.SetAsLastSibling();

            for (int y = 0; y < 5; y++)
            {
                tileRects[y].DOAnchorPosY(tileRects[y].anchoredPosition.y + offset, duration)
                    .SetEase(Ease.InOutQuad);
            }

            float targetY = -(padding.top + 4 * offset);
            tempRT.DOAnchorPosY(targetY, duration).SetEase(Ease.InOutQuad);
            DOVirtual.DelayedCall(0.05f, () => tempGroup.DOFade(1f, 0.15f));

            yield return new WaitForSeconds(duration);

            // Logical shift up
            for (int y = 0; y < 5; y++)
            {
                int from = (y + 1) % 5;
                tileGrid[colIndex, y].GetComponent<Tile>().SetTile(colTileIDs[from], colSprites[from]);
            }

            for (int y = 0; y < 5; y++)
            {
                RectTransform rt = tileGrid[colIndex, y].GetComponent<RectTransform>();
                float anchoredY = -(padding.top + y * offset);
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, anchoredY);
            }

            Destroy(tempTile);
        }

        grid.enabled = true;
        LayoutRebuilder.ForceRebuildLayoutImmediate(grid.GetComponent<RectTransform>());

        StartCoroutine(ClearMatchesAndRespawnRoutine());

        isShifting = false;
    }







    private IEnumerator ClearMatchesAndRespawnRoutine()
    {
        inputLocked = true;
        bool foundMatch = true;

        while (foundMatch)
        {
            yield return null;

            bool foundFour, foundFive;
            var matchedPositions = GetMatchedPositions(out foundFour, out foundFive);

            if (matchedPositions.Count == 0)
            {
                foundMatch = false;
                break;
            }

            // Clear tiles with animation
            yield return StartCoroutine(ClearTilesOneByOneRoutine(matchedPositions));

            // Respawn tiles (assign new sprites, scale, etc)
            RespawnTilesAtPositions(matchedPositions);

            // ... rest of your scoring & health code ...

            yield return new WaitForSeconds(0.2f);
        }

        inputLocked = false;
    }


    private List<(int x, int y)> GetMatchedPositions(out bool foundFour, out bool foundFive)
    {
        List<(int, int)> matches = new();
        foundFour = false;
        foundFive = false;

        List<(int, int)> fourMatchesTemp = new();

        // First, find all 5+ matches horizontally and vertically
        for (int y = 0; y < 5; y++)
        {
            int count = 1;
            for (int x = 1; x < 5; x++)
            {
                int prev = tileGrid[x - 1, y].GetComponent<Tile>().tileID;
                int curr = tileGrid[x, y].GetComponent<Tile>().tileID;

                if (prev == curr) count++;
                else
                {
                    if (count >= 5)
                    {
                        for (int k = x - count; k < x; k++) matches.Add((k, y));
                        foundFive = true;
                    }
                    else if (count == 4)
                    {
                        // Temporarily store 4 matches, but do not add yet
                        for (int k = x - count; k < x; k++) fourMatchesTemp.Add((k, y));
                    }
                    count = 1;
                }
            }
            // Check last run
            if (count >= 5)
            {
                for (int k = 5 - count; k < 5; k++) matches.Add((k, y));
                foundFive = true;
            }
            else if (count == 4)
            {
                for (int k = 5 - count; k < 5; k++) fourMatchesTemp.Add((k, y));
            }
        }

        // Same for vertical matches
        for (int x = 0; x < 5; x++)
        {
            int count = 1;
            for (int y = 1; y < 5; y++)
            {
                int prev = tileGrid[x, y - 1].GetComponent<Tile>().tileID;
                int curr = tileGrid[x, y].GetComponent<Tile>().tileID;

                if (prev == curr) count++;
                else
                {
                    if (count >= 5)
                    {
                        for (int k = y - count; k < y; k++) matches.Add((x, k));
                        foundFive = true;
                    }
                    else if (count == 4)
                    {
                        for (int k = y - count; k < y; k++) fourMatchesTemp.Add((x, k));
                    }
                    count = 1;
                }
            }
            // Check last run
            if (count >= 5)
            {
                for (int k = 5 - count; k < 5; k++) matches.Add((x, k));
                foundFive = true;
            }
            else if (count == 4)
            {
                for (int k = 5 - count; k < 5; k++) fourMatchesTemp.Add((x, k));
            }
        }

        // Now, only add 4-matches if there is at least one 5-match
        if (foundFive)
        {
            matches.AddRange(fourMatchesTemp);
            foundFour = fourMatchesTemp.Count > 0;
        }
        else
        {
            // No 5 matches found, so ignore all 4 matches
            foundFour = false;
            fourMatchesTemp.Clear();
        }

        // Remove duplicates because 4-matches might overlap with 5-matches
        var distinctMatches = new HashSet<(int, int)>(matches);
        return new List<(int, int)>(distinctMatches);
    }


private void ClearTiles(List<(int x, int y)> positions)
{
    StartCoroutine(ClearTilesOneByOneRoutine(positions));
}

private IEnumerator ClearTilesOneByOneRoutine(List<(int x, int y)> positions)
{
    float delayBetween = 0.05f;

    foreach (var pos in positions)
    {
        GameObject tileObj = tileGrid[pos.x, pos.y];
        Tile tileComp = tileObj.GetComponent<Tile>();
        RectTransform tileRT = tileObj.GetComponent<RectTransform>();

        tileRT.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() =>
        {
            int randomIndex = Random.Range(0, tilePrefabs.Length);
            tileComp.SetTile(randomIndex, tilePrefabs[randomIndex].GetComponent<Tile>().tileImage.sprite);
            tileRT.localScale = Vector3.zero;
            tileRT.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        });

        yield return new WaitForSeconds(delayBetween);
    }
}
    //private void RespawnTilesAtPositions(List<(int x, int y)> positions) { }
    private void RespawnTilesAtPositions(List<(int x, int y)> positions)
    {
        foreach (var pos in positions)
        {
            GameObject tileObj = tileGrid[pos.x, pos.y];
            Tile tileComp = tileObj.GetComponent<Tile>();
            RectTransform tileRT = tileObj.GetComponent<RectTransform>();

            // Assign new random sprite & tileID
            int randomIndex = Random.Range(0, tilePrefabs.Length);
            Sprite newSprite = tilePrefabs[randomIndex].GetComponent<Tile>().tileImage.sprite;

            tileComp.SetTile(randomIndex, newSprite);
            tileRT.localScale = Vector3.one; // Make sure scale reset

            // Optional: If you want a respawn animation here, add it
            tileRT.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }
    }


    public int GetScore()
    {
        return 0;
    }
    private bool CheckForAnyFiveMatchesOnBoard()
    {
        bool dummyFour, foundFive;
        GetMatchedPositions(out dummyFour, out foundFive);
        return foundFive;
    }
}
