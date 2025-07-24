using System;
using System.Collections;
using UnityEngine;

namespace Project.Core
{
    public class MovableTile : BaseTile
    {
        [SerializeField] private float m_MoveDuration = 0.2f;

        public event Action<MovableTile> FinishMoving;

        private Coroutine m_Moving;

        public bool TryMove(Transform target)
        {
            if (transform.position == target.position)
            {
                return false;
            }
            else
            {

                if (m_Moving != null)
                {
                    StopCoroutine(m_Moving);
                }

                StartCoroutine(Moving(target));
                return true;
            }
        }

        protected virtual void OnFinishMoving(MovableTile movableTile) { }

        private IEnumerator Moving(Transform target)
        {
            float value = 0;
            Vector3 startPosition = transform.position;
            while (transform.position != target.position)
            {
                value += Time.deltaTime / m_MoveDuration;
                transform.position = Vector3.Lerp(startPosition, target.position, value);
                yield return null;
            }

            m_Moving = null;
            OnFinishMoving(this);
            FinishMoving?.Invoke(this);
        }
    }
}