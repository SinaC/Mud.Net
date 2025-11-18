using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Commands.Character.Item;

[CharacterCommand("quaff", "Drink", MinPosition = Positions.Resting)]
[Syntax("[cmd] <potion>")]
[Help(@"[cmd] quaffs a magical potion (as opposed to DRINK, which drinks mundane liquids)")]
public class Quaff : CastSpellCharacterGameActionBase
{
    private IItemManager ItemManager { get; }

    public Quaff(IAbilityManager abilityManager, IItemManager itemManager)
        : base(abilityManager)
    {
        ItemManager = itemManager;
    }

    protected IItemPotion Potion { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            return "Quaff what?";

        var item = FindHelpers.FindByName(Actor.Inventory.Where(x => Actor.CanSee(x)), actionInput.Parameters[0]);
        if (item == null)
            return "You do not have that potion.";

        if (item is not IItemPotion potion)
            return "You can quaff only potions.";
        Potion = potion;

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
