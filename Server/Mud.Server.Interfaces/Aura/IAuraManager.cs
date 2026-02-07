using Mud.Domain.SerializationData.Avatar;
using Mud.Flags.Interfaces;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Entity;

namespace Mud.Server.Interfaces.Aura;

public interface IAuraManager
{
    IAura AddAura(IEntity target, string abilityName, IEntity source, int level, TimeSpan duration, IAuraFlags flags, bool recompute, params IAffect?[]? affects);
    IAura AddAura(IEntity target, string abilityName, IEntity source, int level, TimeSpan duration, bool recompute, params IAffect?[]? affects);
    IAura AddAura(IEntity target, string abilityName, IEntity source, int level, IAuraFlags flags, bool recompute, params IAffect?[]? affects);
    IAura AddAura(IEntity target, IEntity source, int level, IAuraFlags auraFlags, bool recompute, params IAffect?[]? affects);
    IAura AddAura(IEntity target, AuraData auraData, bool recompute);
}
