using System;

namespace Mud.Container
{
    public class DependencyContainer
    {
        private static readonly Lazy<SimpleInjector.Container> Lazy = new Lazy<SimpleInjector.Container>(() => new SimpleInjector.Container());

        public static SimpleInjector.Container Instance => Lazy.Value;
    }
}
