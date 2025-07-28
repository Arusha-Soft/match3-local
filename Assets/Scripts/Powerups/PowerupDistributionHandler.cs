using Project.Core;
using Project.Factions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Project.Powerups
{
    public class PowerupDistributionHandler : MonoBehaviour
    {
        [SerializeField] private float m_ChangeTargetAttackDuration = 15;
        [SerializeField] private float m_ChangePowerupDuration = 10;
        [SerializeField] private PowerupProperty[] m_Powerups;

        [SerializeField] private BoardsData m_BoardDatas;

        private Coroutine m_PowerupChangeHandling;
        private Coroutine m_TargetAttackHandling;

        private void Awake()
        {
            Init();
        }

        public void Init()
        {
            m_PowerupChangeHandling = StartCoroutine(PowerupChangeHandling());
            m_TargetAttackHandling = StartCoroutine(TargetAttackChangeHandling());
        }

        private IEnumerator TargetAttackChangeHandling()
        {
            WaitForSeconds delay = new WaitForSeconds(m_ChangeTargetAttackDuration);

            while (true)
            {
                ChangeTargetAttack();
                yield return delay;
            }

            void ChangeTargetAttack()
            {
                bool isTeamMode = m_BoardDatas.IsTeamMode();

                if (isTeamMode)
                {
                    List<TeamProperty> freeForAttackTeams = m_BoardDatas.Teams.ToList();

                    for (int i = freeForAttackTeams.Count - 1; i >= 0; i--)
                    {
                        TeamProperty team = freeForAttackTeams[i];
                        TeamProperty targetAttackTeam = team;

                        while (team == targetAttackTeam)
                        {
                            targetAttackTeam = m_BoardDatas.Teams[Random.Range(0, m_BoardDatas.Teams.Count)];
                        }

                        freeForAttackTeams.Remove(team);

                        foreach (var item in m_BoardDatas.PlayerTeams[team])
                        {
                            List<BoardIdentity> targets = m_BoardDatas.PlayerTeams[targetAttackTeam];
                            BoardIdentity randomTarget = targets[Random.Range(0, targets.Count)];
                            item.SetAttackTarget(randomTarget);
                        }
                    }
                }
            }
        }

        private IEnumerator PowerupChangeHandling()
        {
            WaitForSeconds delay = new WaitForSeconds(m_ChangePowerupDuration);

            while (true)
            {
                UpdatePowerups();
                yield return delay;
            }

            void UpdatePowerups()
            {
                List<PowerupProperty> temp = new List<PowerupProperty>();
                temp.AddRange(m_Powerups);

                for (int i = 0; i < m_BoardDatas.ActiveBoards.Count; i++)
                {
                    if (temp.Count <= 0)
                    {
                        temp.AddRange(m_Powerups);
                    }

                    PowerupProperty powerup = temp[Random.Range(0, temp.Count)];
                    m_BoardDatas.ActiveBoards[i].SetPowerup(powerup);
                    temp.Remove(powerup);
                }
            }
        }
    }
}