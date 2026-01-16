using Mud.Server.Domain;

namespace Mud.Server.Common.Extensions;

public static class BasicAttributesExtensions
{
    public static string ShortName(this BasicAttributes attribute)
        => attribute switch
        {
            BasicAttributes.Strength => "Str",
            BasicAttributes.Intelligence => "Int",
            BasicAttributes.Wisdom => "Wis",
            BasicAttributes.Dexterity => "Dex",
            BasicAttributes.Constitution => "Con",
            _ => attribute.ToString(),//Logger.LogError("BasicAttributes.ShortName: Invalid attribute {0}", attribute);
        };
}
