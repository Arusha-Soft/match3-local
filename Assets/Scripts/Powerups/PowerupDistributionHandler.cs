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

        private BoardsController Controller => BoardsController.Instance;

        private Coroutine m_PowerupChangeHandling;
        private Coroutine m_TargetAttackHandling;

        private void Start()
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
                bool isTeamMode = Controller.IsTeamMode();

                if (isTeamMode)
                {
                    List<TeamProperty> freeForAttackTeams = Controller.Teams.ToList();

                    for (int i = freeForAttackTeams.Count - 1; i >= 0; i--)
                    {
                        TeamProperty team = freeForAttackTeams[i];
                        TeamProperty targetAttackTeam = team;

                        while (team == targetAttackTeam)
                        {
                            targetAttackTeam = Controller.Teams[Random.Range(0, Controller.Teams.Count)];
                        }

                        freeForAttackTeams.Remove(team);

                        foreach (var item in Controller.BoardTeams[team])
                        {
                            List<BoardIdentity> targets = Controller.BoardTeams[targetAttackTeam];
                            BoardIdentity randomTarget = targets[Random.Range(0, targets.Count)];
                            item.SetAttackTarget(randomTarget);
                        }
                    }
                }
                else
                {
                    List<PlayerProperty> allPlayers = Controller.Players.ToList();
                    List<PlayerProperty> shuffledTargets = allPlayers.OrderBy(p => Random.Range(0, 10000)).ToList();

                    for (int i = 0; i < allPlayers.Count; i++)
                    {
                        PlayerProperty player = allPlayers[i];

                        if (shuffledTargets[i] == player)
                        {
                            int swapIndex = (i + 1) % allPlayers.Count;
                            PlayerProperty temp = shuffledTargets[i];
                            shuffledTargets[i] = shuffledTargets[swapIndex];
                            shuffledTargets[swapIndex] = temp;
                        }

                        PlayerProperty target = shuffledTargets[i];
                        Controller.BoardPlayers[player].SetAttackTarget(Controller.BoardPlayers[target]);
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

                for (int i = 0; i < Controller.ActiveBoards.Count; i++)
                {
                    if (!Controller.ActiveBoards[i].IsAvailableUsePowerup)
                    {
                        continue;
                    }

                    if (temp.Count <= 0)
                    {
                        temp.AddRange(m_Powerups);
                    }

                    PowerupProperty powerup = temp[Random.Range(0, temp.Count)];
                    Controller.ActiveBoards[i].SetPowerup(powerup);
                    temp.Remove(powerup);
                }
            }
        }
    }
}