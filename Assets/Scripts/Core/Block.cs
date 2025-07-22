using UnityEngine;

namespace Project.Core
{
    public class Block : BaseTile
    {
        [field: SerializeField] public bool IsVisible { private set; get; }


        public Block SetVisibility(bool visible)
        {
            IsVisible = visible;
            return this;
        }
    }
}