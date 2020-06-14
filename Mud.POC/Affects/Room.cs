using System.Collections.Generic;
using System.Linq;

namespace Mud.POC.Affects
{
    public class Room : EntityBase, IRoom
    {
        private List<ICharacter> _people;
        private List<IItem> _content;

        public Room(string name)
            : base(name)
        {
            _people = new List<ICharacter>();
            _content = new List<IItem>();
        }

        public Room(string name, RoomFlags initRoomFlags)
            : this(name)
        {
            BaseRoomFlags = initRoomFlags;
        }

        public IEnumerable<ICharacter> People => _people;

        public IEnumerable<IItem> Content => _content;

        public RoomFlags BaseRoomFlags { get; protected set; }
        public RoomFlags CurrentRoomFlags { get; protected set; }

        public void ApplyAffect(RoomFlagsAffect affect)
        {
            switch (affect.Operator)
            {
                case AffectOperators.Add:
                case AffectOperators.Or:
                    CurrentRoomFlags |= affect.Modifier;
                    break;
                case AffectOperators.Assign:
                    CurrentRoomFlags = affect.Modifier;
                    break;
                case AffectOperators.Nor:
                    CurrentRoomFlags &= ~affect.Modifier;
                    break;
                default:
                    break;
            }
            return;
        }

        public override void Recompute()
        {
            // 0) Reset
            ResetAttributes();

            // 1) Apply own auras
            ApplyAuras(this);

            // 2) Apply people auras
            foreach (ICharacter character in People)
                ApplyAuras(character);

            // 3) Apply content auras
            foreach (IItem item in Content)
                ApplyAuras(item);
        }

        public void AddItem(IItem item)
        {
            _content.Add(item);
        }

        public void AddCharacter(ICharacter character)
        {
            _people.Add(character);
        }

        protected override void ResetAttributes()
        {
            CurrentRoomFlags = BaseRoomFlags;
        }

        protected void ApplyAuras(IEntity entity)
        {
            if (!entity.IsValid)
                return;
            foreach (IAura aura in entity.Auras.Where(x => x.IsValid))
            {
                foreach (IRoomAffect affect in aura.Affects.OfType<IRoomAffect>())
                {
                    affect.Apply(this);
                }
            }
        }
    }
}
