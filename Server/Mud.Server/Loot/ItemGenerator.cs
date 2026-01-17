using Mud.Common;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Random;
using Mud.Server.Affects.Character;
using Mud.Server.Domain;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Loot;
using Mud.Server.Interfaces.Room;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.Server.Loot;

[Export(typeof(IItemGenerator)), Shared]
public class ItemGenerator : IItemGenerator
{
    private readonly HashSet<int> _blueprintIds;

    private IRandomManager RandomManager { get; }
    private IItemManager ItemManager { get; }

    public ItemGenerator(IRandomManager randomManager, IItemManager itemManager)
    {
        _blueprintIds = [];

        RandomManager = randomManager;
        ItemManager = itemManager;
    }

    public void AddBlueprintId(int id)
    {
        _blueprintIds.Add(id);
    }

    public IItem? Generate(INonPlayableCharacter victim, IItemCorpse? corpse, IRoom room)
    {
        // select random blueprint id in available range
        var blueprintId = RandomManager.Random(_blueprintIds);
        // create item
        var item = corpse == null
            ? ItemManager.AddItem(Guid.NewGuid(), blueprintId, "ItemGenerator", room) // TODO: Act(ActOptions.ToRoom, "{0} falls to the floor.", item); ?
            : ItemManager.AddItem(Guid.NewGuid(), blueprintId, "ItemGenerator", corpse);
        if (item == null)
            return null;
        // enchant item
        Enchant(item, victim.Level);
        return item;
    }

    public void Enchant(IItem item, int level)
    {
        // 1/ select rarity (normal, uncommon, rare)
        var rarityPercent = RandomManager.Range(1, 100);
        var rarity = Rarities.Common;
        if (rarityPercent > 90)
            rarity = Rarities.Rare;
        else if (rarityPercent > 70)
            rarity = Rarities.Uncommon;
        // 2/ select suffix
        var suffix = RandomManager.Random(Suffixes);
        // 3/ add affects
        AddAffects(item, level, rarity, suffix.bonus);
        // 4/ set armor/weapon dices
        if (item is IItemWeapon weapon)
        {
            var diceValue = Math.Max(1, level * RarityMultiplierFactor100(rarity) / 300);
            var diceCount = level / diceValue;
            weapon.SetDices(diceCount, diceValue);
        }
        else if (item is IItemArmor armor)
        {
            var ac = -10 * level * RarityMultiplierFactor100(rarity) / 200;
            armor.SetArmor(ac, ac, ac, ac);
        }
        // 5/ set level, cost, descriptions
        item.SetLevel(level);
        var cost = 10 * level * RarityMultiplierFactor100(rarity) / 100;
        item.SetCost(cost);
        // descriptions
        var shortDescription = item.Blueprint.ShortDescription + " " + suffix.suffix.ToLowerInvariant();
        var description = item.Blueprint.ShortDescription.ToPascalCase() + " " + suffix.suffix + " is here.";
        item.SetShortDescription(shortDescription);
        item.SetDescription(description);
    }

    //  max(1, level * rarity.multiplier * bonus.multiplier / (4 * #bonus))
    private static void AddAffects(IItem item, int level, Rarities rarity, (CharacterAttributeAffectLocations attribute, int multiplier)[] bonus)
    {
        var affects = new List<IAffect>();
        var rarityMultiplierFactor100 = RarityMultiplierFactor100(rarity);
        foreach (var (attribute, multiplier) in bonus)
        {
            var modifier = Math.Max(1, level * rarityMultiplierFactor100 * multiplier / (4 * 100 * bonus.Length));
            var affect = new CharacterAttributeAffect { Location = attribute, Operator = AffectOperators.Add, Modifier = modifier };
            affects.Add(affect);
        }
        var aura = new Aura.Aura(null, item, AuraFlags.NoDispel | AuraFlags.Permanent | AuraFlags.Inherent, level, affects.ToArray());
        item.AddAura(aura, false);
    }

    private static int RarityMultiplierFactor100(Rarities rarities)
        => rarities switch
        {
            Rarities.Common => 100,
            Rarities.Uncommon => 150,
            Rarities.Rare => 200,
            _ => 100
        };

    // TODO: of XXX Resistance: fire, cold, lightning, acid, poison, negative, holy
    private static (string suffix, (CharacterAttributeAffectLocations attribute, int multiplier)[] bonus)[] Suffixes { get; } =
    [
        // 1-stat (level * rarity.multiplier * multiplier / 4)
        ("of Strength", [(CharacterAttributeAffectLocations.Strength, 1)]),
        ("of Intelligence", [(CharacterAttributeAffectLocations.Intelligence, 1)]),
        ("of Wisdom", [(CharacterAttributeAffectLocations.Wisdom, 1)]),
        ("of Dexterity", [(CharacterAttributeAffectLocations.Dexterity, 1)]),
        ("of Constitution", [(CharacterAttributeAffectLocations.Constitution, 1)]),
        ("of Precision", [(CharacterAttributeAffectLocations.HitRoll, 5)]),
        ("of Power", [(CharacterAttributeAffectLocations.DamRoll, 5)]),
        ("of Protection", [(CharacterAttributeAffectLocations.AllArmor, -10)]),
        // 2-stat (level * rarity.multiplier * multiplier / 2*4)
        ("of the Gorilla",[(CharacterAttributeAffectLocations.Strength, 1), (CharacterAttributeAffectLocations.Intelligence, 1)]),
        ("of the Boar",[(CharacterAttributeAffectLocations.Strength, 1), (CharacterAttributeAffectLocations.Wisdom, 1)]),
        ("of the Tiger",[(CharacterAttributeAffectLocations.Strength, 1), (CharacterAttributeAffectLocations.Dexterity, 1)]),
        ("of the Bear",[(CharacterAttributeAffectLocations.Strength, 1), (CharacterAttributeAffectLocations.Constitution, 1)]),
        ("of the Owl",[(CharacterAttributeAffectLocations.Intelligence, 1), (CharacterAttributeAffectLocations.Wisdom, 1)]),
        ("of the Falcon",[(CharacterAttributeAffectLocations.Intelligence, 1), (CharacterAttributeAffectLocations.Dexterity, 1)]),
        ("of the Eagle",[(CharacterAttributeAffectLocations.Intelligence, 1), (CharacterAttributeAffectLocations.Constitution, 1)]),
        ("of the Wolf",[(CharacterAttributeAffectLocations.Wisdom, 1), (CharacterAttributeAffectLocations.Dexterity, 1)]),
        ("of the Whale",[(CharacterAttributeAffectLocations.Wisdom, 1), (CharacterAttributeAffectLocations.Constitution, 1)]),
        ("of the Monkey",[(CharacterAttributeAffectLocations.Dexterity, 1), (CharacterAttributeAffectLocations.Constitution, 1)]),
    ];

    private enum Rarities
    {
        Common = 1,
        Uncommon = 2,
        Rare = 3,
    };
}
