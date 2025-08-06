using Project.Factions;
using Project.InputHandling;
using TMPro;
using UnityEngine;

namespace Project.Test
{
    public class FuseDisabler : Button2D
    {
        [SerializeField] private JoinController m_JoinController;
        [SerializeField] private TextMeshPro m_Text;

        private BoardsController Controller => BoardsController.Instance;

        private bool m_IsDisable = false;

        private void OnEnable()
        {
            m_JoinController.OnGameStarted += OnGameStarted;

            ChangeStatus();
        }

        private void OnDisable()
        {
            m_JoinController.OnGameStarted -= OnGameStarted;
        }

        protected override void Click(PlayerPointer player)
        {
            ChangeStatus();
        }

        private void ChangeStatus()
        {
            m_IsDisable = !m_IsDisable;

            if (m_IsDisable)
            {
                m_Text.text = "Enable Fuse";
            }
            else
            {
                m_Text.text = "Disable Fuse";
            }
        }

        private void OnGameStarted()
        {
            if (m_IsDisable)
            {
                for (int i = 0; i < Controller.ActiveBoards.Count; i++)
                {
                    Controller.ActiveBoards[i].BoardFuse.ForceStopWorking();
                }
            }
        }
    }
}
