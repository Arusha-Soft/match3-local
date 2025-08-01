using DG.Tweening;
using System;
using UnityEngine;

namespace Project.Core
{
    public class BoardScore : MonoBehaviour
    {
        [SerializeField] private GameObject[] m_Points;
        [SerializeField] private DOTweenAnimation[] m_PlusPoints;
        [SerializeField] private DOTweenAnimation[] m_NegativePoints;

        [field: SerializeField] public int Score { private set; get; }
        public event Action OnReachMaxScore;

        private int m_PlayingCountAnimations = 0;
        private int m_MaxScore;

        public void Init()
        {
            m_MaxScore = m_Points.Length;

            SetScore(0);
        }

        public void SetScore(int score)
        {
            Score = score;
            UpdatePointsVisual();

            if(Score >= m_MaxScore)
            {
                OnReachMaxScore?.Invoke();
            }
        }

        public void ChangeScoreWithEffect(int amount, Action onFinshAnimation)
        {
            int oldScore = Score;
            int newScore = Mathf.Clamp(Score + amount, 0, m_Points.Length);

            if (amount < 0) // Decreasing score
            {
                for (int i = Score + amount; i < Score; i++)
                {
                    Debug.Log(i);
                    if (i < 0)
                    {
                        continue;
                    }

                    m_PlayingCountAnimations++;

                    m_Points[i].SetActive(false);
                    m_NegativePoints[i].gameObject.SetActive(true);
                    m_NegativePoints[i].tween.onComplete = () =>
                    {
                        m_PlayingCountAnimations--;

                        if (m_PlayingCountAnimations <= 0)
                        {
                            SetScore(newScore);
                            m_PlayingCountAnimations = 0;
                            onFinshAnimation?.Invoke();
                        }
                    };

                    m_NegativePoints[i].tween.Restart();
                }
            }
            else if (amount > 0) // Increasing score
            {
                int startIndex = Score;

                for (int i = startIndex; i < (amount + startIndex); i++)
                {
                    if (i >= m_PlusPoints.Length)
                    {
                        break;
                    }

                    m_PlayingCountAnimations++;

                    m_PlusPoints[i].gameObject.SetActive(true);
                    m_PlusPoints[i].tween.onComplete = () =>
                    {
                        m_PlayingCountAnimations--;

                        if (m_PlayingCountAnimations <= 0)
                        {
                            SetScore(newScore);
                            m_PlayingCountAnimations = 0;
                            onFinshAnimation?.Invoke();
                        }
                    };

                    m_PlusPoints[i].tween.Restart();
                }
            }
        }

        private void UpdatePointsVisual()
        {
            for (int i = 0; i < m_Points.Length; i++)
            {
                m_Points[i].SetActive(i < Score);
                m_PlusPoints[i].gameObject.SetActive(false);
                m_NegativePoints[i].gameObject.SetActive(false);
            }
        }

        public void ResetIt()
        {
            SetScore(0);
        }
    }
}