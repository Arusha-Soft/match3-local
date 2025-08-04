using Project.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    public GameObject MenuePanel;
    public GameObject CoreGameParent;
    public GameObject BoardPrefab;

    public Sprite SelectSprites;
    void Start()
    {
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
            
        List<Vector3> positionList = GetPosition(sortedList.Count);

        for (int i = 0; i < sortedList.Count; i++)
        {
            var board = Instantiate(BoardPrefab, CoreGameParent.transform);
            board.transform.SetParent(CoreGameParent.transform,true);
            //board.GetComponent<BoardIdentity>().SetSprite(sortedList[i].ColorNo);
            //board.GetComponent<BoardIdentity>().SetInputHandler(sortedList[i].PlayerNo);
            //board.GetComponent<BoardIdentity>().Initialize();
            board.transform.position = positionList[i];
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
