using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Mud.Datas.Filesystem.DataContracts
{
    [DataContract]
    public class LoginRepositoryData
    {
        [DataMember]
        public List<LoginData> Logins { get; set; }
    }
}
