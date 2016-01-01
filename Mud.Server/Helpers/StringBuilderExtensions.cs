
using System.Text;

namespace Mud.Server.Helpers
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder AppendFormatLine(this StringBuilder sb, string format, params object[] parameters)
        {
            sb.AppendFormat(format, parameters);
            sb.AppendLine();
            return sb;
        }
    }
}
