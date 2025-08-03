using Project.Factions;
using System;
using UnityEngine;

namespace Project.InputHandling
{
    public class Button2D : MonoBehaviour , I2DClickable
    {
        public event Action<Button2D> OnClick;

        public void DoClick(PlayerPointer player)
        {
            Click(player);
            OnClick?.Invoke(this);
        }

        protected virtual void Click(PlayerPointer player) { }
    }
}