using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class JoinManager : MonoBehaviour
{
    public GameObject CursorPrefab;
    public Transform CursorParent;

    public GameObject FreeToAllPanel,TeamPanel;
    public GameObject PlayerBoardPrefabFreeToAll;
    public Transform PlayerBoardParentFreeToAll;
    public GameObject PlayerBoardPrefabTeam;
    public Transform PlayerBoardParentTeam;
    public List<PlayerOnBoard> PlayerOnBoardList = new List<PlayerOnBoard>();
    public List<GameObject> BoardFreeToAllList = new List<GameObject>();
    private List<GameObject> BoardTeamList = new List<GameObject>();
    public List<int> NumberPlayerList = new List<int>();
    private List<GameObject> CursorList = new List<GameObject>();
    public Text ButtonText;
    private bool isFreeToAll = true;

    public static JoinManager Instance;
    public Color[] playerColors;
    public Sprite DefaultSprite;
    public Sprite[] playerSprites;
    public Sprite[] CursorSprites;
    public Sprite[] NumberSprites;
    private int LimitFreeToAll = 3;
    private int LimitTeam = 8;

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        CursorList.Clear();
        NumberPlayerList.Clear();

        SetModeButoon(isFreeToAll);
        SpawnFreeToAllBoards();
        SpawnTeamBoards();
    }
    private void SpawnFreeToAllBoards()
    {
        for (int i = 0; i < LimitFreeToAll - 1; i++)
        {
            GeneratePlayerBoard(PlayerBoardPrefabFreeToAll, PlayerBoardParentFreeToAll, i, BoardFreeToAllList);
        }
    }
    private void SpawnTeamBoards()
    {
        for (int i = 0; i < LimitTeam; i++)
        {
            GeneratePlayerBoard(PlayerBoardPrefabTeam, PlayerBoardParentTeam, i, BoardTeamList);
        }
    }
    private void GeneratePlayerBoard(GameObject PlayerBoardPrefab, Transform PlayerBoardParent,int id, List<GameObject> BoardList)
    {
        var board = SimplePool.Spawn(PlayerBoardPrefab, Vector3.zero, PlayerBoardPrefab.transform.rotation);
        board.transform.SetParent(PlayerBoardParent,false);
        board.name = id.ToString();
        board.GetComponent<PlayerBoard>().boardIndex = id;
        BoardList.Add(board);
    }
    //private void RemoveBoards()
    //{
    //    foreach (var board in BoardList)
    //        SimplePool.Despawn(board);

    //    BoardList.Clear();

    //}
    public void ToggleButton()
    {
        isFreeToAll = !isFreeToAll;

        //RemoveBoards();
        //if (isFreeToAll)
        //    SpawnFreeToAllBoards();
        //else
        //    SpawnTeamBoards();

        SetModeButoon(isFreeToAll);
    }
    private void SetModeButoon(bool isFreeToAll)
    {
        FreeToAllPanel.SetActive(isFreeToAll);
        TeamPanel.SetActive(!isFreeToAll);
        if (isFreeToAll)
        {
            ButtonText.text = "FreeToAll";
        }
        else
        {
            ButtonText.text = "Team";
        }
    }
    void Update()
    {
        var gamepads = Gamepad.all;
        if(gamepads.Count>0)
        {
            for (int i = 0; i < gamepads.Count; i++)
            {
                if (isFreeToAll)
                {
                    if (i < LimitFreeToAll)
                    {
                        if (i == 2)
                        {
                            //GeneratePlayerBoard(PlayerBoardPrefabFreeToAll, PlayerBoardParentFreeToAll, 2);
                            //CheckJoinGame(i);
                        }
                        else
                            CheckJoinGame(i);
                    }
                }
                else
                {
                    if (i < LimitTeam)
                    {
                        CheckJoinGame(i);
                    }

                }
            }

        }

        var mouse = Mouse.current;
        if(mouse != null)
        {
            if (!NumberPlayerList.Contains(2))
            {
                if (mouse.leftButton.wasPressedThisFrame)
                {
                    GeneratePlayerBoard(PlayerBoardPrefabFreeToAll, PlayerBoardParentFreeToAll, 2, BoardFreeToAllList);
                    var cursor = SimplePool.Spawn(CursorPrefab, Vector3.zero, CursorPrefab.transform.rotation);
                    cursor.transform.SetParent(CursorParent, false);
                    CursorList.Add(cursor);
                    NumberPlayerList.Add(2);
                    cursor.GetComponent<PlayerCursor>().playerNumber = 2;
                    cursor.GetComponent<PlayerCursor>().mouse = mouse;
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
        if (NumberPlayerList.Contains(index))
            return;

        var cursor = SimplePool.Spawn(CursorPrefab, Vector3.zero, CursorPrefab.transform.rotation);
        cursor.transform.SetParent(CursorParent, false);
        CursorList.Add(cursor);
        NumberPlayerList.Add(index);
        cursor.GetComponent<PlayerCursor>().playerNumber = index;
        cursor.GetComponent<PlayerCursor>().GamepadPlayer = Gamepad.all[index];
    }

    public void BindPlayerOnBoard(int playerNumber, int PlayerBoard)
    {
        PlayerOnBoardList.Add(new PlayerOnBoard(playerNumber, PlayerBoard));
    }
    public void UnBindPlayerOnBoard(int playerNumber, int PlayerBoard)
    {
        var item=PlayerOnBoardList.Where((pon) => pon.PlayerNumber == playerNumber).FirstOrDefault();
        if (item != null)
            PlayerOnBoardList.Remove(item);
    }
    public bool CheckPlayerOnBoard(int playerNumber)
    {
       return PlayerOnBoardList.Where((pon)=>pon.PlayerNumber == playerNumber).Any(); 
    }
    public bool CheckBoardDontUse(int boardIndex)
    {
        return PlayerOnBoardList.Where((pon) => pon.PlayerBoard == boardIndex).Any();
    }
    public bool CheckPlayerAndBoard(int playerNumber, int boardIndex)
    {
        return PlayerOnBoardList.Where((pon) => (pon.PlayerNumber == playerNumber && pon.PlayerBoard == boardIndex)).Any();
    }

}
[Serializable]
public class PlayerOnBoard
{ 
    public int PlayerNumber;
    public int PlayerBoard;
    public PlayerOnBoard(int playerNumber, int playerBoard)
    {
        this.PlayerNumber = playerNumber;
        this.PlayerBoard = playerBoard;
    }

}
