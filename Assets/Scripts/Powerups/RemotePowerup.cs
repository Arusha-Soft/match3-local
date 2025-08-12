using Project.Core;

namespace Project.Powerups
{
    public class RemotePowerup : DurationalPowerup
    {
        protected override void OnStartPowerup(BoardIdentity attacker, BoardIdentity defender, bool isTeammode)
        {
            defender.SelectionBox.DoMoveForce(attacker.SelectionBox.CurrentBlockId);
            defender.BoardInput.RebindInputAction(attacker.BoardInput.CurrentInputActions);
            defender.SetPowerup(null);
            defender.SetIsAvailableToUsePowerup(false, false);
        }

        protected override void OnEndPowerup(BoardIdentity attacker, BoardIdentity defender, bool isTeammode)
        {
            defender.BoardInput.ResetInputActtion();
            defender.SetIsAvailableToUsePowerup(true);
        }
    }
}