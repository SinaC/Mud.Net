using Mud.Domain;
using Mud.Domain.SerializationData.Avatar;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Entity;

namespace Mud.Server.Interfaces.Aura;

public interface IAuraManager
{
    IAura AddAura(IEntity target, string abilityName, IEntity source, int level, TimeSpan duration, AuraFlags flags, bool recompute, params IAffect?[]? affects);
    IAura AddAura(IEntity target, string abilityName, IEntity source, int level, AuraFlags flags, bool recompute, params IAffect?[]? affects);
    IAura AddAura(IEntity target, IEntity source, int level, AuraFlags auraFlags, bool recompute, params IAffect?[]? affects);
    IAura AddAura(IEntity target, AuraData auraData, bool recompute);
}
