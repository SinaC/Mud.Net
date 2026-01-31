using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.CharacterGuards;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Commands.Character.Item;

[CharacterCommand("quaff", "Drink")]
[Syntax("[cmd] <potion>")]
[Help(@"[cmd] quaffs a magical potion (as opposed to DRINK, which drinks mundane liquids)")]
public class Quaff : CastSpellCharacterGameActionBase<ICharacter, ICharacterGameActionInfo>
{
    protected override IGuard<ICharacter>[] Guards => [new RequiresMinPosition(Positions.Resting), new RequiresAtLeastOneArgument { Message = "Quaff what ?" }];

    private IItemManager ItemManager { get; }

    public Quaff(ILogger<Quaff> logger, ICommandParser commandParser, IAbilityManager abilityManager, IItemManager itemManager)
        : base(logger, commandParser, abilityManager)
    {
        ItemManager = itemManager;
    }

    private IItemPotion Potion { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        var item = FindHelpers.FindByName(Actor.Inventory.Where(Actor.CanSee), actionInput.Parameters[0]);
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
        if (Actor is IPlayableCharacter pc)
            pc.IncrementStatistics(AvatarStatisticTypes.PotionQuaffed);

        Actor.Act(ActOptions.ToRoom, "{0:N} quaff{0:v} {1}.", Actor, Potion);

        if (Potion.FirstSpellName != null)
            CastSpell(Potion, Potion.FirstSpellName, Potion.SpellLevel);
        if (Potion.SecondSpellName != null)
            CastSpell(Potion, Potion.SecondSpellName, Potion.SpellLevel);
        if (Potion.ThirdSpellName != null)
            CastSpell(Potion, Potion.ThirdSpellName, Potion.SpellLevel);
        if (Potion.FourthSpellName != null)
            CastSpell(Potion, Potion.FourthSpellName, Potion.SpellLevel);
        ItemManager.RemoveItem(Potion);
    }
}
