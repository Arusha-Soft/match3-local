using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[Serializable]
public class Team 
{
    public int TeamID;
    public string TeamName;
    public Color color;
    //private List<Board> boards = new List<Board>();
    public Team(int id,Color color)
    {
        TeamID = id;
        this.color = color;
        if (id == 0)
            TeamName = "";
        else if (id == 1)
            TeamName = "";
        else if (id == 2)
            TeamName = "";
        else
            TeamName = "";
    }
    //public void AddPlayer(Board board)
    //{
    //    if (!boards.Contains(board))
    //        boards.Add(board);
    //}
    //public List<Board> GetPlayers() => boards;
}
