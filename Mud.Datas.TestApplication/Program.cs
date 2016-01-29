using Mud.Datas.Mongo;

namespace Mud.Datas.TestApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            LoginManager manager = new LoginManager();
            bool isAdmin;
            bool found = manager.CheckUsername("sinac", out isAdmin);
        }
    }
}
