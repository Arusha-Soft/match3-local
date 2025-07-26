using UnityEngine;

namespace Project.Core
{
    public class BoardScore : MonoBehaviour
    {
        [SerializeField] private GameObject[] m_Points;

        public int Score { private set; get; }

        public void SetScore(int score)
        {
            Score = score;

            for (int i = 0; i < m_Points.Length; i++)
            {
                m_Points[i].SetActive(i <= score - 1);
            }
        }

        public void ResetIt()
        {
            SetScore(0);
        }
    }
}