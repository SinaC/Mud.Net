namespace Mud.POC.Abilities
{
    public enum AbilityTargets
    {
        // No target
        None, // TAR_IGNORE
        // Fighting if no parameter, character in room if parameter specified
        CharacterOffensive, // TAR_CHAR_OFFENSIVE
        // Itself if no parameter, character in room if parameter specified
        CharacterDefensive, // TAR_CHAR_DEFENSIVE
        // Itself if no parameter, check if parameter == itself if parameter specified
        CharacterSelf, // TAR_CHAR_SELF
        // Item in inventory
        ItemInventory, // TAR_OBJ_INV
        // Fighting if no parameter, character in room, then item in room, then in inventory, then in equipment if parameter specified
        ItemHereOrCharacterOffensive, // TAR_OBJ_CHAR_OFF
        // Itself if no parameter, character in room or item in inventory if parameter specified
        ItemInventoryOrCharacterDefensive, //TAR_OBJ_CHAR_DEF
        // Target will be 'computed' by spell
        Custom,
        // Optional item in inventory
        OptionalItemInventory,
        // Armor in inventory
        ArmorInventory,
        // Weapon in inventory
        WeaponInventory,
        // Victim is source.Fighting
        Fighting,
    }
}
