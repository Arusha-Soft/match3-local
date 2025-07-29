using Project.Core;
using UnityEngine;

namespace Project.Powerups
{
    public class PointChangerPowerup : BasePowerup
    {
        [SerializeField] private int m_ChangeAmount = 0;

        protected override void DoActionForTeamMode(BoardIdentity attacker, BoardIdentity defender)
        {
            defender.BoardScore.ChangeScoreWithEffect(m_ChangeAmount);
        }

        protected override void DoActionFreeForAllMode(BoardIdentity attacker, BoardIdentity defender)
        {
            defender.BoardScore.ChangeScoreWithEffect(m_ChangeAmount);
        }
    }
}