using Mud.Container;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using Mud.Server.Input;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Colour Spray", AbilityEffects.Damage | AbilityEffects.Debuff)]
    public class ColourSpray : DamageTableSpellBase
    {
        private IAbilityManager AbilityManager { get; }
        public ColourSpray(IRandomManager randomManager, IWiznet wiznet, IAbilityManager abilityManager)
            : base(randomManager, wiznet)
        {
            AbilityManager = abilityManager;
        }

        protected override SchoolTypes DamageType => SchoolTypes.Light;
        protected override string DamageNoun => "colour spray";
        protected override int[] Table => new[]
        {
             0,
             0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            30, 35, 40, 45, 50, 55, 55, 55, 56, 57,
            58, 58, 59, 60, 61, 61, 62, 63, 64, 64,
            65, 66, 67, 67, 68, 69, 70, 70, 71, 72,
            73, 73, 74, 75, 76, 76, 77, 78, 79, 79
        };

        protected override void Invoke()
        {
            base.Invoke();
            if (SavesSpellResult || DamageResult != DamageResults.Done)
                return;
            // TODO: not a huge fan of following code
            // TODO: we have to register every GameAction in DependencyContainer
            AbilityInfo blindnessAbilityInfo = AbilityManager["Blindness"];
            IAbilityAction blindnessAbilityInstance = (IAbilityAction)DependencyContainer.Current.GetInstance(blindnessAbilityInfo.AbilityExecutionType);
            AbilityActionInput abilityActionInput = new AbilityActionInput(blindnessAbilityInfo, Caster, Victim.Name, new CommandParameter(Victim.Name, false));
            blindnessAbilityInstance.Guards(abilityActionInput);
            blindnessAbilityInstance.Execute(abilityActionInput);
        }
    }
}
