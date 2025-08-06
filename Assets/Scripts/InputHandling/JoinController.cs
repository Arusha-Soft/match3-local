using Project.Core;
using Project.Factions;
using Project.Powerups;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.SceneManagement;

namespace Project.InputHandling
{
    public class JoinController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BoardsController m_BoardsController;
        [SerializeField] private PowerupDistributionHandler m_PowerupDistributionHandler;
        [SerializeField] private Button2D m_ModeButton;
        [SerializeField] private TextMeshPro m_ModeText;

        [Header("Settings")]
        [SerializeField] private BoardIdentity m_BoardPrefab;
        [SerializeField] PlayerPointer m_PlayerPointerPrefab;
        [SerializeField] private PlayerProperty[] m_PlayerProperties;
        [SerializeField] private List<SpawnPoints> m_BoardSpawnPoints;
        [SerializeField] private GameMode m_GameMode = GameMode.FreeForAll;
        [SerializeField] private float m_ReloadDelay = 3f;
        [SerializeField] private TextMeshPro m_StatusText;

        public event Action OnGameStarted;

        private BoardInputAction m_InputAction;

        private Dictionary<InputDevice, (BoardInputAction, InputUser, PlayerPointer, BoardIdentity)> m_Players = new Dictionary<InputDevice, (BoardInputAction, InputUser, PlayerPointer, BoardIdentity)>();
        private int m_PlayersCount = 0;
        private List<BoardIdentity> m_Boards = new List<BoardIdentity>();
        private List<BoardIdentity> m_WinnerBoards = new List<BoardIdentity>();
        private List<BoardIdentity> m_LoserBoards = new List<BoardIdentity>();

        private BoardsController Controller => BoardsController.Instance;

        private void Start()
        {
            m_InputAction = new BoardInputAction();
            m_InputAction.Game.Join.Enable();

            m_InputAction.Game.Join.performed += OnJoinClicked;
            m_ModeButton.OnClick += OnClickModeButton;

            ChangeGameMode(GameMode.FreeForAll);
        }

        private void OnJoinClicked(InputAction.CallbackContext input)
        {
            InputDevice inputDevice = input.control.device;

            if (m_Players.ContainsKey(inputDevice))
            {
                return;
            }

            BoardInputAction inputActions = new BoardInputAction();
            InputUser inputUser = InputUser.CreateUserWithoutPairedDevices();
            InputUser.PerformPairingWithDevice(inputDevice, inputUser);

            inputUser.AssociateActionsWithUser(inputActions);

            BoardIdentity board = Instantiate(m_BoardPrefab);
            board.name = $"Board_{m_PlayersCount}";
            board.transform.SetParent(GetEmptyPoint());
            board.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            board.SetIsTeamMode(m_GameMode == GameMode.TeamMode);
            board.transform.localScale = Vector3.one;

            PlayerPointer playerPointer = Instantiate(m_PlayerPointerPrefab);
            playerPointer.Init(m_PlayerProperties[m_PlayersCount], inputActions);
            playerPointer.OnSelectBoard += OnSelectBoard;
            playerPointer.OnDeselectBoard += OnDeselectBoard;

            m_Boards.Add(board);
            m_Players.Add(inputDevice, (inputActions, inputUser, playerPointer, null));
            m_PlayersCount++;
        }

        private void OnSelectBoard(PlayerPointer playerPointer, BoardIdentity selectedBoard)
        {
            foreach (InputDevice device in m_Players.Keys)
            {
                (BoardInputAction, InputUser, PlayerPointer, BoardIdentity) player = m_Players[device];

                if (player.Item3 == playerPointer)
                {
                    player.Item4 = selectedBoard;
                    player.Item4.SetPlayer(playerPointer.PlayerProperty);
                    player.Item3.transform.SetParent(selectedBoard.transform);
                    m_Players[device] = player;
                    break;
                }
            }

            TryStartGame();
        }

        private void OnDeselectBoard(PlayerPointer playerPointer)
        {
            foreach (InputDevice device in m_Players.Keys)
            {
                (BoardInputAction, InputUser, PlayerPointer, BoardIdentity) player = m_Players[device];

                if (player.Item3 == playerPointer)
                {
                    player.Item4.SetPlayer(null);
                    player.Item4 = null;
                    player.Item3.transform.SetParent(null);
                    m_Players[device] = player;
                    break;
                }
            }
        }

        private Transform GetEmptyPoint()
        {
            int playerCount = m_PlayersCount + 1;

            List<Transform> selectedList = null;

            foreach (SpawnPoints points in m_BoardSpawnPoints)
            {
                if (points.Points.Count >= playerCount)
                {
                    selectedList = points.Points;
                    break;
                }
            }

            int index = 0;

            for (int i = 0; i < m_Boards.Count; i++)
            {
                Transform selectTransform = selectedList[index];

                BoardIdentity board = m_Boards[i];
                board.transform.SetParent(selectTransform);
                board.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                board.transform.localScale = Vector3.one;

                index++;
            }

            return selectedList[index];
        }

        private int m_StartedBoardCount = 0;

        private void TryStartGame()
        {
            if (AllBoardsAreSelected())
            {
                Debug.Log("Do Try Start Game");
                m_InputAction.Game.Join.Disable();

                foreach (InputDevice device in m_Players.Keys)
                {
                    (BoardInputAction, InputUser, PlayerPointer, BoardIdentity) player = m_Players[device];

                    player.Item4.Initialize(player.Item1);
                    player.Item4.OnWin += OnWinBoardIdentity;
                    player.Item4.OnLose += OnLoseBoardIdentity;
                    player.Item4.OnStart += OnStartBoard;
                    Destroy(player.Item3.gameObject);
                    m_StartedBoardCount++;
                }
            }
        }

        private bool AllBoardsAreSelected()
        {
            return BoardsSelectedCount() >= m_PlayersCount;
        }

        private int BoardsSelectedCount()
        {
            int result = 0;

            foreach (InputDevice device in m_Players.Keys)
            {
                (BoardInputAction, InputUser, PlayerPointer, BoardIdentity) player = m_Players[device];

                if (player.Item4 != null && player.Item4.Player != null)
                {
                    result++;
                }
            }

            return result;
        }

        private void OnClickModeButton(Button2D button)
        {
            GameMode gameMode = m_GameMode == GameMode.FreeForAll ? GameMode.TeamMode : GameMode.FreeForAll;
            ChangeGameMode(gameMode);
        }

        private void ChangeGameMode(GameMode gameMode)
        {
            if (BoardsSelectedCount() > 0) // ignore changing game mode when one or more player allready select a baord
            {
                return;
            }

            m_GameMode = gameMode;
            m_ModeText.text = m_GameMode == GameMode.FreeForAll ? "Free For All" : "Teams";
            bool isTeamMode = m_GameMode == GameMode.TeamMode;

            for (int i = 0; i < m_Boards.Count; i++)
            {
                m_Boards[i].SetTeam(isTeamMode ? m_Boards[i].Team : null);
                m_Boards[i].SetIsTeamMode(isTeamMode);
            }
        }

        private void OnStartBoard(BoardIdentity boardIdentity)
        {
            m_StartedBoardCount--;

            if (m_StartedBoardCount <= 0)
            {
                Debug.Log("On Game Start");

                m_BoardsController.Init(m_Boards);
                m_PowerupDistributionHandler.Init();
                m_StartedBoardCount = 0;

                OnGameStarted?.Invoke();
            }
        }

        private void OnLoseBoardIdentity(BoardIdentity boardIdentity)
        {
            boardIdentity.DoTimerOver();
            m_LoserBoards.Add(boardIdentity);

            if (m_LoserBoards.Count >= Controller.ActiveBoards.Count)
            {
                HandleAllLose();
            }
        }

        private void OnWinBoardIdentity(BoardIdentity boardIdentity)
        {
            bool isTeamMode = m_GameMode == GameMode.TeamMode;
            m_WinnerBoards.Add(boardIdentity);

            if (isTeamMode)
            {
                foreach (TeamProperty team in Controller.Teams)
                {
                    List<BoardIdentity> boardTeams = Controller.BoardTeams[team];

                    bool allExist = m_WinnerBoards.All(winner => boardTeams.Contains(winner));
                    if (allExist)
                    {
                        DoGameDoneTeamMode(team);
                        break;
                    }
                }
            }
            else
            {
                DoGameDoneFreeForAllMode(boardIdentity);
            }
        }

        private void DoGameDoneFreeForAllMode(BoardIdentity winner)
        {
            winner.DoWin();

            for (int i = 0; i < Controller.ActiveBoards.Count; i++)
            {
                if (Controller.ActiveBoards[i] != winner)
                {
                    Controller.ActiveBoards[i].DoLose();
                }
            }

            Debug.Log($"Winner Player: {winner.Player.PlayerName}");
            m_StatusText.text = $"Winner Player: {winner.Player.PlayerName}";
            DoGameFinished();
        }

        private void DoGameDoneTeamMode(TeamProperty winnerTeam)
        {
            List<BoardIdentity> boardTeams = Controller.BoardTeams[winnerTeam];

            for (int i = 0; i < boardTeams.Count; i++)
            {
                boardTeams[i].DoWin();
            }

            Debug.Log($"Winner Team:{winnerTeam.TeamName}");
            m_StatusText.text = $"Winner Team:{winnerTeam.TeamName}";
            DoGameFinished();
        }

        private void HandleAllLose()
        {
            Debug.Log("All Players Lose");
            m_StatusText.text = "All Players Lose";
            DoGameFinished();
        }

        private void DoGameFinished()
        {
            Debug.Log("Game Finished");
            StartCoroutine(Reload());
        }

        private IEnumerator Reload()
        {
            yield return new WaitForSeconds(m_ReloadDelay);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    [System.Serializable]
    public struct SpawnPoints
    {
        public List<Transform> Points;
    }

    public enum GameMode
    {
        FreeForAll,
        TeamMode
    }
}