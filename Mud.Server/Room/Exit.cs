namespace Mud.Server.Room
{
    public class Exit : IExit
    {
        public string Keyword { get; private set; }
        public string Description { get; private set; }
        public IRoom Destination { get; private set; }

        public Exit(string keyword, string description, IRoom destination)
        {
            Keyword = keyword;
            Description = description;
            Destination = destination;
        }
    }
}
