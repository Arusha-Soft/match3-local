using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerInputManagerMe : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnpoints;

    private bool wasdJoined = false;
    private bool arrowsJoined = false;

    private HashSet<Gamepad> joinedGamepads = new HashSet<Gamepad>();
    private int playerIndex = 0;

    private void Update()
    {
        if (Keyboard.current == null) return;

        // WASD Join
        if (!wasdJoined && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            var player = PlayerInput.Instantiate(playerPrefab, controlScheme: "WASD", pairWithDevice: Keyboard.current);
            if (spawnpoints.Length > 0 && playerIndex < spawnpoints.Length)
                player.transform.position = spawnpoints[playerIndex].position;

            wasdJoined = true;
            playerIndex++;
        }

        // Gamepad Join
        foreach (var gamepad in Gamepad.all)
        {
            if (!joinedGamepads.Contains(gamepad) && gamepad.buttonSouth.wasPressedThisFrame)
            {
                var player = PlayerInput.Instantiate(playerPrefab, controlScheme: "GamePad", pairWithDevice: gamepad);

                if (spawnpoints.Length > 0 && playerIndex < spawnpoints.Length)
                    player.transform.position = spawnpoints[playerIndex].position;

                joinedGamepads.Add(gamepad);
                Debug.Log("Gamepad joined: " + gamepad);

                playerIndex++;
            }
        }
    }
}
