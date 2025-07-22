using Project.Core;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private List<PlayerOnBoard> playerOnBoardList;
    public GameObject MenuePanel;
    public GameObject CoreGameParent;
    public GameObject BoardPrefab;
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
        this.playerOnBoardList=playerOnBoardList;
        MenuePanel.SetActive(false);
        foreach (var borad in playerOnBoardList)
        {
           var board= Instantiate(BoardPrefab, CoreGameParent.transform);
            board.GetComponent<BoardIdentity>().SetSprite(borad.BoardSprite);
           // board.GetComponent<BoardIdentity>().
        }
    }

    void Update()
    {
        
    }
}
