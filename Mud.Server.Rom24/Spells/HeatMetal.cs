using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Damage, PulseWaitTime = 18)]
public class HeatMetal : OffensiveSpellBase
{
    private const string SpellName = "Heat Metal";

    public HeatMetal(ILogger<HeatMetal> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override void Invoke()
    {
        bool fail = true;
        int damage = 0;
        if (!Victim.SavesSpell(Level + 2, SchoolTypes.Fire) && !Victim.Immunities.IsSet("Fire"))
        {
            bool recompute = false;
            // Check equipments
            foreach (var equippedItem in Victim.Equipments.Where(x => x.Item != null))
            {
                var item = equippedItem.Item!;
                if (!item.ItemFlags.HasAny("BurnProof", "NonMetal")
                    && RandomManager.Range(1, 2 * Level) > item.Level
                    && !Victim.SavesSpell(Level, SchoolTypes.Fire))
                {
                    switch (item)
                    {
                        case IItemArmor itemArmor:
                            if (!itemArmor.ItemFlags.HasAny("NoDrop", "NoRemove") // remove the item
                                && itemArmor.Weight / 10 < RandomManager.Range(1, 2 * Victim[CharacterAttributes.Dexterity]))
                            {
                                itemArmor.ChangeEquippedBy(null, false);
                                itemArmor.ChangeContainer(Victim.Room);
                                Victim.Act(ActOptions.ToRoom, "{0:N} yelps and throws {1} to the ground!", Victim, itemArmor);
                                Victim.Act(ActOptions.ToCharacter, "You remove and drop {0} before it burns you.", itemArmor);
                                damage += RandomManager.Range(1, itemArmor.Level) / 3;
                                fail = false;
                                recompute = true;
                            }
                            else // stuck on the body! ouch! 
                            {
                                Victim.Act(ActOptions.ToCharacter, "Your skin is seared by {0}!", itemArmor);
                                damage += RandomManager.Range(1, itemArmor.Level);
                                fail = false;
                            }
                            break;
                        case IItemWeapon itemWeapon:
                            if (itemWeapon.DamageType != SchoolTypes.Fire)
                            {
                                if (!itemWeapon.ItemFlags.HasAny("NoDrop", "NoRemove")) // remove the item
                                {
                                    itemWeapon.ChangeEquippedBy(null, false);
                                    itemWeapon.ChangeContainer(Victim.Room);
                                    Victim.Act(ActOptions.ToRoom, "{0:N} is burned by {1}, and throws it to the ground.", Victim, itemWeapon);
                                    Victim.Send("You throw your red-hot weapon to the ground!");
                                    damage += 1;
                                    fail = false;
                                    recompute = true;
                                }
                                else // YOWCH
                                {
                                    Victim.Send("Your weapon sears your flesh!");
                                    damage += RandomManager.Range(1, itemWeapon.Level);
                                    fail = false;
                                }
                            }
                            break;
                    }
                }
            }
            // Check inventory
            foreach (var item in Victim.Inventory)
            {
                if (!item.ItemFlags.HasAny("BurnProof", "NonMetal")
                    && RandomManager.Range(1, 2 * Level) > item.Level
                    && !Victim.SavesSpell(Level, SchoolTypes.Fire))
                {
                    switch (item)
                    {
                        case IItemArmor itemArmor:
                            if (!itemArmor.ItemFlags.IsSet("NoDrop")) // drop it if we can
                            {
                                itemArmor.ChangeContainer(Victim.Room);
                                Victim.Act(ActOptions.ToRoom, "{0:N} yelps and throws {1} to the ground!", Victim, itemArmor);
                                Victim.Act(ActOptions.ToCharacter, "You remove and drop {0} before it burns you.", itemArmor);
                                damage += RandomManager.Range(1, itemArmor.Level) / 6;
                                fail = false;
                            }
                            else // cannot drop
                            {
                                Victim.Act(ActOptions.ToCharacter, "Your skin is seared by {0}!", itemArmor);
                                damage += RandomManager.Range(1, itemArmor.Level) / 2;
                                fail = false;
                            }
                            break;
                        case IItemWeapon itemWeapon:
                            if (!itemWeapon.ItemFlags.IsSet("NoDrop")) // drop it if we can
                            {
                                itemWeapon.ChangeContainer(Victim.Room);
                                Victim.Act(ActOptions.ToRoom, "{0:N} throws a burning hot {1} to the ground!", Victim, itemWeapon);
                                Victim.Act(ActOptions.ToCharacter, "You and drop {0} before it burns you.", itemWeapon);
                                damage += RandomManager.Range(1, itemWeapon.Level) / 6;
                                fail = false;
                            }
                            else // cannot drop
                            {
                                Victim.Act(ActOptions.ToCharacter, "Your skin is seared by {0}!", itemWeapon);
                                damage += RandomManager.Range(1, itemWeapon.Level) / 2;
                                fail = false;
                            }
                            break;
                    }
                }
            }
            if (recompute)
                Victim.Recompute();
        }
        if (fail)
        {
            Caster.Send("Your spell had no effect.");
            Victim.Send("You feel momentarily warmer.");
            return;
        }
        // damage
        if (Victim.SavesSpell(Level, SchoolTypes.Fire))
            damage /= 2;
        Victim.AbilityDamage(Caster, damage, SchoolTypes.Fire, "spell", true);
    }
}
