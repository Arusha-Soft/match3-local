using UnityEngine;

namespace Project.Core
{
    public class BoardFuse : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer m_FuseRenderer;

        public void SetValue(float value)
        {
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            m_FuseRenderer.GetPropertyBlock(mpb);
            mpb.SetFloat("_FillAmount", value);
            m_FuseRenderer.SetPropertyBlock(mpb);
        }
    }
}