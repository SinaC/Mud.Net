
using System.Text;

namespace Mud.Server.Common
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder AppendFormatLine(this StringBuilder sb, string format, params object[] parameters)
        {
            sb.AppendFormat(format, parameters);
            sb.AppendLine();
            return sb;
        }

        public static StringBuilder AppendFormatAndLineIfNotEmpty(this StringBuilder sb, string format, params object[] parameters)
        {
            if (sb.Length > 0)
                sb.AppendLine();
            sb.AppendFormat(format, parameters);
            return sb;
        }
    }
}
