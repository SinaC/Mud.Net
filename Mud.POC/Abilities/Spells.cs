namespace Mud.POC.Abilities
{
    public static class Spells
    {
        [Spell(60, "Teleport", AbilityTargets.None)]
        public static void SpellMassInvis(IAbility ability, int level, ICharacter caster)
        {
            System.Diagnostics.Debug.Print($"Teleport {ability.Name} {level} {caster.Name}");
        }

        [Spell(16, "Acid Blast", AbilityTargets.CharacterOffensive)]
        public static void SpellAcidBlast(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            System.Diagnostics.Debug.Print($"Acid Blast {ability.Name} {level} {caster.Name} {victim.Name}");
        }

        [Spell(17, "Armor", AbilityTargets.CharacterDefensive)]
        public static void SpellArmor(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            System.Diagnostics.Debug.Print($"Armor {ability.Name} {level} {caster.Name} {victim.Name}");
        }

        [Spell(30, "Detect Evil", AbilityTargets.CharacterSelf)]
        public static void SpellDetectEvil(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            System.Diagnostics.Debug.Print($"Teleport {ability.Name} {level} {caster.Name}");
        }

        [Spell(42, "Identify", AbilityTargets.ItemInventory, PulseWaitTime = 24)]
        public static void SpellIdentify(IAbility ability, int level, ICharacter caster, IItem item)
        {
            System.Diagnostics.Debug.Print($"Identify {ability.Name} {level} {caster.Name} {item.Name}");
        }

        [Spell(24, "Curse", AbilityTargets.ItemHereOrCharacterOffensive)]
        public static void SpellCurse(IAbility ability, int level, ICharacter caster, IEntity target)
        {
            System.Diagnostics.Debug.Print($"Curse {ability.Name} {level} {caster.Name} {target.Name}");
        }

        [Spell(21, "Bless", AbilityTargets.ItemInventoryOrCharacterDefensive)]
        public static void SpellBless(IAbility ability, int level, ICharacter caster, IEntity target)
        {
            System.Diagnostics.Debug.Print($"Bless {ability.Name} {level} {caster.Name} {target.Name}");
        }

        [Spell(70, "Locate Object", AbilityTargets.Custom)]
        public static void SpellLocateObject(IAbility ability, int level, ICharacter caster, string rawParameters)
        {
            System.Diagnostics.Debug.Print($"Locate Object {ability.Name} {level} {caster.Name} {rawParameters}");
        }

        [Spell(1000, "Continual Light", AbilityTargets.OptionalItemInventory)]
        public static void SpellContinualLight(IAbility ability, int level, ICharacter caster, IItem item)
        {
            System.Diagnostics.Debug.Print($"Continual Light {ability.Name} {level} {caster.Name} {item?.Name}");
        }

        [Spell(1001, "Enchant Armor", AbilityTargets.ArmorInventory)]
        public static void SpellEnchantArmor(IAbility ability, int level, ICharacter caster, IItemArmor armor)
        {
            System.Diagnostics.Debug.Print($"Enchant Armor {ability.Name} {level} {caster.Name} {armor.Name}");
        }

        [Spell(1002, "Enchant Weapon", AbilityTargets.WeaponInventory)]
        public static void SpellEnchantWeapon(IAbility ability, int level, ICharacter caster, IItemWeapon weapon)
        {
            System.Diagnostics.Debug.Print($"Enchant Weapon {ability.Name} {level} {caster.Name} {weapon.Name}");
        }
    }
}
