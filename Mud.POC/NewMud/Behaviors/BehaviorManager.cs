using System;
using System.Collections;
using System.Collections.Generic;

namespace Mud.POC.NewMud.Behaviors
{
    public class BehaviorManager : IEnumerable<Behavior>, IDisposable
    {
        public Thing Parent { get; private set; }

        private List<Behavior> ManagedBehaviors { get; set; }

        public BehaviorManager(Thing parent)
        {
            Parent = parent;
        }

        public void Add(Behavior newBehavior)
        {
            lock (ManagedBehaviors)
            {
                if (!ManagedBehaviors.Contains(newBehavior))
                {
                    ManagedBehaviors.Add(newBehavior);
                    newBehavior.Parent = Parent;
                    newBehavior.OnAddBehavior();
                }
            }
        }

        public void Remove(Behavior behavior)
        {
            lock (ManagedBehaviors)
            {
                if (ManagedBehaviors.Contains(behavior))
                {
                    ManagedBehaviors.Remove(behavior);
                    behavior.OnRemoveBehavior();
                    behavior.Parent = null;
                }
            }
        }

        #region IEnumerable

        public IEnumerator<Behavior> GetEnumerator()
        {
            return ManagedBehaviors.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            lock (ManagedBehaviors)
            {
                foreach (Behavior behavior in ManagedBehaviors)
                    behavior.Dispose();
                ManagedBehaviors.Clear();
            }
            Parent = null;
        }

        #endregion
    }
}
