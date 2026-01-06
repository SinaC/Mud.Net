using Microsoft.Extensions.Logging;
using Mud.Blueprints.Character;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;

namespace Mud.Server.Commands.Character.PlayableCharacter.Shop;

[PlayableCharacterCommand("buy", "Shop"), MinPosition(Positions.Resting)]
[Syntax(
    "[cmd] [number] <item>",
    "[cmd] pet [name]")]
[Help(
@"[cmd] buys an object or a pet from a shop keeper.
When multiple items of the same name are listed, type 'buy n.item', where n
is the position of the item in a list of that name.  So if there are two
swords, buy 2.sword will buy the second. If you want to buy multiples of
an item, use an * (buy 5*pie will buy 5 pies).  These can be combined into
(for example) buy 2*2.shield, as long as the * is first.")]
public class Buy : ShopPlayableCharacterGameActionBase
{
    private ILogger<Buy> Logger { get; }
    private IItemManager ItemManager { get; }
    private ICharacterManager CharacterManager { get; }
    public IUniquenessManager UniquenessManager { get; }

    public Buy(ILogger<Buy> logger, ITimeManager timeManager, IAbilityManager abilityManager, IRandomManager randomManager, IItemManager itemManager, ICharacterManager characterManager, IUniquenessManager uniquenessManager)
        : base(timeManager, abilityManager, randomManager)
    {
        Logger = logger;
        ItemManager = itemManager;
        CharacterManager = characterManager;
        UniquenessManager = uniquenessManager;
    }

    protected IItem? Item { get; set; }
    protected int? Count { get; set; }
    protected CharacterNormalBlueprint? PetBlueprint { get; set; }
    protected string? PetName { get; set; }
    protected long TotalCost { get; set; }

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            return BuildCommandSyntax();
        if (Keeper.shopBlueprintBase is CharacterShopBlueprint shopBlueprint)
        {
            if (actionInput.Parameters.Length >= 2 && !actionInput.Parameters[0].IsNumber)
                return BuildCommandSyntax();
            Count = actionInput.Parameters.Length >= 2
                ? actionInput.Parameters[0].AsNumber
                : 1;
            var whatParameter = actionInput.Parameters.Length >= 2
                ? actionInput.Parameters[1]
                : actionInput.Parameters[0];

            Item = FindHelpers.FindByName(Keeper.shopKeeper.Inventory.Where(Actor.CanSee), whatParameter)!;
            var cost = GetBuyCost(Keeper.shopKeeper, shopBlueprint, Item, true);
            if (Item == null || cost <= 0)
                return Actor.ActPhrase("{0:N} tells you 'I don't sell that -- try 'list''.", Keeper.shopKeeper);
            if (Count <= 0 || Count > 10)
                return "Number must be between 1 and 10";
            // can afford ?
            TotalCost = cost * Count.Value;
            var wealth = Actor.SilverCoins + Actor.GoldCoins * 100;
            if (TotalCost > wealth)
            {
                if (Count == 1)
                    return Actor.ActPhrase("{0:N} tells you 'You can't afford to buy {1}'.", Keeper.shopKeeper, Item);
                else
                {
                    // how many can afford?
                    var affordableCount = wealth / cost;
                    if (affordableCount > 0)
                        return Actor.ActPhrase("{0:N} tells you 'You can only afford {1} of these'.", Keeper.shopKeeper, affordableCount);
                    else
                        return Actor.ActPhrase("{0:N} tells you '{1}? You must be kidding - you can't even afford a single one, let alone {2}!'", Keeper.shopKeeper, Item, Count);
                }
            }
            // can use item ?
            if (Item.Level > Actor.Level)
                return Actor.ActPhrase("{0:N} tells you 'You can't use {1} yet'.", Keeper.shopKeeper, Item);
            // can carry more items ?
            if (Actor.CarryNumber + Item.CarryCount * Count > Actor.MaxCarryNumber)
                return "You can't carry that many items.";
            // can carry more weight ?
            if (Actor.CarryWeight + Item.TotalWeight * Count > Actor.MaxCarryWeight)
                return "You can't carry that much weight.";
            // check for object sold to the keeper
            if (Count > 1 && !Item.ItemFlags.IsSet("Inventory"))
                return Actor.ActPhrase("{0:N} tells you 'Sorry - {1} is something I have only one of'.", Keeper.shopKeeper, Item);
        }
        else if (Keeper.shopBlueprintBase is CharacterPetShopBlueprint petShopBlueprint)
        {
            if (actionInput.Parameters.Length > 2)
                return BuildCommandSyntax();
            var whatParameter = actionInput.Parameters[0];
            if (actionInput.Parameters.Length == 2)
            {
                PetName = actionInput.Parameters[1].Value;
                if (!UniquenessManager.IsAvatarNameAvailable(PetName))
                    return Actor.ActPhrase("{0:N} tells you 'Sorry, this name will not suit your familiar.'", Keeper.shopKeeper);
            }

            PetBlueprint = petShopBlueprint.PetBlueprints.Where(x => StringCompareHelpers.AllStringsStartsWith(x!.Keywords, whatParameter.Tokens)).ElementAtOrDefault(whatParameter.Count - 1);
            if (PetBlueprint == null)
                return Actor.ActPhrase("{0:N} tells you 'Sorry, you can't buy that here.'", Keeper.shopKeeper);
            if (Actor.Pets.Any())
                return Actor.ActPhrase("{0:N} tells you 'You already own a pet.'", Keeper.shopKeeper);
            if (PetBlueprint.Level > Actor.Level)
                return Actor.ActPhrase("{0:N} tells you 'You're not powerful enough to master this pet.'", Keeper.shopKeeper);
            var cost = GetBuyCost(Keeper.shopKeeper, petShopBlueprint, PetBlueprint, true);
            // can afford ?
            TotalCost = cost;
            var wealth = Actor.SilverCoins + Actor.GoldCoins * 100;
            if (TotalCost > wealth)
                return Actor.ActPhrase("{0:N} tells you 'You can't afford to buy {1}'.", Keeper.shopKeeper, PetBlueprint.ShortDescription);
        }
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        var (silver, gold) = Actor.DeductCost(TotalCost);
        Keeper.shopKeeper.UpdateMoney(TotalCost % 100, TotalCost / 100);

        if (Item is not null)
        {
            if (Count == 1)
                Actor.Act(ActOptions.ToAll, "{0:N} buy{0:v} {1} for {2} silver and {3} gold piece{4}.", Actor, Item, silver, gold, TotalCost == 1 ? string.Empty : "s");
            else
                Actor.Act(ActOptions.ToAll, "{0:N} buy{0:v} {1} * {2} for {3} silver and {4} gold piece{5}.", Actor, Count!, Item, silver, gold, TotalCost == 1 ? string.Empty : "s");
            // Inventory items are created on the fly
            if (Item.ItemFlags.IsSet("Inventory"))
            {
                for (int i = 0; i < Count; i++)
                    ItemManager.AddItem(Guid.NewGuid(), Item.Blueprint, Actor);
            }
            // Items previously sold to keeper are 'given' to buyer
            else
            {
                Item.ChangeContainer(Actor);
                Item.SetTimer(TimeSpan.FromMinutes(0));
            }
        }
        else if (PetBlueprint is not null)
        {
            var pet = CharacterManager.AddNonPlayableCharacter(Guid.NewGuid(), PetBlueprint, Actor.Room);
            if (pet == null)
            {
                Logger.LogError("Pet {blueprintId} cannot be created", PetBlueprint.Id);
                Actor.Send(StringHelpers.SomethingGoesWrong);
                return;
            }
            if (PetName is not null)
                pet.SetName(PetName!);
            pet.SetDescription($"{PetBlueprint.Description}A neck tag says 'I belong to {Actor.DisplayName}'." + Environment.NewLine);
            pet.ActFlags.Set("pet");
            pet.AddBaseCharacterFlags(true, "charm");
            Actor.AddPet(pet);
            Actor.Act(ActOptions.ToCharacter, "{0:N} tells you 'Enjoy your pet.'", Keeper.shopKeeper);
            Actor.Act(ActOptions.ToRoom, "{0} bought {1} as a pet.", Actor, pet);
        }
    }
}
