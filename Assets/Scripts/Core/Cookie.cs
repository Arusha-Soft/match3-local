using DG.Tweening;
using System;
using UnityEngine;

namespace Project.Core
{
    public class Cookie : MovableTile
    {
        [SerializeField] private SpriteRenderer m_SpriteRenderer;

        [Header("Animations")]
        [SerializeField] private DOTweenAnimation m_BlinkAnimation;
        [SerializeField] private DOTweenAnimation m_ScaleDownAnimation;
        [SerializeField] private DOTweenAnimation m_FadeOutAnimation;

        public BoardIdentity Owner { private set; get; }
        public CookieProperties Properties { private set; get; }
        public event Action<Cookie> OnFinishHideAnimation;
        public event Action<Cookie> OnFinishComboHideAnimation;

        private Vector3 m_Scale;
        private Color m_Color;

        public void Init(CookieProperties properties, BoardIdentity owner)
        {
            Properties = properties;
            Owner = owner;
            m_SpriteRenderer.sprite = properties.Icon;
            name = $"Cookie_{properties.m_CookieName}";

            m_Scale = transform.localScale;
            m_Color = m_SpriteRenderer.color;
        }

        public void SetColor(Color color)
        {
            m_SpriteRenderer.color = color;
            m_Color = color;
        }

        public virtual void PlayHideAnimation()
        {
            m_BlinkAnimation.tween.onComplete = () =>
            {
                m_ScaleDownAnimation.tween.onComplete = () =>
                {
                    OnFinishHideAnimation?.Invoke(this);
                };

                m_ScaleDownAnimation.tween.Restart();
            };

            m_BlinkAnimation.tween.Restart();
        }

        public virtual void PlayComboHideAnimation()
        {
            m_ScaleDownAnimation.tween.Pause();
            m_BlinkAnimation.tween.Pause();

            m_FadeOutAnimation.tween.onComplete = () =>
            {
                OnFinishComboHideAnimation?.Invoke(this);
            };
            m_FadeOutAnimation.tween.Restart();
        }

        public virtual void ResetIt()
        {
            transform.localScale = m_Scale;
            m_SpriteRenderer.color = m_Color;
        }
    }
}