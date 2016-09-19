using System.Runtime.Serialization;
using Mud.Server.Constants;

namespace Mud.Datas.DataContracts
{
    [DataContract]
    public class AdminData : PlayerData
    {
        public AdminLevels Level { get; set; }
        public WiznetFlags WiznetFlags { get; set; }

        // TODO: extra fields
    }
}
