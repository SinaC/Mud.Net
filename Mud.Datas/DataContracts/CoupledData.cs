using System.Runtime.Serialization;

namespace Mud.Datas.DataContracts
{
    [DataContract]
    public class CoupledData<TKey, TData>
    {
        public TKey Key { get; set; }
        public TData Data { get; set; }
    }
}
