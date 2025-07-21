using DG.Tweening;
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

        public void Init(CookieProperties properties, BoardIdentity owner)
        {
            Properties = properties;
            Owner = owner;
            m_SpriteRenderer.sprite = properties.Icon;
            name = $"Cookie_{properties.m_CookieName}";
        }

        public virtual void PlayHideAnimation()
        {
            m_BlinkAnimation.tween.onComplete = () =>
            {
                m_ScaleDownAnimation.tween.Restart();
            };

            m_BlinkAnimation.tween.Restart();
        }

        public virtual void PlayComboHideAnimation()
        {
            m_ScaleDownAnimation.tween.Pause();
            m_BlinkAnimation.tween.Pause();
            m_FadeOutAnimation.tween.Restart();
        }
    }
}