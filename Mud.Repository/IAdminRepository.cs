using Mud.Domain;

namespace Mud.Repository
{
    public interface IAdminRepository
    {
        AdminData Load(string adminName);
        void Save(AdminData adminData);
    }
}
