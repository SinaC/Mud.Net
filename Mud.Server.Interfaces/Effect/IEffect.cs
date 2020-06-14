﻿using Mud.Server.Interfaces.Entity;

namespace Mud.Server.Interfaces.Effect
{
    public interface IEffect<in TEntity>
        where TEntity : IEntity
    {
        void Apply(TEntity target, IEntity source, string auraName, int level, int modifier);
    }
}
