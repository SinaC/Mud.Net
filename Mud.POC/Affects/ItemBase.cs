using System.Linq;

namespace Mud.POC.Affects
{
    public abstract class ItemBase : EntityBase, IItem
    {
        protected ItemBase(string name, IEntity containedIn, ICharacter equippedBy)
            : base(name)
        {
            ContainedIn = containedIn;
            EquippedBy = equippedBy;
        }

        protected ItemBase(string name, IEntity containedIn, ICharacter equippedBy, ItemFlags itemFlags)
            : this(name, containedIn, equippedBy)
        {
            BaseItemFlags = itemFlags;
        }

        public IEntity ContainedIn { get; }
        public ICharacter EquippedBy { get; }

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
            }
        }

        public override void Recompute()
        {
            ResetAttributes();

            // 1/ Apply auras from room containing item if in a room
            if (ContainedIn is IRoom room && room.IsValid)
            {
                 ApplyAuras<IItem>(room, this);
            }

            // 2/ Apply auras from character equipping item if equipped by a character
            if (EquippedBy != null && EquippedBy.IsValid)
            {
                ApplyAuras<IItem>(EquippedBy, this);
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
