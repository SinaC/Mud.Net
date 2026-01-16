using Mud.POC.Misc;
using Mud.POC.TestLuaCSharp;
using System.Text;

namespace Mud.POC
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Program p = new();

            await p.Run();
        }

        private async Task Run()
        {
            //https://github.com/nuskey8/Lua-CSharp
            var lua = new TestLuaObject();
            await lua.Run2();

            Console.WriteLine();
        }

        //private void TestPaging()
        //{
        //    TestPaging paging = new TestPaging();
        //    paging.SetData(new StringBuilder("1/Lorem ipsum dolor sit amet, " + Environment.NewLine +
        //                                     "2/consectetur adipiscing elit, " + Environment.NewLine +
        //                                     "3/sed do eiusmod tempor incididunt " + Environment.NewLine +
        //                                     "4/ut labore et dolore magna aliqua. " + Environment.NewLine +
        //                                     "5/Ut enim ad minim veniam, " + Environment.NewLine +
        //                                     "6/quis nostrud exercitation ullamco " + Environment.NewLine +
        //                                     "7/laboris nisi ut aliquip ex " + Environment.NewLine +
        //                                     "8/ea commodo consequat. " + Environment.NewLine +
        //                                     "9/Duis aute irure dolor in " + Environment.NewLine +
        //                                     "10/reprehenderit in voluptate velit " + Environment.NewLine +
        //                                     "11/esse cillum dolore eu fugiat " + Environment.NewLine +
        //                                     "12/nulla pariatur. " + Environment.NewLine +
        //                                     "13/Excepteur sint occaecat " + Environment.NewLine +
        //                                     "14/cupidatat non proident, " + Environment.NewLine +
        //                                     "15/sunt in culpa qui officia deserunt " + Environment.NewLine +
        //                                     "16/mollit anim id est laborum."));
        //    bool hasPaging1 = paging.HasPaging;
        //    string line1 = paging.GetNextLines(1);
        //    bool hasPaging2 = paging.HasPaging;
        //    string line2_10 = paging.GetNextLines(9);
        //    bool hasPaging3 = paging.HasPaging;
        //    string line11_19 = paging.GetNextLines(9);
        //    bool hasPaging4 = paging.HasPaging;
        //}
    }
}
