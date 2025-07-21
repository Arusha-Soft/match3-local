using UnityEngine;

namespace Project.Core
{
    public class Cookie : MovableTile
    {
        [SerializeField] private SpriteRenderer m_SpriteRenderer;

        public BoardIdentity Owner { private set; get; }
        public CookieProperties Properties { private set; get; }

        public void Init(CookieProperties properties , BoardIdentity owner)
        {
            Properties = properties;
            Owner = owner;
            m_SpriteRenderer.sprite = properties.Icon;
            name = $"Cookie_{properties.m_CookieName}";
        }
    }
}