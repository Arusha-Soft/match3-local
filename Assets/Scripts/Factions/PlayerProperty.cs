using UnityEngine;

namespace Project.Factions
{
    [CreateAssetMenu(fileName = "PlayerProperty", menuName = "Factions/PlayerProperty")]
    public class PlayerProperty : ScriptableObject
    {
        [field: SerializeField] public int Number { private set; get; }
        [field: SerializeField] public string PlayerName { private set; get; }
        [field: SerializeField] public Sprite BoardTheme { private set; get; }
        [field: SerializeField] public Sprite AttackTheme { private set; get; }
        [field: SerializeField] public Sprite Pin { private set; get; }
        [field: SerializeField] public Sprite Arrow { private set; get; }
    }
}
