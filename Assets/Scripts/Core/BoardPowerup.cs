using Project.Powerups;
using UnityEngine;

namespace Project.Core
{
    public class BoardPowerup : MonoBehaviour
    {
        [SerializeField] private BasePowerup[] m_Powerups;

        public void ApplyPowerup(BoardIdentity attacker, BoardIdentity defender, PowerupProperty powerup)
        {
            GetPowreup<BasePowerup>(powerup).DoAction(attacker, defender);
        }

        public T GetPowreup<T>(PowerupProperty property) where T : BasePowerup
        {
            for (int i = 0; i < m_Powerups.Length; i++)
            {
                if (m_Powerups[i].Powerup == property)
                {
                    return m_Powerups[i] as T;
                }
            }

            return null;
        }
    }
}
