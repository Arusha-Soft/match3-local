using Project.Core;
using Project.Factions;
using System;
using System.Collections;
using UnityEngine;

namespace Project.Powerups
{
    public abstract class BasePowerup : MonoBehaviour
    {
        [SerializeField] private float m_TeamPowerupUseColdDown = 30f;

        public PowerupProperty Powreup;

        public event Action<BasePowerup> OnFinishPowerup;

        protected BoardsController Controller => BoardsController.Instance;

        private BoardIdentity m_Defender;

        public void DoAction(BoardIdentity attacker, BoardIdentity defender)
        {
            m_Defender = defender;

            if (!attacker.IsAvailableUsePowerup)
            {
                Debug.Log($"{attacker.name} is not available to use powerup!");
                return;
            }

            if (defender.IsUnderAttack)
            {
                Debug.Log($"{defender.name} is under attack");
                return;
            }

            if (Controller.IsTeamMode())
            {
                DoActionForTeamMode(attacker, defender);
                StartCoroutine(PowerupColdDown(attacker));
            }
            else
            {
                DoActionFreeForAllMode(attacker, defender);
            }

            defender.SetUnderAttack(true);
        }

        protected abstract void DoActionFreeForAllMode(BoardIdentity attacker, BoardIdentity defender);

        protected abstract void DoActionForTeamMode(BoardIdentity attacker, BoardIdentity defender);

        protected void InvokeFinish()
        {
            Debug.Log($"On finish powerup: {this}");
            m_Defender.SetUnderAttack(false);
            OnFinishPowerup?.Invoke(this);
        }

        private IEnumerator PowerupColdDown(BoardIdentity attcker)
        {
            foreach (BoardIdentity board in Controller.BoardTeams[attcker.Team])
            {
                board.SetIsAvailableToUsePowerup(false);
            }

            yield return new WaitForSeconds(m_TeamPowerupUseColdDown);

            foreach (BoardIdentity board in Controller.BoardTeams[attcker.Team])
            {
                board.SetIsAvailableToUsePowerup(true);
            }
        }
    }
}