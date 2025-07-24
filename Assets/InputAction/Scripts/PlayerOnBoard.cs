using System;
using UnityEngine;

[Serializable]
public class PlayerOnBoard
{
    public int PlayerNo;
    public int BoardNo;
    public int ColorNo;
    public PlayerOnBoard(int PlayerNo, int BoardNo, int ColorNo)
    {
        this.PlayerNo = PlayerNo;
        this.BoardNo = BoardNo;
        this.ColorNo = ColorNo;
    }

}
