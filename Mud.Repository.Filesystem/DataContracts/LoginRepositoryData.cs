using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract(Namespace = "")]
    public class LoginRepositoryData
    {
        [DataMember]
        public List<LoginData> Logins { get; set; }
    }
}
