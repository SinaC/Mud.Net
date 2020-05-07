using System.Linq;

namespace Mud.POC.Affects
{
    public abstract class ItemBase : EntityBase, IItem
    {
        protected ItemBase(string name, IEntity containedIn, ICharacter equipedBy)
            : base(name)
        {
            ContainedIn = containedIn;
            EquipedBy = equipedBy;
        }

        protected ItemBase(string name, IEntity containedIn, ICharacter equipedBy, ItemFlags itemFlags)
            : this(name, containedIn, equipedBy)
        {
            BaseItemFlags = itemFlags;
        }

        public IEntity ContainedIn { get; private set; }
        public ICharacter EquipedBy { get; private set; }

        public ItemFlags BaseItemFlags { get; protected set; }
        public ItemFlags CurrentItemFlags { get; protected set; }

        public void ApplyAffect(ItemFlagsAffect affect)
        {
            switch (affect.Operator)
            {
                case AffectOperators.Add:
                case AffectOperators.Or:
                    CurrentItemFlags |= affect.Modifier;
                    break;
                case AffectOperators.Assign:
                    CurrentItemFlags = affect.Modifier;
                    break;
                case AffectOperators.Nor:
                    CurrentItemFlags &= ~affect.Modifier;
                    break;
                default:
                    break;
            }
            return;
        }

        public override void Recompute()
        {
            ResetAttributes();

            // 1/ Apply auras from room containing item if in a room
            if (ContainedIn is IRoom room && room.IsValid)
            {
                 ApplyAuras<IItem>(room, this);
            }

            // 2/ Apply auras from charcter equiping item if equiped by a character
            if (EquipedBy != null && EquipedBy.IsValid)
            {
                ApplyAuras<IItem>(EquipedBy, this);
            }

            // 3/ Apply own auras
            ApplyAuras<IItem>(this, this);
        }

        protected override void ResetAttributes()
        {
            CurrentItemFlags = BaseItemFlags;
        }

        protected void ApplyAuras<T>(IEntity source, T target)
            where T : IItem
        {
            if (!source.IsValid)
                return;
            foreach (IAura aura in source.Auras.Where(x => x.IsValid))
            {
                foreach (IItemAffect<T> affect in aura.Affects.OfType<IItemAffect<T>>())
                {
                    affect.Apply(target);
                }
            }
        }
    }
}
