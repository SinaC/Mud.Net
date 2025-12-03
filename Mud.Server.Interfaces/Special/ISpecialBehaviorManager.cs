namespace Mud.Server.Interfaces.Special
{
    public  interface ISpecialBehaviorManager
    {
        IReadOnlyCollection<string> Specials { get; }

        public ISpecialBehavior? CreateInstance(string name);
    }
}
