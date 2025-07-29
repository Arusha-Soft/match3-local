using DG.Tweening;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

namespace Project.Core
{
    public class BoardScore : MonoBehaviour
    {
        [SerializeField] private GameObject[] m_Points;
        [SerializeField] private DOTweenAnimation[] m_PlusPoints;
        [SerializeField] private DOTweenAnimation[] m_NegativePoints;

        [field: SerializeField] public int Score { private set; get; }

        private void Awake()
        {
            for (int i = 0; i < m_Points.Length; i++)
            {
                m_Points[i].SetActive(i <= Score - 1);
            }
        }
        public void Init()
        {
            //SetScore(0);
        }

        public void SetScore(int score)
        {
            Score = score;

            for (int i = 0; i < m_Points.Length; i++)
            {
                m_Points[i].SetActive(i <= score - 1);
            }
        }

        [ContextMenu("Test Change")]
        private void TestChange()
        {
            ChangeScoreWithEffect(ww);
        }

        public int ww;

        /// <summary>
        /// Changes the score and plays tween effects for increasing or decreasing points.
        /// </summary>
        public void ChangeScoreWithEffect(int amount)
        {
            int oldScore = Score;
            int newScore = Mathf.Clamp(Score + amount, 0, m_Points.Length);

            if (amount < 0) // Decreasing score
            {

            }
            else if (amount > 0) // Increasing score
            {
                int startIndex = Score;

                for (int i = startIndex; i < (amount + startIndex); i++)
                {
                    Debug.Log(i);
                    if(i >= m_PlusPoints.Length)
                    {
                        break;
                    }

                    m_PlusPoints[i].tween.Restart();
                }
            }
        }

        /// <summary>
        /// Updates which point GameObjects are active based on the current score.
        /// </summary>
        private void UpdatePointsVisual()
        {
            for (int i = 0; i < m_Points.Length; i++)
            {
                m_Points[i].SetActive(i < Score);
            }
        }

        public void ResetIt()
        {
            SetScore(0);
        }
    }
}