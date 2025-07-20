using UnityEngine;

namespace Project.Core
{
    [CreateAssetMenu(fileName = "CookieProperties", menuName = "Core/CookieProperties")]
    public class CookieProperties : ScriptableObject
    {
        public string m_CookieName;
        public Sprite Icon;
    }
}
