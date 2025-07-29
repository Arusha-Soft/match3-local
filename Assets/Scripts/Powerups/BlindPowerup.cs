using Project.Core;
using UnityEngine;

namespace Project.Powerups
{
    public class BlindPowerup : DurationalPowerup
    {
        [SerializeField] private GameObject m_BlindObject;

        protected override void OnStartPowerup(BoardIdentity attacker, BoardIdentity defender, bool isTeammode)
        {
            defender.BoardPowerup.GetPowreup<BlindPowerup>(Powerup).m_BlindObject.SetActive(true);
        }

        protected override void OnEndPowerup(BoardIdentity attacker, BoardIdentity defender, bool isTeammode)
        {
            defender.BoardPowerup.GetPowreup<BlindPowerup>(Powerup).m_BlindObject.SetActive(false);
        }
    }
}