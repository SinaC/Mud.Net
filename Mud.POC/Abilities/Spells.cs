namespace Mud.POC.Abilities
{
    public static class Spells
    {
        [Spell(17, "Armor", AbilityTargets.CharacterDefensive)]
        public static void SpellArmor(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            System.Diagnostics.Debug.Print($"Armor {ability.Name} {level} {caster.Name} {victim.Name}");
        }

        [Spell(16, "Acid Blast", AbilityTargets.CharacterOffensive)]
        public static void SpellAcidBlast(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            System.Diagnostics.Debug.Print($"Acid Blast {ability.Name} {level} {caster.Name} {victim.Name}");
        }

        [Spell(21, "Bless", AbilityTargets.ItemInventoryOrCharacterDefensive)]
        public static void SpellBless(IAbility ability, int level, ICharacter caster, IEntity target)
        {
            System.Diagnostics.Debug.Print($"Bless {ability.Name} {level} {caster.Name} {target.Name}");
        }

        [Spell(24, "Curse", AbilityTargets.ItemHereOrCharacterOffensive)]
        public static void SpellCurse(IAbility ability, int level, ICharacter caster, IEntity target)
        {
            System.Diagnostics.Debug.Print($"Curse {ability.Name} {level} {caster.Name} {target.Name}");
        }

        [Spell(30, "Detect Evil", AbilityTargets.CharacterSelf)]
        public static void SpellDetectEvil(IAbility ability, int level, ICharacter caster)
        {
            System.Diagnostics.Debug.Print($"Teleport {ability.Name} {level} {caster.Name}");
        }

        [Spell(60, "Teleport", AbilityTargets.None)]
        public static void SpellTeleport(IAbility ability, int level, ICharacter caster)
        {
            System.Diagnostics.Debug.Print($"Teleport {ability.Name} {level} {caster.Name}");
        }

        [Spell(42, "Identity", AbilityTargets.ItemInventory, PulseWaitTime = 24)]
        public static void SpellIdentify(IAbility ability, int level, ICharacter caster, IItem item)
        {
        }
    }
}
