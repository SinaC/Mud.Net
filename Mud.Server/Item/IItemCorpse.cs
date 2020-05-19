namespace Mud.Server.Item
{
    public interface IItemCorpse : IItemCanContain
    {
        bool IsPlayableCharacterCorpse { get; }
    }
}
