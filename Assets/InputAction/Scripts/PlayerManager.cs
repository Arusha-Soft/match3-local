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
    }
    public void JoinGamePad(int index)
    {
        if (PlayerList.Where(p => p.PlayerID == index).Any())
            return;

        var p = SimplePool.Spawn(PlayerPrefab, Vector3.zero, PlayerPrefab.transform.rotation);
        p.transform.SetParent(PlayerParent, false);
        p.GetComponent<Player>().PlayerID = index;
        p.GetComponent<Player>().GamepadPlayer = Gamepad.all[index];
        PlayerList.Add(p.GetComponent<Player>());
    }
   
    public void BindPlayerOnBoard(int playerNo, int boardNo, int colorNo,int teamNo)
    {

        PlayerOnBoardList.Add(new PlayerOnBoard(playerNo, boardNo, colorNo, teamNo));
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

    #region test
    public Player JoinGamePadTest(int index)
    {
        if (PlayerList.Where(p => p.PlayerID == index).Any())
            return null;

        var p = SimplePool.Spawn(PlayerPrefab, Vector3.zero, PlayerPrefab.transform.rotation);
        p.transform.SetParent(PlayerParent, false);
        p.GetComponent<Player>().PlayerID = index;
        PlayerList.Add(p.GetComponent<Player>());
        return p.GetComponent<Player>();
    }
    #endregion
}
