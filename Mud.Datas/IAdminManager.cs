using Mud.Datas.DataContracts;

namespace Mud.Datas
{
    public interface IAdminManager
    {
        AdminData Load(string adminName);
        void Save(AdminData data);
    }
}
