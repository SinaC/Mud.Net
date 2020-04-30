using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.Domain;

namespace Mud.POC.Affects
{
    public class Aura : IAura
    {
        private readonly List<IAffect> _affects;

        public Aura(IAbility ability, IEntity source, AuraFlags flags, int level, int pulseLeft, params IAffect[] affects)
        {
            IsValid = true;

            Ability = ability;
            Source = source;
            AuraFlags = flags;
            Level = level;
            PulseLeft = pulseLeft;
            _affects = (affects ?? Enumerable.Empty<IAffect>()).ToList();
        }

        public bool IsValid { get; private set; }

        public int Level { get; private set; }

        public int PulseLeft { get; private set; }

        public IAbility Ability { get; private set; }

        public IEntity Source { get; private set; }

        public AuraFlags AuraFlags { get; private set; }

        public IEnumerable<IAffect> Affects => _affects;

        public void Append(StringBuilder sb)
        {
            sb.AppendFormat("{0}(level {1}) duration {2} flags {3} source {4}", Ability?.Name ?? "???", Level, PulseLeft, AuraFlags, Source?.Name ?? "???");
            foreach (IAffect affect in Affects)
            {
                sb.Append("    ");
                affect.Append(sb);
                sb.AppendLine();
            }
        }

        public bool DecreasePulseLeft(int pulseCount)
        {
            throw new NotImplementedException();
        }

        public void OnRemoved()
        {
            IsValid = false;
        }

        public AuraData MapAuraData()
        {
            // TODO
            throw new NotImplementedException();
        }
    }
}
