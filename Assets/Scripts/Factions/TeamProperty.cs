using UnityEngine;

namespace Project.Factions
{

    [CreateAssetMenu(fileName = "TeamProperty", menuName = "Factions/TeamProperty")]
    public class TeamProperty : ScriptableObject
    {
        [field: SerializeField] public int Number { private set; get; }
        [field: SerializeField] public string TeamName { private set; get; }
        [field: SerializeField] public Color Color { private set; get; }
        [field: SerializeField] public Sprite BoardTheme { private set; get; }
        [field: SerializeField] public Sprite AttackTheme { private set; get; }
    }
}
