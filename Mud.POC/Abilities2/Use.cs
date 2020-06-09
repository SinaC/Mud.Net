using System.Linq;
using Mud.Container;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using Mud.Server.Input;

namespace Mud.POC.Abilities2
{
    [Command("Use", "Abilities", "Skills")]
    public class Use : IGameAction
    {
        protected IAbilityManager AbilityManager { get; }

        protected AbilityInfo AbilityInfo { get; set; }
        protected ICharacter User { get; set; }
        protected AbilityActionInput AbilityActionInput { get; set; }
        protected ISkill SkillInstance { get; set; }

        public Use(IAbilityManager abilityManager)
        {
            AbilityManager = abilityManager;
        }

        public string Guards(ActionInput actionInput)
        {
            if (actionInput.Actor == null)
                return "Cannot use a skill without an actor!";
            // check if actor is Character
            User = actionInput.Actor as ICharacter;
            if (User == null)
                return "Only character are allowed to use skills!";
            bool extracted = CommandHelpers.ExtractCommandAndParameters(actionInput.CommandLine, out var skillName, out var rawParameters, out var parameters);
            if (!extracted)
                return "Use what ?";
            AbilityInfo = AbilityManager.Search(skillName, AbilityTypes.Spell);
            if (AbilityInfo == null)
                return "This skill doesn't exist.";
            AbilityActionInput = new AbilityActionInput(AbilityInfo, User, User.Level, rawParameters, parameters);
            if (DependencyContainer.Current.GetRegistration(AbilityInfo.AbilityExecutionType, false) == null)
                return "Skill not found in DependencyContainer!";
            SkillInstance = (ISkill)DependencyContainer.Current.GetInstance(AbilityInfo.AbilityExecutionType);
            if (SkillInstance == null)
                return "Skill instance cannot be created!";
            string abilityInstanceGuards = SkillInstance.Setup(AbilityActionInput);
            return abilityInstanceGuards;
        }

        public void Execute(ActionInput actionInput)
        {
            SkillInstance.Execute();
        }
    }
}
