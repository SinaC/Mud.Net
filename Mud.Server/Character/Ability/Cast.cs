using Mud.Domain;
using Mud.Logger;
using Mud.Server.Ability.Spell;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.Ability
{
    [CharacterCommand("cast", "Ability", Priority = 2, MinPosition = Positions.Fighting)]
    [Syntax("[cmd] <ability> <target>")]
    public class Cast : CharacterGameAction
    {
        private IAbilityManager AbilityManager { get; }

        public ISpell SpellInstance { get; protected set; }

        public Cast(IAbilityManager abilityManager)
        {
            AbilityManager = abilityManager;
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            var spellName = actionInput.Parameters.Length > 0 ? actionInput.Parameters[0].Value : null;
            if (string.IsNullOrWhiteSpace(spellName))
                return "Cast what ?";
            var abilityInfo = AbilityManager.Search(spellName, AbilityTypes.Spell);
            if (abilityInfo == null)
                return "This spell doesn't exist.";
            var abilityLearned = Actor.GetAbilityLearned(abilityInfo.Name);
            if (abilityLearned == null)
                return "You don't know any spells of that name.";
            ISpell spellInstance = AbilityManager.CreateInstance<ISpell>(abilityInfo.Name);
            if (spellInstance == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Spell {0} cannot be created.", abilityInfo.Name);
                return "Something goes wrong.";
            }

            var newParameters = CommandHelpers.SkipParameters(actionInput.Parameters, 1);
            var spellActionInput = new SpellActionInput(abilityInfo, Actor, Actor.Level, null, newParameters.rawParameters, newParameters.parameters);
            string spellInstanceGuards = spellInstance.Setup(spellActionInput);
            if (spellInstanceGuards != null)
                return spellInstanceGuards;
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            SpellInstance.Execute();
        }
    }
}
