using Mud.Common.Attributes;
using Mud.Server.Domain;

namespace Mud.Server.Domain.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public abstract class AbilityBaseAttribute(string name, AbilityEffects effects) : ExportAttribute // every ability will be exported without ContractType
{
    public abstract AbilityTypes Type { get; }
    public string Name { get; set; } = name;
    public AbilityEffects Effects { get; set; } = effects;
    public int CooldownInSeconds { get; set; } = -1;
    public int LearnDifficultyMultiplier { get; set; } = 1;
}

public abstract class ActiveAbilityBaseAttribute(string name, AbilityEffects effects) : AbilityBaseAttribute(name, effects)
{
    public int PulseWaitTime { get; set; } = 12;
}

public class SpellAttribute(string name, AbilityEffects effects) : ActiveAbilityBaseAttribute(name, effects)
{
    public override AbilityTypes Type => AbilityTypes.Spell;
}

public class SkillAttribute(string name, AbilityEffects effects) : ActiveAbilityBaseAttribute(name, effects)
{
    public override AbilityTypes Type => AbilityTypes.Skill;
}

public class PassiveAttribute(string name) : AbilityBaseAttribute(name, AbilityEffects.None)
{
    public override AbilityTypes Type => AbilityTypes.Passive;
}

public class WeaponAttribute(string name, string[] weaponTypes) : AbilityBaseAttribute(name, AbilityEffects.None)
{
    public override AbilityTypes Type => AbilityTypes.Weapon;

    public string[] WeaponTypes { get; set; } = weaponTypes;
}

[AttributeUsage(AttributeTargets.Class)]
public abstract class AbilityAdditionalInfoAttribute : Attribute
{
}

public class AbilityCharacterWearOffMessageAttribute(string message) : AbilityAdditionalInfoAttribute
{
    public string Message { get; set; } = message;
}

public class AbilityItemWearOffMessageAttribute(string holderMessage) : AbilityAdditionalInfoAttribute
{
    public string HolderMessage { get; set; } = holderMessage;
}

public class AbilityDispellableAttribute : AbilityAdditionalInfoAttribute
{
    public string RoomMessage { get; set; } = default!; // displayed to room

    public AbilityDispellableAttribute()
    {
    }

    public AbilityDispellableAttribute(string roomMessage)
    {
        RoomMessage = roomMessage;
    }
}