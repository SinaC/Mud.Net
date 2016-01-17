using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mud.Datas.Mongo;

namespace Mud.Datas.TestApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            LoginManager manager = new LoginManager();
            bool isAdmin;
            manager.CheckUsername("sinac", out isAdmin);
        }
    }
}
