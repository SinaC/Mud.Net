using System.Runtime.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract(Namespace = "")]
    public class LoginData
    {
        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Password { get; set; } // TODO: crypt

        [DataMember]
        public bool IsAdmin { get; set; }
    }
}
