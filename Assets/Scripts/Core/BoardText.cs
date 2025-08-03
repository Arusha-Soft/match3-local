using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Project.Core
{
    public class BoardText : MonoBehaviour
    {
        [SerializeField] private TextMeshPro m_Text;
        [SerializeField] private DOTweenAnimation m_Animation;

        public void SetText(string text)
        {
            m_Text.text = text;
        }

        public void PlayAnimation()
        {
            m_Animation.tween.Restart();
        }
    }
}