using Project.Factions;
using Project.InputHandling;
using Project.Powerups;
using System;
using System.Collections;
using System.Collections.Generic;
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

        [SerializeField] private BoardTeamButton m_TeamButton;
        [SerializeField] private BoardText m_Text;

        [Header("Settings")]
        [SerializeField] private Color m_ColdDownAttackColor;
        [SerializeField] private int m_ColdDownTimerDuration = 3;
        [SerializeField] private float m_DestroyDelayInRows = 0.1f;

        [field: SerializeField] public PlayerProperty Player { private set; get; }
        [field: SerializeField] public TeamProperty Team { private set; get; }
        [field: SerializeField] public PowerupProperty Powerup { private set; get; }
        [field: SerializeField] public BoardIdentity AttackTarget { private set; get; }
        public bool IsUnderAttack { private set; get; } = false;
        public bool IsAvailableUsePowerup { private set; get; } = true;
        public bool IsTeamMode { private set; get; } = false;
        public bool IsWorking { private set; get; } = false;

        public event Action<BoardIdentity> OnStart;
        public event Action<BoardIdentity> OnWin;
        public event Action<BoardIdentity> OnLose;

        public BoardInputHandler BoardInput => m_BoardInput;
        public BoardData BoardData => m_BoardData;
        public CookiesController CookiesController => m_CookieGenerator;
        public SelectionBox SelectionBox => m_SelectionBox;
        public BoardScore BoardScore => m_BoardScore;
        public BoardPowerup BoardPowerup => m_BoardPowerup;

        public void Initialize(BoardInputAction inputActions)
        {
            StartCoroutine(ColdDown(() =>
            {
                m_BoardScore.OnReachMaxScore += OnReachMaxScore;
                m_BoardFuse.OnFuseFinished += OnFuseFinished;

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
                m_PowerupIcon.gameObject.SetActive(true);
                m_BoardFuse.StartWorking();

                IsWorking = true;

                OnStart?.Invoke(this);
            }));
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

            m_PowerupIcon.gameObject.SetActive(powerup != null);
            m_PowerupIcon.sprite = powerup == null ? null : powerup.Icon;
        }

        public void SetAttackTarget(BoardIdentity target)
        {
            AttackTarget = target;

            m_SelectSprite.sprite = target == null ? (IsTeamMode ? Team.AttackTheme : Player.AttackTheme) :
                (target.IsTeamMode ? target.Team.AttackTheme : target.Player.AttackTheme);
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

        public void SetIsTeamMode(bool isTeamMode)
        {
            IsTeamMode = isTeamMode;

            if (isTeamMode)
            {
                m_TeamButton.SetTeam(0);
            }

            m_TeamButton.gameObject.SetActive(isTeamMode);
        }

        public void DoWin()
        {
            StopBoard();
            m_Text.gameObject.SetActive(true);
            m_Text.SetText("WIN!!");
            m_Text.PlayAnimation();
        }

        public void DoLose()
        {
            StopBoard();
            m_Text.gameObject.SetActive(true);
            m_Text.SetText("LOSE");
            m_Text.PlayAnimation();
        }

        public void DoTimerOver()
        {
            StopBoard();

            StartCoroutine(TimerOverAction());
        }

        private IEnumerator TimerOverAction()
        {
            WaitForSeconds delay = new WaitForSeconds(m_DestroyDelayInRows);

            float rowCount = m_BoardData.OriginalBoardSize.y;
            IReadOnlyList<Block> blocks;
            IReadOnlyList<Cookie> cookies;

            for (int i = 0; i < rowCount; i++)
            {
                blocks = m_BoardData.GetBlokcsAtRow(i, true);
                if (blocks.Count <= 0)
                {
                    continue;
                }

                cookies = m_BoardData.GetRowCookiesAtId(blocks[0].Id);

                for (int j = 0; j < cookies.Count; j++)
                {
                    cookies[j].DestroyIt();
                }

                yield return delay;
            }

            yield return delay;

            m_Text.gameObject.SetActive(true);
            m_Text.PlayAnimation();
            m_Text.SetText("Time Over");
        }

        private void UpdateBoardTheme()
        {
            if (Team == null)
            {
                if (Player != null)
                {
                    m_SelectSprite.sprite = Player.AttackTheme;
                    m_BoardSprite.sprite = Player.BoardTheme;
                }
            }
            else
            {
                m_SelectSprite.sprite = Team.AttackTheme;
                m_BoardSprite.sprite = Team.BoardTheme;
            }
        }

        private IEnumerator ColdDown(Action onFinishColdDown)
        {
            int timer = m_ColdDownTimerDuration;
            WaitForSeconds delay = new WaitForSeconds(1f);

            m_Text.gameObject.SetActive(true);

            while (timer > 0)
            {
                m_Text.SetText($"{timer}");
                yield return delay;
                timer--;
            }

            m_Text.SetText("GO!");
            delay = new WaitForSeconds(1f / 2f);
            yield return delay;
            m_Text.gameObject.SetActive(false);
            onFinishColdDown?.Invoke();
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

        private void OnReachMaxScore()
        {
            Debug.Log($"{name} Is Win");
            OnWin?.Invoke(this);
        }

        private void OnFuseFinished()
        {
            Debug.Log($"{name} Is Lose");
            OnLose?.Invoke(this);
        }

        private void StopBoard()
        {
            m_BoardScore.OnReachMaxScore -= OnReachMaxScore;
            m_BoardFuse.OnFuseFinished -= OnFuseFinished;

            m_BoardInput.DisableInput();
            m_CookieGenerator.DisableMoveHandling();
            m_SelectionBox.Stop();

            m_CookiesMatcher.OnMatchFind -= OnMatchFind;
            m_CookieGenerator.OnFinishRefilling -= OnFinishRefilling;

            SetAttackTarget(null);
            SetPowerup(null);

            IsWorking = false;
        }
    }
}