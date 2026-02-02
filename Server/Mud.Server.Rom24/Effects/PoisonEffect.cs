using Mud.Domain;
using Mud.Flags;
using Mud.Random;
using Mud.Server.Affects.Character;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Rom24.Affects;

namespace Mud.Server.Rom24.Effects;

[Effect("Poison")]
public class PoisonEffect : IEffect<IRoom>, IEffect<ICharacter>, IEffect<IItem>
{
    private IRandomManager RandomManager { get; }
    private IAuraManager AuraManager { get; }

    public PoisonEffect(IRandomManager randomManager, IAuraManager auraManager)
    {
        RandomManager = randomManager;
        AuraManager = auraManager;
    }

    public void Apply(IRoom room, IEntity source, string auraName, int level, int modifier)
    {
        if (!room.IsValid)
            return;
        var roomContent = room.Content.ToArray();
        foreach (var itemInRoom in roomContent)
            Apply(itemInRoom, source, auraName, level, modifier);
        room.Recompute();
    }

    public void Apply(ICharacter victim, IEntity source, string auraName, int level, int modifier)
    {
        if (!victim.IsValid)
            return;
        // chance of poisoning
        if (!victim.SavesSpell(level / 4 + modifier / 20, SchoolTypes.Poison))
        {
            victim.Send("You feel poison coursing through your veins.");
            victim.Act(ActOptions.ToRoom, "{0} looks very ill.", victim);
            var duration = level / 2;
            var poisonAura = victim.GetAura(auraName);
            if (poisonAura != null)
            {
                poisonAura.Update(level, TimeSpan.FromMinutes(duration));
                poisonAura.AddOrUpdateAffect(
                    x => x.Location == CharacterAttributeAffectLocations.Strength,
                    () => new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -1, Operator = AffectOperators.Add },
                    x => x.Modifier -= 1);
            }
            else
                AuraManager.AddAura(victim, auraName, source, level, TimeSpan.FromMinutes(duration), new AuraFlags(), false,
                    new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -1, Operator = AffectOperators.Add },
                    new CharacterFlagsAffect { Modifier = new CharacterFlags("Poison"), Operator = AffectOperators.Or },
                    new PoisonDamageAffect(),
                    new CharacterRegenModifierAffect { Modifier = 4, Operator = AffectOperators.Divide });
        }
        // equipment
        var inventoryAndEquipments = victim.Inventory.Union(victim.Equipments.Where(x => x.Item != null).Select(x => x.Item!)).ToArray();
        foreach (var itemOnVictim in inventoryAndEquipments)
            Apply(itemOnVictim, source, auraName, level, modifier);
        victim.Recompute();
    }

    public void Apply(IItem item, IEntity source, string auraName, int level, int modifier)
    {
        if (!item.IsValid)
            return;
        var poisonable = item as IItemPoisonable;
        if (poisonable == null)
            return;
        if (poisonable.ItemFlags.HasAny("BurnProof", "Bless")
            || RandomManager.Chance(25))
            return;
        int chance = level / 4 + modifier / 10;
        if (chance > 25)
            chance = (chance - 25) / 2 + 25;
        if (chance > 50)
            chance = (chance - 50) / 2 + 50;
        if (poisonable.ItemFlags.IsSet("Bless"))
            chance -= 5;
        chance -= poisonable.Level * 2;
        chance = Math.Clamp(chance, 5, 95);
        if (!RandomManager.Chance(chance))
            return;
        poisonable.Poison();
    }
}
