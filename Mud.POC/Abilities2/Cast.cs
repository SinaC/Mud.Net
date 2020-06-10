using Mud.Container;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Input;

namespace Mud.POC.Abilities2
{
    [Command("Cast", "Abilities", "Spells")]
    public class Cast : IGameAction
    {
        protected IAbilityManager AbilityManager { get; }

        protected AbilityInfo AbilityInfo { get; set; }
        protected ICharacter Caster { get; set; }
        protected SpellActionInput SpellActionInput { get; set; }
        protected ISpell SpellInstance { get; set; }

        public Cast(IAbilityManager abilityManager)
        {
            AbilityManager = abilityManager;
        }

        public string Guards(ActionInput actionInput)
        {
            if (actionInput.Actor == null)
                return "Cannot cast a spell without an actor!";
            // check if actor is Character
            Caster = actionInput.Actor as ICharacter;
            if (Caster == null)
                return "Only character are allowed to cast spells!";
            bool extracted = CommandHelpers.ExtractCommandAndParameters(actionInput.CommandLine, out var spellName, out var rawParameters, out var parameters);
            if (!extracted)
                return "Cast what ?";
            AbilityInfo = AbilityManager.Search(spellName, AbilityTypes.Spell);
            if (AbilityInfo == null)
                return "This spell doesn't exist.";
            var abilityLearned = Caster.GetAbilityLearned(AbilityInfo.Name);
            if (abilityLearned.abilityLearned == null)
                return "You don't know any spells of that name.";
            if (DependencyContainer.Current.GetRegistration(AbilityInfo.AbilityExecutionType, false) == null)
                return "Spell not found in DependencyContainer!";
            SpellInstance = (ISpell)DependencyContainer.Current.GetInstance(AbilityInfo.AbilityExecutionType);
            if (SpellInstance == null)
                return "Spell instance cannot be created!";
            SpellActionInput = new SpellActionInput(AbilityInfo, Caster, Caster.Level, null, rawParameters, parameters);
            string spellInstanceGuards = SpellInstance.Setup(SpellActionInput);
            return spellInstanceGuards;
        }

        public void Execute(ActionInput actionInput)
        {
            SpellInstance.Execute();
        }
    }
}
