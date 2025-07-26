using UnityEngine;

namespace Project.Core
{
    public class BoardHealth : MonoBehaviour
    {
        [SerializeField] private GameObject[] m_Points;

        public void SetHealth(int health)
        {
            for (int i = 0; i < m_Points.Length; i++)
            {
                m_Points[i].SetActive(i <= health - 1);
            }
        }
    }
}