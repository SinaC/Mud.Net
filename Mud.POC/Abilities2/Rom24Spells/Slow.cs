using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System;
using System.Linq;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Slow", AbilityEffects.Debuff)]
    public class Slow : OffensiveSpellBase
    {
        private IAuraManager AuraManager { get; }
        private IAbilityManager AbilityManager { get; }

        public Slow(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager, IAbilityManager abilityManager)
            : base(randomManager, wiznet)
        {
            AuraManager = auraManager;
            AbilityManager = abilityManager;
        }

        protected override void Invoke()
        {
            if (Victim.CharacterFlags.HasFlag(CharacterFlags.Slow)
                || Victim.GetAura(AbilityInfo.Name) != null)
            {
                if (Victim == Caster)
                    Caster.Send("You can't move any slower!");
                else
                    Caster.Act(ActOptions.ToCharacter, "{0:N} can't get any slower than that.", Victim);
                return;
            }

            if (Victim.Immunities.HasFlag(IRVFlags.Magic)
                || Victim.SavesSpell(Level, SchoolTypes.Other))
            {
                if (Victim != Caster)
                    Caster.Send("Nothing seemed to happen.");
                Victim.Send("You feel momentarily lethargic.");
                return;
            }

            if (Victim.CharacterFlags.HasFlag(CharacterFlags.Haste))
            {
                if (TryDispel(Level, Victim, "Haste") != CheckDispelReturnValues.Dispelled)
                {
                    if (Victim != Caster)
                        Caster.Send("Spell failed.");
                    Victim.Send("You feel momentarily slower.");
                    return;
                }
                Victim.Act(ActOptions.ToRoom, "{0:N} is moving less quickly.", Victim);
                return;
            }

            int duration = Level / 2;
            int modifier = -1 - (Level >= 18 ? 1 : 0) - (Level >= 25 ? 1 : 0) - (Level >= 32 ? 1 : 0);
            AuraManager.AddAura(Victim, AbilityInfo.Name, Caster, Level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Dexterity, Modifier = modifier, Operator = AffectOperators.Add },
                new CharacterFlagsAffect { Modifier = CharacterFlags.Slow, Operator = AffectOperators.Or });
            Victim.Recompute();
            Victim.Send("You feel yourself slowing d o w n...");
            Caster.Act(ActOptions.ToRoom, "{0} starts to move in slow motion.", Victim);
        }

        // TODO: refactoring, almost same code in DispelMagic and Cancellation
        protected CheckDispelReturnValues TryDispel(int dispelLevel, ICharacter victim, string abilityName) // was called check_dispel in Rom24
        {
            bool found = false;
            foreach (IAura aura in victim.Auras.Where(x => x.AbilityName == abilityName)) // no need to clone because at most one entry will be removed
            {
                if (!SavesDispel(dispelLevel, aura))
                {
                    victim.RemoveAura(aura, true); // RemoveAura will display DispelMessage
                    AbilityInfo abilityInfo = AbilityManager[aura.AbilityName];
                    string dispelRoomMessage = abilityInfo?.DispelRoomMessage;
                    if (!string.IsNullOrWhiteSpace(dispelRoomMessage))
                        victim.Act(ActOptions.ToRoom, dispelRoomMessage, victim);
                    return CheckDispelReturnValues.Dispelled; // stop at first aura dispelled
                }
                else
                    aura.DecreaseLevel();
                found = true;
            }
            return found
                ? CheckDispelReturnValues.FoundAndNotDispelled
                : CheckDispelReturnValues.NotFound;
        }
        protected enum CheckDispelReturnValues
        {
            NotFound,
            Dispelled,
            FoundAndNotDispelled
        }
    }
}
