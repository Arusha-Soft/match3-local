using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.InputHandling
{
    public class BoardInputHandler : MonoBehaviour
    {
        [field: SerializeField] public bool IsUp { private set; get; }
        [field: SerializeField] public bool IsDown { private set; get; }
        [field: SerializeField] public bool IsLeft { private set; get; }
        [field: SerializeField] public bool IsRight { private set; get; }
        [field: SerializeField] public bool SelectIsPressed { private set; get; }

        private Vector2 m_InputValue;
        private bool m_IsInitialized = false;
        private Coroutine m_Checking;
        private BoardInputAction m_InputActions;
        public BoardInputAction CurrentInputActions { private set; get; }
        public Gamepad m_gamepad;

        public void Init(BoardInputAction inputActions)
        {
            if (m_IsInitialized)
            {
                return;
            }

            m_InputActions = inputActions;
            m_InputActions.Enable();

            CurrentInputActions = m_InputActions;

            m_IsInitialized = true;
            EnableInput();
        }

        public void RebindInputAction(BoardInputAction inputActions)
        {
            CurrentInputActions = inputActions;
        }

        public void ResetInputActtion()
        {
            CurrentInputActions = m_InputActions;
        }

        public void EnableInput()
        {
            if (m_Checking == null)
            {
                m_Checking = StartCoroutine(Checking());
            }
        }

        public void DisableInput()
        {
            if (m_Checking != null)
            {
                StopCoroutine(m_Checking);
            }


            IsUp = false;
            IsDown = false;
            IsRight = false;
            IsLeft = false;
            SelectIsPressed = false;

            m_Checking = null;
        }

        private IEnumerator Checking()
        {
            while (true)
            {
                if (m_IsInitialized)
                {
                    m_InputValue = CurrentInputActions.Board.Move.ReadValue<Vector2>();

                    IsUp = m_InputValue.y > 0;
                    IsDown = m_InputValue.y < 0;
                    IsRight = m_InputValue.x > 0;
                    IsLeft = m_InputValue.x < 0;

                    SelectIsPressed = CurrentInputActions.Board.Select.IsPressed();
                }

                yield return null;
            }
        }
    }
}