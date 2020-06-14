using Mud.Logger;

namespace Mud.Domain.Extensions
{
    public static class BasicAttributesExtensions
    {
        public static string ShortName(this BasicAttributes attribute)
        {
            switch (attribute)
            {
                case BasicAttributes.Strength: return "Str";
                case BasicAttributes.Intelligence: return "Int";
                case BasicAttributes.Wisdom: return "Wis";
                case BasicAttributes.Dexterity: return "Dex";
                case BasicAttributes.Constitution: return "Con";
                default:
                    Log.Default.WriteLine(LogLevels.Error, "BasicAttributes.ShortName: Invalid attribute {0}", attribute);
                    return attribute.ToString();
            }
        }
    }
}
