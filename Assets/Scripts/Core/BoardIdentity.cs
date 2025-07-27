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

        public bool m_isFreeToAllMode;
        public int m_playerNo;
        public int m_boardNo;
        public int m_colorNo;
        public int m_teamNo;
        private void Start()
        {
           
        }
        public void SetData(bool isFreeToAllMode,int playerNo, int boardNo, int colorNo, int teamNo)
        {
            m_isFreeToAllMode = isFreeToAllMode;
            m_playerNo = playerNo;
            m_boardNo = boardNo;
            m_colorNo = colorNo;
            m_teamNo = teamNo;
        }
        public void SetBoardInitialize()
        {
            m_BoardSprite.sprite = BoardSprites[m_colorNo];
            m_SelectSprite.sprite = SelectSprites[m_colorNo];
            if (m_playerNo == 0 || m_playerNo == 1)             //mina test remove it
                m_BoardInput.m_gamepad = Gamepad.all[m_playerNo];
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