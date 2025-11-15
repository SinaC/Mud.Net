using Mud.Domain;
using Mud.Server.Affects;
using Mud.Server.Common;
using Mud.Server.Flags;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;

namespace Mud.Server.Character.Item;

[CharacterCommand("eat", "Food", MinPosition = Positions.Resting)]
[Syntax("[cmd] <food|pill>")]
public class Eat : CastSpellCharacterGameActionBase
{
    private IRandomManager RandomManager { get; }
    private IAuraManager AuraManager { get; }
    private IAffectManager AffectManager { get; }
    private IItemManager ItemManager { get; }

    public Eat(IAbilityManager abilityManager, IRandomManager randomManager, IAuraManager auraManager, IAffectManager affectManager, IItemManager itemManager)
        : base(abilityManager)
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

        var item = FindHelpers.FindByName(Actor.Inventory.Where(x => Actor.CanSee(x)), actionInput.Parameters[0]);
        if (item == null)
            return StringHelpers.ItemInventoryNotFound;

        if (item is IItemFood food)
            Food = food;
        else if (item is IItemPill pill)
            Pill = pill;
        else
            return "That's not edible.";

        if (Actor is IPlayableCharacter pc && pc[Conditions.Full] > 40 && pc.IsImmortal != true)
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
            CastSpell(Pill, Pill.FirstSpellName, Pill.SpellLevel);
            CastSpell(Pill, Pill.SecondSpellName, Pill.SpellLevel);
            CastSpell(Pill, Pill.ThirdSpellName, Pill.SpellLevel);
            CastSpell(Pill, Pill.FourthSpellName, Pill.SpellLevel);
            ItemManager.RemoveItem(Pill);
        }
    }
}
