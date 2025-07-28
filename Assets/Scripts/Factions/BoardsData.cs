using Project.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Project.Factions
{
    public class BoardsData : MonoBehaviour
    {
        [SerializeField] private List<BoardIdentity> m_ActiveBoards;

        private Dictionary<TeamProperty, List<BoardIdentity>> m_BoardTeams;

        public IReadOnlyList<BoardIdentity> ActiveBoards => m_ActiveBoards;
        public IReadOnlyDictionary<TeamProperty, List<BoardIdentity>> PlayerTeams => m_BoardTeams;
        public IReadOnlyList<TeamProperty> Teams => PlayerTeams == null ? new List<TeamProperty>() : PlayerTeams.Keys.ToList();


        private void Awake()
        {
            Init(m_ActiveBoards);
        }

        public void Init(List<BoardIdentity> activeBoards)
        {
            m_ActiveBoards = activeBoards;

            if (IsTeamMode())
            {
                m_BoardTeams = new Dictionary<TeamProperty, List<BoardIdentity>>();

                for (int i = 0; i < m_ActiveBoards.Count; i++)
                {
                    if (m_BoardTeams.TryGetValue(m_ActiveBoards[i].Team, out List<BoardIdentity> players))
                    {
                        players.Add(m_ActiveBoards[i]);
                    }
                    else
                    {
                        List<BoardIdentity> teamPlayers = new List<BoardIdentity>();
                        teamPlayers.Add(m_ActiveBoards[i]);
                        m_BoardTeams.Add(m_ActiveBoards[i].Team, teamPlayers);
                    }
                }
            }
        }

        public bool IsTeamMode() => m_ActiveBoards[0].IsTeamMode();
    }
}
