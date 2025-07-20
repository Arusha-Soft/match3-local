using Project.InputHandling;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

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

        private ObjectPool<Cookie> m_CookiePool;

        public void Init(BoardData boardData, BoardInputHandler input, SelectionBox selectionBox, BoardIdentity boardIdentity)
        {
            m_BoardData = boardData;
            m_Input = input;
            m_SelectionBox = selectionBox;
            m_BoardIdentity = boardIdentity;

            m_CookiePool = new ObjectPool<Cookie>(OnCreate, OnGet, OnRelease);

            FillBoard();
        }

        private IEnumerator Checking()
        {
            while (true)
            {

                yield return null;
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