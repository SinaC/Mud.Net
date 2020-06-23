using Mud.Container;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.GameAction;

namespace Mud.POC.Abilities2
{
    [Command("Cast", "Ability", "Spell")]
    public class Cast : IGameAction
    {
        protected IAbilityManager AbilityManager { get; }

        protected IAbilityInfo AbilityInfo { get; set; }
        protected ICharacter Caster { get; set; }
        protected SpellActionInput SpellActionInput { get; set; }
        protected ISpell SpellInstance { get; set; }

        public Cast(IAbilityManager abilityManager)
        {
            AbilityManager = abilityManager;
        }

        public string Guards(IActionInput actionInput)
        {
            if (actionInput.Actor == null)
                return "Cannot cast a spell without an actor!";
            // check if actor is Character
            Caster = actionInput.Actor as ICharacter;
            if (Caster == null)
                return "Only character are allowed to cast spells!";
            var spellName = actionInput.Parameters.Length > 0 ? actionInput.Parameters[0].Value : null;
            if (string.IsNullOrWhiteSpace(spellName))
                return "Cast what ?";
            AbilityInfo = AbilityManager.Search(spellName, AbilityTypes.Spell);
            if (AbilityInfo == null)
                return "This spell doesn't exist.";
            var abilityLearned = Caster.GetAbilityLearned(AbilityInfo.Name);
            if (abilityLearned.abilityLearned == null)
                return "You don't know any spells of that name.";
            if (DependencyContainer.Current.GetRegistration(AbilityInfo.AbilityExecutionType, false) == null)
                return "Spell not found in DependencyContainer!";
            SpellInstance = DependencyContainer.Current.GetInstance(AbilityInfo.AbilityExecutionType) as ISpell;
            if (SpellInstance == null)
                return "Spell instance cannot be created!";
            SpellActionInput = new SpellActionInput(actionInput, AbilityInfo, Caster, Caster.Level);
            string spellInstanceGuards = SpellInstance.Setup(SpellActionInput);
            return spellInstanceGuards;
        }

        public void Execute(IActionInput actionInput)
        {
            SpellInstance.Execute();
        }
    }
}
