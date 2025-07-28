using Project.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Factions
{
    public class BoardsData : MonoBehaviour
    {
        [SerializeField] private List<BoardIdentity> m_ActiveBoards;

        private Dictionary<TeamProperty, PlayerProperty> m_PlayerTeams;

        public IReadOnlyList<BoardIdentity> ActiveBoards => m_ActiveBoards;
        public IReadOnlyDictionary<TeamProperty, PlayerProperty> PlayerTeams => m_PlayerTeams;

        private void Awake()
        {
            Init(m_ActiveBoards);
        }

        public void Init(List<BoardIdentity> activeBoards)
        {
            m_ActiveBoards = activeBoards;

            if (IsTeamMode())
            {
                m_PlayerTeams = new Dictionary<TeamProperty, PlayerProperty>();

                for (int i = 0; i < m_ActiveBoards.Count; i++)
                {
                    m_PlayerTeams.TryAdd(m_ActiveBoards[i].Team, m_ActiveBoards[i].Player);
                }
            }
        }

        public bool IsTeamMode() => m_ActiveBoards[0].IsTeamMode();
    }
}
