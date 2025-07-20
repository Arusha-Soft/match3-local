using Project.InputHandling;
using UnityEngine;

namespace Project.Core
{
    public class BoardIdentity : MonoBehaviour
    {
        [SerializeField] private BoardInputHandler m_BoardInput;
        [SerializeField] private BoardData m_BoardData;
        [SerializeField] private SelectionBoxMover m_SelectionBoxMover;

        private void Start()
        {
            m_BoardInput.Init();
            m_BoardData.Init();
            m_SelectionBoxMover.Init(m_BoardInput, m_BoardData);
        }
    }
}