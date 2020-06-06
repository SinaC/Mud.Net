using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Refresh", AbilityEffects.Healing, PulseWaitTime = 18)]
    public class Refresh : DefensiveSpellBase
    {
        public Refresh(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override void Invoke()
        {
            Victim.UpdateMovePoints(Level);
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (Victim.MovePoints == Victim.MaxMovePoints)
                Victim.Send("You feel fully refreshed!");
            else
                Victim.Send("You feel less tired.");
            if (Caster != Victim)
                Caster.Send("Ok");
        }
    }
}
