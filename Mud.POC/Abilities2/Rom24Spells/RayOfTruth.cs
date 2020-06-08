using Mud.Container;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using Mud.Server.Input;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Ray of Truth", AbilityEffects.Damage)]
    public class RayOfTruth : OffensiveSpellBase
    {
        private IAbilityManager AbilityManager { get; }

        public RayOfTruth(IRandomManager randomManager, IWiznet wiznet, IAbilityManager abilityManager)
            : base(randomManager, wiznet)
        {
            AbilityManager = abilityManager;
        }

        protected override string SetTargets(AbilityActionInput abilityActionInput)
        {
            string baseSetTargets = base.SetTargets(abilityActionInput);
            if (baseSetTargets != null)
                return baseSetTargets;
            // Check alignment
            if (Caster is IPlayableCharacter && !Caster.IsGood)
            {
                Victim = Caster;
                Caster.Send("The energy explodes inside you!");
            }
            return null;
        }


        protected override void Invoke()
        {
            if (Victim != Caster)
                Caster.Act(ActOptions.ToAll, "{0:N} raise{0:v} {0:s} hand, and a blinding ray of light shoots forth!", Caster);

            if (Victim.IsGood)
            {
                Victim.Act(ActOptions.ToRoom, "{0:N} seems unharmed by the light.", Victim);
                Victim.Send("The light seems powerless to affect you.");
                return;
            }

            int damage = RandomManager.Dice(Level, 10);
            if (Victim.SavesSpell(Level, SchoolTypes.Holy))
                damage /= 2;

            int alignment = Victim.Alignment - 350;
            if (alignment < -1000)
                alignment = -1000 + (alignment + 1000) / 3;

            damage = (damage * alignment * alignment) / (1000 * 1000);

            Victim.AbilityDamage(Caster, this, damage, SchoolTypes.Holy, "ray of truth", true);
            CastSpell("Blindness", Victim, (3 * Level) / 4);
        }

        private void CastSpell(string spellName, ICharacter victim, int level) // TODO: use level
        {
            // TODO: not a huge fan of following code
            AbilityInfo abilityInfo = AbilityManager[spellName];
            IAbilityAction abilityInstance = (IAbilityAction)DependencyContainer.Current.GetInstance(abilityInfo.AbilityExecutionType);
            AbilityActionInput abilityActionInput = new AbilityActionInput(abilityInfo, Caster, victim.Name, new CommandParameter(victim.Name, false));
            abilityInstance.Setup(abilityActionInput);
            abilityInstance.Execute(abilityActionInput);
        }
    }
}
