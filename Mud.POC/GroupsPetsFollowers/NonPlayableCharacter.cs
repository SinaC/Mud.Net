using Mud.Server.Interfaces.GameAction;

namespace Mud.POC.GroupsPetsFollowers
{
    public class NonPlayableCharacter : CharacterBase, INonPlayableCharacter
    {
        public NonPlayableCharacter(string name, IRoom room)
            : base(name, room)
        {
        }

        #region INonPlayableCharacter

        public IPlayableCharacter Master { get; protected set; }

        public void ChangeMaster(IPlayableCharacter master)
        {
            if (master == this)
                return;
            if (Master != null && master != null)
                return; // cannot change from one master to another
            Master = master;
        }

        public void Order(string rawParameters, params ICommandParameter[] parameters)
        {
            if (Master == null)
                return;
            Act(ActOptions.ToCharacter, "{0:N} orders you to {1}", Master, rawParameters);
            // TODO: real implementation
        }

        public override void OnRemoved()
        {
            base.OnRemoved();

            // Free from slavery
            Master?.RemovePet(this);
        }

        #endregion
    }
}
