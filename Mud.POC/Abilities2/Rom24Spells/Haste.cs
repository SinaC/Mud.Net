using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System;
using System.Linq;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Haste", AbilityEffects.Buff)]
    [AbilityCharacterWearOffMessage("You feel yourself slow down.")]
    [AbilityDispellable("{0:N} is no longer moving so quickly.")]
    public class Haste : DefensiveSpellBase
    {
        private IAuraManager AuraManager { get; }
        private IAbilityManager AbilityManager { get; }

        public Haste(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager, IAbilityManager abilityManager)
            : base(randomManager, wiznet)
        {
            AuraManager = auraManager;
            AbilityManager = abilityManager;
        }

        protected override void Invoke()
        {
            if (Victim.CharacterFlags.HasFlag(CharacterFlags.Haste)
                || Victim.GetAura("Haste") != null
                || (Victim is INonPlayableCharacter npcVictim && npcVictim.OffensiveFlags.HasFlag(OffensiveFlags.Fast)))
            {
                if (Victim == Caster)
                    Caster.Send("You can't move any faster!");
                else
                    Caster.Act(ActOptions.ToCharacter, "{0:N} is already moving as fast as {0:e} can.", Victim);
                return;
            }
            if (Victim.CharacterFlags.HasFlag(CharacterFlags.Slow))
            {
                if (TryDispel(Level, Victim, "Slow") != CheckDispelReturnValues.Dispelled)
                {
                    if (Victim != Caster)
                        Caster.Send("Spell failed.");
                    Victim.Send("You feel momentarily faster.");
                    return;
                }
                Victim.Act(ActOptions.ToRoom, "{0:N} is moving less slowly.", Victim);
                return;
            }
            int duration = Victim == Caster
                ? Level / 2
                : Level / 4;
            int modifier = 1 + (Level >= 18 ? 1 : 0) + (Level >= 25 ? 1 : 0) + (Level >= 32 ? 1 : 0);
            AuraManager.AddAura(Victim, AbilityInfo.Name, Caster, Level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Dexterity, Modifier = modifier, Operator = AffectOperators.Add },
                new CharacterFlagsAffect { Modifier = CharacterFlags.Haste, Operator = AffectOperators.Or });
            Victim.Send("You feel yourself moving more quickly.");
            Victim.Act(ActOptions.ToRoom, "{0:N} is moving more quickly.", Victim);
            if (Caster != Victim)
                Caster.Send("Ok.");
        }


        // TODO: refactoring, almost same code in DispelMagic, Cancellation and CureSpellBase
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
