using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System;

public class PlayerCountSelector : MonoBehaviour
{
    public static Action<int> OnCreateBoard;
    [Header("UI References")]
    public GameObject selectionPanel;
    public TextMeshProUGUI playerCountText;

    [Header("Player Count Limits")]
    public int minPlayers = 1;
    public int maxPlayers = 8;

    private int currentCount = 1;
    private bool panelActive = true;

    private void Start()
    {
        currentCount = minPlayers;
        UpdateText();
    }


    private void Update()
    {
        if (!panelActive) return;

        // Gamepad or Keyboard Up
        if ((Gamepad.current != null && Gamepad.current.dpad.up.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.leftStick.up.wasPressedThisFrame) ||
            Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            IncreaseCount();
        }

        // Gamepad or Keyboard Down
        if ((Gamepad.current != null && Gamepad.current.dpad.down.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.leftStick.down.wasPressedThisFrame) ||
            Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            DecreaseCount();
        }

        // Gamepad South Button or Enter key to confirm
        if ((Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame) ||
            Keyboard.current.enterKey.wasPressedThisFrame)
        {
            ConfirmSelection();
        }
    }

    void IncreaseCount()
    {
        if (currentCount < maxPlayers)
        {
            currentCount++;
            UpdateText();
        }
    }

    void DecreaseCount()
    {
        if (currentCount > minPlayers)
        {
            currentCount--;
            UpdateText();
        }
    }

    void UpdateText()
    {
        playerCountText.text = currentCount.ToString();
    }

    void ConfirmSelection()
    {
        panelActive = false;
        selectionPanel.SetActive(false);

        GameManager.instance.numberofBoards = currentCount;
        Debug.Log("Number of board set  to " + currentCount);
        selectionPanel.SetActive(false);
        OnCreateBoard?.Invoke(currentCount);
        //// Clear previous boards if any
        //foreach (Transform child in GameManager.instance.boardParent)
        //{
        //    Destroy(child.gameObject);
        //}

        //// Manually recreate the boards
        //typeof(GameManager)
        //    .GetMethod("CreateBoards", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
        //    ?.Invoke(GameManager.instance, new object[] { currentCount });
    }

}
