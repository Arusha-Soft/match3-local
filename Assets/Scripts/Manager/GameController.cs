using Project.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    public Canvas canvas;
    public GameObject MenuePanel;
    public GameObject CoreGameParent;
    public GameObject BoardPrefab;
    private Camera camera;
    private List<GameObject> boardsWorld=new List<GameObject>();

    public Sprite SelectSprites;
    void Start()
    {
        camera= GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        Vector3 canvasPos = canvas.transform.position;
        camera.transform.position= new Vector3(canvasPos.x, canvasPos.y, camera.transform.position.z);
        PlayerManager.PlayerOnBoardEvent += OnStartGame;
    }
    private void OnDestroy()
    {
        PlayerManager.PlayerOnBoardEvent -= OnStartGame;
    }

    private void OnStartGame(List<PlayerOnBoard> playerOnBoardList)
    {
        var sortedList = playerOnBoardList.OrderBy(pon => pon.BoardNo).ToList();

        MenuePanel.SetActive(false);
        foreach (var player in PlayerManager.Instance.PlayerList)
            player.gameObject.SetActive(false);

        //List<Vector3> positionList = GetPosition(sortedList.Count);
        int playerCount= BoardManager.Instance.PlayerCount;
        int scale = 22;
        if (playerCount == 2)
            scale = 22;
        else if (playerCount == 3)
            scale = 19;
        else if (playerCount == 4)
            scale = 15;
        else 
            scale = 10;
        List<Vector3> positionList = BoardManager.Instance.GetWorldPosListByPlayerCount();
        for (int i = 0; i < sortedList.Count; i++)
        {
            var board = Instantiate(BoardPrefab, CoreGameParent.transform);
            board.transform.SetParent(CoreGameParent.transform,true);
            board.GetComponent<BoardIdentity>().SetSprite(sortedList[i].ColorNo);
            if(i == 0 || i==1)
            board.GetComponent<BoardIdentity>().SetInputHandler(sortedList[i].PlayerNo);
            board.GetComponent<BoardIdentity>().Initialize();
            board.transform.position = positionList[i];
           // board.transform.localScale *= scale;
            boardsWorld.Add(board);
        }
       
    }

    private List<Vector3> GetPosition(int count)
    {
        if (count <= 0)
            return null;

        List<Vector3> list = new List<Vector3>();
        float spriteWidth = SelectSprites.bounds.size.x;
        float totalWidth = (count * spriteWidth) + (count - 1) * (spriteWidth/2);
        float startX = -totalWidth / 2f + spriteWidth / 2f;
        for (int i = 0; i < count; i++)
        {
            float x = startX + i * (spriteWidth+ (spriteWidth / 2));
            list.Add(new Vector3(x, 0f, 0f));
        }
        return list;
    }

    void Update()
    {
        
    }
}
