using Project.Core;
using Project.Factions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public Canvas canvas;
    public GameObject BoardPanel;
    public GameObject FinishePanel;
    public GameObject CoreGameParent;
    public GameObject BoardPrefab;
    public BoardManager boardManager;
    public PlayerManager playerManager;
    private Camera camera;
    private List<GameObject> boardsWorld=new List<GameObject>();
    private List<Vector3> originalWorldSizeSpriteRender= new List<Vector3>();
    public GameObject player2Panel, player3Panel, player4Panel, player8Panel;
    public Sprite[] BoardSprites;
    public Sprite[] SelectSprites;

    private float previousAspect = -1f;
    private float aspectCheckTimer = 0f;
    private float aspectStableTime = 0.2f;
    private bool isChangeing = false;
    public static GameController Instance;
    public Text FinidhedText;
    private List<BoardIdentity> activeBoards=new List<BoardIdentity>();
    private bool isFinisheGame;
    public Action GameFinished;

    public enum Orientation
    {
        Portrait,
        Landscape
    }
    private void Awake()
    {
        Instance = this;
    }
    public Orientation currentOrientation= Orientation.Landscape;
    void Start()
    {
        InitGame();
        camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        PlayerManager.PlayerOnBoardEvent += OnStartGame;
        previousAspect = GetAspectRatio();
    }
    public void InitGame()
    {
        BoardPanel.SetActive(true);
        boardManager.Init();
        playerManager.Init();
        FinishePanel.SetActive(false);
        RemoveWorldBoards();
        isFinisheGame = false;

    }
    private void OnDestroy()
    {
        PlayerManager.PlayerOnBoardEvent -= OnStartGame;
    }

    private void OnStartGame(List<PlayerOnBoard> playerOnBoardList)
    {
        var sortedList = playerOnBoardList.OrderBy(pon => pon.boardNo).ToList();

        BoardPanel.SetActive(false);
        foreach (var player in playerManager.PlayerList)
            player.gameObject.SetActive(false);

        for (int i = 0; i < sortedList.Count; i++)
        {
            var board = Instantiate(BoardPrefab, CoreGameParent.transform);
            board.transform.SetParent(CoreGameParent.transform,true);
            board.GetComponent<BoardIdentity>().SetData(boardManager.isFreeToAll,sortedList[i].playerNo, sortedList[i].boardNo, sortedList[i].colorNo, sortedList[i].team);
            board.GetComponent<BoardIdentity>().SetBoardInitialize(BoardSprites[sortedList[i].colorNo], SelectSprites[sortedList[i].colorNo]);
            board.GetComponent<BoardIdentity>().Initialize();
            board.GetComponentInChildren<BoardFuse>().OnFuseFinished += OnFuseFinished;
            board.GetComponent<BoardIdentity>().winBoardAction += OnWinBoardAction;

            boardsWorld.Add(board);
            originalWorldSizeSpriteRender.Add(board.transform.GetComponentInChildren<Renderer>().bounds.size);
            activeBoards.Add(board.GetComponent<BoardIdentity>());
        }
        BoardsController.Instance.Init(activeBoards);
        UpdatePositionAndScaleBaseOnWorld();
    }
    private void RemoveWorldBoards()
    {
        foreach (var board in boardsWorld)
            Destroy(board.gameObject);
        boardsWorld.Clear();
        activeBoards.Clear();
    }
    private void UpdatePositionAndScaleBaseOnWorld()
    {
        if(boardsWorld.Count == 0) return;

        switch (boardManager.PlayerCount)
        {
            case 2:
                SetPositionAndScaleBaseOnWorld(0, player2Panel);
                SetPositionAndScaleBaseOnWorld(1, player2Panel);
                break;
            case 3:
                SetPositionAndScaleBaseOnWorld(0, player3Panel);
                SetPositionAndScaleBaseOnWorld(1, player3Panel);
                SetPositionAndScaleBaseOnWorld(2, player3Panel);
                break;
            case 4:
                SetPositionAndScaleBaseOnWorld(0, player4Panel);
                SetPositionAndScaleBaseOnWorld(1, player4Panel);
                SetPositionAndScaleBaseOnWorld(2, player4Panel);
                SetPositionAndScaleBaseOnWorld(3, player4Panel);
                break;
            case 5:
                SetPositionAndScaleBaseOnWorld(0, player8Panel);
                SetPositionAndScaleBaseOnWorld(1, player8Panel);
                SetPositionAndScaleBaseOnWorld(2, player8Panel);
                SetPositionAndScaleBaseOnWorld(3, player8Panel);
                SetPositionAndScaleBaseOnWorld(4, player8Panel);
                break;
            case 6:
                SetPositionAndScaleBaseOnWorld(0, player8Panel);
                SetPositionAndScaleBaseOnWorld(1, player8Panel);
                SetPositionAndScaleBaseOnWorld(2, player8Panel);
                SetPositionAndScaleBaseOnWorld(3, player8Panel);
                SetPositionAndScaleBaseOnWorld(4, player8Panel);
                SetPositionAndScaleBaseOnWorld(5, player8Panel);
                break;
            case 7:
                SetPositionAndScaleBaseOnWorld(0, player8Panel);
                SetPositionAndScaleBaseOnWorld(1, player8Panel);
                SetPositionAndScaleBaseOnWorld(2, player8Panel);
                SetPositionAndScaleBaseOnWorld(3, player8Panel);
                SetPositionAndScaleBaseOnWorld(4, player8Panel);
                SetPositionAndScaleBaseOnWorld(5, player8Panel);
                SetPositionAndScaleBaseOnWorld(6, player8Panel);
                break;
            case 8:
                SetPositionAndScaleBaseOnWorld(0, player8Panel);
                SetPositionAndScaleBaseOnWorld(1, player8Panel);
                SetPositionAndScaleBaseOnWorld(2, player8Panel);
                SetPositionAndScaleBaseOnWorld(3, player8Panel);
                SetPositionAndScaleBaseOnWorld(4, player8Panel);
                SetPositionAndScaleBaseOnWorld(5, player8Panel);
                SetPositionAndScaleBaseOnWorld(6, player8Panel);
                SetPositionAndScaleBaseOnWorld(7, player8Panel);
                break;
        }
    }

    private void SetPositionAndScaleBaseOnWorld(int index, GameObject playerPanel)
    {
        Transform transformPlayer = playerPanel.transform.GetChild(index);
        boardsWorld[index].transform.position = GetWorldPosition(transformPlayer.GetComponent<RectTransform>(), transformPlayer.position);

        float widthInWorld = GetWidthInWorldScale(transformPlayer.GetComponent<RectTransform>());
        boardsWorld[index].transform.localScale = Vector3.one * (widthInWorld / originalWorldSizeSpriteRender[index].x);
    }
    private Vector3 GetWorldPosition(RectTransform rectTransform, Vector3 worldPos)
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
    private float GetWidthInWorldScale(RectTransform rectTransform)
    {
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(null, rectTransform.position);
        Vector2 uiSize = rectTransform.rect.size;
        Vector2 uiScale = rectTransform.lossyScale;
        Vector2 uiSizeScaled = new Vector2(uiSize.x * uiScale.x, uiSize.y * uiScale.y);
        Vector3 worldCorner1, worldCorner2;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, screenPos, camera, out worldCorner1);
        RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, screenPos + new Vector3(uiSizeScaled.x, 0), camera, out worldCorner2);
        float widthInWorld = Vector3.Distance(worldCorner1, worldCorner2);
        return widthInWorld;
    }
    private float GetAspectRatio()
    {
        return (float)Screen.width / Screen.height;
    }
    void Update()
    {
        float currentAspect = GetAspectRatio();
       
        if (!Mathf.Approximately(previousAspect, currentAspect))
        {
            previousAspect = currentAspect;
            aspectCheckTimer = 0f;
            isChangeing = true;
        }
        else
        {
            if (isChangeing)
            {
                aspectCheckTimer += Time.deltaTime;
                if (aspectCheckTimer >= aspectStableTime)
                {
                    isChangeing = false;
                    UpdatePositionAndScaleBaseOnWorld();
                }
            }
        }
    }
    public void OnFuseFinished(BoardIdentity boardIdentity)
    {
        boardIdentity.SetDeactiveBoard();
        var activeboards = activeBoards.Where(board => !board.m_isDeactiveBoard).ToList();
        if (activeboards.Count <= 1)
            FinishedGame(null);

        //if(activeboards.Count==1)
        //     FinishedGame(activeboards[0].gameObject);
        // else if (activeboards.Count <=0)
        //     FinishedGame(null);
    }
    public void OnWinBoardAction(GameObject winBoard)
    {
         FinishedGame(winBoard);
    }
    
    public void FinishedGame(GameObject board)
    {
        if (isFinisheGame)
            return;
        isFinisheGame = false;
       
        if (board != null)
        {
            if (boardManager.isFreeToAll)
                FinidhedText.text = $"{board.GetComponent<BoardIdentity>().Player.PlayerName} win";
            else
                FinidhedText.text = $"Team {board.GetComponent<BoardIdentity>().Team.TeamName} win";
        }
        else
            FinidhedText.text = "Time's up";
        FinishePanel.SetActive(true);
        foreach (var player in playerManager.PlayerList)
            player.gameObject.SetActive(true);

        GameFinished?.Invoke();
    }

    //private List<Vector3> GetPosition(int count)
    //{
    //    if (count <= 0)
    //        return null;

    //    List<Vector3> list = new List<Vector3>();
    //    float spriteWidth = SelectSprites.bounds.size.x;
    //    float totalWidth = (count * spriteWidth) + (count - 1) * (spriteWidth/2);
    //    float startX = -totalWidth / 2f + spriteWidth / 2f;
    //    for (int i = 0; i < count; i++)
    //    {
    //        float x = startX + i * (spriteWidth+ (spriteWidth / 2));
    //        list.Add(new Vector3(x, 0f, 0f));
    //    }
    //    return list;
    //}

}
