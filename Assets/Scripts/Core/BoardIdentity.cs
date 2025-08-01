using NUnit.Framework;
using Project.Factions;
using Project.InputHandling;
using Project.Powerups;
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
        [SerializeField] private SpriteRenderer m_BlankBoard;
        [SerializeField] private SpriteRenderer m_FuseIcon;

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

        public void SetInputHandler(int PlayerNo)
        {
            m_BoardInput.m_gamepad = Gamepad.all[PlayerNo];
        }

        public void Initialize(BoardInputAction inputActions)
        {
            m_BoardInput.Init(inputActions);
            m_BoardData.Init(this);
            m_CookieGenerator.Init(m_BoardData, m_BoardInput, m_SelectionBox, this, m_CookiesMatcher);
            m_CookiesMatcher.Init(m_CookieGenerator, m_BoardData);
            m_SelectionBox.Init(m_BoardInput, m_BoardData);
            m_BoardScore.Init();

            m_CookiesMatcher.OnMatchFind += OnMatchFind;
            m_CookieGenerator.OnFinishRefilling += OnFinishRefilling;

            UpdateBoardTheme();

            m_FuseIcon.gameObject.SetActive(true);
            m_BoardFuse.StartWorking();
        }

        public void SetPlayer(PlayerProperty player)
        {
            Player = player;

            m_BlankBoard.gameObject.SetActive(player == null);
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
    }
}