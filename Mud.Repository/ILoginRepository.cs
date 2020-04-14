namespace Mud.Repository
{
    public interface ILoginRepository
    {
        bool InsertLogin(string username, string password);
        bool CheckUsername(string username, out bool isAdmin);
        bool CheckPassword(string username, string password);
        bool ChangePassword(string username, string password);
        bool DeleteLogin(string username);
    }
}
