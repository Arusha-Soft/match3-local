using Project.Core;
using Project.Factions;
using Project.Powerups;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

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

        private BoardInputAction m_InputAction;

        private Dictionary<InputDevice, (BoardInputAction, InputUser, PlayerPointer, BoardIdentity)> m_Players = new Dictionary<InputDevice, (BoardInputAction, InputUser, PlayerPointer, BoardIdentity)>();
        private int m_PlayersCount = 0;
        private List<BoardIdentity> m_Boards = new List<BoardIdentity>();

        private void Start()
        {
            m_InputAction = new BoardInputAction();
            m_InputAction.Game.Join.Enable();

            m_InputAction.Game.Join.performed += OnJoinClicked;
            m_ModeButton.OnClick += OnClickModeButton;
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

            //board.Initialize(inputActions);

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

        private void TryStartGame()
        {
            if (AllBoardsAreSelected())
            {
                Debug.Log("Do Start Game");

                foreach (InputDevice device in m_Players.Keys)
                {
                    (BoardInputAction, InputUser, PlayerPointer, BoardIdentity) player = m_Players[device];

                    player.Item4.Initialize(player.Item1);
                    Destroy(player.Item3.gameObject);
                }

                m_BoardsController.Init(m_Boards);
                m_PowerupDistributionHandler.Init();
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
            if(BoardsSelectedCount() > 0) // ignore changing game mode when one or more player allready select a baord
            {
                return;
            }

            m_ModeText.text = m_GameMode == GameMode.FreeForAll ? "Free For All" : "Teams";

            m_GameMode = gameMode;
            bool isTeamMode = m_GameMode == GameMode.TeamMode;

            for (int i = 0; i < m_Boards.Count; i++)
            {
                m_Boards[i].SetIsTeamMode(isTeamMode);
            }
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