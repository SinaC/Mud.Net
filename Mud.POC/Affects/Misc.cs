namespace Mud.POC.Affects
{
    public static class Misc
    {
        public static string PrettyPrint(this AffectOperators op)
        {
            switch (op)
            {
                case AffectOperators.Add: return "by";
                case AffectOperators.Or: return "by adding";
                case AffectOperators.Assign: return "by setting to";
                case AffectOperators.Nor: return "by removing";
            }
            return "???";
        }
    }
}
