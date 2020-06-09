using Mud.POC.Abilities2.Domain;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell(SpellName, AbilityEffects.Dispel)]
    public class DispelMagic : OffensiveSpellBase
    {
        public const string SpellName = "Dispel Magic";

        private IDispelManager DispelManager { get; }

        public DispelMagic(IRandomManager randomManager, IDispelManager dispelManager)
            : base(randomManager)
        {
            DispelManager = dispelManager;
        }

        protected override void Invoke()
        {
            if (Victim.SavesSpell(Level, SchoolTypes.Other))
            {
                Victim.Send("You feel a brief tingling sensation.");
                Caster.Send("You failed.");
                return;
            }

            bool found = DispelManager.TryDispels(Level, Victim);

            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (found)
                Caster.Send("Ok.");
            else
                Caster.Send("Spell failed.");
        }
    }
}
