using Mud.Server.Blueprints;

namespace Mud.Server.Room
{
    public class Exit : IExit
    {
        public Exit(string description, IRoom destination)
        {
            Description = description;
            Destination = destination;
        }

        #region IExit

        public ExitBlueprint Blueprint { get; private set; } // TODO: 1st parameter in ctor

        public string Name { get; private set; }
        public string Description { get; private set; }
        public IRoom Destination { get; private set; }
        
        #endregion
    }
}
