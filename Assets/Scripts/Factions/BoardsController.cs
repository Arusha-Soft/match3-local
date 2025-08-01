using Project.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Project.Factions
{
    public class BoardsController : MonoBehaviour
    {
        [SerializeField] private List<BoardIdentity> m_ActiveBoards;

        private Dictionary<TeamProperty, List<BoardIdentity>> m_BoardTeams;
        private Dictionary<PlayerProperty, BoardIdentity> m_BoardPlayers;

        public static BoardsController Instance { private set; get; }

        public IReadOnlyList<BoardIdentity> ActiveBoards => m_ActiveBoards;
        public IReadOnlyDictionary<TeamProperty, List<BoardIdentity>> BoardTeams => m_BoardTeams;
        public IReadOnlyDictionary<PlayerProperty, BoardIdentity> BoardPlayers => m_BoardPlayers;
        public IReadOnlyList<TeamProperty> Teams => BoardTeams == null ? new List<TeamProperty>() : BoardTeams.Keys.ToList();
        public IReadOnlyList<PlayerProperty> Players => m_BoardPlayers.Keys.ToList();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
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

            m_BoardPlayers = new Dictionary<PlayerProperty, BoardIdentity>();

            for (int i = 0; i < m_ActiveBoards.Count; i++)
            {
                m_BoardPlayers.Add(m_ActiveBoards[i].Player, m_ActiveBoards[i]);
            }
        }

        public bool IsTeamMode() => m_ActiveBoards[0].IsTeamMode();
    }
}
