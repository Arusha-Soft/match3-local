using System;
using UnityEngine;

[Serializable]
public class PlayerOnBoard
{
    public int playerNo;
    public int boardNo;
    public int colorNo;
    public Team team;
    public PlayerOnBoard(int playerNo, int boardNo, int colorNo, Team team)
    {
        this.playerNo = playerNo;
        this.boardNo = boardNo;
        this.colorNo = colorNo;
        this.team = team;
    }

}
