using Project.InputHandling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;
using static Project.Core.CookiesMatcher;
using Random = UnityEngine.Random;

namespace Project.Core
{
    public class CookiesController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CookieProperties[] m_CookieProperties;
        [SerializeField] private Cookie m_CookiePrefab;
        [SerializeField] private Transform m_CookieParent;

        [Header("Settigns")]
        [SerializeField] private float m_CleanCookieMoveDuration = 0.2f;
        [SerializeField] private float m_RefillCookieMoveDuration = 0.2f;

        public event Action OnFinishMovingCookies;
        public event Action OnFinishCleanBoard;
        public event Action OnFinishRefilling;

        private BoardData m_BoardData;
        private BoardInputHandler m_Input;
        private SelectionBox m_SelectionBox;
        private BoardIdentity m_BoardIdentity;
        private CookiesMatcher m_CookiesMatcher;

        private List<Block> m_TempBlocks = new List<Block>();
        private List<Cookie> m_TempMovingCookies = new List<Cookie>();
        private List<MovableTile> m_TempCleanCookies = new List<MovableTile>();
        private List<MovableTile> m_TempRefillCookies = new List<MovableTile>();
        private Action m_OnFinishMoveCookies;

        private Coroutine m_MoveHandling;

        private bool m_IsMoving = false;
        private int m_OnMovingCookieCount = 0;

        private ObjectPool<Cookie> m_CookiePool;

        public void Init(BoardData boardData, BoardInputHandler input, SelectionBox selectionBox,
            BoardIdentity boardIdentity, CookiesMatcher cookiesMatcher)
        {
            m_BoardData = boardData;
            m_Input = input;
            m_SelectionBox = selectionBox;
            m_BoardIdentity = boardIdentity;
            m_CookiesMatcher = cookiesMatcher;

            m_CookiePool = new ObjectPool<Cookie>(OnCreate, OnGet, OnRelease);

            m_CookiesMatcher.OnDisappearCookiesStepFinished += OnDisappearCookiesStepFinished;
            m_CookiesMatcher.OnDisappearCookiesFinished += OnDisappearCookiesFinished;
            m_CookiesMatcher.OnMatchingProcessFinished += OnMatchingProcessFinished;

            EnableMoveHandling();
            FillBoard();
        }

        #region Movement

        public void EnableMoveHandling()
        {
            if (m_MoveHandling == null)
            {
                m_MoveHandling = StartCoroutine(MoveHandling());
            }
        }

        public void DisableMoveHandling()
        {
            if(m_MoveHandling != null)
            {
                StopCoroutine(m_MoveHandling);
                m_MoveHandling = null;
            }
        }

        private IEnumerator MoveHandling()
        {
            while (true)
            {
                if (m_Input.SelectIsPressed && !m_IsMoving)
                {
                    if (m_Input.IsRight)
                    {
                        m_TempBlocks = m_BoardData.GetRowBlocksAtId(m_SelectionBox.CurrentBlockId).ToList();
                        m_TempMovingCookies = m_BoardData.GetRowCookiesAtId(m_SelectionBox.CurrentBlockId).ToList();
                        HandleMoveTowardRightOrDown();
                    }
                    else if (m_Input.IsLeft)
                    {
                        m_TempBlocks = m_BoardData.GetRowBlocksAtId(m_SelectionBox.CurrentBlockId).ToList();
                        m_TempMovingCookies = m_BoardData.GetRowCookiesAtId(m_SelectionBox.CurrentBlockId).ToList();
                        HandleMoveTowardLeftOrUp();
                    }
                    else if (m_Input.IsUp)
                    {
                        m_TempBlocks = m_BoardData.GetColumnBlocksAtId(m_SelectionBox.CurrentBlockId).ToList();
                        m_TempMovingCookies = m_BoardData.GetColumnCookiesAtId(m_SelectionBox.CurrentBlockId).ToList();
                        HandleMoveTowardLeftOrUp();
                    }
                    else if (m_Input.IsDown)
                    {
                        m_TempBlocks = m_BoardData.GetColumnBlocksAtId(m_SelectionBox.CurrentBlockId).ToList();
                        m_TempMovingCookies = m_BoardData.GetColumnCookiesAtId(m_SelectionBox.CurrentBlockId).ToList();
                        HandleMoveTowardRightOrDown();
                    }
                }

                yield return null;
            }

            void HandleMoveTowardRightOrDown()
            {
                Cookie lastCookie = m_TempMovingCookies[m_TempMovingCookies.Count - 1];
                Block firstBlock = m_TempBlocks[0];
                Cookie tempCookie = m_CookiePool.Get();
                tempCookie.Init(lastCookie.Properties, tempCookie.Owner);
                tempCookie.transform.position = firstBlock.transform.position;
                tempCookie.name = "TempCookie";
                m_TempMovingCookies.Insert(0, tempCookie);
                m_OnFinishMoveCookies += OnFinishMoveCookiesTowardRightOrDown;
                DoMoveCookiesTowardRightOrDown();
            }

            void HandleMoveTowardLeftOrUp()
            {
                Cookie firstCookie = m_TempMovingCookies[0];
                Block firstBlock = m_TempBlocks[m_TempBlocks.Count - 1];
                Cookie tempCookie = m_CookiePool.Get();
                tempCookie.Init(firstCookie.Properties, tempCookie.Owner);
                tempCookie.transform.position = firstBlock.transform.position;
                tempCookie.name = "TempCookie";
                m_TempMovingCookies.Add(tempCookie);
                m_OnFinishMoveCookies += OnFinishMoveCookiesTowardLeftOrUp;
                DoMoveCookiesTowardLeftOrUp();
            }

            void DoMoveCookiesTowardRightOrDown()
            {
                m_IsMoving = true;

                for (int i = 0; i < m_TempMovingCookies.Count; i++)
                {
                    Cookie cookie = m_TempMovingCookies[i];
                    cookie.FinishMoving += OnCookieFinishMoving;
                    cookie.TryMove(m_TempBlocks[i + 1].transform);
                    m_OnMovingCookieCount++;
                }
            }

            void DoMoveCookiesTowardLeftOrUp()
            {
                m_IsMoving = true;

                for (int i = 0; i < m_TempMovingCookies.Count; i++)
                {
                    Cookie cookie = m_TempMovingCookies[i];
                    cookie.FinishMoving += OnCookieFinishMoving;
                    cookie.TryMove(m_TempBlocks[i].transform);
                    m_OnMovingCookieCount++;
                }
            }
        }

        private void OnFinishMoveCookiesTowardRightOrDown()
        {
            m_OnFinishMoveCookies -= OnFinishMoveCookiesTowardRightOrDown;

            Cookie lastCookie = m_TempMovingCookies[m_TempMovingCookies.Count - 1];
            lastCookie.gameObject.SetActive(false);
            Cookie tempCoocike = m_TempMovingCookies[0];
            lastCookie.transform.position = tempCoocike.transform.position;
            lastCookie.gameObject.SetActive(true);
            m_CookiePool.Release(tempCoocike);
        }

        private void OnFinishMoveCookiesTowardLeftOrUp()
        {
            m_OnFinishMoveCookies -= OnFinishMoveCookiesTowardLeftOrUp;

            Cookie firstCookie = m_TempMovingCookies[0];
            firstCookie.gameObject.SetActive(false);
            Cookie tempCoocike = m_TempMovingCookies[m_TempMovingCookies.Count - 1];
            firstCookie.transform.position = tempCoocike.transform.position;
            firstCookie.gameObject.SetActive(true);
            m_CookiePool.Release(tempCoocike);
        }

        private void OnFinishMoveCookiesTowardUp()
        {
            m_OnFinishMoveCookies -= OnFinishMoveCookiesTowardUp;

            Cookie lastCookie = m_TempMovingCookies[m_TempMovingCookies.Count - 1];
            lastCookie.gameObject.SetActive(false);
            Cookie tempCoocike = m_TempMovingCookies[0];
            lastCookie.transform.position = tempCoocike.transform.position;
            lastCookie.gameObject.SetActive(true);
            m_CookiePool.Release(tempCoocike);
        }

        private void OnCookieFinishMoving(MovableTile cookie)
        {
            cookie.FinishMoving -= OnCookieFinishMoving;
            m_OnMovingCookieCount--;

            if (m_OnMovingCookieCount <= 0)
            {
                m_OnMovingCookieCount = 0;
                m_OnFinishMoveCookies?.Invoke();
                m_TempBlocks.Clear();
                m_TempMovingCookies.Clear();
                m_IsMoving = false;
                DoFinishCookieMovement();
            }
        }

        #endregion

        private void DoFinishCookieMovement()
        {
            OnFinishMovingCookies?.Invoke();
        }

        private void OnDisappearCookiesStepFinished(CookiesMatcher.MatchCookiesData data)
        {
            for (int i = 0; i < data.MatchedCookies.Count; i++)
            {
                m_CookiePool.Release(data.MatchedCookies[i]);
            }
        }

        private void OnDisappearCookiesFinished(List<CookiesMatcher.MatchCookiesData> list)
        {
            CleanBoard();
        }

        private void OnMatchingProcessFinished()
        {
            Debug.Log("OnMatchingProcessFinished");

            RefillBoard();
        }

        private void CleanBoard()
        {
            m_TempCleanCookies.Clear();

            Cookie[,] cookies;
            Block[,] blocks;
            TakeBoardSnapshot(out cookies, out blocks);

            //for (int y = 0; y < cookies.GetLength(1); y++)
            //{
            //    for (int x = 0; x < cookies.GetLength(0); x++)
            //    {
            //        Debug.Log($"X:{x} Y:{y} / {cookies[x, y]}");
            //    }
            //}

            int xCount = cookies.GetLength(0);
            int yCount = cookies.GetLength(1);

            for (int y = yCount - 1; y >= 0; y--)
            {
                for (int x = 0; x < xCount; x++)
                {
                    int yPointer = y;
                    Cookie currentCookie = cookies[x, y];

                    if (currentCookie != null)
                    {
                        continue;
                    }

                    while (yPointer >= 0)
                    {
                        Cookie pointerCookie = cookies[x, yPointer];
                        if (pointerCookie != null)
                        {
                            cookies[x, y] = pointerCookie;
                            cookies[x, yPointer] = null;
                            break;
                        }

                        yPointer--;
                    }
                }
            }

            for (int y = 0; y < yCount; y++)
            {
                for (int x = 0; x < xCount; x++)
                {
                    Cookie cookie = cookies[x, y];
                    if (cookie != null)
                    {
                        cookie.FinishMoving += OnFinishMovingCookieCleanBoard;

                        if (cookie.TryMove(blocks[x, y].transform, m_CleanCookieMoveDuration))
                        {
                            m_TempCleanCookies.Add(cookie);
                        }
                        else
                        {
                            cookie.FinishMoving -= OnFinishMovingCookieCleanBoard;
                        }
                    }
                }
            }

            if (m_TempCleanCookies.Count <= 0)
            {
                Debug.Log("No move");
                OnFinishCleanBoard?.Invoke();
            }

            //Debug.Log("////////////////////////////////////////////////////////////////");

            //for (int y = 0; y < cookies.GetLength(1); y++)
            //{
            //    for (int x = 0; x < cookies.GetLength(0); x++)
            //    {
            //        Debug.Log($"X:{x} Y:{y} / {cookies[x, y]}");
            //    }
            //}
        }

        private void TakeBoardSnapshot(out Cookie[,] cookies, out Block[,] blocks)
        {
            cookies = new Cookie[m_BoardData.VisibleBoardSize.x, m_BoardData.VisibleBoardSize.y];
            blocks = new Block[m_BoardData.VisibleBoardSize.x, m_BoardData.VisibleBoardSize.y];
            IReadOnlyList<Block> rowBlocks;
            IReadOnlyList<Cookie> rowCookies;
            int xOffset = Mathf.Abs(m_BoardData.OriginalBoardSize.x - m_BoardData.VisibleBoardSize.x) / 2;
            int yOffset = Mathf.Abs(m_BoardData.OriginalBoardSize.y - m_BoardData.VisibleBoardSize.y) / 2;

            for (int y = yOffset; y < m_BoardData.OriginalBoardSize.y - yOffset; y++)
            {
                rowBlocks = m_BoardData.GetBlokcsAtRow(y, true);
                if (rowBlocks.Count <= 0)
                {
                    continue;
                }

                rowCookies = m_BoardData.GetRowCookiesAtId(rowBlocks[0].Id);

                for (int x = xOffset; x < m_BoardData.OriginalBoardSize.x - xOffset; x++)
                {
                    cookies[x - xOffset, y - yOffset] = x - xOffset < rowCookies.Count ? rowCookies[x - xOffset] : null;
                    blocks[x - xOffset, y - yOffset] = rowBlocks[x - xOffset];
                }
            }
        }

        private void OnFinishMovingCookieCleanBoard(MovableTile cookie)
        {
            cookie.FinishMoving -= OnFinishMovingCookieCleanBoard;

            m_TempCleanCookies.Remove(cookie);

            if (m_TempCleanCookies.Count <= 0)
            {
                OnFinishCleanBoard?.Invoke();
            }
        }

        private void RefillBoard()
        {
            Cookie[,] cookies;
            Block[,] blocks;
            TakeBoardSnapshot(out cookies, out blocks);
            int xCount = cookies.GetLength(0);
            int yCount = cookies.GetLength(1);
            int xOffset = Mathf.Abs((m_BoardData.OriginalBoardSize.x - m_BoardData.VisibleBoardSize.x) / 2);
            int yOffset = Mathf.Abs((m_BoardData.OriginalBoardSize.y - m_BoardData.VisibleBoardSize.y) / 2);
            IReadOnlyList<Block> firstRowBlocks = m_BoardData.GetBlokcsAtRow(0);
            IReadOnlyList<Block> lastColumnBlock = m_BoardData.GetBlocksAtColumn(m_BoardData.OriginalBoardSize.x - 1);
            IReadOnlyList<Cookie> tempCookieList;

            MatchVerticaly();
            MatchHorizontaly();

            void MatchVerticaly()
            {
                for (int x = 0; x < xCount; x++)
                {
                    tempCookieList = GetCookiesAtColumn(x);
                    if (tempCookieList.Count < m_BoardData.VisibleBoardSize.y - 1)
                    {
                        int startCount = tempCookieList.Count;
                        for (int y2 = (m_BoardData.VisibleBoardSize.y - 1) - startCount; y2 >= 0; y2--)
                        {
                            Cookie cookie = m_CookiePool.Get();
                            m_TempRefillCookies.Add(cookie);
                            cookie.transform.position = lastColumnBlock[y2 + yOffset].transform.position;
                            cookie.Init(GetRandomCookieProperty(), m_BoardIdentity);
                            cookie.FinishMoving += OnFinishMovingInRefilling;
                            cookie.TryMove(blocks[x, y2].transform, m_RefillCookieMoveDuration);
                            cookies[x, y2] = cookie;
                        }
                    }
                }
            }

            void MatchHorizontaly()
            {
                for (int y = 0; y < yCount; y++)
                {
                    for (int x = 0; x < xCount; x++)
                    {
                        Cookie cookie = cookies[x, y];
                        if (cookie == null)
                        {
                            cookie = m_CookiePool.Get();
                            m_TempRefillCookies.Add(cookie);
                            cookie.transform.position = firstRowBlocks[x + xOffset].transform.position;
                            cookie.Init(GetRandomCookieProperty(), m_BoardIdentity);
                            cookie.FinishMoving += OnFinishMovingInRefilling;
                            cookie.TryMove(blocks[x, y].transform, m_RefillCookieMoveDuration);
                        }
                    }
                }
            }

            IReadOnlyList<Cookie> GetCookiesAtColumn(int columnIndex)
            {
                List<Cookie> result = new List<Cookie>();

                for (int y = 0; y < yCount; y++)
                {
                    if (cookies[columnIndex, y] != null)
                    {
                        result.Add(cookies[columnIndex, y]);
                    }
                }

                return result;
            }
        }

        private void OnFinishMovingInRefilling(MovableTile cookie)
        {
            cookie.FinishMoving -= OnFinishMovingInRefilling;

            m_TempRefillCookies.Remove(cookie);
            if (m_TempRefillCookies.Count <= 0)
            {
                Debug.Log("On Finish Refilling");
                OnFinishRefilling?.Invoke();
            }
        }

        private void FillBoard()
        {
            for (int i = 0; i < m_BoardData.VisibleBlocks.Count; i++)
            {
                Cookie cookie = m_CookiePool.Get();
                cookie.Init(GetRandomCookieProperty(), m_BoardIdentity);
                cookie.transform.position = m_BoardData.VisibleBlocks[i].transform.position;
            }
        }

        private CookieProperties GetRandomCookieProperty()
        {
            return m_CookieProperties[Random.Range(0, m_CookieProperties.Length)];
        }

        #region Pool
        private Cookie OnCreate()
        {
            Cookie cookie = Instantiate(m_CookiePrefab, m_CookieParent);
            return cookie;
        }

        private void OnGet(Cookie cookie)
        {
            cookie.gameObject.SetActive(true);
        }

        private void OnRelease(Cookie cookie)
        {
            cookie.ResetIt();
            cookie.gameObject.SetActive(false);
        }
        #endregion
    }
}