using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[Serializable]
public class Team 
{
    public int TeamID;
    public Color color;
    //private List<Board> boards = new List<Board>();
    public Team(int id,Color color)
    {
        TeamID = id;
        this.color = color;
    }
    //public void AddPlayer(Board board)
    //{
    //    if (!boards.Contains(board))
    //        boards.Add(board);
    //}
    //public List<Board> GetPlayers() => boards;
}
