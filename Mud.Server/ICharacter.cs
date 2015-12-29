using Mud.Server.Blueprints;

namespace Mud.Server
{
    public enum Sex
    {
        Neutral,
        Male,
        Female
    }

    public interface ICharacter : IEntity, IContainer
    {
        CharacterBlueprint Blueprint { get; }

        IRoom Room { get; }

        Sex Sex { get; }

        bool Impersonable { get; }
        IPlayer ImpersonatedBy { get; }

        ICharacter Slave { get; } // who is our slave (related to charm command/spell)
        ICharacter ControlledBy { get; } // who is our master (related to charm command/spell)

        bool ChangeImpersonation(IPlayer player); // if non-null, start impersonation, else, stop impersonation
        bool ChangeController(ICharacter master); // if non-null, start slavery, else, stop slavery

        bool CanSee(ICharacter character);
        bool CanSee(IItem obj);
    }
}
