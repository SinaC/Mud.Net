using Mud.Logger;
using Mud.Server.Ability.Spell;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using System.Linq;

namespace Mud.Server.Character.Item
{
    public abstract class CastSpellCharacterGameActionBase : CharacterGameAction
    {
        private IAbilityManager AbilityManager { get; }

        protected CastSpellCharacterGameActionBase(IAbilityManager abilityManager)
        {
            AbilityManager = abilityManager;
        }

        protected string CastSpell(IItem item, string spellName, int spellLevel)
        {
            if (string.IsNullOrWhiteSpace(spellName))
                return null; // not really an error but don't continue
            var abilityInfo = AbilityManager.Search(spellName, AbilityTypes.Spell);
            if (abilityInfo == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Unknown spell '{0}' on item {1}.", spellName, item.DebugName);
                return "Something goes wrong.";
            }
            var spellInstance = AbilityManager.CreateInstance<ISpell>(abilityInfo.Name);
            if (spellInstance == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Spell '{0}' on item {1} cannot be instantiated.", spellName, item.DebugName);
                return "Something goes wrong.";
            }
            var spellActionInput = new SpellActionInput(abilityInfo, Actor, spellLevel, new CastFromItemOptions { Item = item }, CommandHelpers.NoParameters);
            string spellInstanceGuards = spellInstance.Setup(spellActionInput);
            if (spellInstanceGuards != null)
                return spellInstanceGuards;
            spellInstance.Execute();
            return null;
        }
    }
}
