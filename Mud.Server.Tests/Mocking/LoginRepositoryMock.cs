using Mud.Repository.Interfaces;

namespace Mud.Server.Tests.Mocking
{
    public class LoginRepositoryMock : ILoginRepository
    {
        public bool ChangeAdminStatus(string username, bool isAdmin)
        {
            throw new NotImplementedException();
        }

        public bool ChangePassword(string username, string password)
        {
            throw new NotImplementedException();
        }

        public bool CheckPassword(string username, string password)
        {
            throw new NotImplementedException();
        }

        public bool CheckUsername(string username, out bool isAdmin)
        {
            throw new NotImplementedException();
        }

        public bool DeleteLogin(string username)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetLogins()
        {
            throw new NotImplementedException();
        }

        public bool InsertLogin(string username, string password)
        {
            throw new NotImplementedException();
        }
    }
}
