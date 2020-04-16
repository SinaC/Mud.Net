using System.Diagnostics;

namespace Mud.POC.TestLua
{
    public class LuaOutput
    {
        public static void Print(string msg)
        {
            Debug.WriteLine(msg);
        }
    }
}
