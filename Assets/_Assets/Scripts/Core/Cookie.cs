using UnityEngine;

namespace Project.Core
{
    public class Cookie : MovableTile
    {
        [SerializeField] private SpriteRenderer m_SpriteRenderer;

        public void Init(CookieProperties properties)
        {
            m_SpriteRenderer.sprite = properties.Icon;
            name = $"Cookie_{properties.m_CookieName}";
        }
    }
}