using System.Linq;
using Mud.Server.Input;

namespace Mud.POC.NewMud2
{
    public abstract class AbilityBase : IAbility
    {
        protected IRandomManager RandomManager { get; }

        public abstract string Name { get; }

        public abstract AbilityTypes Type { get; }

        public virtual AbilityActionResults PreAction(ICharacter user, string rawParameters, params CommandParameter[] parameters)
        {
            return AbilityActionResults.Ok;
        }

        public abstract AbilityActionResults Action(ICharacter user, string rawParameters, params CommandParameter[] parameters);

        public virtual AbilityActionResults PostAction(ICharacter user, string rawParameters, params CommandParameter[] parameters)
        {
            return AbilityActionResults.Ok;
        }

        protected bool SetFirstArgAsTargetEnemy(ICharacter user, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                if (user.TargetEnemy == null)
                {
                    user.Send("On whom?");
                    return false;
                }

                return true;
            }

            ICharacter target = FindHelpers.FindByName(user, null, parameters[0], CharacterFindLocations.CharacterInRoom);
            if (target == null)
            {
                if (user.TargetEnemy == null)
                {
                    user.Send("They aren't here!");
                    return false;
                }

                return true;
            }
            user.SetTargetEnemy(target);
            return true;
        }

        protected bool SetFirstArgAsTargetAlly(ICharacter user, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                user.SetTargetAlly(user);
                return true;
            }

            ICharacter target = FindHelpers.FindByName(user, null, parameters[0], CharacterFindLocations.CharacterInRoom);
            if (target == null)
            {
                if (user.TargetAlly == null)
                {
                    user.Send("They aren't here!");
                    return false;
                }

                return true;
            }
            user.SetTargetAlly(target);
            return true;
        }
    }

    public abstract class SingleTargetDamageAbilityBase : AbilityBase, IDamageAbility
    {
        public override AbilityTypes Type => AbilityTypes.Damage;

        public abstract string DamageNoun { get; }
        public abstract DamageTypes DamageType { get; }
        public abstract int BaseDamage(int level);
        public virtual bool CheckSavesSpells => true;

        public override AbilityActionResults Action(ICharacter user, string rawParameters, params CommandParameter[] parameters)
        {
            if (!SetFirstArgAsTargetEnemy(user, parameters))
                return AbilityActionResults.TargetNotFound;

            int damage = BaseDamage(user.Level);
            if (CheckSavesSpells && user.TargetEnemy.SavesSpell(user.Level, DamageType))
                damage /= 2;

            user.AbilityDamage(this, user.TargetEnemy, damage, DamageType);

            return AbilityActionResults.Ok;
        }
    }

    public abstract class BuffAbilityBase : AbilityBase
    {
        public override AbilityTypes Type => AbilityTypes.Buff;

        public abstract string AlreadyAffectMessage { get; }
        public abstract string TargetAffectMessage { get; }
        public abstract string UserAffectMessage { get; }
        public abstract IAura CreateAura(ICharacter source);

        public override AbilityActionResults Action(ICharacter user, string rawParameters, params CommandParameter[] parameters)
        {
            if (!SetFirstArgAsTargetAlly(user, parameters))
                return AbilityActionResults.TargetNotFound;

            if (user.TargetAlly.Auras.Any(x => x.Ability == this))
            {
                if (!string.IsNullOrWhiteSpace(AlreadyAffectMessage))
                    user.Act(ActTargets.ToCharacter, AlreadyAffectMessage, user.TargetAlly);
                return AbilityActionResults.InvalidTarget;
            }

            user.TargetAlly.AddAura(CreateAura(user));

            if (!string.IsNullOrWhiteSpace(TargetAffectMessage))
                user.TargetAlly.Act(ActTargets.ToCharacter, TargetAffectMessage, user);
            if (user != user.TargetAlly && !string.IsNullOrWhiteSpace(UserAffectMessage))
                user.Act(ActTargets.ToCharacter, UserAffectMessage, user.TargetAlly);
            return AbilityActionResults.Ok;
        }
    }

    public abstract class DebuffAbilityBase : AbilityBase
    {
        public override AbilityTypes Type => AbilityTypes.Debuff;

        public abstract string AlreadyAffectMessage { get; }
        public abstract string TargetAffectMessage { get; }
        public abstract string RoomAffectMessage { get; }
        public abstract IAura CreateAura(ICharacter source);
        public abstract DamageTypes DamageType { get; }
        public virtual bool CheckSavesSpells => true;

        public override AbilityActionResults Action(ICharacter user, string rawParameters, params CommandParameter[] parameters)
        {
            if (!SetFirstArgAsTargetEnemy(user, parameters))
                return AbilityActionResults.TargetNotFound;
            if (user.IsSafe(user.TargetEnemy))
                return AbilityActionResults.InvalidTarget;

            if (user.TargetEnemy.Auras.Any(x => x.Ability == this))
            {
                if (!string.IsNullOrWhiteSpace(AlreadyAffectMessage))
                    user.Act(ActTargets.ToCharacter, AlreadyAffectMessage, user.TargetEnemy);
                return AbilityActionResults.InvalidTarget;
            }

            if (CheckSavesSpells && user.TargetEnemy.SavesSpell(user.Level, DamageType))
                return AbilityActionResults.SavesCheckSuccess;

            user.TargetEnemy.AddAura(CreateAura(user));

            if (!string.IsNullOrWhiteSpace(TargetAffectMessage))
                user.TargetEnemy.Act(ActTargets.ToCharacter, TargetAffectMessage, user);
            if (!string.IsNullOrWhiteSpace(RoomAffectMessage))
                user.Act(ActTargets.ToRoom, RoomAffectMessage, user.TargetEnemy);
            return AbilityActionResults.Ok;
        }
    }

    //public abstract class ItemOrCharacterBuffAbilityBase : AbilityBase
    //{
    //    public override AbilityTypes Type => AbilityTypes.Buff;

    //    protected IEntity SetFirstArgAsItemOrTargetAlly(ICharacter user, params CommandParameter[] parameters)
    //    {
    //        if (parameters.Length == 0)
    //        {
    //            if (user.TargetAlly == null)
    //            {
    //                user.Send("You don't see that here.");
    //                return null;
    //            }
    //        }
    //    }
    //}
}
