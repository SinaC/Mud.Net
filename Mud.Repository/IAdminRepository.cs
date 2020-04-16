using Mud.Domain;
using System.Collections.Generic;

namespace Mud.Repository
{
    public interface IAdminRepository
    {
        AdminData Load(string adminName);
        void Save(AdminData adminData);
        IEnumerable<string> GetAvatarNames();
    }
}
