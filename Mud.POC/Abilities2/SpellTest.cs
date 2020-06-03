using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Helpers;
using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;
using Mud.Server.Input;

namespace Mud.POC.Abilities2
{
    public abstract class SpellTestBase<TContext> : ISpell
        where TContext : SpellTestContextBase
    {
        protected IRandomManager RandomManager { get; }
        protected IWiznet Wiznet { get; }

        protected SpellTestBase(IRandomManager randomManager, IWiznet wiznet)
        {
            RandomManager = randomManager;
            Wiznet = wiznet;
        }

        #region ISpell

        #region IAbility

        public abstract int Id { get; }

        public abstract string Name { get; }

        public virtual int PulseWaitTime => 12;

        public virtual int Cooldown => 0;

        public virtual int LearnDifficultyMultiplier => 1;

        public virtual AbilityFlags Flags => AbilityFlags.None;

        public abstract AbilityEffects Effects { get; }

        #endregion

        public CastResults Cast(ICharacter caster, string rawParameters, params CommandParameter[] parameters)
        {
            TContext context = CreateContext(caster, caster.Level, rawParameters, parameters);
            if (context == null)
                return CastResults.Error;
            if (context.AbilityTargetResult != AbilityTargetResults.Ok)
                return MapAbilityTargetResultsToCastResults(context.AbilityTargetResult);

            InvokeSpell(context);

            return CastResults.Ok;
        }

        #endregion

        protected abstract TContext CreateContext(ICharacter caster, int level, string rawParameters, params CommandParameter[] parameters);
        protected abstract void InvokeSpell(TContext context);

        private CastResults MapAbilityTargetResultsToCastResults(AbilityTargetResults result)
        {
            switch (result)
            {
                case AbilityTargetResults.MissingParameter:
                    return CastResults.MissingParameter;
                case AbilityTargetResults.InvalidTarget:
                    return CastResults.InvalidTarget;
                case AbilityTargetResults.TargetNotFound:
                    return CastResults.TargetNotFound;
                case AbilityTargetResults.Error:
                    return CastResults.Error;
                default:
                    Wiznet.Wiznet($"Unexpected AbilityTargetResults {result}", WiznetFlags.Bugs, AdminLevels.Implementor);
                    return CastResults.Error;
            }
        }

        #region IEquatable

        public bool Equals(IAbility other)
        {
            if (other == null)
                return false;
            return Id == other.Id;
        }

        #endregion
    }

    public abstract class SpellTestContextBase
    {
        public ICharacter Caster { get; }
        public int Level { get; }
        public string RawParameters { get; }
        public CommandParameter[] Parameters { get; }
        public AbilityTargetResults AbilityTargetResult { get; }

        protected SpellTestContextBase(AbilityTargetResults result)
        {
            AbilityTargetResult = result;
        }

        protected SpellTestContextBase(ICharacter caster, int level, string rawParameters, params CommandParameter[] parameters)
        {
            AbilityTargetResult = AbilityTargetResults.Ok;
            Caster = caster;
            Level = level;
            RawParameters = rawParameters;
            Parameters = parameters;
        }
    }

    public class OffensiveSpellTestContext : SpellTestContextBase
    {
        public ICharacter Victim { get; }

        public OffensiveSpellTestContext(AbilityTargetResults result)
            : base(result)
        {
        }

        public OffensiveSpellTestContext(ICharacter caster, int level, ICharacter victim, string rawParameters, params CommandParameter[] parameters)
            : base(caster, level, rawParameters, parameters)
        {
            Victim = victim;
        }
    }

    public abstract class OffensiveSpellTestBase : SpellTestBase<OffensiveSpellTestContext>
    {
        protected OffensiveSpellTestBase(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override OffensiveSpellTestContext CreateContext(ICharacter caster, int level, string rawParameters, params CommandParameter[] parameters)
        {
            ICharacter victim;
            INonPlayableCharacter npcCaster = caster as INonPlayableCharacter;
            if (parameters.Length < 1)
            {
                victim = caster.Fighting;
                if (victim == null)
                {
                    caster.Send("Cast the spell on whom?");
                    return new OffensiveSpellTestContext(AbilityTargetResults.MissingParameter);
                }
            }
            else
                victim = FindHelpers.FindByName(caster.Room.People, parameters[0]);
            if (victim == null)
            {
                caster.Send("They aren't here.");
                return new OffensiveSpellTestContext(AbilityTargetResults.TargetNotFound);
            }
            if (caster is IPlayableCharacter)
            {
                if (caster != victim && victim.IsSafe(caster))
                {
                    caster.Send("Not on that Victim.");
                    return new OffensiveSpellTestContext(AbilityTargetResults.InvalidTarget);
                }
                // TODO: check_killer
            }
            if (npcCaster != null && npcCaster.CharacterFlags.HasFlag(CharacterFlags.Charm) && npcCaster.Master == victim)
            {
                caster.Send("You can't do that on your own follower.");
                return new OffensiveSpellTestContext(AbilityTargetResults.InvalidTarget);
            }
            // victim found
            return new OffensiveSpellTestContext(caster, level, victim, rawParameters, parameters);
        }
    }

    public abstract class DamageSpellTestBase : OffensiveSpellTestBase
    {
        protected DamageSpellTestBase(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        #region IAbility

        public override AbilityEffects Effects => AbilityEffects.Damage;

        #endregion

        #region OffensiveSpellBase

        protected override void InvokeSpell(OffensiveSpellTestContext context)
        {
            //
            bool preDamage = PreDamage(context);
            if (!preDamage)
                return;
            //
            int damage = DamageValue(context);
            bool savesSpellResult = context.Victim.SavesSpell(context.Level, DamageType);
            if (savesSpellResult)
                damage /= 2;
            DamageResults damageResult = context.Victim.AbilityDamage(context.Caster, this, damage, DamageType, DamageNoun, true);
            //
            PostDamage(context, savesSpellResult, damageResult);
        }

        #endregion

        protected abstract SchoolTypes DamageType { get; }
        protected abstract int DamageValue(OffensiveSpellTestContext context);
        protected abstract string DamageNoun { get; }

        protected virtual bool PreDamage(OffensiveSpellTestContext context)
        {
            // NOP
            return true;
        }

        protected virtual void PostDamage(OffensiveSpellTestContext context, bool savesSpellResult, DamageResults damageResult)
        {
            // NOP
        }
    }
}
