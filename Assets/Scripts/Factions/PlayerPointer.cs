using Project.Core;
using Project.InputHandling;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Factions
{
    public class PlayerPointer : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer m_Number;
        [SerializeField] private SpriteRenderer m_Arrow;
        [SerializeField] private Transform m_RayPoint;
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

            m_InputActions.Game.Click.performed += OnClick;
            m_InputActions.Game.Deselect.performed += OnDeselectClicked;

            m_InputActions.Game.Click.Enable();
        }

        public void EnableMoving()
        {
            if (m_Moving == null)
            {
                m_InputActions.Game.Move.Enable();
                m_InputActions.Game.Select.Enable();
                m_InputActions.Game.Deselect.Disable();

                m_Moving = StartCoroutine(Moving());
            }
        }

        public void DisableMoving()
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

        private void OnSelectClicked()
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

        private void OnClick(InputAction.CallbackContext obj)
        {
            List<Collider2D> results = new List<Collider2D>();
            ContactFilter2D contactFilter = new ContactFilter2D()
            {
                useTriggers = true,
            };

            Physics2D.OverlapPoint(m_RayPoint.position, contactFilter, results);

            for (int i = results.Count - 1; i >= 0; i--)
            {
                if (results[i] == null)
                {
                    continue;
                }

                if (results[i].gameObject == this.gameObject)
                {
                    results.Remove(results[i]);
                }
            }

            float minDistance = float.MaxValue;
            Collider2D collider = results.Count > 0 ? results[0] : null;

            for (int i = 0; i < results.Count; i++)
            {
                float distance = Vector3.Distance(m_RayPoint.transform.position, results[i].transform.position);

                if (distance < minDistance)
                {
                    collider = results[i];
                    minDistance = distance;
                }
            }

            if (collider != null)
            {
                if (collider.TryGetComponent<I2DClickable>(out I2DClickable component))
                {
                    component.DoClick(this);

                    if (component is BlankBoard blank)
                    {
                        m_CurrentBoard = blank.Owner;
                        OnSelectClicked();
                    }
                }
            }

            results.Clear();
        }
    }
}