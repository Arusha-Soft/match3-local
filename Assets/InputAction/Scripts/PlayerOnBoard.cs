using System;
using UnityEngine;

[Serializable]
public class PlayerOnBoard
{
    public int PlayerNo;
    public int BoardNo;
    public int ColorNo;
    public int TeamNo;
    public PlayerOnBoard(int playerNo, int boardNo, int colorNo, int teamNo)
    {
        PlayerNo = playerNo;
        BoardNo = boardNo;
        ColorNo = colorNo;
        TeamNo = teamNo;
    }

}
