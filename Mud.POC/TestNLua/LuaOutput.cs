using System.Diagnostics;

namespace Mud.POC.TestNLua
{
    public class LuaOutput
    {
        public static void Print(string msg)
        {
            Debug.WriteLine(">" + msg);
        }
    }
}
