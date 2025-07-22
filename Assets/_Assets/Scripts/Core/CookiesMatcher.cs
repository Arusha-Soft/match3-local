using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Project.Core
{
    public class CookiesMatcher : MonoBehaviour
    {
        [SerializeField] private int m_MatchCount = 5;
        [SerializeField] private int m_ComboMatchCount = 4;

        [SerializeField] private MatchAction[] m_MatchActions;

        [SerializeField] List<MatchCookiesData> m_MatchedCookies = new List<MatchCookiesData>();

        public event Action<MatchCookiesData> OnDisappearCookies;

        private CookiesController m_CookiesController;
        private BoardData m_BoardData;

        public void Init(CookiesController cookiesController, BoardData boardData)
        {
            m_CookiesController = cookiesController;
            m_BoardData = boardData;
            m_CookiesController.OnFinishMovingCookies += OnFinishMovingCookies;
        }

        private void OnFinishMovingCookies()
        {
            FindAllMatchCookies();
        }

        private void FindAllMatchCookies()
        {
            m_MatchedCookies.Clear();

            FindMatchCookiesInHorizontal();
            FindMatchCookiesInVertical();
            TryDisappearCookies();
        }

        private void TryDisappearCookies()
        {
            bool canDoCombo = false;

            for (int i = m_MatchedCookies.Count - 1; i >= 0; i--)
            {
                MatchCookiesData data = m_MatchedCookies[i];
                if (data.MatchCount >= m_MatchCount)
                {
                    for (int j = 0; j < data.MatchedCookies.Count; j++)
                    {
                        data.MatchedCookies[j].PlayHideAnimation();
                    }

                    OnDisappearCookies?.Invoke(data);
                    m_MatchedCookies.RemoveAt(i);
                    canDoCombo = true;
                }
            }

            if (canDoCombo)
            {
                for (int i = m_MatchedCookies.Count - 1; i >= 0; i--)
                {
                    MatchCookiesData data = m_MatchedCookies[i];
                    if (data.MatchCount >= m_ComboMatchCount)
                    {
                        for (int j = 0; j < data.MatchedCookies.Count; j++)
                        {
                            data.MatchedCookies[j].PlayComboHideAnimation();
                        }

                        OnDisappearCookies?.Invoke(data);
                        m_MatchedCookies.RemoveAt(i);
                    }
                }
            }
        }

        private void FindMatchCookiesInHorizontal()
        {
            IReadOnlyList<Block> blocks;
            List<Cookie> cookies;

            for (int i = 0; i < m_BoardData.OriginalBoardSize.x; i++)
            {
                blocks = m_BoardData.GetBlocksAtColumn(i);
                cookies = m_BoardData.GetColumnCookiesAtId(blocks[0].Id).ToList();
                TryFindMatchSet(cookies, true);
            }
        }

        private void FindMatchCookiesInVertical()
        {
            IReadOnlyList<Block> blocks;
            List<Cookie> cookies;

            for (int i = 0; i < m_BoardData.OriginalBoardSize.x; i++)
            {
                blocks = m_BoardData.GetBlokcsAtRow(i);
                cookies = m_BoardData.GetRowCookiesAtId(blocks[0].Id).ToList();
                TryFindMatchSet(cookies, false);
            }
        }

        private void TryFindMatchSet(List<Cookie> cookies, bool isHorizontal)
        {
            for (int j = 0; j < m_MatchActions.Length; j++)
            {
                if (m_MatchActions[j].TryMatch(cookies, out int matchScore))
                {
                    MatchCookiesData matchCookiesData = new MatchCookiesData()
                    {
                        IsHorizontal = isHorizontal,
                        MatchCookieProperties = m_MatchActions[j].TargetCookie,
                        AllCookies = cookies,
                        Score = matchScore,
                        MatchCount = m_MatchActions[j].MatchCount
                    };
                    matchCookiesData.InitializeMatchedCookies();

                    m_MatchedCookies.Add(matchCookiesData);
                }
                else if (m_MatchActions[j].TryMatch(cookies, out int comboScore, m_ComboMatchCount))
                {
                    MatchCookiesData matchCookiesData = new MatchCookiesData()
                    {
                        IsHorizontal = isHorizontal,
                        MatchCookieProperties = m_MatchActions[j].TargetCookie,
                        AllCookies = cookies,
                        Score = comboScore,
                        MatchCount = m_ComboMatchCount
                    };
                    matchCookiesData.InitializeMatchedCookies();

                    m_MatchedCookies.Add(matchCookiesData);
                }
            }
        }

        [System.Serializable]
        public struct MatchCookiesData
        {
            public bool IsHorizontal;
            public CookieProperties MatchCookieProperties;
            public List<Cookie> AllCookies; // list of cookies which have some match
            public int Score;
            public int MatchCount;

            public IReadOnlyList<Cookie> MatchedCookies { private set; get; }// list of the cookies which are matched

            public void InitializeMatchedCookies()
            {
                List<Cookie> cookies = new List<Cookie>(AllCookies.Count);

                for (int i = 0; i < AllCookies.Count; i++)
                {
                    if (AllCookies[i].Properties == MatchCookieProperties)
                    {
                        cookies.Add(AllCookies[i]);
                    }
                }

                MatchedCookies = cookies;
            }
        }
    }
}
