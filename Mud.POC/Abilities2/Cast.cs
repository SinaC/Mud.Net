using System.Linq;
using Mud.Container;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using Mud.Server.Input;

namespace Mud.POC.Abilities2
{
    public class Cast : IGameAction
    {
        protected IAbilityManager AbilityManager { get; }

        protected AbilityInfo AbilityInfo { get; set; }
        protected ICharacter Caster { get; set; }
        protected AbilityActionInput AbilityActionInput { get; set; }
        protected IAbilityAction AbilityInstance { get; set; }

        public Cast(IAbilityManager abilityManager)
        {
            AbilityManager = abilityManager;
        }

        public string Guards(ActionInput actionInput)
        {
            if (actionInput.Actor == null)
                return "Cannot cast a spell without an actor.";
            // check if actor is Character
            Caster = actionInput.Actor as ICharacter;
            if (Caster == null)
                return "Only character are allowed to cast spells.";
            bool extracted = CommandHelpers.ExtractCommandAndParameters(actionInput.CommandLine, out var spellName, out var rawParameters, out var parameters);
            if (!extracted)
                return "Cast what ?";
            AbilityLearned abilityLearned = Caster.LearnedAbilities.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, spellName)); // TODO: Dictionary
            if (abilityLearned == null)
                return "You don't know any spells of that name.";
            AbilityInfo = AbilityManager[abilityLearned.Name];
            if (AbilityInfo == null)
                return "Ability not found in AbilityManager";
            AbilityActionInput = new AbilityActionInput(AbilityInfo, Caster, rawParameters, parameters);
            if (DependencyContainer.Current.GetRegistration(AbilityInfo.AbilityExecutionType, false) == null)
                return "Ability not found in DependencyContainer";
            AbilityInstance = (IAbilityAction)DependencyContainer.Current.GetInstance(AbilityInfo.AbilityExecutionType);
            if (AbilityInstance == null)
                return "Ability instance cannot be created";
            string abilityInstanceGuards = AbilityInstance.Setup(AbilityActionInput);
            return abilityInstanceGuards;
        }

        public void Execute(ActionInput actionInput)
        {
            AbilityInstance.Execute();
        }
    }
}
