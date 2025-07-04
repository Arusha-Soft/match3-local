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
    private float moveCooldown = 0.075f;
    private float lastMoveTime = 0f;

    private Transform tileParent;
    private GameObject[,] tileGrid = new GameObject[5, 5];
    private int cursorX = 0;
    private int cursorY = 0;

    private Image healthBar;
    private bool gameWon = false;

    public bool HasClaimed => hasClaimed;

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

        // Restart after win
        if (gameWon && Keyboard.current.rKey.wasPressedThisFrame)
        {
            Debug.Log("🔄 Restarting game...");
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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

    public void SetJoinManager(PlayerJoinManager manager)
    {
        joinManager = manager;
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        if (inputLocked) return;
        if (Time.time - lastMoveTime < moveCooldown) return;

        lastMoveTime = Time.time;
        Vector2 direction = ctx.ReadValue<Vector2>();

        if (!hasClaimed || !puzzleSpawned)
        {
            if (boards == null || boards.Length == 0) return;

            // Only allow moving to boards player can claim (check via joinManager)
            if (direction.x > 0.5f)
                currentIndex = (currentIndex + 1) % boards.Length;
            else if (direction.x < -0.5f)
                currentIndex = (currentIndex - 1 + boards.Length) % boards.Length;

            // Check if player can claim this board, skip if can't
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
            // Check if board can be claimed
            if (joinManager.CanClaimBoard(currentIndex, playerIndex))
            {
                bool success = joinManager.ClaimBoard(currentIndex, playerIndex);
                if (success)
                {
                    hasClaimed = true;
                    inputLocked = true;
                    Debug.Log($"{gameObject.name} initially claimed board {currentIndex}. Press again to confirm.");
                }
            }
            else
            {
                Debug.Log($"{gameObject.name} cannot claim board {currentIndex} - already claimed by another player.");
            }
        }
        else if (hasClaimed && !isBoardLocked)
        {
            isBoardLocked = true;
            inputLocked = true;
            Debug.Log($"{gameObject.name} fully claimed board {currentIndex}.");

            // Link health bar
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
            Debug.Log("Exited edit mode");
            return;
        }

        if (hasClaimed && !isBoardLocked)
        {
            joinManager.ResetBoard(currentIndex);
            hasClaimed = false;
            inputLocked = false;
            Debug.Log($"{gameObject.name} undid board claim.");
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

    private void RollRow(int rowIndex, bool toRight)
    {
        Sprite[] rowSprites = new Sprite[5];
        int[] rowTileIDs = new int[5];

        for (int x = 0; x < 5; x++)
        {
            var tileComp = tileGrid[x, rowIndex].GetComponent<Tile>();
            rowSprites[x] = tileComp.tileImage.sprite;
            rowTileIDs[x] = tileComp.tileID;
        }

        for (int x = 0; x < 5; x++)
        {
            int from = toRight ? (x + 4) % 5 : (x + 1) % 5;
            var tileComp = tileGrid[x, rowIndex].GetComponent<Tile>();
            tileComp.SetTile(rowTileIDs[from], rowSprites[from]);
        }

        StartCoroutine(ClearMatchesAndRespawnRoutine());
    }

    private void RollColumn(int colIndex, bool toDown)
    {
        Sprite[] colSprites = new Sprite[5];
        int[] colTileIDs = new int[5];

        for (int y = 0; y < 5; y++)
        {
            var tileComp = tileGrid[colIndex, y].GetComponent<Tile>();
            colSprites[y] = tileComp.tileImage.sprite;
            colTileIDs[y] = tileComp.tileID;
        }

        for (int y = 0; y < 5; y++)
        {
            int from = toDown ? (y + 4) % 5 : (y + 1) % 5;
            var tileComp = tileGrid[colIndex, y].GetComponent<Tile>();
            tileComp.SetTile(colTileIDs[from], colSprites[from]);
        }

        StartCoroutine(ClearMatchesAndRespawnRoutine());
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

            ClearTiles(matchedPositions);
            RespawnTilesAtPositions(matchedPositions);

            float unit = 0.036f;
            float gain = 0f;
            if (foundFour) gain += 4 * unit;
            if (foundFive) gain += 5 * unit;

            // Add fuse boost if combos found
            if (foundFour)
            {
                joinManager?.AddFuseAmount(playerIndex, 0.4f);
            }
            if (foundFive)
            {
                joinManager?.AddFuseAmount(playerIndex, 0.5f);
            }

            if (healthBar != null && !gameWon)
            {
                healthBar.fillAmount += gain;

                if (healthBar.fillAmount >= 0.99f)
                {
                    healthBar.fillAmount = 1f;
                    gameWon = true;
                    Time.timeScale = 0f;
                    Debug.Log($"🏆 Game Finished! Player: {gameObject.name}, Color: {iconImage.color}, Index: {playerIndex}");
                    Debug.Log("Press R to restart");
                }
            }

            yield return new WaitForSeconds(0.2f);
        }

        inputLocked = false;
    }

    private List<(int x, int y)> GetMatchedPositions(out bool foundFour, out bool foundFive)
    {
        List<(int, int)> matches = new List<(int, int)>();
        foundFour = false;
        foundFive = false;

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
                    if (count >= 4)
                    {
                        for (int k = x - count; k < x; k++) matches.Add((k, y));
                        if (count == 4) foundFour = true;
                        if (count >= 5) foundFive = true;
                    }
                    count = 1;
                }
            }
            if (count >= 4)
            {
                for (int k = 5 - count; k < 5; k++) matches.Add((k, y));
                if (count == 4) foundFour = true;
                if (count >= 5) foundFive = true;
            }
        }

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
                    if (count >= 4)
                    {
                        for (int k = y - count; k < y; k++) matches.Add((x, k));
                        if (count == 4) foundFour = true;
                        if (count >= 5) foundFive = true;
                    }
                    count = 1;
                }
            }
            if (count >= 4)
            {
                for (int k = 5 - count; k < 5; k++) matches.Add((x, k));
                if (count == 4) foundFour = true;
                if (count >= 5) foundFive = true;
            }
        }

        return matches;
    }

    private void ClearTiles(List<(int x, int y)> positions)
    {
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
        }
    }

    private void RespawnTilesAtPositions(List<(int x, int y)> positions) { }

    // Helper to get player score - adjust with your real score logic
    public int GetScore()
    {
        // Placeholder: return 0 or actual score tracking
        return 0;
    }
}
