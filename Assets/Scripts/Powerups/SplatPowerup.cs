using Project.Core;
using UnityEngine;

namespace Project.Powerups
{
    public class SplatPowerup : DurationalPowerup
    {
        [SerializeField] private Color m_StartColor = Color.white;
        [SerializeField] private Color m_SplatColor;

        protected override void OnStartPowerup(BoardIdentity attacker, BoardIdentity defender, bool isTeammode)
        {
            defender.CookiesController.OnGetCookie += OnGetCookie;

            foreach (Cookie cookie in defender.CookiesController.ActiveCookies)
            {
                cookie.SetColor(m_SplatColor);
            }
        }

        protected override void OnEndPowerup(BoardIdentity attacker, BoardIdentity defender, bool isTeammode)
        {
            defender.CookiesController.OnGetCookie -= OnGetCookie;

            foreach (Cookie cookie in defender.CookiesController.AllCookies)
            {
                cookie.SetColor(m_StartColor);
            }
        }


        private void OnGetCookie(Cookie cookie)
        {
            cookie.SetColor(m_SplatColor);
        }
    }
}