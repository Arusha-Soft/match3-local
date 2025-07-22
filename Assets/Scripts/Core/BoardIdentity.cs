using Project.InputHandling;
using UnityEngine;

namespace Project.Core
{
    public class BoardIdentity : MonoBehaviour
    {
        [SerializeField] private BoardInputHandler m_BoardInput;
        [SerializeField] private BoardData m_BoardData;
        [SerializeField] private CookiesController m_CookieGenerator;
        [SerializeField] private CookiesMatcher m_CookiesMatcher;
        [SerializeField] private SelectionBox m_SelectionBox;
        [SerializeField] private SpriteRenderer m_sprite;

        private void Start()
        {
            m_BoardInput.Init();
            m_BoardData.Init(m_SelectionBox, this);
            m_CookieGenerator.Init(m_BoardData, m_BoardInput, m_SelectionBox, this, m_CookiesMatcher);
            m_CookiesMatcher.Init(m_CookieGenerator, m_BoardData);
            m_SelectionBox.Init(m_BoardInput, m_BoardData);
        }
        public void SetSprite(Sprite sprite)
        {
            m_sprite.sprite = sprite;
        }
    }
}