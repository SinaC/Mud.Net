using System;
using System.Collections.Generic;
using System.Linq;
using Mud.Server.Blueprints;
using Mud.Server.Server;

namespace Mud.Server.Item
{
    public class ItemCorpse : ItemBase<ItemCorpseBlueprint>, IItemCorpse
    {
        private readonly string _corpseName;
        private readonly List<IItem> _content;

        public override string DisplayName => "The corpse of " + _corpseName;

        public ItemCorpse(Guid guid, ItemCorpseBlueprint blueprint, IContainer containedInto, ICharacter character)
            : base(guid, blueprint, containedInto)
        {
            _corpseName = character.DisplayName;
            Name = "corpse of " + _corpseName;
            // TODO: custom short description
            Description = "The corpse of " + _corpseName + " is laying here.";
            _content = new List<IItem>();
            List<IItem> inventory = new List<IItem>(character.Content); // clone
            if (inventory.Any())
                DecayPulseLeft = ServerOptions.PulsePerMinutes * 5;
            else
                DecayPulseLeft = ServerOptions.PulsePerMinutes;
            foreach (IItem item in inventory) // TODO: check stay death flag
                item.ChangeContainer(this);
            foreach (IEquipable item in character.Equipments.Where(x => x.Item != null).Select(x => x.Item))
            {
                item.ChangeContainer(this);
                item.ChangeEquipedBy(null);
            }
        }

        public override int Weight => base.Weight + _content.Sum(x => x.Weight);

        #region IContainer

        public IEnumerable<IItem> Content => _content;

        public bool PutInContainer(IItem obj)
        {
            //return false; // cannot put anything in a corpse, puttin something is done thru constructor
            _content.Insert(0, obj);
            return true;
        }

        public bool GetFromContainer(IItem obj)
        {
            return _content.Remove(obj);
        }

        #endregion
    }
}
