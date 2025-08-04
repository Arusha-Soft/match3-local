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
                    if (Controller.Teams.Count > 1) //when one team start game don't search for target attack
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
                                List<BoardIdentity> targets = Controller.BoardTeams[targetAttackTeam].Where(T => T.IsWorking).ToList();

                                BoardIdentity randomTarget = targets.Count > 0 ? targets[Random.Range(0, targets.Count)] : null;
                                item.SetAttackTarget(randomTarget);
                            }
                        }
                    }
                    else if (Controller.Teams.Count == 1)
                    {
                        Controller.BoardTeams[Controller.Teams[0]].ForEach(F => F.SetAttackTarget(null));
                    }
                }
                else
                {
                    if (Controller.Players.Count > 1)//when one player start game don't search for target attack
                    {
                        List<PlayerProperty> allPlayers = Controller.Players.Where(P => Controller.BoardPlayers[P].IsWorking).ToList();
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
                    else if (Controller.Players.Count == 1)
                    {
                        Controller.BoardPlayers[Controller.Players[0]].SetAttackTarget(null);
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
                bool isTeamMode = Controller.IsTeamMode();

                if (isTeamMode)
                {
                    if (Controller.Teams.Count > 1) //when one team start game don't search for target attack
                    {
                        SetPowerup();
                    }
                    else
                    {
                        Controller.BoardTeams[Controller.Teams[0]].ForEach(F => F.SetPowerup(null));
                    }
                }
                else
                {
                    if (Controller.Players.Count > 1)
                    {
                        SetPowerup();
                    }
                    else
                    {
                        Controller.BoardPlayers[Controller.Players[0]].SetPowerup(null);
                    }
                }
            }

            void SetPowerup()
            {
                List<PowerupProperty> temp = new List<PowerupProperty>();
                temp.AddRange(m_Powerups);
                List<BoardIdentity> activeBoards = Controller.ActiveBoards.Where(B => B.IsWorking).ToList();

                if (activeBoards.Count > 1)
                {
                    for (int i = 0; i < activeBoards.Count; i++)
                    {
                        if (!activeBoards[i].IsAvailableUsePowerup)
                        {
                            continue;
                        }

                        if (temp.Count <= 0)
                        {
                            temp.AddRange(m_Powerups);
                        }

                        PowerupProperty powerup = temp[Random.Range(0, temp.Count)];
                        activeBoards[i].SetPowerup(powerup);
                        temp.Remove(powerup);
                    }
                }
                else if(activeBoards.Count == 1)
                {
                    activeBoards[0].SetPowerup(null);
                }
            }
        }
    }
}