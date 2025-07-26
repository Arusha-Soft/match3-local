using System;
using System.Collections;
using UnityEngine;

namespace Project.Core
{
    public class BoardFuse : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer m_FuseRenderer;
        [SerializeField] private float m_FuseDuration = 5f;
        [SerializeField] private float m_DefaultValue = 1f;

        public float FuseValue { private set; get; }

        public event Action OnFuseFinished;

        private Coroutine m_Working;

        public void StartWorking()
        {
            FuseValue = m_DefaultValue;

            if (m_Working != null)
            {
                StopCoroutine(m_Working);
            }

            m_Working = StartCoroutine(Working());
        }

        public void ResetIt()
        {
            StartWorking();
        }

        private IEnumerator Working()
        {
            while (FuseValue > 0)
            {
                FuseValue -= (Time.deltaTime / m_FuseDuration);
                SetValue(FuseValue);
                yield return null;
            }

            m_Working = null;
            SetValue(0);
            Debug.Log("On Fuse Finished");
            OnFuseFinished?.Invoke();
        }

        private void SetValue(float value)
        {
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            m_FuseRenderer.GetPropertyBlock(mpb);
            mpb.SetFloat("_FillAmount", value);
            m_FuseRenderer.SetPropertyBlock(mpb);
        }
    }
}