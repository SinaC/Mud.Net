using Mud.Server.Blueprints;

namespace Mud.Server.Room
{
    // TODO: use blueprint for name/keywords/...
    public class Exit : IExit
    {
        public Exit(ExitBlueprint blueprint, IRoom destination)
        {
            Description = blueprint.Description;
            Destination = destination;
            Blueprint = blueprint;
        }

        #region IExit

        public ExitBlueprint Blueprint { get; private set; }

        public string Name { get; private set; }
        public string Keywords { get; private set; }
        public string Description { get; }
        public IRoom Destination { get; private set; }

        public void OnRemoved()
        {
            Destination = null;
            Blueprint = null;
        }

        #endregion
    }
}
