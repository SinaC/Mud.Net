using Mud.Container;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using Mud.Server.Input;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Holy Word", AbilityEffects.Damage | AbilityEffects.Buff, PulseWaitTime = 24)]
    public class HolyWord : NoTargetSpellBase
    {
        private IAbilityManager AbilityManager { get; }

        public HolyWord(IRandomManager randomManager, IWiznet wiznet, IAbilityManager abilityManager)
            : base(randomManager, wiznet)
        {
        }

        protected override void Invoke()
        {
            Caster.Act(ActOptions.ToRoom, "{0:N} utters a word of divine power!");
            Caster.Send("You utter a word of divine power.");

            foreach (ICharacter victim in Caster.Room.People)
            {
                if ((Caster.IsGood && victim.IsGood)
                    || (Caster.IsNeutral && victim.IsNeutral)
                    || (Caster.IsEvil && victim.IsEvil))
                {
                    victim.Send("You feel full more powerful.");
                    CastSpell("Frenzy", victim, Level);
                    CastSpell("Bless", victim, Level);
                }
                else if ((Caster.IsGood && victim.IsEvil)
                        || (Caster.IsEvil && victim.IsGood))
                {
                    if (!victim.SavesSpell(Level, SchoolTypes.Holy))
                    {
                        victim.Send("You are struck down!");
                        CastSpell("Curse", victim, Level);
                        int damage = RandomManager.Dice(Level, 6);
                        victim.AbilityDamage(Caster, this, damage, SchoolTypes.Holy, "divine wrath", true);
                    }
                }
                else if (Caster.IsNeutral)
                {
                    if (!victim.SavesSpell(Level, SchoolTypes.Holy))
                    {
                        victim.Send("You are struck down!");
                        CastSpell("Curse", victim, Level / 2);
                        int damage = RandomManager.Dice(Level, 4);
                        victim.AbilityDamage(Caster, this, damage, SchoolTypes.Holy, "divine wrath", true);
                    }
                }
            }

            Caster.Send("You feel drained.");
            Caster.UpdateMovePoints(-Caster.MovePoints); // set to 0
            Caster.UpdateHitPoints(-Caster.HitPoints / 2);
        }

        private void CastSpell(string spellName, ICharacter victim, int level) // TODO: use level
        {
            // TODO: not a huge fan of following code
            AbilityInfo abilityInfo = AbilityManager[spellName];
            IAbilityAction abilityInstance = (IAbilityAction)DependencyContainer.Current.GetInstance(abilityInfo.AbilityExecutionType);
            AbilityActionInput abilityActionInput = new AbilityActionInput(abilityInfo, Caster, victim.Name, new CommandParameter(victim.Name, false));
            abilityInstance.Guards(abilityActionInput);
            abilityInstance.Execute(abilityActionInput);
        }
    }
}
