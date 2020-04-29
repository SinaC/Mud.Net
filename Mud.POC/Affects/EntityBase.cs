using System.Collections.Generic;

namespace Mud.POC.Affects
{
    public abstract class EntityBase : IEntity
    {
        private List<IAura> _auras;

        public EntityBase(string name)
        {
            _auras = new List<IAura>();
            IsValid = true;
            Name = name;
        }

        public bool IsValid { get; protected set; }

        public string Name { get; protected set; }

        public IEnumerable<IAura> Auras => _auras;

        public void AddAura(IAura aura)
        {
            _auras.Add(aura);
        }

        public abstract void Recompute();

        protected abstract void ResetAttributes();

        //protected static void ApplyAuras<TEntity, TAffect>(IEntity source, TEntity target)
        //    where TEntity: IEntity
        //    where TAffect: IAffect
        //{
        //    if (!source.IsValid || !target.IsValid)
        //        return;
        //    foreach (IAura aura in source.Auras.Where(x => x.IsValid))
        //    {
        //        foreach (TAffect affect in aura.Affects.OfType<TAffect>())
        //        {
        //            affect.Apply(target); // Apply method doesn't exist on IAffect
        //        }
        //    }
        //}
    }
}
