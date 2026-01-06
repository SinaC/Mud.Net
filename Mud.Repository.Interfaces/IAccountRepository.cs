using Mud.Domain.SerializationData;

namespace Mud.Repository.Interfaces;

public interface IAccountRepository
{
    AccountData? Load(string accountName);
    void Save(AccountData accountData);
    void Delete(string accountName); // this will not delete avatar found in account
    IEnumerable<string> AccountNames { get; }
    IEnumerable<string> AvatarNames { get; }
}
