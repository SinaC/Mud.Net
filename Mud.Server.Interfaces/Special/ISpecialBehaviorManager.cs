namespace Mud.Server.Interfaces.Special
{
    public  interface ISpecialBehaviorManager
    {
        int Count { get; }

        public ISpecialBehavior? CreateInstance(string name);
    }
}
