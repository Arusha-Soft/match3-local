using Project.InputHandling;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Core
{
    public class BoardIdentity : MonoBehaviour
    {
        [SerializeField] private BoardInputHandler m_BoardInput;
        [SerializeField] private BoardData m_BoardData;
        [SerializeField] private CookiesController m_CookieGenerator;
        [SerializeField] private CookiesMatcher m_CookiesMatcher;
        [SerializeField] private SelectionBox m_SelectionBox;
        [SerializeField] private SpriteRenderer m_BoardSprite;
        [SerializeField] private SpriteRenderer m_SelectSprite;
        public Sprite[] BoardSprites;
        public Sprite[] SelectSprites;
        private void Start()
        {
           
        }
        public void SetSprite(int spriteIndex)
        {
            m_BoardSprite.sprite = BoardSprites[spriteIndex];
            m_SelectSprite.sprite = SelectSprites[spriteIndex];
        }
        public void SetInputHandler(int PlayerNo)
        {
            m_BoardInput.m_gamepad = Gamepad.all[PlayerNo];
        }
        public void Initialize()
        {
            m_BoardInput.Init();
            m_BoardData.Init(m_SelectionBox, this);
            m_CookieGenerator.Init(m_BoardData, m_BoardInput, m_SelectionBox, this, m_CookiesMatcher);
            m_CookiesMatcher.Init(m_CookieGenerator, m_BoardData);
            m_SelectionBox.Init(m_BoardInput, m_BoardData);
        }
        
    }
}