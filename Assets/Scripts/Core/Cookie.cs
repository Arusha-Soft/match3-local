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

        private Collider2D m_Collider;

        public void Init(CookieProperties properties, BoardIdentity owner)
        {
            Properties = properties;
            Owner = owner;
            m_SpriteRenderer.sprite = properties.Icon;
            name = $"Cookie_{properties.m_CookieName}";

            m_Collider = GetComponent<Collider2D>();
        }

        public virtual void PlayHideAnimation(Action<Cookie> onFinish = null)
        {
            m_BlinkAnimation.tween.onComplete = () =>
            {
                m_ScaleDownAnimation.tween.onComplete = () =>
                {
                    onFinish?.Invoke(this);
                };

                m_ScaleDownAnimation.tween.Restart();
            };

            m_BlinkAnimation.tween.Restart();
        }

        public virtual void PlayComboHideAnimation(Action<Cookie> onFinish = null)
        {
            m_ScaleDownAnimation.tween.Pause();
            m_BlinkAnimation.tween.Pause();

            m_FadeOutAnimation.tween.onComplete = () =>
            {
                onFinish?.Invoke(this);
            };
            m_FadeOutAnimation.tween.Restart();
        }

        public void EnableCollider(bool enable)
        {
            m_Collider.enabled = enable;
        }
    }
}