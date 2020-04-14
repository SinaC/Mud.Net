using Mud.Repository.Mongo;

namespace Mud.Repository.TestApplication
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
