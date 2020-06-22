using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using System.Linq;

namespace Mud.Server.Character.Item
{
    [CharacterCommand("quaff", "Drink", MinPosition = Positions.Resting)]
    [Syntax("[cmd] <potion>")]
    public class Quaff : CastSpellCharacterGameActionBase
    {
        private IItemManager ItemManager { get; }

        public IItemPotion Potion { get; protected set; }

        public Quaff(IAbilityManager abilityManager, IItemManager itemManager)
            : base(abilityManager)
        {
            ItemManager = itemManager;
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
                return "Quaff what?";

            IItem item = FindHelpers.FindByName(Actor.Inventory.Where(x => Actor.CanSee(x)), actionInput.Parameters[0]);
            if (item == null)
                return "You do not have that potion.";

            Potion = item as IItemPotion;
            if (Potion == null)
                return "You can quaff only potions.";

            if (Actor.Level < Potion.Level)
                return "This liquid is too powerful for you to drink.";

            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            Actor.Act(ActOptions.ToRoom, "{0:N} quaff{0:v} {1}.", Actor, Potion);

            CastSpell(Potion, Potion.FirstSpellName, Potion.SpellLevel);
            CastSpell(Potion, Potion.SecondSpellName, Potion.SpellLevel);
            CastSpell(Potion, Potion.ThirdSpellName, Potion.SpellLevel);
            CastSpell(Potion, Potion.FourthSpellName, Potion.SpellLevel);
            ItemManager.RemoveItem(Potion);
        }
    }
}
