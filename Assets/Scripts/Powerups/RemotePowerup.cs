using Project.Core;

namespace Project.Powerups
{
    public class RemotePowerup : DurationalPowerup
    {
        protected override void OnStartPowerup(BoardIdentity attacker, BoardIdentity defender, bool isTeammode)
        {
            defender.SelectionBox.DoMoveForce(attacker.SelectionBox.CurrentBlockId);
            defender.BoardInput.RebindInputAction(attacker.BoardInput.CurrentInputActions);
            defender.CookiesController.DisableMoveHandling();
        }

        protected override void OnEndPowerup(BoardIdentity attacker, BoardIdentity defender, bool isTeammode)
        {
            defender.BoardInput.ResetInputActtion();
            defender.CookiesController.EnableMoveHandling();
        }
    }
}