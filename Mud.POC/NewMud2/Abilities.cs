using Mud.Server.Input;

namespace Mud.POC.NewMud2
{
    public class AutoAttack : AbilityBase
    {
        public override string Name => "Autoattack";

        public override AbilityTypes Type => AbilityTypes.Damage;

        public override AbilityActionResults Action(ICharacter user, string rawParameters, params CommandParameter[] parameters)
        {
            if (!SetFirstArgAsTargetEnemy(user, parameters))
                return AbilityActionResults.TargetNotFound;

            user.MultiHit(user.TargetEnemy);

            return AbilityActionResults.Ok;
        }
    }

    public class AcidBlast : SingleTargetDamageAbilityBase
    {
        public override string Name => "AcidBlast";

        public override string DamageNoun => "acid blast";
        public override DamageTypes DamageType => DamageTypes.Acid;
        public override int BaseDamage(int level) => RandomManager.Dice(level, 12);
    }

    public class Armor : BuffAbilityBase
    {
        public override string Name => "Armor";

        public override string AlreadyAffectMessage => "{0:N} is already armored.";
        public override string TargetAffectMessage => "You feel someone protecting you.";
        public override string UserAffectMessage => "{0} is protected by your magic.";
        public override IAura CreateAura(ICharacter source) => Factory.CreateAura(this, source, source.Level);
    }

    public class Blindness : DebuffAbilityBase
    {
        public override string Name => "Blindness";

        public override string AlreadyAffectMessage => "{0:N} is already blinded.";
        public override string TargetAffectMessage => "You are blinded!";
        public override string RoomAffectMessage => "{0:N} appears to be blinded.";
        public override DamageTypes DamageType => DamageTypes.Other;
        public override IAura CreateAura(ICharacter source) => Factory.CreateAura(this, source, source.Level);
    }
}
