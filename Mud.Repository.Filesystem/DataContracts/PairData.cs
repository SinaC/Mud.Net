using System.Runtime.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    public class PairData<TKey, TValue>
    {
        [DataMember]
        public TKey Key { get; set; }

        [DataMember]
        public TValue Value { get; set; }

        public PairData()
        {
        }

        public PairData(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }
}
