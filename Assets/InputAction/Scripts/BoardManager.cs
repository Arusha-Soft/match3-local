using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    public List<Board> BoardList = new List<Board>();
    public GameObject FreeToAllPanel, TeamPanel;
    public GameObject BoardPrefab;
    public Transform PlayerBoardParentFreeToAll;
    public Transform PlayerBoardParentTeam;
    public Text ButtonText;
    public bool isFreeToAll = true;

    public Sprite DefaultSprite;
    public Sprite[] playerSprites;
    public Sprite[] BoardSprites;
    public Color[] playerColors;
    public int LimitFreeToAll = 3;
    public int LimitTeam = 8;
    public int TeamCount = 4;
    public TeamManager teamManager;
    private void Awake()
    {
        Instance = this;
        for (int i = 0; i < TeamCount; i++)
            teamManager.CreateTeam(i, playerColors[i]);
    }
    void Start()
    {
        Init();
    }
    public void Init()
    {
        BoardList.Clear();
        SetModeButoon(isFreeToAll);
        SpawnFreeToAllBoards();
        //SpawnTeamBoards();
    }
    private void SpawnFreeToAllBoards()
    {
        for (int i = 0; i < LimitFreeToAll - 1; i++)
        {
            GeneratePlayerBoard(i,PlayerBoardParentFreeToAll);
        }
    }
    private void SpawnTeamBoards()
    {
        for (int i = 0; i < LimitTeam; i++)
        {
            GeneratePlayerBoard(i,PlayerBoardParentTeam);
        }
    }
    private void GeneratePlayerBoard(int id, Transform PlayerBoardParent)
    {
        var board = Instantiate(BoardPrefab, Vector3.zero, BoardPrefab.transform.rotation);
        board.transform.SetParent(PlayerBoardParent, false);
        board.name = id.ToString();
        board.GetComponent<Board>().BoardID = id;
        BoardList.Add(board.GetComponent<Board>());
    }
    public void GeneratePlayerBoard(int id)
    {
        if (BoardList.Where(b => b.BoardID == id).Any())
            return;

        var board = Instantiate(BoardPrefab, Vector3.zero, BoardPrefab.transform.rotation);
        board.transform.SetParent(PlayerBoardParentFreeToAll, false);
        board.name = id.ToString();
        board.GetComponent<Board>().BoardID = id;
        BoardList.Add(board.GetComponent<Board>());
    }
    private void RemoveBoards()
    {
        foreach (var board in BoardList)
            Destroy(board.gameObject);

        BoardList.Clear();

    }
    public void ToggleButton()
    {
        isFreeToAll = !isFreeToAll;

        RemoveBoards();
        if (isFreeToAll)
            SpawnFreeToAllBoards();
        else
            SpawnTeamBoards();

        PlayerManager.Instance.PlayerOnBoardList.Clear();
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
    //public void AssignBoardToTeam(int boardId, int teamId)
    //{
    //    var board = BoardList.FirstOrDefault(p => p.BoardID == boardId);
    //    var team = TeamManager.Instance.GetTeamByName(teamId);
    //    if (board != null && team != null)
    //        board.JoinTeam(team);
    //}
    void Update()
    {
        
    }
}
