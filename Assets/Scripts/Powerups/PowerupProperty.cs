using UnityEngine;

namespace Project.Powerups
{
    [CreateAssetMenu(fileName = "PowerupProperty", menuName = "Powerups/PowerupProperty")]
    public class PowerupProperty : ScriptableObject
    {
        [field: SerializeField] public string PowerupName { private set; get; }
        [field: SerializeField] public Sprite Icon { private set; get; }
        [field: SerializeField] public bool CanEffectOnAttackerSelf { private set; get; } = false;
    }
}
