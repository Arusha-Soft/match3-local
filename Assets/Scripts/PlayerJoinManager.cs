using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    private List<PlayerInput> players = new List<PlayerInput>();
    private List<Gamepad> joinedDevices = new List<Gamepad>();

    private List<GameObject> boards;

    private Coroutine countdownCoroutine;
    private float countdownDuration = 5f;

    // Track claims: index = boardIndex, value = playerIndex or null if unclaimed
    private int?[] initiallyClaimedByPlayer;
    private int?[] fullyClaimedByPlayer;

    private void Start()
    {
        boards = GameManager.instance.GetBoards();

        int count = boards.Count;
        initiallyClaimedByPlayer = new int?[count];
        fullyClaimedByPlayer = new int?[count];
    }

    private void Update()
    {
        foreach (var gamepad in Gamepad.all)
        {
            if (!joinedDevices.Contains(gamepad) && gamepad.buttonSouth.wasPressedThisFrame)
            {
                JoinPlayer(gamepad);
                break;
            }
        }
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

        icon.playerIndex = players.Count - 1; // assign player index (0-based)
        icon.SetColor(playerColors[icon.playerIndex]);
        icon.SetBoards(boards.ConvertAll(b => b.GetComponent<RectTransform>()).ToArray());
        icon.SetJoinManager(this);

        Debug.Log($"{playerInput.name} joined using {gamepad.displayName}");
    }

    // Initial claim (called on first click)
    public bool ClaimBoard(int boardIndex, int playerIndex)
    {
        if (boards == null || boardIndex < 0 || boardIndex >= boards.Count) return false;
        if (playerIndex < 0 || playerIndex >= playerSprites.Length) return false;

        // If already claimed by someone else initially or fully, can't claim
        if (initiallyClaimedByPlayer[boardIndex].HasValue && initiallyClaimedByPlayer[boardIndex].Value != playerIndex)
            return false;

        if (fullyClaimedByPlayer[boardIndex].HasValue && fullyClaimedByPlayer[boardIndex].Value != playerIndex)
            return false;

        initiallyClaimedByPlayer[boardIndex] = playerIndex;

        var board = boards[boardIndex];
        var image = board.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = playerSprites[playerIndex]; // Show player's sprite to indicate claim
        }

        return true;
    }

    // Full claim (called on second confirm)
    public void LockBoard(int boardIndex, int playerIndex)
    {
        if (boards == null || boardIndex < 0 || boardIndex >= boards.Count) return;
        if (playerIndex < 0 || playerIndex >= playerSprites.Length) return;

        // Must be initially claimed by same player
        if (initiallyClaimedByPlayer[boardIndex] != playerIndex) return;

        fullyClaimedByPlayer[boardIndex] = playerIndex;

        Debug.Log($"Board {boardIndex} fully claimed by player {playerIndex}");
    }

    // Undo initial claim (before full lock)
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
    }

    // Check if a player can select/claim the board
    // Return true if board is unclaimed or claimed by this player (initial or full)
    public bool CanClaimBoard(int boardIndex, int playerIndex)
    {
        if (boards == null || boardIndex < 0 || boardIndex >= boards.Count) return false;
        if (playerIndex < 0 || playerIndex >= playerSprites.Length) return false;

        // Allowed if unclaimed fully and initially
        if (!initiallyClaimedByPlayer[boardIndex].HasValue && !fullyClaimedByPlayer[boardIndex].HasValue)
            return true;

        // Allowed if claimed initially or fully by this same player
        if (initiallyClaimedByPlayer[boardIndex] == playerIndex) return true;
        if (fullyClaimedByPlayer[boardIndex] == playerIndex) return true;

        // Otherwise no
        return false;
    }

    // Called by PlayerIconController when a player confirms (full claim)
    public void OnPlayerClaimed(int boardIndex, int playerIndex)
    {
        LockBoard(boardIndex, playerIndex);

        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);

        countdownCoroutine = StartCoroutine(StartCountdown());
    }

    private IEnumerator StartCountdown()
    {
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
    }
}
