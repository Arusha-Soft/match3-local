using Project.Powerups;
using UnityEngine;

namespace Project.Core
{
    public class BoardPowerup : MonoBehaviour
    {
        [SerializeField] private BasePowerup[] m_Powerups;

        public void ApplyPowerup(BoardIdentity attacker, BoardIdentity defender, PowerupProperty powerup)
        {
            for (int i = 0; i < m_Powerups.Length; i++)
            {
                if (m_Powerups[i].Powreup == powerup)
                {
                    m_Powerups[i].DoAction(attacker, defender);
                    break;
                }
            }
        }
    }
}
