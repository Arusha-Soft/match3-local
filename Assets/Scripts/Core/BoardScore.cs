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

            if (Score >= m_MaxScore)
            {
                OnReachMaxScore?.Invoke();
            }
        }

        public void ChangeScoreWithEffect(int amount, Action onFinshAnimation)
        {
            int oldScore = Score;
            int newScore = Mathf.Clamp(Score + amount, 0, m_MaxScore);

            if (amount < 0)
            {
                if (newScore == 0)
                {
                    SetScore(0);
                    onFinshAnimation?.Invoke();
                    return;
                }

                for (int i = Score - 1; i >= newScore; i--)
                {
                    if (i < 0 || i >= m_NegativePoints.Length)
                        continue;

                    int index = i;
                    m_PlayingCountAnimations++;

                    m_Points[index].SetActive(false);
                    m_NegativePoints[index].gameObject.SetActive(true);

                    m_NegativePoints[index].tween.onComplete = () =>
                    {
                        m_PlayingCountAnimations--;
                        if (m_PlayingCountAnimations == 0)
                        {
                            SetScore(newScore);
                            onFinshAnimation?.Invoke();
                        }
                    };

                    m_NegativePoints[index].tween.Restart();
                }
            }
            else if (amount > 0)
            {
                if (Score >= m_MaxScore)
                {
                    onFinshAnimation?.Invoke();
                    return;
                }

                int startIndex = Score;
                int endIndex = Mathf.Min(startIndex + amount, m_MaxScore);

                for (int i = startIndex; i < endIndex; i++)
                {
                    int index = i;
                    m_PlayingCountAnimations++;

                    m_PlusPoints[index].gameObject.SetActive(true);
                    m_PlusPoints[index].tween.onComplete = () =>
                    {
                        m_PlayingCountAnimations--;
                        if (m_PlayingCountAnimations == 0)
                        {
                            SetScore(newScore);
                            onFinshAnimation?.Invoke();
                        }
                    };

                    m_PlusPoints[index].tween.Restart();
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