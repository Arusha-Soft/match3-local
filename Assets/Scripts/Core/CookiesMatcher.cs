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

        public event Action<MatchCookiesData> OnDisappearCookiesStarted;
        public event Action<MatchCookiesData> OnDisappearCookiesStepFinished;
        public event Action<List<MatchCookiesData>> OnDisappearCookiesFinished;
        public event Action OnMatchingProcessFinished;

        private List<MatchCookiesData> m_DisapparMatchedCookies = new List<MatchCookiesData>();
        private CookiesController m_CookiesController;
        private BoardData m_BoardData;

        private bool m_CanDoCombo = false;

        public void Init(CookiesController cookiesController, BoardData boardData)
        {
            m_CookiesController = cookiesController;
            m_BoardData = boardData;
            m_CookiesController.OnFinishMovingCookies += OnFinishMovingCookies;
            m_CookiesController.OnFinishCleanBoard += OnFinishCleanBoard;
        }

        private void OnFinishMovingCookies()
        {
            FindAllMatchCookies(false);
        }

        private void OnFinishCleanBoard()
        {
            Debug.Log("OnFinishCleanBoard");
            FindAllMatchCookies(true);
        }

        private void FindAllMatchCookies(bool combo)
        {
            m_MatchedCookies.Clear();
            m_DisapparMatchedCookies.Clear();
            m_CanDoCombo = false;

            FindMatchCookiesInHorizontal(combo);
            FindMatchCookiesInVertical(combo);

            if (combo)
            {
                TryCombo();
            }
            else
            {
                TryMatch();
            }
        }

        private Dictionary<MatchCookiesData, int> m_MatchDataDictionary = new Dictionary<MatchCookiesData, int>();

        private void TryMatch()
        {
            m_MatchDataDictionary.Clear();

            for (int i = m_MatchedCookies.Count - 1; i >= 0; i--)
            {
                MatchCookiesData data = m_MatchedCookies[i];
                if (data.MatchCount >= m_MatchCount)
                {
                    OnDisappearCookiesStarted?.Invoke(data);

                    m_MatchDataDictionary.Add(data, 0);
                    for (int j = 0; j < data.MatchedCookies.Count; j++)
                    {
                        data.MatchedCookies[j].OnFinishHideAnimation += OnFinishHideAnimationCookie;
                        data.MatchedCookies[j].PlayHideAnimation();
                    }
                }
            }
        }

        private void TryCombo()
        {
            if (m_CanDoCombo)
            {
                m_MatchDataDictionary.Clear();

                for (int i = m_MatchedCookies.Count - 1; i >= 0; i--)
                {
                    MatchCookiesData data = m_MatchedCookies[i];
                    if (data.MatchCount >= m_ComboMatchCount)
                    {
                        OnDisappearCookiesStarted?.Invoke(data);

                        m_MatchDataDictionary.Add(data, 0);

                        for (int j = 0; j < data.MatchedCookies.Count; j++)
                        {
                            data.MatchedCookies[j].OnFinishComboHideAnimation += OnFinishComboHideAnimationCookie;
                            data.MatchedCookies[j].PlayComboHideAnimation();
                        }
                    }
                }
            }
            else
            {
                OnMatchingProcessFinished?.Invoke();
            }
        }

        private void OnFinishHideAnimationCookie(Cookie cookie)
        {
            cookie.OnFinishHideAnimation -= OnFinishHideAnimationCookie;

            foreach (MatchCookiesData data in m_MatchDataDictionary.Keys)
            {
                for (int i = 0; i < data.MatchCount; i++)
                {
                    if (data.MatchedCookies[i] == cookie)
                    {
                        int finishAnimtionCount = m_MatchDataDictionary[data] + 1;
                        m_MatchDataDictionary[data] = finishAnimtionCount;
                        if (finishAnimtionCount >= data.MatchCount)
                        {
                            m_DisapparMatchedCookies.Add(data);
                            OnDisappearCookiesStepFinished?.Invoke(data);

                            if (m_DisapparMatchedCookies.Count == m_MatchedCookies.Count)
                            {
                                OnDisappearCookiesFinished?.Invoke(m_DisapparMatchedCookies);
                            }
                        }
                        return;
                    }
                }
            }
        }

        private void OnFinishComboHideAnimationCookie(Cookie cookie)
        {
            cookie.OnFinishComboHideAnimation-= OnFinishComboHideAnimationCookie;

            foreach (MatchCookiesData data in m_MatchDataDictionary.Keys)
            {
                for (int i = 0; i < data.MatchCount; i++)
                {
                    if (data.MatchedCookies[i] == cookie)
                    {
                        int finishAnimtionCount = m_MatchDataDictionary[data] + 1;
                        m_MatchDataDictionary[data] = finishAnimtionCount;
                        if (finishAnimtionCount >= data.MatchCount)
                        {
                            m_DisapparMatchedCookies.Add(data);
                            OnDisappearCookiesStepFinished?.Invoke(data);

                            if (m_DisapparMatchedCookies.Count == m_MatchedCookies.Count)
                            {
                                OnDisappearCookiesFinished?.Invoke(m_DisapparMatchedCookies);
                            }
                        }
                        return;
                    }
                }
            }
        }

        private void FindMatchCookiesInHorizontal(bool includeCombo)
        {
            IReadOnlyList<Block> blocks;
            List<Cookie> cookies;

            for (int i = 0; i < m_BoardData.OriginalBoardSize.x; i++)
            {
                blocks = m_BoardData.GetBlocksAtColumn(i);
                cookies = m_BoardData.GetColumnCookiesAtId(blocks[0].Id).ToList();
                TryFindMatchSet(cookies, i, true, includeCombo);
            }
        }

        private void FindMatchCookiesInVertical(bool includeCombo)
        {
            IReadOnlyList<Block> blocks;
            List<Cookie> cookies;

            for (int i = 0; i < m_BoardData.OriginalBoardSize.x; i++)
            {
                blocks = m_BoardData.GetBlokcsAtRow(i);
                cookies = m_BoardData.GetRowCookiesAtId(blocks[0].Id).ToList();
                TryFindMatchSet(cookies, i, false, includeCombo);
            }
        }

        private void TryFindMatchSet(List<Cookie> cookies, int clolumnOrRowIndex, bool isHorizontal, bool includeCombo)
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
                        MatchCount = m_MatchActions[j].MatchCount,
                        ColumnOrRowIndex = clolumnOrRowIndex
                    };
                    matchCookiesData.InitializeMatchedCookies();

                    m_MatchedCookies.Add(matchCookiesData);
                }
                else if (includeCombo && m_MatchActions[j].TryMatch(cookies, out int comboScore, m_ComboMatchCount))
                {
                    MatchCookiesData matchCookiesData = new MatchCookiesData()
                    {
                        IsHorizontal = isHorizontal,
                        MatchCookieProperties = m_MatchActions[j].TargetCookie,
                        AllCookies = cookies,
                        Score = comboScore,
                        MatchCount = m_ComboMatchCount,
                        ColumnOrRowIndex = clolumnOrRowIndex
                    };
                    matchCookiesData.InitializeMatchedCookies();

                    m_MatchedCookies.Add(matchCookiesData);
                    m_CanDoCombo = true;
                }
            }
        }

        [System.Serializable]
        public class MatchCookiesData
        {
            public bool IsHorizontal;
            public CookieProperties MatchCookieProperties;
            public List<Cookie> AllCookies; // list of cookies which have some match
            public int Score;
            public int MatchCount;
            public int ColumnOrRowIndex;

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
