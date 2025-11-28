using Mud.Common.Attributes;

namespace Mud.Server.Effects;

[AttributeUsage(AttributeTargets.Class)]
public class WeaponEffectAttribute : ExportAttribute // every weapon affect will be exported without ContractType
{
    public string WeaponFlagName { get; }

    public WeaponEffectAttribute(string weaponFlagName)
    {
        WeaponFlagName = weaponFlagName;
    }
}
