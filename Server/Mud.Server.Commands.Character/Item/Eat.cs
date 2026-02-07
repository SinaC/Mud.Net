using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Flags;
using Mud.Random;
using Mud.Server.Ability.Interfaces;
using Mud.Server.Affects.Character;
using Mud.Server.Parser.Interfaces;
using Mud.Server.Domain.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain;
using Mud.Server.Guards.CharacterGuards;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Commands.Character.Item;

[CharacterCommand("eat", "Food")]
[Syntax("[cmd] <food|pill>")]
[Help(@"When you are hungry, [cmd] something.")]
public class Eat : CastSpellCharacterGameActionBase<ICharacter, ICharacterGameActionInfo>
{
    protected override IGuard<ICharacter>[] Guards => [new RequiresMinPosition(Positions.Resting), new RequiresAtLeastOneArgument { Message = "Eat what ?" }];

    private IRandomManager RandomManager { get; }
    private IAuraManager AuraManager { get; }
    private IAffectManager AffectManager { get; }
    private IItemManager ItemManager { get; }

    public Eat(ILogger<Eat> logger, IParser parser, IAbilityManager abilityManager, IRandomManager randomManager, IAuraManager auraManager, IAffectManager affectManager, IItemManager itemManager)
        : base(logger, parser, abilityManager)
    {
        RandomManager = randomManager;
        AuraManager = auraManager;
        AffectManager = affectManager;
        ItemManager = itemManager;
    }

    private IItemFood Food { get; set; } = default!;
    private IItemPill Pill { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        var item = FindHelpers.FindByName(Actor.Inventory.Where(Actor.CanSee), actionInput.Parameters[0]);
        if (item == null)
            return StringHelpers.ItemInventoryNotFound;

        if (item is IItemFood food)
            Food = food;
        else if (item is IItemPill pill)
            Pill = pill;
        else
            return "That's not edible.";

        if (Actor is IPlayableCharacter pc && pc[Conditions.Full] > 40 && pc.ImmortalMode.IsSet("Infinite"))
            return "You are too full to eat more.";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        Actor.Act(ActOptions.ToAll, "{0:N} eat{0:v} {1}.", Actor, Food as IItem ?? Pill);

        if (Food != null)
        {
            if (Actor is IPlayableCharacter pc)
            {
                pc.IncrementStatistics(AvatarStatisticTypes.FoodEaten);

                int hunger = pc[Conditions.Hunger];
                pc.GainCondition(Conditions.Full, Food.FullHours);
                pc.GainCondition(Conditions.Hunger, Food.HungerHours);
                if (hunger == 0 && pc[Conditions.Hunger] > 0)
                    Actor.Send("You are no longer hungry.");
                else if (pc[Conditions.Full] > 40)
                    Actor.Send("You are full.");
            }
            // poisoned ?
            if (Food.IsPoisoned)
            {
                Actor.Act(ActOptions.ToAll, "{0:N} choke{0:v} and gag{0:v}.", Actor);
                // search poison affect
                var poisonAura = Actor.GetAura("Poison");
                int level = RandomManager.Fuzzy(Food.FullHours);
                int duration = Food.FullHours * 2;
                if (poisonAura != null)
                {
                    poisonAura.Update(level, TimeSpan.FromMinutes(duration));
                }
                else
                {
                    var poisonAffect = AffectManager.CreateInstance("Poison");
                    AuraManager.AddAura(Actor, "Poison", Food, level, TimeSpan.FromMinutes(duration), false,
                        new CharacterFlagsAffect { Modifier = new CharacterFlags("Poison"), Operator = AffectOperators.Or },
                        poisonAffect);
                }
                Actor.Recompute();
            }
            ItemManager.RemoveItem(Food);
        }
        else if (Pill != null)
        {
            if (Actor is IPlayableCharacter pc)
                pc.IncrementStatistics(AvatarStatisticTypes.PillEaten);

            if (Pill.FirstSpellName != null)
                CastSpell(Pill, Pill.FirstSpellName, Pill.SpellLevel);
            if (Pill.SecondSpellName != null)
                CastSpell(Pill, Pill.SecondSpellName, Pill.SpellLevel);
            if (Pill.ThirdSpellName != null)
                CastSpell(Pill, Pill.ThirdSpellName, Pill.SpellLevel);
            if (Pill.FourthSpellName != null)
                CastSpell(Pill, Pill.FourthSpellName, Pill.SpellLevel);
            ItemManager.RemoveItem(Pill);
        }
    }
}
