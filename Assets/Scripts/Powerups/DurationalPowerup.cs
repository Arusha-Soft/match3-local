using Project.Core;
using System.Collections;
using UnityEngine;

namespace Project.Powerups
{
    public abstract class DurationalPowerup : BasePowerup
    {
        [SerializeField] private float m_Duration = 5f;

        private Coroutine m_PowerupProcess;

        protected override sealed void DoActionForTeamMode(BoardIdentity attacker, BoardIdentity defender)
        {
            m_PowerupProcess = StartCoroutine(PowerupProcess(attacker, defender, true));
        }

        protected override sealed void DoActionFreeForAllMode(BoardIdentity attacker, BoardIdentity defender)
        {
            m_PowerupProcess = StartCoroutine(PowerupProcess(attacker, defender, false));
        }

        private IEnumerator PowerupProcess(BoardIdentity attacker, BoardIdentity defender, bool isTeammode)
        {
            OnStartPowerup(attacker, defender, isTeammode);
            yield return new WaitForSeconds(m_Duration);
            OnEndPowerup(attacker, defender, isTeammode);
            m_PowerupProcess = null;
            InvokeFinish();
        }

        protected abstract void OnStartPowerup(BoardIdentity attacker, BoardIdentity defender, bool isTeammode);
        protected abstract void OnEndPowerup(BoardIdentity attacker, BoardIdentity defender, bool isTeammode);
    }
}