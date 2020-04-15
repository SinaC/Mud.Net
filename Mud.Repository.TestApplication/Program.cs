using Mud.Repository.Mongo;

namespace Mud.Repository.TestApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            LoginRepository manager = new LoginRepository();
            bool isAdmin;
            bool found = manager.CheckUsername("sinac", out isAdmin);
        }
    }
}
