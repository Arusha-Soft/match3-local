using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Networking.PlayerConnection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    public List<Board> BoardList = new List<Board>();
    public GameObject BoardPrefab;
    public Transform BoardPanel;
    public GameObject player2Panel, player3Panel, player4Panel, player8Panel;
    public List<Vector3> player2Pos, player3Pos, player4Pos, player8Pos;
    public Text ButtonText;
    public bool isFreeToAll = true;

    public Sprite DefaultSprite;
    public Sprite[] BoardSprites;
    public Color[] playerColors;

    public int DefaultPlayer = 2;
    public int LimitPlayer = 8;
    public int TeamCount = 4;
    public int PlayerCount;
    public TeamManager teamManager;
    private int gamePadCount;

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
        InitWorldPos(player2Panel, player2Pos);
        InitWorldPos(player3Panel, player3Pos);
        InitWorldPos(player4Panel, player4Pos);
        InitWorldPos(player8Panel, player8Pos);


        gamePadCount = Gamepad.all.Count;
        if (gamePadCount < 2)
            PlayerCount = DefaultPlayer;
        else
            PlayerCount = gamePadCount;

        BoardList.Clear();
        SetModeButoon(isFreeToAll);
        SpawnBoards(PlayerCount);
    }
    private void InitWorldPos(GameObject playerPanel,List<Vector3> playerPos)
    {
        for (int i = 0; i < playerPanel.transform.childCount; i++)
        {
            Transform transform = playerPanel.transform.GetChild(i);
            playerPos.Add(GetWorldPosition(transform.GetComponent<RectTransform>(), transform.position));
        }

    }
    public List<Vector3> GetWorldPosListByPlayerCount()
    {
        if (PlayerCount <= 2)
            return player2Pos;
        else if (PlayerCount == 3)
            return player3Pos;
        else if (PlayerCount == 4)
            return player4Pos;
        else 
            return player8Pos;
    }
    private void SpawnBoards(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GenerateBoard(i);
        }
    }
    public void SpawnOneBoard()
    {
        if (PlayerCount > LimitPlayer)
            return;
        PlayerCount++;
        GenerateBoard(PlayerCount-1);
    }
   
    private void GenerateBoard(int id)
    {
        if (BoardList.Where(b => b.BoardID == id).Any())
            return;

        var board = Instantiate(BoardPrefab, Vector3.zero, BoardPrefab.transform.rotation);
        board.transform.SetParent(BoardPanel, false);
        board.name = id.ToString();
        board.GetComponent<Board>().BoardID = id;
        BoardList.Add(board.GetComponent<Board>());
        ReSizeAllBoard();
    }
    private void ReSizeAllBoard()
    {
        for (int i = 0; i < BoardList.Count; i++)
        {
            GameObject playerPanelPosition = GetPlayerPanelForPosition();
            BoardList[i].transform.localScale = playerPanelPosition.transform.GetChild(i).transform.localScale;
            BoardList[i].transform.position = playerPanelPosition.transform.GetChild(i).position;
        }
    }
    private GameObject GetPlayerPanelForPosition()
    {
        switch (PlayerCount)
        {
            case 2:
                return player2Panel;
            case 3:
                return player3Panel;
            case 4:
                return player4Panel;
            default:
                return player8Panel;
        }
        

    }
    //private void RemoveBoards()
    //{
    //    foreach (var board in BoardList)
    //        Destroy(board.gameObject);

    //    BoardList.Clear();

    //}
    public void ToggleButton()
    {
        isFreeToAll = !isFreeToAll;

        PlayerManager.Instance.PlayerOnBoardList.Clear();
        SetModeButoon(isFreeToAll);
    }
    private void SetModeButoon(bool isFreeToAll)
    {
        if (isFreeToAll)
        {
            ButtonText.text = "FreeToAll";
        }
        else
        {
            ButtonText.text = "Team";
        }
    }

    public void CheckGameStart(List<PlayerOnBoard> playerOnBoardList)
    {
        if (isFreeToAll)
        {
            if (playerOnBoardList.Count == BoardList.Count)
            {
                Debug.Log("Game ready to start");
                PlayerManager.PlayerOnBoardEvent?.Invoke(playerOnBoardList);
            }
        }
        else
        {
            if (playerOnBoardList.Count == BoardList.Count)
            {
                Debug.Log("Game ready to start");
                PlayerManager.PlayerOnBoardEvent?.Invoke(playerOnBoardList);
            }
        }
    }

    void Update()
    {
        var gamepads = Gamepad.all;
        if (gamePadCount < gamepads.Count)
        {
            int diff = gamepads.Count - gamePadCount;
            gamePadCount = gamepads.Count;
            for (int i = 0; i < diff; i++)
            {
                SpawnOneBoard();
            }
        }
        
        for (int i = 0; i < gamepads.Count; i++)
            CheckJoinGame(i);
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            SpawnOneBoardTest();
        }
    }

    private void CheckJoinGame(int index)
    {
        if (Gamepad.all[index].buttonSouth.wasPressedThisFrame ||
                    Keyboard.current.enterKey.wasPressedThisFrame)
        {
            PlayerManager.Instance.JoinGamePad(index);
        }
    }

    private Vector3 GetWorldPosition(RectTransform rectTransform,Vector3 worldPos)
    {
        Vector3 worldPosition;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
           rectTransform,
            RectTransformUtility.WorldToScreenPoint(null, worldPos),
            null,
            out worldPosition
        );
        Vector3 pos = worldPosition;
        pos.z = 0;
        worldPosition = pos;
        return worldPosition;
    }

    #region test
    public void SpawnOneBoardTest()
    {
        Debug.Log(PlayerCount);
        if (PlayerCount >= LimitPlayer)
            return;
        PlayerCount++;
        GenerateBoardTest(PlayerCount - 1);
        Player player = PlayerManager.Instance.JoinGamePadTest(PlayerCount-1);
        if (player == null)
            return;

        BoardList[PlayerCount - 1].OnClick(BoardList[PlayerCount - 1].gameObject, player.PlayerID, true);
    }
    private void GenerateBoardTest(int id)
    {
        if (BoardList.Where(b => b.BoardID == id).Any())
            return;

        var board = Instantiate(BoardPrefab, Vector3.zero, BoardPrefab.transform.rotation);
        board.transform.SetParent(BoardPanel, false);
        board.name = id.ToString();
        board.GetComponent<Board>().BoardID = id;
        BoardList.Add(board.GetComponent<Board>());
        ReSizeAllBoard();
    }
    #endregion
}
