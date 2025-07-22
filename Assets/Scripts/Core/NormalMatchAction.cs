using Project.Core;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NormalMatchAction", menuName = "Core/NormalMatchAction")]
public class NormalMatchAction : MatchAction
{
    protected override bool TryMatch(IReadOnlyList<Cookie> cookies, int matchCount)
    {
        int matchedCount = 0;

        for (int i = 0; i < cookies.Count; i++)
        {
            matchedCount += cookies[i].Properties == m_TargetCookie ? 1 : -matchedCount;

            if (matchedCount >= matchCount)
            {
                return true;
            }
        }

        return false;
    }
}
