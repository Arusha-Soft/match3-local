using System;
using UnityEngine;

[Serializable]
public class PlayerOnBoard
{
    public int PlayerNo;
    public int BoardNo;
    public int ColorNo;
    public Vector3 PositionBoard;
    public PlayerOnBoard(int PlayerNo, int BoardNo, int ColorNo, Vector3 PositionBoard)
    {
        this.PlayerNo = PlayerNo;
        this.BoardNo = BoardNo;
        this.ColorNo = ColorNo;
        this.PositionBoard=PositionBoard; ;
    }

}
