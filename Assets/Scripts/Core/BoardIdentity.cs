using NUnit.Framework;
using Project.Factions;
using Project.InputHandling;
using Project.Powerups;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Core
{
    public class BoardIdentity : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BoardInputHandler m_BoardInput;
        [SerializeField] private BoardData m_BoardData;
        [SerializeField] private CookiesController m_CookieGenerator;
        [SerializeField] private CookiesMatcher m_CookiesMatcher;
        [SerializeField] private SelectionBox m_SelectionBox;
        [SerializeField] private BoardScore m_BoardScore;
        [SerializeField] private BoardFuse m_BoardFuse;
        [SerializeField] private BoardPowerup m_BoardPowerup;

        [SerializeField] private SpriteRenderer m_BoardSprite;
        [SerializeField] private SpriteRenderer m_SelectSprite;
        [SerializeField] private SpriteRenderer m_PowerupIcon;
        [SerializeField] private GameObject m_DeactiveBoard;
        public bool m_isDeactiveBoard=false;

        public bool m_isFreeToAllMode;
        public int m_playerNo;
        public int m_boardNo;
        public int m_colorNo;
        public int m_teamNo;

        [Header("Settings")]
        [SerializeField] private Color m_ColdDownAttackColor;

        [field: SerializeField] public PlayerProperty Player { private set; get; }
        [field: SerializeField] public TeamProperty Team { private set; get; }
        [field: SerializeField] public PowerupProperty Powerup { private set; get; }
        [field: SerializeField] public BoardIdentity AttackTarget { private set; get; }
        public bool IsUnderAttack { private set; get; } = false;
        public bool IsAvailableUsePowerup { private set; get; } = true;

        public BoardInputHandler BoardInput => m_BoardInput;
        public BoardData BoardData => m_BoardData;
        public CookiesController CookiesController => m_CookieGenerator;
        public SelectionBox SelectionBox => m_SelectionBox;
        public BoardScore BoardScore => m_BoardScore;
        public BoardPowerup BoardPowerup => m_BoardPowerup;
        public Action<GameObject> winBoardAction;

        private void Start()
        {
            //Initialize();
        }

        //public void SetInputHandler(int PlayerNo)
        //{
        //    m_BoardInput.m_gamepad = Gamepad.all[PlayerNo];
        //}

        //[ContextMenu("Init Input")]
        //private void InitInput()
        //{
        //    m_BoardInput.Init();
        //}
        public void SetData(bool isFreeToAllMode, int playerNo, int boardNo, int colorNo, Team team)
        {
            m_isFreeToAllMode = isFreeToAllMode;
            m_playerNo = playerNo;
            m_boardNo = boardNo;
            m_colorNo = colorNo;

            var playerProperty=BoardsController.Instance.PlayerPropertyList.Where(pp => pp.Number == playerNo + 1).FirstOrDefault(); 
            if(playerProperty != null) { SetPlayer(playerProperty); }

            if (!isFreeToAllMode)
            {
                var teamProperty = BoardsController.Instance.TeamPropertyList.Where(tp => tp.Number == team.TeamID + 1).FirstOrDefault();
                if (teamProperty != null) { SetTeam(teamProperty); }
            }
        }
        public void SetBoardInitialize(Sprite boardSprite, Sprite selectSprite)
        {
            m_BoardSprite.sprite = boardSprite;
            m_SelectSprite.sprite = selectSprite;
            if (m_playerNo == 0 || m_playerNo == 1)             //mina test remove it
                m_BoardInput.m_gamepad = Gamepad.all[m_playerNo];

        }
        public void Initialize()
        {
            m_DeactiveBoard.SetActive(false);
            m_isDeactiveBoard = false;
            m_BoardInput.Init(); //TODO uncomment this line
            m_BoardData.Init(this);
            m_CookieGenerator.Init(m_BoardData, m_BoardInput, m_SelectionBox, this, m_CookiesMatcher);
            m_CookiesMatcher.Init(m_CookieGenerator, m_BoardData);
            m_SelectionBox.Init(m_BoardInput, m_BoardData);
            m_BoardScore.Init();

            m_CookiesMatcher.OnMatchFind += OnMatchFind;
            m_CookieGenerator.OnFinishRefilling += OnFinishRefilling;

            UpdateBoardTheme();

            m_BoardFuse.StartWorking();
        }

        public void SetPlayer(PlayerProperty player)
        {
            Player = player;
            UpdateBoardTheme();
        }

        public void SetTeam(TeamProperty team)
        {
            Team = team;
            UpdateBoardTheme();
        }

        public void SetPowerup(PowerupProperty powerup)
        {
            Powerup = powerup;
            m_PowerupIcon.sprite = powerup.Icon;
        }

        public void SetAttackTarget(BoardIdentity target)
        {
            AttackTarget = target;

            m_SelectSprite.sprite = target.IsTeamMode() ? target.Team.AttackTheme : target.Player.AttackTheme;
        }

        public void SetUnderAttack(bool isUnderAttack)
        {
            IsUnderAttack = isUnderAttack;
        }

        public void SetIsAvailableToUsePowerup(bool isAvailableUsePowerup)
        {
            IsAvailableUsePowerup = isAvailableUsePowerup;

            m_SelectSprite.color = isAvailableUsePowerup ? Color.white : m_ColdDownAttackColor;
            m_PowerupIcon.gameObject.SetActive(isAvailableUsePowerup);
        }

        public bool IsTeamMode() =>
            Team != null;

        private void UpdateBoardTheme()
        {
            if (Team == null)
            {
                if (Player != null)
                {
                    m_BoardSprite.sprite = Player.BoardTheme;
                }
            }
            else
            {
                m_BoardSprite.sprite = Team.BoardTheme;
            }
        }

        [ContextMenu("Applay Powerup")]
        private void ApplyPowerup()
        {
            if (Powerup == null)
            {
                return;
            }

            Debug.Log($"ApplyPowerup: Attacker: {this} / Defender: {AttackTarget} / Powerup: {Powerup.PowerupName}");
            m_BoardPowerup.ApplyPowerup(this, AttackTarget, Powerup);
        }

        private void OnMatchFind(bool isPowerup)
        {
            m_BoardInput.DisableInput();
            m_BoardScore.SetScore(m_BoardScore.Score + 1);
            if (m_BoardScore.Score >= 2)
                winBoardAction?.Invoke(gameObject);
            m_BoardFuse.ResetIt();

            if (isPowerup)
            {
                ApplyPowerup();
            }
        }

        private void OnFinishRefilling()
        {
            m_BoardInput.EnableInput();
        }
        public void SetDeactiveBoard()
        {
            m_DeactiveBoard.SetActive(true);
            m_BoardInput.DisableInput();
            m_isDeactiveBoard = true;
        }
    }
}