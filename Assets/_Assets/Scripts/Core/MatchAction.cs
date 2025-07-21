using System.Collections.Generic;
using UnityEngine;

namespace Project.Core
{
    public abstract class MatchAction : ScriptableObject
    {
        [SerializeField] protected CookieProperties m_TargetCookie;
        [SerializeField] private int m_MatchCount;
        [SerializeField] private int m_MatchScore;

        public int MatchCount => m_MatchCount;

        /// <summary>
        /// Return result of matching.
        /// </summary>
        /// <param name="score">If matching is success, its return score</param>
        /// <returns></returns>
        public bool TryMatch(IReadOnlyList<Cookie> cookies, out int score, int matchCount = -1)
        {
            matchCount = matchCount < 0 ? m_MatchCount : matchCount;
            score = m_MatchScore;
            return TryMatch(cookies, matchCount);
        }

        protected abstract bool TryMatch(IReadOnlyList<Cookie> cookies, int matchCount);
    }
}