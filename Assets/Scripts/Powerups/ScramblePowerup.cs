using Project.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Project.Powerups
{
    public class ScramblePowerup : DurationalPowerup
    {
        [SerializeField] private float m_ChangeDuration = 0.2f;

        private Coroutine m_Scrambling;

        protected override void OnStartPowerup(BoardIdentity attacker, BoardIdentity defender, bool isTeammode)
        {
            defender.BoardInput.DisableInput();

            m_Scrambling = StartCoroutine(Scrambling(defender));
        }

        protected override void OnEndPowerup(BoardIdentity attacker, BoardIdentity defender, bool isTeammode)
        {
            defender.BoardInput.EnableInput();

            StopCoroutine(m_Scrambling);
        }

        private IEnumerator Scrambling(BoardIdentity defender)
        {
            WaitForSeconds delay = new WaitForSeconds(m_ChangeDuration);

            while (true)
            {
                List<Block> blocks = defender.BoardData.VisibleBlocks.ToList();
                foreach (Cookie cookie in defender.CookiesController.ActiveCookies)
                {
                    cookie.ForceMove(GetRandomBlock().transform);
                }

                Block GetRandomBlock()
                {
                    Block result = blocks[Random.Range(0, blocks.Count)];
                    blocks.Remove(result);
                    return result;
                }

                yield return delay;
            }
        }
    }
}