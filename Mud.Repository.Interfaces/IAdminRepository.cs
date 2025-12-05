using Mud.Domain.SerializationData;

namespace Mud.Repository.Interfaces;

public interface IAdminRepository
{
    AdminData? Load(string adminName);
    void Save(AdminData adminData);
    IEnumerable<string> GetAvatarNames();
}
