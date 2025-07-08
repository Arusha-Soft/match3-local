using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerJoinManager : MonoBehaviour
{
    [Header("UI Setup")]
    public Transform canvasTransform;
    public GameObject playerInputPrefab;
    public Sprite defaulBoard;

    [Header("Player Colors")]
    public Color[] playerColors;

    [Header("Player Sprites (Match order of playerColors)")]
    public Sprite[] playerSprites;

    [Header("Fuse Settings")]
    public float fuseDurationSeconds = 10f;
    private float fuseTickInterval = 1f;
    private float fuseDecreasePercentPerTick;

    private List<PlayerInput> players = new List<PlayerInput>();
    private List<Gamepad> joinedDevices = new List<Gamepad>();

    private List<GameObject> boards;

    private Coroutine countdownCoroutine;
    private float countdownDuration = 1f;

    private int?[] initiallyClaimedByPlayer;
    private int?[] fullyClaimedByPlayer;

    private float[] fuseAmounts;
    private Image[] fuseImages;

    private bool gameOver = false;
    private bool waitingForRestart = false;
    private bool gameStarted = false; // 🚫 NEW: Flag to lock out new players

    private void Start()
    {
        boards = GameManager.instance.GetBoards();

        int count = boards.Count;
        initiallyClaimedByPlayer = new int?[count];
        fullyClaimedByPlayer = new int?[count];

        fuseAmounts = new float[count];
        fuseImages = new Image[count];
        fuseDecreasePercentPerTick = 1f / fuseDurationSeconds;

        for (int i = 0; i < count; i++)
        {
            fuseAmounts[i] = 1f;

            var fuseImageTransform = boards[i].transform.Find("Fuse");
            if (fuseImageTransform != null)
            {
                fuseImages[i] = fuseImageTransform.GetComponent<Image>();
                fuseImages[i].fillAmount = 1f;
                fuseImages[i].gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning($"Board {i} missing Fuse Image child");
            }
        }
    }

    private void Update()
    {
        if (gameOver)
        {
            if (waitingForRestart && Keyboard.current.rKey.wasPressedThisFrame)
            {
                Debug.Log("Restarting game...");
                ResetGame();
            }
            return;
        }

        // ✅ Prevent new players from joining after the game has started
        if (gameStarted) return;

        foreach (var gamepad in Gamepad.all)
        {
            if (!joinedDevices.Contains(gamepad) && gamepad.buttonSouth.wasPressedThisFrame)
            {
                JoinPlayer(gamepad);
                break;
            }
        }
    }

    public void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void JoinPlayer(Gamepad gamepad)
    {
        PlayerInput playerInput = PlayerInput.Instantiate(
            playerInputPrefab,
            playerIndex: players.Count,
            controlScheme: "GamePad",
            pairWithDevice: gamepad
        );

        playerInput.transform.SetParent(canvasTransform, false);
        playerInput.name = "Player " + (players.Count + 1);
        players.Add(playerInput);
        joinedDevices.Add(gamepad);

        var icon = playerInput.GetComponent<PlayerIconController>();

        icon.playerIndex = players.Count - 1;
        icon.SetColor(playerColors[icon.playerIndex]);
        icon.SetBoards(boards.ConvertAll(b => b.GetComponent<RectTransform>()).ToArray());
        icon.SetJoinManager(this);

        Debug.Log($"{playerInput.name} joined using {gamepad.displayName}");
        UIManager.Instance.SetText(playerInput.name + " joined");
    }

    public bool ClaimBoard(int boardIndex, int playerIndex)
    {
        if (boards == null || boardIndex < 0 || boardIndex >= boards.Count) return false;
        if (playerIndex < 0 || playerIndex >= playerSprites.Length) return false;

        if (initiallyClaimedByPlayer[boardIndex].HasValue && initiallyClaimedByPlayer[boardIndex].Value != playerIndex)
            return false;

        if (fullyClaimedByPlayer[boardIndex].HasValue && fullyClaimedByPlayer[boardIndex].Value != playerIndex)
            return false;

        initiallyClaimedByPlayer[boardIndex] = playerIndex;

        var board = boards[boardIndex];
        var image = board.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = playerSprites[playerIndex];
        }

        return true;
    }

    public void LockBoard(int boardIndex, int playerIndex)
    {
        if (boards == null || boardIndex < 0 || boardIndex >= boards.Count) return;
        if (playerIndex < 0 || playerIndex >= playerSprites.Length) return;

        if (initiallyClaimedByPlayer[boardIndex] != playerIndex) return;

        fullyClaimedByPlayer[boardIndex] = playerIndex;

        Debug.Log($"Board {boardIndex} fully claimed by player {playerIndex}");
    }

    public void ResetBoard(int boardIndex)
    {
        if (boards == null || boardIndex < 0 || boardIndex >= boards.Count) return;

        initiallyClaimedByPlayer[boardIndex] = null;
        fullyClaimedByPlayer[boardIndex] = null;

        var board = boards[boardIndex];
        var image = board.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = defaulBoard;
        }

        fuseAmounts[boardIndex] = 1f;
        if (fuseImages[boardIndex] != null)
        {
            fuseImages[boardIndex].fillAmount = 1f;
            fuseImages[boardIndex].gameObject.SetActive(false);
        }
    }

    public bool CanClaimBoard(int boardIndex, int playerIndex)
    {
        if (boards == null || boardIndex < 0 || boardIndex >= boards.Count) return false;
        if (playerIndex < 0 || playerIndex >= playerSprites.Length) return false;

        if (!initiallyClaimedByPlayer[boardIndex].HasValue && !fullyClaimedByPlayer[boardIndex].HasValue)
            return true;

        if (initiallyClaimedByPlayer[boardIndex] == playerIndex) return true;
        if (fullyClaimedByPlayer[boardIndex] == playerIndex) return true;

        return false;
    }

    public void OnPlayerClaimed(int boardIndex, int playerIndex)
    {
        LockBoard(boardIndex, playerIndex);

        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);

        countdownCoroutine = StartCoroutine(StartCountdown());
    }

    private IEnumerator StartCountdown()
    {
        gameStarted = true; // ✅ Prevent new players from joining now

        float timer = countdownDuration;

        while (timer > 0f)
        {
            Debug.Log($"Game starting in {timer:F0}...");
            yield return new WaitForSeconds(1f);
            timer -= 1f;
        }

        Debug.Log("Countdown finished! Spawning puzzles...");

        foreach (var playerInput in players)
        {
            var icon = playerInput.GetComponent<PlayerIconController>();
            if (icon != null && icon.HasClaimed)
            {
                icon.SpawnPuzzleAfterCountdown();
            }
        }

        for (int i = 0; i < fuseImages.Length; i++)
        {
            if (fuseImages[i] != null)
                fuseImages[i].gameObject.SetActive(true);
        }

        StartCoroutine(FuseCountdown());
    }

    private IEnumerator FuseCountdown()
    {
        while (!gameOver)
        {
            yield return new WaitForSeconds(fuseTickInterval);

            for (int i = 0; i < fuseAmounts.Length; i++)
            {
                if (fullyClaimedByPlayer[i].HasValue)
                {
                    fuseAmounts[i] -= fuseDecreasePercentPerTick * fuseTickInterval;
                    fuseAmounts[i] = Mathf.Clamp01(fuseAmounts[i]);
                    if (fuseImages[i] != null)
                    {
                        fuseImages[i].fillAmount = fuseAmounts[i];
                    }

                    if (fuseAmounts[i] <= 0f)
                    {
                        gameOver = true;
                        waitingForRestart = true;
                        Debug.Log($"Game Over! Player {fullyClaimedByPlayer[i].Value} fuse reached 0.");
                        PrintFuseAndScores();
                        yield break;
                    }
                }
            }
        }
    }

    public void AddFuseAmount(int boardIndex, float amount)
    {
        if (gameOver) return;
        if (boardIndex < 0 || boardIndex >= fuseAmounts.Length) return;

        fuseAmounts[boardIndex] += amount;
        fuseAmounts[boardIndex] = Mathf.Clamp01(fuseAmounts[boardIndex]);

        if (fuseImages[boardIndex] != null)
        {
            fuseImages[boardIndex].fillAmount = fuseAmounts[boardIndex];
        }

        Debug.Log($"Board {boardIndex} fuse increased by {amount * 100f}%. New fuse: {fuseAmounts[boardIndex] * 100f}%");
    }

    private void PrintFuseAndScores()
    {
        for (int i = 0; i < players.Count; i++)
        {
            float fuse = (i < fuseAmounts.Length) ? fuseAmounts[i] : 0f;
            Debug.Log($"Player {i} fuse left: {fuse * 100f}%");

            var icon = players[i].GetComponent<PlayerIconController>();
            if (icon != null)
            {
                Debug.Log($"Player {i} score: {icon.GetScore()}");
            }
        }
    }
}
