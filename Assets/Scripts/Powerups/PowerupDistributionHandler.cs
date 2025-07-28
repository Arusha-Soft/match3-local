using Project.Core;
using Project.Factions;
using System.Collections;
using System.Collections.Generic;
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

        public void Init(List<BoardIdentity> activeBoards)
        {
            m_PowerupChangeHandling = StartCoroutine(PowerupChangeHandling());
        }

        private IEnumerator TargetAttackChangeHandling()
        {
            WaitForSeconds delay = new WaitForSeconds(m_ChangeTargetAttackDuration);

            while (true)
            {

            }

            void ChangeTargetAttack()
            {
                bool isTeamMode = m_BoardDatas.IsTeamMode();


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