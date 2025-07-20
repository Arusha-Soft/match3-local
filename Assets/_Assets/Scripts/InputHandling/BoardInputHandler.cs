using UnityEngine;

namespace Project.InputHandling
{
    public class BoardInputHandler : MonoBehaviour
    {
        [field: SerializeField] public bool IsUp { private set; get; }
        [field: SerializeField] public bool IsDown { private set; get; }
        [field: SerializeField] public bool IsLeft { private set; get; }
        [field: SerializeField] public bool IsRight { private set; get; }
        [field:SerializeField]public bool SelectIsPressed { private set; get; }

        private Vector2 m_InputValue;
        private bool m_IsInitialized = false;
        private BoardInputAction m_InputActions;

        public void Init()
        {
            if (m_IsInitialized)
            {
                return;
            }

            m_InputActions = new BoardInputAction();
            m_InputActions.Enable();

            m_IsInitialized = true;
        }

        private void OnEnable()
        {
            Init();
        }

        private void Update()
        {
            if (!m_IsInitialized)
            {
                return;
            }

            m_InputValue = m_InputActions.Board.Move.ReadValue<Vector2>();

            IsUp = m_InputValue.y > 0;
            IsDown = m_InputValue.y < 0;
            IsRight = m_InputValue.x > 0;
            IsLeft = m_InputValue.x < 0;

            SelectIsPressed = m_InputActions.Board.Select.IsPressed();
        }
    }
}