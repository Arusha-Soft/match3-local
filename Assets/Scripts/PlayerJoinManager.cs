using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJoinManager : MonoBehaviour
{
    [Header("UI Setup")]
    public Transform canvasTransform;
    public GameObject playerInputPrefab;

    [Header("Player Colors")]
    public Color[] playerColors;

    private List<PlayerInput> players = new List<PlayerInput>();
    private List<Gamepad> joinedDevices = new List<Gamepad>();

    private List<GameObject> boards;

    private void Start()
    {
        boards = GameManager.instance.GetBoards();
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
        icon.SetColor(playerColors[players.Count - 1]);
        icon.SetBoards(boards.ConvertAll(b => b.GetComponent<RectTransform>()).ToArray());

        Debug.Log($"{playerInput.name} joined using {gamepad.displayName}");
    }
}
