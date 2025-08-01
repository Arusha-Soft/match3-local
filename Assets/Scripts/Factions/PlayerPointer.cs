using Project.Core;
using Project.InputHandling;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Factions
{
    public class PlayerPointer : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer m_Number;
        [SerializeField] private SpriteRenderer m_Arrow;
        [SerializeField] private float m_MoveSpeed;
        [SerializeField] private Vector2 m_MinMoveLimit = new Vector2(-10, -10);
        [SerializeField] private Vector2 m_MaxMoveLimit = new Vector2(10, 10);

        public event Action<PlayerPointer, BoardIdentity> OnSelectBoard;
        public event Action<PlayerPointer> OnDeselectBoard;

        public PlayerProperty PlayerProperty { private set; get; }

        private BoardInputAction m_InputActions;
        private Coroutine m_Moving;

        private BoardIdentity m_CurrentBoard;

        public void Init(PlayerProperty playerProperty, BoardInputAction inputActions)
        {
            PlayerProperty = playerProperty;
            m_InputActions = inputActions;

            m_Number.sprite = playerProperty.Pin;
            m_Arrow.sprite = playerProperty.Arrow;

            EnableMoving();

            m_InputActions.Game.Select.performed += OnSelectClicked;
            m_InputActions.Game.Deselect.performed += OnDeselectClicked;
        }

        private void EnableMoving()
        {
            if (m_Moving == null)
            {
                m_InputActions.Game.Move.Enable();
                m_InputActions.Game.Select.Enable();
                m_InputActions.Game.Deselect.Disable();

                m_Moving = StartCoroutine(Moving());
            }
        }

        private void DisableMoving()
        {
            if (m_Moving != null)
            {
                m_InputActions.Game.Move.Disable();
                m_InputActions.Game.Select.Disable();
                m_InputActions.Game.Deselect.Enable();

                StopCoroutine(m_Moving);
                m_Moving = null;
            }
        }

        private IEnumerator Moving()
        {
            while (true)
            {
                Vector2 movement = m_InputActions.Game.Move.ReadValue<Vector2>();
                float xMove = movement.x * m_MoveSpeed * Time.deltaTime;
                float yMove = movement.y * m_MoveSpeed * Time.deltaTime;
                Vector3 newPostion = new Vector3(xMove, yMove, 0) + transform.position;
                newPostion.x = Mathf.Clamp(newPostion.x, m_MinMoveLimit.x, m_MaxMoveLimit.x);
                newPostion.y = Mathf.Clamp(newPostion.y, m_MinMoveLimit.y, m_MaxMoveLimit.y);
                transform.position = newPostion;
                yield return null;
            }
        }

        private void OnSelectClicked(InputAction.CallbackContext context)
        {
            if (m_CurrentBoard != null)
            {
                OnSelectBoard?.Invoke(this, m_CurrentBoard);
                DisableMoving();
            }
        }

        private void OnDeselectClicked(InputAction.CallbackContext context)
        {
            OnDeselectBoard?.Invoke(this);
            EnableMoving();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.TryGetComponent<BlankBoard>(out BlankBoard blankBoard))
            {
                m_CurrentBoard = blankBoard.Owner;
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.TryGetComponent<BlankBoard>(out BlankBoard blankBoard))
            {
                if (blankBoard.Owner == m_CurrentBoard)
                {
                    m_CurrentBoard = null;
                }
            }
        }
    }
}