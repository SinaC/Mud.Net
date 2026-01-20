using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Affects.Character;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain;
using Mud.Flags;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Random;

namespace Mud.Server.Commands.Character.Item;

[CharacterCommand("eat", "Food"), MinPosition(Positions.Resting)]
[Syntax("[cmd] <food|pill>")]
[Help(@"When you are hungry, [cmd] something.")]
public class Eat : CastSpellCharacterGameActionBase<ICharacter, ICharacterGameActionInfo>
{
    private IRandomManager RandomManager { get; }
    private IAuraManager AuraManager { get; }
    private IAffectManager AffectManager { get; }
    private IItemManager ItemManager { get; }

    public Eat(ILogger<Eat> logger, ICommandParser commandParser, IAbilityManager abilityManager, IRandomManager randomManager, IAuraManager auraManager, IAffectManager affectManager, IItemManager itemManager)
        : base(logger, commandParser, abilityManager)
    {
        RandomManager = randomManager;
        AuraManager = auraManager;
        AffectManager = affectManager;
        ItemManager = itemManager;
    }

    protected IItemFood Food { get; set; } = default!;
    protected IItemPill Pill { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            return "Eat what?";

        var item = FindHelpers.FindByName(Actor.Inventory.Where(Actor.CanSee), actionInput.Parameters[0]);
        if (item == null)
            return StringHelpers.ItemInventoryNotFound;

        if (item is IItemFood food)
            Food = food;
        else if (item is IItemPill pill)
            Pill = pill;
        else
            return "That's not edible.";

        if (Actor is IPlayableCharacter pc && pc[Conditions.Full] > 40 && pc.ImmortalMode.HasFlag(ImmortalModeFlags.Infinite))
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
                    AuraManager.AddAura(Actor, "Poison", Food, level, TimeSpan.FromMinutes(duration), AuraFlags.None, false,
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
