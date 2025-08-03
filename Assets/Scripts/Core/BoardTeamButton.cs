using Project.Factions;
using Project.InputHandling;
using TMPro;
using UnityEngine;

namespace Project.Core
{

    public class BoardTeamButton : Button2D
    {
        [SerializeField] private SpriteRenderer m_TeamButton;
        [SerializeField] private TextMeshPro m_TeamName;
        [SerializeField] private TeamProperty[] m_Teams;

        private int m_Index = 0;
        private TeamProperty m_SelectedTeam;

        private void Awake()
        {
            m_SelectedTeam = m_Teams[0];

            m_TeamButton.color = m_SelectedTeam.Color;
            m_TeamName.text = m_SelectedTeam.TeamName;

            m_Index = 1;
        }

        protected override void Click(PlayerPointer player)
        {
            m_SelectedTeam = m_Teams[m_Index];

            m_TeamButton.color = m_SelectedTeam.Color;
            m_TeamName.text = m_SelectedTeam.TeamName;

            m_Index++;

            if (m_Index > m_Teams.Length - 1)
            {
                m_Index = 0;
            }
        }
    }
}