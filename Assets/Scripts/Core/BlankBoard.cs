using UnityEngine;

namespace Project.Core
{
    public class BlankBoard : MonoBehaviour
    {
        [field: SerializeField] public BoardIdentity Owner { private set; get; }
    }
}