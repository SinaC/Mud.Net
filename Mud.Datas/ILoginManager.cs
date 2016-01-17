namespace Mud.Datas
{
    public interface ILoginManager
    {
        bool InsertLogin(string username, string password);
        bool CheckUsername(string username, out bool isAdmin);
        bool CheckPassword(string username, string password);
    }
}
