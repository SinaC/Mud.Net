using Mud.Domain;
using Mud.Server.Domain;

namespace Mud.Server.Interfaces.Affect.Character
{
    public interface ICharacterResourceAffect : ICharacterAffect
    {
        public AffectOperators Operator { get; } // Or and Nor cannot be used
        public ResourceKinds Location { get; }
        public int Modifier { get; }
    }
}
