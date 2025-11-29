using Mud.DataStructures.Flags;

namespace Mud.Server.Flags.Interfaces
{
    public interface IFlagFactory<TFlag, TFlagValues>
        where TFlag : IFlags<string, TFlagValues>
        where TFlagValues : IFlagValues<string>
    {
        TFlag CreateInstance(string flags);
        TFlag CreateInstance(params string[] flags);
    }


    public interface IFlagFactory
    {
        TFlag CreateInstance<TFlag, TFlagValues>(string flags)
            where TFlag : IFlags<string, TFlagValues>
            where TFlagValues : IFlagValues<string>;

        public TFlag CreateInstance<TFlag, TFlagValues>(params string[] flags)
            where TFlag : IFlags<string, TFlagValues>
            where TFlagValues : IFlagValues<string>;
    }
}
