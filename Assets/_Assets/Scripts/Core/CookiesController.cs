using Project.InputHandling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace Project.Core
{
    public class CookiesController : MonoBehaviour
    {
        [SerializeField] private CookieProperties[] m_CookieProperties;
        [SerializeField] private Cookie m_CookiePrefab;
        [SerializeField] private Transform m_CookieParent;

        private BoardData m_BoardData;
        private BoardInputHandler m_Input;
        private SelectionBox m_SelectionBox;
        private BoardIdentity m_BoardIdentity;
        private List<Block> m_TempBlocks = new List<Block>();
        private List<Cookie> m_TempCookies = new List<Cookie>();

        private Action m_OnFinishMoveCookies;

        private bool m_IsMoving = false;
        private int m_OnMovingCookieCount = 0;

        private ObjectPool<Cookie> m_CookiePool;

        public void Init(BoardData boardData, BoardInputHandler input, SelectionBox selectionBox, BoardIdentity boardIdentity)
        {
            m_BoardData = boardData;
            m_Input = input;
            m_SelectionBox = selectionBox;
            m_BoardIdentity = boardIdentity;

            m_CookiePool = new ObjectPool<Cookie>(OnCreate, OnGet, OnRelease);

            StartCoroutine(MoveHandling());
            FillBoard();
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
                        m_TempCookies = m_BoardData.GetRowCookiesAtId(m_SelectionBox.CurrentBlockId).ToList();
                        HandleMoveTowardRightOrDown();
                    }
                    else if (m_Input.IsLeft)
                    {
                        m_TempBlocks = m_BoardData.GetRowBlocksAtId(m_SelectionBox.CurrentBlockId).ToList();
                        m_TempCookies = m_BoardData.GetRowCookiesAtId(m_SelectionBox.CurrentBlockId).ToList();
                        HandleMoveTowardLeftOrUp();
                    }
                    else if (m_Input.IsUp)
                    {
                        m_TempBlocks = m_BoardData.GetColumnBlocksAtId(m_SelectionBox.CurrentBlockId).ToList();
                        m_TempCookies = m_BoardData.GetColumnCookiesAtId(m_SelectionBox.CurrentBlockId).ToList();
                        HandleMoveTowardLeftOrUp();
                    }
                    else if (m_Input.IsDown)
                    {
                        m_TempBlocks = m_BoardData.GetColumnBlocksAtId(m_SelectionBox.CurrentBlockId).ToList();
                        m_TempCookies = m_BoardData.GetColumnCookiesAtId(m_SelectionBox.CurrentBlockId).ToList();
                        HandleMoveTowardRightOrDown();
                    }
                }

                yield return null;
            }

            void HandleMoveTowardRightOrDown()
            {
                Cookie lastCookie = m_TempCookies[m_TempCookies.Count - 1];
                Block firstBlock = m_TempBlocks[0];
                Cookie tempCookie = m_CookiePool.Get();
                tempCookie.Init(lastCookie.Properties, tempCookie.Owner);
                tempCookie.transform.position = firstBlock.transform.position;
                tempCookie.name = "TempCookie";
                m_TempCookies.Insert(0, tempCookie);
                m_OnFinishMoveCookies += OnFinishMoveCookiesTowardRightOrDown;
                DoMoveCookiesTowardRightOrDown();
            }

            void HandleMoveTowardLeftOrUp()
            {
                Cookie firstCookie = m_TempCookies[0];
                Block firstBlock = m_TempBlocks[m_TempBlocks.Count - 1];
                Debug.Log(firstBlock);
                Cookie tempCookie = m_CookiePool.Get();
                tempCookie.Init(firstCookie.Properties, tempCookie.Owner);
                tempCookie.transform.position = firstBlock.transform.position;
                tempCookie.name = "TempCookie";
                m_TempCookies.Add(tempCookie);
                m_OnFinishMoveCookies += OnFinishMoveCookiesTowardLeftOrUp;
                DoMoveCookiesTowardLeftOrUp();
            }

            void DoMoveCookiesTowardRightOrDown()
            {
                m_IsMoving = true;

                for (int i = 0; i < m_TempCookies.Count; i++)
                {
                    Cookie cookie = m_TempCookies[i];
                    cookie.FinishMoving += OnCookieFinishMoving;
                    cookie.Move(m_TempBlocks[i + 1].transform);
                    m_OnMovingCookieCount++;
                }
            }

            void DoMoveCookiesTowardLeftOrUp()
            {
                m_IsMoving = true;

                for (int i = 0; i < m_TempCookies.Count; i++)
                {
                    Cookie cookie = m_TempCookies[i];
                    cookie.FinishMoving += OnCookieFinishMoving;
                    cookie.Move(m_TempBlocks[i].transform);
                    m_OnMovingCookieCount++;
                }
            }
        }

        private void OnFinishMoveCookiesTowardRightOrDown()
        {
            m_OnFinishMoveCookies -= OnFinishMoveCookiesTowardRightOrDown;

            Cookie lastCookie = m_TempCookies[m_TempCookies.Count - 1];
            lastCookie.gameObject.SetActive(false);
            Cookie tempCoocike = m_TempCookies[0];
            lastCookie.transform.position = tempCoocike.transform.position;
            lastCookie.gameObject.SetActive(true);
            m_CookiePool.Release(tempCoocike);
        }

        private void OnFinishMoveCookiesTowardLeftOrUp()
        {
            m_OnFinishMoveCookies -= OnFinishMoveCookiesTowardLeftOrUp;

            Cookie firstCookie = m_TempCookies[0];
            firstCookie.gameObject.SetActive(false);
            Cookie tempCoocike = m_TempCookies[m_TempCookies.Count - 1];
            firstCookie.transform.position = tempCoocike.transform.position;
            firstCookie.gameObject.SetActive(true);
            m_CookiePool.Release(tempCoocike);
        }

        private void OnFinishMoveCookiesTowardUp()
        {
            m_OnFinishMoveCookies -= OnFinishMoveCookiesTowardUp;

            Cookie lastCookie = m_TempCookies[m_TempCookies.Count - 1];
            lastCookie.gameObject.SetActive(false);
            Cookie tempCoocike = m_TempCookies[0];
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
                m_TempCookies.Clear();
                m_IsMoving = false;
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
            cookie.gameObject.SetActive(false);
        }
        #endregion
    }
}