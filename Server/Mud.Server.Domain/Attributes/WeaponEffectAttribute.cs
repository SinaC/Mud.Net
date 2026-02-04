using Mud.Common.Attributes;

namespace Mud.Server.Domain.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class WeaponEffectAttribute(string weaponFlagName) : ExportAttribute // every weapon affect will be exported without ContractType
{
    public string WeaponFlagName { get; } = weaponFlagName.ToLowerInvariant();
}
