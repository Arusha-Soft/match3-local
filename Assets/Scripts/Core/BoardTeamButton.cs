using Project.Factions;
using Project.InputHandling;
using TMPro;
using UnityEngine;

namespace Project.Core
{

    public class BoardTeamButton : Button2D
    {
        [SerializeField] private BoardIdentity m_Owner;
        [SerializeField] private SpriteRenderer m_TeamButton;
        [SerializeField] private TextMeshPro m_TeamName;
        [SerializeField] private TeamProperty[] m_Teams;

        private int m_Index = 0;
        private TeamProperty m_SelectedTeam;

        private void Awake()
        {
            m_SelectedTeam = m_Teams[0];

            UpdateTeam(m_SelectedTeam, false);

            m_Index = 1;
        }

        protected override void Click(PlayerPointer player)
        {
            m_SelectedTeam = m_Teams[m_Index];

            UpdateTeam(m_SelectedTeam, true);

            m_Index++;

            if (m_Index > m_Teams.Length - 1)
            {
                m_Index = 0;
            }
        }

        public void SetTeam(int index)
        {
            m_SelectedTeam = m_Teams[index];
            UpdateTeam(m_SelectedTeam, true);
        }

        private void UpdateTeam(TeamProperty teamProperty, bool applyToBoard)
        {
            m_TeamButton.color = teamProperty.Color;
            m_TeamName.text = teamProperty.TeamName;

            if (applyToBoard)
            {
                m_Owner.SetTeam(teamProperty);
            }
        }
    }
}