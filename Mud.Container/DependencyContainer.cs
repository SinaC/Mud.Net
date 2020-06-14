using System;

namespace Mud.Container
{
    public class DependencyContainer
    {
        private static readonly Lazy<SimpleInjector.Container> Lazy = new Lazy<SimpleInjector.Container>(() => new SimpleInjector.Container());

        public static SimpleInjector.Container Current => _manual ?? Lazy.Value;

        // TODO: remove this when every class will have ctor with used interface
        private static SimpleInjector.Container _manual;
        public static void SetManualContainer(SimpleInjector.Container container)
        {
            _manual = container;
        }
    }
}
