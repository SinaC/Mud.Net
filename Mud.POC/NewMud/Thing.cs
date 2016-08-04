using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mud.POC.NewMud.Behaviors;

namespace Mud.POC.NewMud
{
    public class Thing : IThing, IDisposable
    {
        #region IThing

        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        #endregion

        public Thing Parent { get; set; }

        public List<Thing> Children { get; set; }

        public BehaviorManager BehaviorManager { get; }

        public Thing(params Behavior[] behaviors)
        {
            Children = new List<Thing>();
            BehaviorManager = new BehaviorManager(this);

            Name = String.Empty;
            Description = String.Empty;

            if (behaviors != null)
                foreach(Behavior behavior in behaviors)
                    BehaviorManager.Add(behavior);
        }

        public void PerformAdd(Thing toAdd)
        {
            if (!Children.Contains(toAdd))
                Children.Add(toAdd);
            toAdd.Parent = this;
        }

        public void PerformRemove(Thing toRemove)
        {
            if (Children.Contains(toRemove))
                Children.Remove(toRemove);
            toRemove.Parent = null;
        }

        #region IDisposable

        public void Dispose()
        {
            // TODO: Unregister from all things we subscribed to (just the current parent, individual behaviors may differ).
            // TODO: Dispose all our Children and Behaviors too (things should not be disposed lightly).

            BehaviorManager.Dispose();

            foreach(Thing child in Children)
                child.Dispose();
        }

        #endregion
    }
}
