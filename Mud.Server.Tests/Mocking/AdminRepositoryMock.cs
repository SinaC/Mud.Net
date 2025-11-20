using Mud.Domain;
using Mud.Repository.Interfaces;

namespace Mud.Server.Tests.Mocking
{
    public class AdminRepositoryMock : IAdminRepository
    {
        public IEnumerable<string> GetAvatarNames()
        {
            throw new NotImplementedException();
        }

        public AdminData Load(string adminName)
        {
            throw new NotImplementedException();
        }

        public void Save(AdminData adminData)
        {
            throw new NotImplementedException();
        }
    }
}
