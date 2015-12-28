namespace Mud.Server.Input
{
    public class CommandParameter
    {
        public static readonly CommandParameter Empty = new CommandParameter();
        public static readonly CommandParameter Invalid = new CommandParameter();

        public int Count { get; set; }
        public string Value { get; set; }
    }
}
