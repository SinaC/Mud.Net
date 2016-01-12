using Mud.Server.Blueprints;

namespace Mud.Server.Room
{
    public class Exit : IExit
    {
        public Exit(string description, ExitBlueprint blueprint, IRoom destination)
        {
            Description = description;
            Destination = destination;
            Blueprint = blueprint;
        }

        #region IExit

        public ExitBlueprint Blueprint { get; private set; } // TODO: 1st parameter in ctor

        public string Name { get; private set; }
        public string Keywords { get; private set; }
        public string Description { get; private set; }
        public IRoom Destination { get; private set; }

        public void OnRemoved()
        {
            Destination = null;
            Blueprint = null;
        }

        #endregion
    }
}
