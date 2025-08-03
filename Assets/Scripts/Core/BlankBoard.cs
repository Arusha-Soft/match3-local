using Project.InputHandling;
using UnityEngine;

namespace Project.Core
{
    public class BlankBoard : Button2D
    {
        [field: SerializeField] public BoardIdentity Owner { private set; get; }
    }
}