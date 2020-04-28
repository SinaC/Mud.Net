using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.Logger;
using Mud.Server.Actor;
using Mud.Server.Input;

namespace Mud.Server.Entity
{
    public abstract class EntityBase : ActorBase, IEntity
    {
        protected readonly List<IPeriodicAura> _periodicAuras;
        protected readonly List<IAura> _auras;

        protected EntityBase(Guid guid, string name, string description)
        {
            IsValid = true;
            if (guid == Guid.Empty)
                guid = Guid.NewGuid();
            Id = guid;
            Name = name;
            Keywords = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            Description = description;

            _periodicAuras = new List<IPeriodicAura>();
            _auras = new List<IAura>();
        }

        #region IEntity

        #region IActor

        public override bool ProcessCommand(string commandLine)
        {
            // Extract command and parameters
            bool extractedSuccessfully = CommandHelpers.ExtractCommandAndParameters(commandLine, out var command, out var rawParameters, out var parameters, out _);
            if (!extractedSuccessfully)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Command and parameters not extracted successfully");
                Send("Invalid command or parameters");
                return false;
            }

            Log.Default.WriteLine(LogLevels.Debug, "[{0}] executing [{1}]", DebugName, commandLine);
            return ExecuteCommand(command, rawParameters, parameters);
        }

        public override void Send(string message, bool addTrailingNewLine)
        {
            Log.Default.WriteLine(LogLevels.Debug, "SEND[{0}]: {1}", DebugName, message);

            if (IncarnatedBy != null)
            {
                if (Settings.PrefixForwardedMessages)
                    message = "<INC|" + DisplayName + ">" + message;
                IncarnatedBy.Send(message, addTrailingNewLine);
            }
        }

        public override void Page(StringBuilder text)
        {
            IncarnatedBy?.Page(text);
        }

        #endregion

        public Guid Id { get; }
        public bool IsValid { get; protected set; }
        public string Name { get; protected set; }
        public abstract string DisplayName { get; }
        public IEnumerable<string> Keywords { get; }
        public string Description { get; protected set; }
        public abstract string DebugName { get; }

        public bool Incarnatable { get; private set; }
        public IAdmin IncarnatedBy { get; protected set; }

        // Auras
        public IEnumerable<IPeriodicAura> PeriodicAuras => _periodicAuras;

        public IEnumerable<IAura> Auras => _auras;

        // Incarnation

        public virtual bool ChangeIncarnation(IAdmin admin) // if non-null, start incarnation, else, stop incarnation
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "IEntity.ChangeIncarnation: {0} is not valid anymore", DisplayName);
                IncarnatedBy = null;
                return false;
            }
            if (admin != null)
            {
                if (!Incarnatable)
                {
                    Log.Default.WriteLine(LogLevels.Warning, "IEntity.ChangeIncarnation: {0} cannot be incarnated", DebugName);
                    return false;
                }
                if (IncarnatedBy != null)
                {
                    Log.Default.WriteLine(LogLevels.Warning, "IEntity.ChangeIncarnation: {0} is already incarnated by {1}", DebugName, IncarnatedBy.DisplayName);
                    return false;
                }
            }
            IncarnatedBy = admin;
            return true;
        }

        // Recompute
        public virtual void Reset() 
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "IEntity.ResetAttributes: {0} is not valid anymore", DebugName);
                return;
            }

            // Remove periodic auras on character
            _periodicAuras.Clear();
            _auras.Clear();
        }

        public abstract void RecomputeAttributes();

        // Auras
        public IAura GetAura(int abilityId)
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "IEntity.IsAffected: {0} is not valid anymore", DebugName);
                return null;
            }

            return _auras.FirstOrDefault(x => x.Ability?.Id == abilityId);
        }

        public IAura GetAura(string abilityName)
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "IEntity.IsAffected: {0} is not valid anymore", DebugName);
                return null;
            }

            return _auras.FirstOrDefault(x => x.Ability?.Name == abilityName);
        }

        public IAura GetAura(IAbility ability)
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "IEntity.IsAffected: {0} is not valid anymore", DebugName);
                return null;
            }

            return _auras.FirstOrDefault(x => x.Ability == ability);
        }

        public void AddPeriodicAura(IPeriodicAura aura)
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "IEntity.AddPeriodicAura: {0} is not valid anymore", DebugName);
                return;
            }
            //IPeriodicAura same = _periodicAuras.FirstOrDefault(x => ReferenceEquals(x.Ability, aura.Ability) && x.AuraType == aura.AuraType && x.School == aura.School && x.Source == aura.Source);
            IPeriodicAura same = _periodicAuras.FirstOrDefault(x => x.Ability == aura.Ability && x.AuraType == aura.AuraType && x.School == aura.School && x.Source == aura.Source);
            if (same != null)
            {
                Log.Default.WriteLine(LogLevels.Info, "IEntity.AddPeriodicAura: Refresh: {0} {1}", DebugName, aura.Ability == null ? "<<??>>" : aura.Ability.Name);
                same.Refresh(aura);
            }
            else
            {
                Log.Default.WriteLine(LogLevels.Info, "IEntity.AddPeriodicAura: Add: {0} {1}", DebugName, aura.Ability == null ? "<<??>>" : aura.Ability.Name);
                _periodicAuras.Add(aura);
                if (aura.Ability == null || (aura.Ability.Flags & AbilityFlags.AuraIsHidden) != AbilityFlags.AuraIsHidden)
                    Send("You are now affected by {0}.", aura.Ability == null ? "Something" : aura.Ability.Name);
                if (aura.Source != null && aura.Source != this)
                {
                    ICharacter characterSource = aura.Source as ICharacter;
                    if (aura.Ability == null || (aura.Ability.Flags & AbilityFlags.AuraIsHidden) != AbilityFlags.AuraIsHidden && characterSource != null)
                        characterSource.Act(ActOptions.ToCharacter, "{0} is now affected by {1}", this, aura.Ability == null ? "Something" : aura.Ability.Name);
                    if (aura.AuraType == PeriodicAuraTypes.Damage && characterSource != null && this is ICharacter characterThis)
                    {
                        if (characterThis.Fighting == null)
                            characterThis.StartFighting(characterSource);
                        if (characterSource.Fighting == null)
                            characterSource.StartFighting(characterThis);
                    }
                }
            }
        }

        public void RemovePeriodicAura(IPeriodicAura aura)
        {
            Log.Default.WriteLine(LogLevels.Info, "IEntity.RemovePeriodicAura: {0} {1}", DebugName, aura.Ability == null ? "<<??>>" : aura.Ability.Name);
            bool removed = _periodicAuras.Remove(aura);
            if (!removed)
                Log.Default.WriteLine(LogLevels.Warning, "IEntity.RemovePeriodicAura: Trying to remove unknown PeriodicAura");
            else
            {
                if (aura.Ability == null || (aura.Ability.Flags & AbilityFlags.AuraIsHidden) != AbilityFlags.AuraIsHidden)
                {
                    Send("{0} vanishes.", aura.Ability == null ? "Something" : aura.Ability.Name);
                    if (aura.Source != null && aura.Source != this && aura.Source is ICharacter characterSource)
                        characterSource.Act(ActOptions.ToCharacter, "{0} vanishes on {1}.", aura.Ability == null ? "Something" : aura.Ability.Name, this);
                }
                aura.ResetSource();
            }
        }

        public void AddAura(IAura aura, bool recompute)
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "IEntity.AddAura: {0} is not valid anymore", DebugName);
                return;
            }
            //IAura same = _auras.FirstOrDefault(x => ReferenceEquals(x.Ability, aura.Ability) && x.Modifier == aura.Modifier && x.Source == aura.Source);
            IAura same = _auras.FirstOrDefault(x => x.Ability == aura.Ability && x.Modifier == aura.Modifier && x.Source == aura.Source);
            if (same != null)
            {
                Log.Default.WriteLine(LogLevels.Info, "IEntity.AddAura: Refresh: {0} {1}| recompute: {2}", DebugName, aura.Ability == null ? "<<??>>" : aura.Ability.Name, recompute);
                same.Refresh(aura);
            }
            else
            {
                Log.Default.WriteLine(LogLevels.Info, "IEntity.AddAura: Add: {0} {1}| recompute: {2}", DebugName, aura.Ability == null ? "<<??>>" : aura.Ability.Name, recompute);
                _auras.Add(aura);
                if (aura.Ability == null || (aura.Ability.Flags & AbilityFlags.AuraIsHidden) != AbilityFlags.AuraIsHidden)
                    Send("You are now affected by {0}.", aura.Ability == null ? "Something" : aura.Ability.Name);
            }
            if (recompute)
                RecomputeAttributes();
        }

        public void RemoveAura(IAura aura, bool recompute)
        {
            Log.Default.WriteLine(LogLevels.Info, "IEntity.RemoveAura: {0} {1} | recompute: {2}", DebugName, aura.Ability == null ? "<<??>>" : aura.Ability.Name, recompute);
            bool removed = _auras.Remove(aura);
            if (!removed)
                Log.Default.WriteLine(LogLevels.Warning, "ICharacter.RemoveAura: Trying to remove unknown aura");
            else if (aura.Ability == null || (aura.Ability.Flags & AbilityFlags.AuraIsHidden) != AbilityFlags.AuraIsHidden)
                Send("{0} vanishes.", aura.Ability == null ? "Something" : aura.Ability.Name);
            if (recompute && removed)
                RecomputeAttributes();
        }

        public void RemoveAuras(Func<IAura, bool> filterFunc, bool recompute)
        {
            Log.Default.WriteLine(LogLevels.Info, "IEntity.RemoveAuras: {0} | recompute: {1}", DebugName, recompute);
            _auras.RemoveAll(x => filterFunc(x));
            if (recompute)
                RecomputeAttributes();
        }

        public virtual string RelativeDisplayName(ICharacter beholder, bool capitalizeFirstLetter = false)
        {
            return DisplayName; // no behavior by default
        }

        public virtual string RelativeDescription(ICharacter beholder)
        {
            return Description; // no behavior by default
        }

        // Overriden in inherited class
        public virtual void OnRemoved() // called before removing an item from the game
        {
            IsValid = false;
            // TODO: warn IncarnatedBy about removing
            IncarnatedBy?.StopIncarnating();
            IncarnatedBy = null;
        }

        #endregion
    }
}
