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
    private RectTransform globalSelector;


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

        // ✅ Selector setup
        var selectorTransform = boardTransform.Find("Selector");
        if (selectorTransform != null)
        {
            globalSelector = selectorTransform.GetComponent<RectTransform>();
            globalSelector.gameObject.SetActive(false); // hide initially
        }
        else
        {
            Debug.LogWarning("Selector not found inside board!");
        }

        SpawnPuzzleGrid();               // Spawns the tiles
        puzzleSpawned = true;
        inPuzzleMode = true;
        inputLocked = false;
        puzzleSpawnAllowed = true;
        iconImage.enabled = false;

        // ✅ Force selector update next frame to fix its first-frame positioning
        StartCoroutine(ForceInitialSelectorUpdate());
    }
    private IEnumerator ForceInitialSelectorUpdate()
    {
        yield return null;       // Wait one frame for layout to update
        UpdateSelector();        // Force selector to update to correct tile position
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
        if (globalSelector == null || tileParent == null) return;

        // Get RectTransform of the target tile in tileParent space
        RectTransform targetTileRT = tileGrid[cursorX, cursorY].GetComponent<RectTransform>();

        // Convert tile local position from TileParent space to world space
        Vector3 worldPos = targetTileRT.transform.position;

        // Convert world position to Board local position (selector parent)
        Vector3 localPosInBoard = globalSelector.parent.InverseTransformPoint(worldPos);

        // Set selector anchoredPosition relative to Board
        globalSelector.localPosition = localPosInBoard;

        if (!globalSelector.gameObject.activeSelf)
            globalSelector.gameObject.SetActive(true);
    }
    private void SetSelectorHighlight(bool on)
    {
        if (globalSelector == null) return;

        var highlight = globalSelector.Find("Highlight");
        if (highlight != null)
            highlight.gameObject.SetActive(on);
    }


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
            tileRects[x].DOKill();
        }

        float duration = 0.2f;
        float offset = cellWidth + spacing;
        float anchoredY = tileRects[0].anchoredPosition.y;

        if (toRight)
        {
            // حرکت به سمت راست: 
            // تایل خارج شونده سمت راست، تایل وارد شونده از خارج سمت چپ

            Vector2 targetPos = tileRects[0].anchoredPosition; // موقعیت تایل اول (جایی که تایل وارد میشه)
            Vector2 outsideLeft = targetPos - new Vector2(offset * 1.5f, 0f); // فاصله مناسب خارج از کادر سمت چپ

            // ساخت تایل موقت ورودی خارج از کادر
            GameObject tempTile = Instantiate(tilePrefabs[0], tileGrid[0, rowIndex].transform.parent);
            RectTransform tempRT = tempTile.GetComponent<RectTransform>();
            tempRT.anchoredPosition = outsideLeft;
            tempRT.SetAsLastSibling();

            Tile tempTileComp = tempTile.GetComponent<Tile>();
            tempTileComp.SetTile(rowTileIDs[4], rowSprites[4]);

            // انیمیشن ورود تایل موقت به موقعیت هدف
            tempRT.DOAnchorPos(targetPos, duration).SetEase(Ease.InOutQuad);

            // انیمیشن حرکت تایل‌های اصلی به سمت راست
            for (int x = 0; x < 5; x++)
            {
                Vector2 newPos = tileRects[x].anchoredPosition + new Vector2(offset, 0f);
                tileRects[x].DOAnchorPos(newPos, duration).SetEase(Ease.InOutQuad);
            }

            yield return new WaitForSeconds(duration);

            // جابجایی منطقی داده‌ها در آرایه (چرخش به راست)
            for (int x = 0; x < 5; x++)
            {
                int from = (x + 4) % 5;
                tileGrid[x, rowIndex].GetComponent<Tile>().SetTile(rowTileIDs[from], rowSprites[from]);
            }

            // تنظیم دقیق موقعیت‌های تایل‌ها روی گرید
            for (int x = 0; x < 5; x++)
            {
                RectTransform rt = tileGrid[x, rowIndex].GetComponent<RectTransform>();
                float anchoredX = padding.left + x * offset;
                rt.anchoredPosition = new Vector2(anchoredX, anchoredY);
            }

            Destroy(tempTile);
        }
        else
        {
            // حرکت به سمت چپ: 
            // تایل خارج شونده سمت چپ، تایل وارد شونده از خارج سمت راست

            Vector2 targetPos = tileRects[4].anchoredPosition; // موقعیت تایل آخر (جایی که تایل وارد میشه)
            Vector2 outsideRight = targetPos + new Vector2(offset * 1.5f, 0f); // فاصله مناسب خارج از کادر سمت راست

            // ساخت تایل موقت ورودی خارج از کادر
            GameObject tempTile = Instantiate(tilePrefabs[0], tileGrid[0, rowIndex].transform.parent);
            RectTransform tempRT = tempTile.GetComponent<RectTransform>();
            tempRT.anchoredPosition = outsideRight;
            tempRT.SetAsLastSibling();

            Tile tempTileComp = tempTile.GetComponent<Tile>();
            tempTileComp.SetTile(rowTileIDs[0], rowSprites[0]);

            // انیمیشن ورود تایل موقت به موقعیت هدف
            tempRT.DOAnchorPos(targetPos, duration).SetEase(Ease.InOutQuad);

            // انیمیشن حرکت تایل‌های اصلی به سمت چپ
            for (int x = 0; x < 5; x++)
            {
                Vector2 newPos = tileRects[x].anchoredPosition - new Vector2(offset, 0f);
                tileRects[x].DOAnchorPos(newPos, duration).SetEase(Ease.InOutQuad);
            }

            yield return new WaitForSeconds(duration);

            // جابجایی منطقی داده‌ها در آرایه (چرخش به چپ)
            for (int x = 0; x < 5; x++)
            {
                int from = (x + 1) % 5;
                tileGrid[x, rowIndex].GetComponent<Tile>().SetTile(rowTileIDs[from], rowSprites[from]);
            }

            // تنظیم دقیق موقعیت‌های تایل‌ها روی گرید
            for (int x = 0; x < 5; x++)
            {
                RectTransform rt = tileGrid[x, rowIndex].GetComponent<RectTransform>();
                float anchoredX = padding.left + x * offset;
                rt.anchoredPosition = new Vector2(anchoredX, anchoredY);
            }

            Destroy(tempTile);
        }

        grid.enabled = true;
        LayoutRebuilder.ForceRebuildLayoutImmediate(grid.GetComponent<RectTransform>());

        StartCoroutine(ClearMatchesAndRespawnRoutine());

        isShifting = false;
    }




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
            // حرکت به پایین:
            // تایل خارج شونده پایین، تایل وارد شونده از بالا (با فاصله مناسب)

            Vector2 targetPos = tileRects[0].anchoredPosition; // جای تایل اول (از بالا وارد میشه)
            Vector2 outsideTop = targetPos + new Vector2(0f, offset * 1.5f); // فاصله خارج از کادر بالایی

            GameObject tempTile = Instantiate(tilePrefabs[0], tileGrid[colIndex, 0].transform.parent);
            RectTransform tempRT = tempTile.GetComponent<RectTransform>();
            tempRT.anchoredPosition = outsideTop;
            tempRT.SetAsLastSibling();

            Tile tempTileComp = tempTile.GetComponent<Tile>();
            tempTileComp.SetTile(colTileIDs[4], colSprites[4]);

            // انیمیشن ورود تایل موقت از بالا به جایگاه اول
            tempRT.DOAnchorPos(targetPos, duration).SetEase(Ease.InOutQuad);

            // انیمیشن حرکت تایل‌های اصلی به پایین
            for (int y = 0; y < 5; y++)
            {
                Vector2 newPos = tileRects[y].anchoredPosition - new Vector2(0f, offset);
                tileRects[y].DOAnchorPos(newPos, duration).SetEase(Ease.InOutQuad);
            }

            yield return new WaitForSeconds(duration);

            // جابجایی منطقی داده‌ها (چرخش به پایین)
            for (int y = 0; y < 5; y++)
            {
                int from = (y + 4) % 5;
                tileGrid[colIndex, y].GetComponent<Tile>().SetTile(colTileIDs[from], colSprites[from]);
            }

            // تنظیم دقیق موقعیت‌های تایل‌ها روی گرید
            for (int y = 0; y < 5; y++)
            {
                RectTransform rt = tileGrid[colIndex, y].GetComponent<RectTransform>();
                float anchoredY = padding.top + y * -offset; // توجه به منفی بودن محور Y در anchoredPosition
                rt.anchoredPosition = new Vector2(anchoredX, anchoredY);
            }

            Destroy(tempTile);
        }
        else
        {
            // حرکت به بالا:
            // تایل خارج شونده بالا، تایل وارد شونده از پایین (با فاصله مناسب)

            Vector2 targetPos = tileRects[4].anchoredPosition; // جای تایل آخر (از پایین وارد میشه)
            Vector2 outsideBottom = targetPos - new Vector2(0f, offset * 1.5f); // فاصله خارج از کادر پایینی

            GameObject tempTile = Instantiate(tilePrefabs[0], tileGrid[colIndex, 0].transform.parent);
            RectTransform tempRT = tempTile.GetComponent<RectTransform>();
            tempRT.anchoredPosition = outsideBottom;
            tempRT.SetAsLastSibling();

            Tile tempTileComp = tempTile.GetComponent<Tile>();
            tempTileComp.SetTile(colTileIDs[0], colSprites[0]);

            // انیمیشن ورود تایل موقت از پایین به جایگاه آخر
            tempRT.DOAnchorPos(targetPos, duration).SetEase(Ease.InOutQuad);

            // انیمیشن حرکت تایل‌های اصلی به بالا
            for (int y = 0; y < 5; y++)
            {
                Vector2 newPos = tileRects[y].anchoredPosition + new Vector2(0f, offset);
                tileRects[y].DOAnchorPos(newPos, duration).SetEase(Ease.InOutQuad);
            }

            yield return new WaitForSeconds(duration);

            // جابجایی منطقی داده‌ها (چرخش به بالا)
            for (int y = 0; y < 5; y++)
            {
                int from = (y + 1) % 5;
                tileGrid[colIndex, y].GetComponent<Tile>().SetTile(colTileIDs[from], colSprites[from]);
            }

            // تنظیم دقیق موقعیت‌های تایل‌ها روی گرید
            for (int y = 0; y < 5; y++)
            {
                RectTransform rt = tileGrid[colIndex, y].GetComponent<RectTransform>();
                float anchoredY = padding.top + y * -offset;
                rt.anchoredPosition = new Vector2(anchoredX, anchoredY);
            }

            Destroy(tempTile);
        }

        grid.enabled = true;
        LayoutRebuilder.ForceRebuildLayoutImmediate(grid.GetComponent<RectTransform>());

        StartCoroutine(ClearMatchesAndRespawnRoutine());

        isShifting = false;
    }

    //private IEnumerator ClearTilesOneByOneRoutine(List<(int x, int y)> positions, bool useFade)
    //{
    //    float delayBetween = 0.05f;

    //    foreach (var pos in positions)
    //    {
    //        GameObject tileObj = tileGrid[pos.x, pos.y];
    //        Tile tileComp = tileObj.GetComponent<Tile>();
    //        RectTransform tileRT = tileObj.GetComponent<RectTransform>();

    //        if (useFade)
    //        {
    //            // Fade animation
    //            CanvasGroup cg = tileObj.GetComponent<CanvasGroup>();
    //            if (cg == null) cg = tileObj.AddComponent<CanvasGroup>();

    //            cg.DOFade(0f, 0.2f).SetEase(Ease.InOutQuad).OnComplete(() =>
    //            {
    //                int randomIndex = Random.Range(0, tilePrefabs.Length);
    //                tileComp.SetTile(randomIndex, tilePrefabs[randomIndex].GetComponent<Tile>().tileImage.sprite);
    //                cg.alpha = 0f;
    //                cg.DOFade(1f, 0.3f).SetEase(Ease.OutQuad);
    //            });
    //        }
    //        else
    //        {
    //            // Scale animation - با تأخیر بین هر تایل
    //            yield return tileRT.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).WaitForCompletion();

    //            int randomIndex = Random.Range(0, tilePrefabs.Length);
    //            tileComp.SetTile(randomIndex, tilePrefabs[randomIndex].GetComponent<Tile>().tileImage.sprite);
    //            tileRT.localScale = Vector3.zero;

    //            yield return tileRT.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).WaitForCompletion();
    //        }

    //        yield return new WaitForSeconds(delayBetween);
    //    }
    //}

    private IEnumerator ClearTilesOneByOneRoutine(List<(int x, int y)> positions, bool useFade)
    {
        float delayBetween = 0.05f;

        foreach (var pos in positions)
        {
            GameObject tileObj = tileGrid[pos.x, pos.y];
            Tile tileComp = tileObj.GetComponent<Tile>();
            RectTransform tileRT = tileObj.GetComponent<RectTransform>();

            if (useFade)
            {
                CanvasGroup cg = tileObj.GetComponent<CanvasGroup>();
                if (cg == null) cg = tileObj.AddComponent<CanvasGroup>();

                yield return cg.DOFade(0f, 0.2f).SetEase(Ease.InOutQuad).WaitForCompletion();

                int randomIndex = Random.Range(0, tilePrefabs.Length);
                tileComp.SetTile(randomIndex, tilePrefabs[randomIndex].GetComponent<Tile>().tileImage.sprite);

                cg.alpha = 0f;
                yield return cg.DOFade(1f, 0.3f).SetEase(Ease.OutQuad).WaitForCompletion();
            }
            else
            {
                yield return tileRT.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).WaitForCompletion();

                int randomIndex = Random.Range(0, tilePrefabs.Length);
                tileComp.SetTile(randomIndex, tilePrefabs[randomIndex].GetComponent<Tile>().tileImage.sprite);

                tileRT.localScale = Vector3.zero;
                yield return tileRT.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).WaitForCompletion();
            }

            yield return new WaitForSeconds(delayBetween);
        }
    }

    private IEnumerator AnimateFiveCombo(List<(int x, int y)> positions)
    {
        float delayBetween = 0.1f;
        float punchDuration = 0.3f;

        foreach (var pos in positions)
        {
            GameObject tileObj = tileGrid[pos.x, pos.y];
            RectTransform tileRT = tileObj.GetComponent<RectTransform>();

            yield return tileRT.DOPunchScale(new Vector3(0.3f, 0.3f, 0), punchDuration, 10, 1).WaitForCompletion();

            yield return new WaitForSeconds(delayBetween);
        }
    }

    private void RespawnTilesAtPositions(List<(int x, int y)> positions)
    {
        foreach (var pos in positions)
        {
            GameObject tileObj = tileGrid[pos.x, pos.y];
            Tile tileComp = tileObj.GetComponent<Tile>();
            RectTransform tileRT = tileObj.GetComponent<RectTransform>();

            int randomIndex = Random.Range(0, tilePrefabs.Length);
            Sprite newSprite = tilePrefabs[randomIndex].GetComponent<Tile>().tileImage.sprite;

            tileComp.SetTile(randomIndex, newSprite);
            tileRT.localScale = Vector3.one;

            tileRT.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }
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

            // افزایش نوار سلامتی
            if (healthBar != null)
            {
                float healthToAdd = 0f;

                if (foundFive)
                    healthToAdd = 5 * 0.036f;
                else if (foundFour)
                    healthToAdd = 4 * 0.036f;

                healthBar.fillAmount += healthToAdd;
                healthBar.fillAmount = Mathf.Clamp01(healthBar.fillAmount);

                if (joinManager != null)
                    joinManager.AddFuseAmount(currentIndex, healthToAdd);
            }

            bool didClearRowOrColumn = false;

            // حذف ردیف‌ها و ستون‌های کامل (کمبو ۵)
            if (foundFive)
            {
                HashSet<int> rowsToClear = new();
                for (int y = 0; y < 5; y++)
                {
                    int count = 0;
                    foreach (var pos in matchedPositions)
                        if (pos.y == y) count++;
                    if (count >= 5) rowsToClear.Add(y);
                }

                foreach (int rowIndex in rowsToClear)
                {
                    yield return StartCoroutine(mycleanRow(rowIndex));
                    yield return new WaitForSeconds(0.3f);
                    ShiftRowsDownFrom(rowIndex);
                    yield return new WaitForSeconds(0.3f);
                }

                HashSet<int> colsToClear = new();
                for (int x = 0; x < 5; x++)
                {
                    int count = 0;
                    foreach (var pos in matchedPositions)
                        if (pos.x == x) count++;
                    if (count >= 5) colsToClear.Add(x);
                }

                foreach (int colIndex in colsToClear)
                {
                    yield return StartCoroutine(ClearColumn(colIndex));
                    yield return new WaitForSeconds(0.3f);
                    ShiftColumnsRightFrom(colIndex);
                    yield return new WaitForSeconds(0.3f);
                }

                didClearRowOrColumn = rowsToClear.Count > 0 || colsToClear.Count > 0;
            }

            if (!didClearRowOrColumn)
            {
                // اگر هم کمبو ۵ بود هم کمبو ۴ → fade حذف شود
                // اگر فقط کمبو ۵ بود → scale حذف شود

                bool useFade = foundFive && foundFour;
                bool useScale = foundFive && !foundFour;

                if (useFade)
                {
                    yield return StartCoroutine(ClearTilesOneByOneRoutine(matchedPositions, true)); // fade
                    RespawnTilesAtPositions(matchedPositions);
                    yield return new WaitForSeconds(0.2f);
                }
                else if (useScale)
                {
                    yield return StartCoroutine(AnimateFiveCombo(matchedPositions)); // punch scale
                    yield return StartCoroutine(ClearTilesOneByOneRoutine(matchedPositions, false)); // scale حذف
                    RespawnTilesAtPositions(matchedPositions);
                    yield return new WaitForSeconds(0.2f);
                }
            }
        }

        inputLocked = false;
    }


    private IEnumerator mycleanRow(int rowIndex)
    {
        for (int x = 0; x < 5; x++)
        {
            var tile = tileGrid[x, rowIndex].GetComponent<Tile>();
            var rt = tile.GetComponent<RectTransform>();

            tile.SetTile(-1, null); // حذف محتوای تایل
            rt.localScale = Vector3.zero; // اسکیل به صفر برای شروع

            yield return rt.DOScale(Vector3.one, 0.02f)
                .SetEase(Ease.InOutBounce)
                .WaitForCompletion();

            yield return new WaitForSeconds(0.02f); // تأخیر بین هر تایل
        }
    }
    private IEnumerator ClearColumn(int colIndex)
    {
        for (int y = 0; y < 5; y++)
        {
            var tile = tileGrid[colIndex, y].GetComponent<Tile>();
            var rt = tile.GetComponent<RectTransform>();

            tile.SetTile(-1, null); // حذف محتوای تایل
            rt.localScale = Vector3.zero; // اسکیل به صفر برای شروع

            yield return rt.DOScale(Vector3.one, 0.02f)
                .SetEase(Ease.InOutBounce)
                .WaitForCompletion();

            yield return new WaitForSeconds(0.02f); // تأخیر بین هر تایل
        }
    }

    //private IEnumerator AnimateFiveCombo(List<(int x, int y)> positions)
    //{
    //    float delayBetween = 1f;
    //    float punchDuration = 0.3f;

    //    foreach (var pos in positions)
    //    {
    //        GameObject tileObj = tileGrid[pos.x, pos.y];
    //        RectTransform tileRT = tileObj.GetComponent<RectTransform>();

    //        // انیمیشن Punch Scale و صبر تا کامل شدنش
    //        yield return tileRT.DOPunchScale(new Vector3(0.3f, 0.3f, 0), punchDuration, 10, 1).WaitForCompletion();

    //        // صبر کمی قبل از رفتن به تایل بعدی
    //        yield return new WaitForSeconds(delayBetween);
    //    }
    //}


    private void ShiftColumnsRightFrom(int startCol)
    {
        for (int x = startCol; x < 4; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                var rightTile = tileGrid[x + 1, y].GetComponent<Tile>();
                var currentTile = tileGrid[x, y].GetComponent<Tile>();
                currentTile.SetTile(rightTile.tileID, rightTile.tileImage.sprite);
            }
        }

        // اسپاون تایل رندوم برای ستون آخر (سمت راست)
        for (int y = 0; y < 5; y++)
        {
            int randomIndex = Random.Range(0, tilePrefabs.Length);
            var rightmostTile = tileGrid[4, y].GetComponent<Tile>();
            var newSprite = tilePrefabs[randomIndex].GetComponent<Tile>().tileImage.sprite;
            rightmostTile.SetTile(randomIndex, newSprite);
        }
    }


    private IEnumerator ClearRow(int rowIndex)
    {
        for (int x = 0; x < 5; x++)
        {
            var tile = tileGrid[x, rowIndex].GetComponent<Tile>();
            tile.SetTile(-1, null);
            RectTransform rt = tile.GetComponent<RectTransform>();
            rt.localScale = Vector3.zero;
            rt.DOScale(Vector3.one, 0.1f).SetEase(Ease.InOutBounce);
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void ShiftRowsDownFrom(int startRow)
    {
        for (int y = startRow; y > 0; y--)
        {
            for (int x = 0; x < 5; x++)
            {
                var upperTile = tileGrid[x, y - 1].GetComponent<Tile>();
                var currentTile = tileGrid[x, y].GetComponent<Tile>();
                currentTile.SetTile(upperTile.tileID, upperTile.tileImage.sprite);
            }
        }

        // بالاترین سطر ریسپاون رندوم جدید
        for (int x = 0; x < 5; x++)
        {
            int randomIndex = Random.Range(0, tilePrefabs.Length);
            var topTile = tileGrid[x, 0].GetComponent<Tile>();
            var newSprite = tilePrefabs[randomIndex].GetComponent<Tile>().tileImage.sprite;
            topTile.SetTile(randomIndex, newSprite);
        }
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
    //private void RespawnTilesAtPositions(List<(int x, int y)> positions)
    //{
    //    foreach (var pos in positions)
    //    {
    //        GameObject tileObj = tileGrid[pos.x, pos.y];
    //        Tile tileComp = tileObj.GetComponent<Tile>();
    //        RectTransform tileRT = tileObj.GetComponent<RectTransform>();

    //        // Assign new random sprite & tileID
    //        int randomIndex = Random.Range(0, tilePrefabs.Length);
    //        Sprite newSprite = tilePrefabs[randomIndex].GetComponent<Tile>().tileImage.sprite;

    //        tileComp.SetTile(randomIndex, newSprite);
    //        tileRT.localScale = Vector3.one; // Make sure scale reset

    //        // Optional: If you want a respawn animation here, add it
    //        tileRT.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    //    }
    //}

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
