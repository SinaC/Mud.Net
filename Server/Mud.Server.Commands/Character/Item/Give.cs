using Mud.Blueprints.Character;
using Mud.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.CharacterGuards;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Commands.Character.Item;

[CharacterCommand("give", "Item", "Equipment")]
[Syntax(
        "[cmd] <item> <character>",
        "[cmd] <amount> coin|coins|silver|gold <character>")]
[Help(
@"[cmd] gives an object, or some coins, to another character.
[cmd] X.sword is also allowed to give the Xth sword of the list.")]
public class Give : CharacterGameAction
{
    protected override IGuard<ICharacter>[] Guards => [new RequiresMinPosition(Positions.Resting), new RequiresAtLeastTwoArguments { Message = "Give what to whom ?" }];

    private long? Silver { get; set; }
    private long? Gold { get; set; }
    private IItem What { get; set; } = default!;
    private ICharacter Whom { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        // money ?
        if (actionInput.Parameters[0].IsLong && actionInput.Parameters.Length > 2)
        {
            long amount = actionInput.Parameters[0].AsLong;
            string whatMoney = actionInput.Parameters[1].Value;
            if (whatMoney == "coin" || whatMoney == "coins" || whatMoney == "silver" || whatMoney == "gold")
            {
                // check parameters
                Whom = FindHelpers.FindByName(Actor.Room.People.Where(Actor.CanSee), actionInput.Parameters[2])!;
                if (Whom == null)
                    return StringHelpers.CharacterNotFound;
                if (amount <= 0)
                    return "Sorry, you can't do that.";

                if (whatMoney == "coin" || whatMoney == "coins" || whatMoney == "silver")
                {
                    if (amount > Actor.SilverCoins)
                        return "You don't have that much silver.";
                    Silver = amount;
                }
                else
                {
                    if (amount > Actor.GoldCoins)
                        return "You don't have that much gold.";
                    Gold = amount;
                }
                return null;
            }
        }

        What = FindHelpers.FindByName(Actor.Inventory.Where(Actor.CanSee), actionInput.Parameters[0])!;
        if (What == null)
            return StringHelpers.ItemInventoryNotFound;

        Whom = FindHelpers.FindByName(Actor.Room.People.Where(Actor.CanSee), actionInput.Parameters[1])!;
        if (Whom == null)
            return StringHelpers.CharacterNotFound;

        if (What.ItemFlags.IsSet("NoDrop"))
            return "You can't let go of it.";

        if (What is IItemQuest)
            return "You cannot give that.";

        if (Whom.CarryNumber + What.CarryCount > Whom.MaxCarryNumber)
            return Actor.ActPhrase("{0:N} has {0:s} hands full.", Whom);

        if (Whom.CarryWeight + What.TotalWeight > Whom.MaxCarryWeight)
            return Actor.ActPhrase("{0:N} can't carry that much weight.", Whom);

        if (!Whom.CanSee(What))
            return Actor.ActPhrase("{0:n} can't see it.", Whom);

        if (What is IItemQuest)
            return Actor.ActPhrase("You cannot give quest items.");

        if (Whom is INonPlayableCharacter npcVictim && npcVictim.Blueprint is CharacterShopBlueprintBase)
            return Actor.ActPhrase("{0:N} tells you 'Sorry, you'll have to sell that.'", npcVictim);
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        // Let's give the money
        if (Silver > 0 || Gold > 0)
        {
            Actor.UpdateMoney(-Silver ?? 0, -Gold ?? 0);
            Whom.UpdateMoney(Silver ?? 0, Gold ?? 0);
            Actor.Act(ActOptions.ToCharacter, "You give {0:N} {1} {2}.", Whom, Silver ?? Gold ?? 0, Silver.HasValue ? "silver" : "gold");
            Whom.Act(ActOptions.ToCharacter, "{0:N} gives you {1} {2}.", Actor, Silver ?? Gold ?? 0, Silver.HasValue ? "silver" : "gold");
            Actor.ActToNotVictim(Whom, "{0:N} gives {1:N} some coins.", Actor, Whom);
            return;
        }

        // Give item to victim
        What.ChangeContainer(Whom);
        Whom.Recompute();
        Actor.Recompute();

        Actor.ActToNotVictim(Whom, "{0} gives {1} to {2}.", Actor, What, Whom);
        Whom.Act(ActOptions.ToCharacter, "{0} gives you {1}.", Actor, What);
        Actor.Act(ActOptions.ToCharacter, "You give {0} to {1}.", What, Whom);
    }
}
