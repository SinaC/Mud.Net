using System.Collections.Generic;

namespace Mud.Server
{
    public interface IUniquenessManager
    {
        void Initialize();

        bool IsAvatarNameAvailable(string avatarName);
        bool IsAccountNameAvailable(string accountName);

        void AddAvatarName(string avatarName);
        void RemoveAvatarName(string avatarName);
        void RemoveAvatarNames(IEnumerable<string> avatarNames);
        //void AddAccountName(string accountName);
        //void RemoveAccountName(string accountName);
    }
}
