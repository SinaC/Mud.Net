using Mud.Logger;

namespace Mud.Domain.Extensions
{
    public static class AffectOperatorsExtensions
    {
        public static string PrettyPrint(this AffectOperators op)
        {
            switch (op)
            {
                case AffectOperators.Add: return "by";
                case AffectOperators.Or: return "by adding";
                case AffectOperators.Assign: return "by setting to";
                case AffectOperators.Nor: return "by removing";
                default:
                    Log.Default.WriteLine(LogLevels.Error, "AffectOperators.PrettyPrint: Invalid operator {0}", op);
                    return op.ToString();
            }
        }
    }
}
