using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Commands.Character.Movement;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Random;
using System.Text;

namespace Mud.Server.Commands.Character.Information;

[CharacterCommand("examine", "Information", Priority = 20/*must be greater than 'Exits' priority */), MinPosition(Positions.Resting)]
[Syntax(
    "[cmd] <character>",
    "[cmd] <item>",
    "[cmd] <container>",
    "[cmd] <corpse>")]
[Help(@"[cmd] is short for 'LOOK container' followed by 'LOOK IN container'.")]
public class Examine : CharacterGameAction
{
    private IAbilityManager AbilityManager { get; }

    public Examine(IAbilityManager abilityManager)
    {
        AbilityManager = abilityManager;
    }

    protected IEntity Target { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
           return "Examine what or whom?";

        // Search character
        Target = FindHelpers.FindByName(Actor.Room.People, actionInput.Parameters[0])!;
        if (Target != null)
            return null;

        // Search item
        Target = FindHelpers.FindItemHere(Actor, actionInput.Parameters[0])!;
        if (Target != null)
            return null;

        return $"You don't see any {actionInput.Parameters[0].Value}.";
    }

    public override void Execute(IActionInput actionInput)
    {
        if (Target is ICharacter victim)
            Execute(victim);
        else if (Target is IItem item)
            Execute(item);
    }

    private void Execute(ICharacter victim)
    {
        Actor.Act(ActOptions.ToAll, "{0:N} examine{0:v} {1}.", Actor, victim);
        StringBuilder sb = new();
        var peek = CheckPeek();
        victim.Append(sb, Actor, peek);
        // TODO: display race and size
        Actor.Send(sb);
    }

    private void Execute(IItem item)
    {
        Actor.Act(ActOptions.ToAll, "{0:N} examine{0:v} {1}.", Actor, item);
        StringBuilder sb = new();
        switch (item)
        {
            case IContainer container:
                if (container is IItemContainer itemContainer && itemContainer.IsClosed)
                    sb.AppendLine("It's closed.");
                else
                {
                    sb.AppendFormatLine("{0} holds:", container.RelativeDisplayName(Actor));
                    ItemsHelpers.AppendItems(sb, container.Content.Where(Actor.CanSee), Actor, true, true);
                }
                break;
            case IItemMoney money:
                if (money.SilverCoins == 0 && money.GoldCoins == 0)
                    sb.AppendLine("Odd...there's no coins in the pile.");
                else if (money.SilverCoins == 0 && money.GoldCoins > 0)
                {
                    if (money.GoldCoins == 1)
                        sb.AppendLine("Wow. one gold coin.");
                    else
                        sb.AppendFormatLine("There are {0} gold coins in the pile.", money.GoldCoins);
                }
                else if (money.SilverCoins > 0 && money.GoldCoins == 0)
                {
                    if (money.SilverCoins == 1)
                        sb.AppendLine("Wow. one silver coin.");
                    else
                        sb.AppendFormatLine("There are {0} silver coins in the pile.", money.SilverCoins);
                }
                else
                    sb.AppendFormatLine("There are {0} gold and {1} silver coins in the pile.", money.SilverCoins, money.GoldCoins);
                break;
            default:
                item.Append(sb, Actor, true);
                sb.AppendLine();
                break;
        }

        Actor.Send(sb);
    }

    private bool CheckPeek()
    {
        if (Actor is IPlayableCharacter pc)
        {
            var (percentage, abilityLearned) = Actor.GetAbilityLearnedAndPercentage("peek");
            if (abilityLearned is not null && percentage > 0)
            {
                var peekAbility = AbilityManager.CreateInstance<IPassive>("peek");
                if (peekAbility is not null)
                {
                    return peekAbility.IsTriggered(pc, null!, true, out _, out _);
                }
            }
        }
        return false;
    }
}
