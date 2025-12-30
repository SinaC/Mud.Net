using Mud.Server.Domain;

namespace Mud.Server.Common.Extensions;

public static class AffectOperatorsExtensions
{
    public static string PrettyPrint(this AffectOperators op)
        => op switch
        {
            AffectOperators.Add => "by",
            AffectOperators.Or => "by adding",
            AffectOperators.Assign => "by setting to",
            AffectOperators.Nor => "by removing",
            _ => op.ToString(),//Logger.LogError("AffectOperators.PrettyPrint: Invalid operator {0}", op);
        };
}
