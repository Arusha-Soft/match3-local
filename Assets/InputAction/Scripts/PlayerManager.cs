using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    public List<Player> PlayerList = new List<Player>();
    public List<PlayerOnBoard> PlayerOnBoardList = new List<PlayerOnBoard>();
    public GameObject PlayerPrefab;
    public Transform PlayerParent;
    public Sprite[] CursorSprites;
    public Sprite[] NumberSprites;
    public BoardManager boardManager;

    public delegate void PlayerOnBoardHandler(List<PlayerOnBoard> playerOnBoardList);
    public static PlayerOnBoardHandler PlayerOnBoardEvent;
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        Init();
    }
    public void Init()
    {
        PlayerList.Clear();

    }
    void Update()
    {
        var gamepads = Gamepad.all;
        if (gamepads.Count > 0)
        {
            for (int i = 0; i < gamepads.Count; i++)
            {
                if (boardManager.isFreeToAll)
                {
                    if (i < boardManager.LimitFreeToAll)
                    {
                        if (i == 2)
                        {
                            boardManager.GeneratePlayerBoard(2);
                            CheckJoinGame(i);
                        }
                        else
                            CheckJoinGame(i);
                    }
                }
                else
                {
                    if (i < boardManager.LimitTeam)
                    {
                        CheckJoinGame(i);
                    }

                }
            }

        }
    }
    private void CheckJoinGame(int index)
    {
        if (Gamepad.all[index].buttonSouth.wasPressedThisFrame ||
                    Keyboard.current.enterKey.wasPressedThisFrame)
        {
            JoinGamePad(index);
        }
    }
    void JoinGamePad(int index)
    {
        //if (NumberPlayerList.Contains(index))
        //    return;

        if (PlayerList.Where(p => p.PlayerID == index).Any())
            return;

        var p = SimplePool.Spawn(PlayerPrefab, Vector3.zero, PlayerPrefab.transform.rotation);
        p.transform.SetParent(PlayerParent, false);
        p.GetComponent<Player>().PlayerID = index;
        p.GetComponent<Player>().GamepadPlayer = Gamepad.all[index];
        PlayerList.Add(p.GetComponent<Player>());
    }

    public void BindPlayerOnBoard(int playerNo, int BoardNo, int ColorNo)
    {
        PlayerOnBoardList.Add(new PlayerOnBoard(playerNo, BoardNo, ColorNo, boardManager.playerSprites[BoardNo]));
        boardManager.CheckGameStart(PlayerOnBoardList);
    }
    public void UnBindPlayerOnBoard(int playerNumber, int PlayerBoard)
    {
        var item = PlayerOnBoardList.Where((pon) => pon.PlayerNo == playerNumber).FirstOrDefault();
        if (item != null)
            PlayerOnBoardList.Remove(item);
    }
    public bool CheckPlayerOnBoard(int playerNumber)
    {
        return PlayerOnBoardList.Where(pon => pon.PlayerNo == playerNumber).Any();
    }
    public bool CheckBoardDontUse(int boardIndex)
    {
        return PlayerOnBoardList.Where((pon) => pon.BoardNo == boardIndex).Any();
    }
    public bool CheckPlayerAndBoard(int playerNumber, int boardIndex)
    {
        return PlayerOnBoardList.Where((pon) => (pon.PlayerNo == playerNumber && pon.BoardNo == boardIndex)).Any();
    }

}
