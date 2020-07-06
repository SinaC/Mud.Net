﻿using Mud.Domain;
using Mud.Server.Effects;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;

namespace Mud.Server.Rom24.WeaponEffects
{
    [WeaponEffect("Flaming")]
    public class Flaming : IPostHitDamageWeaponEffect
    {
        private IRandomManager RandomManager { get; }
        private IEffectManager EffectManager { get; }

        public Flaming(IRandomManager randomManager, IEffectManager effectManager)
        {
            RandomManager = randomManager;
            EffectManager = effectManager;
        }

        public bool Apply(ICharacter holder, ICharacter victim, IItemWeapon weapon)
        {
            int damage = RandomManager.Range(1, 1 + weapon.Level / 4);
            victim.Act(ActOptions.ToRoom, "{0} is burned by {1}.", victim, weapon);
            victim.Act(ActOptions.ToCharacter, "{0} sears your flesh.", weapon);
            victim.Damage(holder, damage, SchoolTypes.Fire, null, false);
            IEffect<ICharacter> fireEffect = EffectManager.CreateInstance<ICharacter>("Fire");
            fireEffect?.Apply(victim, holder, "Fire breath", weapon.Level / 2, damage);
            return true;
        }
    }
}
