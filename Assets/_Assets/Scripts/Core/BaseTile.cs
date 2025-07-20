using UnityEngine;

namespace Project.Core
{
    public abstract class BaseTile : MonoBehaviour
    {
        [field: SerializeField] public int Id { private set; get; }

        public BaseTile SetId(int id)
        {
            Id = id;
            return this;
        }
    }
}