using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mud.Server.Blueprints;

namespace Mud.Server.Item
{
    public class ItemWeapon : ItemBase
    {
        public WeaponTypes Type { get; set; }
        public int DiceCount { get; private set; }
        public int DiceValue { get; private set; }
        public DamageTypes DamageType { get; private set; }
        // TODO: special type, damage string (see 2nd col  in const.C:208), proc

        public ItemWeapon(Guid guid, ItemBlueprint blueprint, IContainer containedInto) 
            : base(guid, blueprint, containedInto)
        {
            Type = (WeaponTypes) blueprint.Values[0]; // TODO: sub blueprint
            DiceCount = blueprint.Values[1];
            DiceValue = blueprint.Values[2];
            DamageType = (DamageTypes)blueprint.Values[3];
        }
    }
}
