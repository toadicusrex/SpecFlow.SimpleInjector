using BoDi;
using SimpleInjector;
using SpecFlow.SimpleInjector;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Infrastructure;
using TechTalk.SpecFlow.Plugins;
using TechTalk.SpecFlow.UnitTestProvider;

[assembly: RuntimePlugin(typeof(SimpleInjectorPlugin))]

namespace SpecFlow.SimpleInjector
{
    public class SimpleInjectorPlugin : IRuntimePlugin
    {
        private static object _registrationLock = new object();

        public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters)
        {
            runtimePluginEvents.CustomizeGlobalDependencies += (sender, args) =>
            {
                args.ObjectContainer.RegisterTypeAs<SimpleInjectorTestObjectResolver, ITestObjectResolver>();
                args.ObjectContainer.RegisterTypeAs<ContainerBuilderFinder, IContainerBuilderFinder>();
            };

            runtimePluginEvents.CustomizeScenarioDependencies += (sender, args) =>
            {
                args.ObjectContainer.RegisterFactoryAs<Container>(() =>
                {
                    var containerBuilderFinder = args.ObjectContainer.Resolve<IContainerBuilderFinder>();
                    var createScenarioContainerBuilder = containerBuilderFinder.GetCreateScenarioContainerBuilder();
                    var containerBuilder = createScenarioContainerBuilder();

                    return containerBuilder;
                });
            };
        }

        public void Initialize(
            RuntimePluginEvents runtimePluginEvents,
            RuntimePluginParameters runtimePluginParameters,
            UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            runtimePluginEvents.CustomizeGlobalDependencies += (sender, args) =>
            {
                // temporary fix for CustomizeGlobalDependencies called multiple times
                // see https://github.com/techtalk/SpecFlow/issues/948
                if (!args.ObjectContainer.IsRegistered<IContainerBuilderFinder>())
                {
                    // an extra lock to ensure that there are not two super fast threads re-registering the same stuff
                    lock (_registrationLock)
                    {
                        if (!args.ObjectContainer.IsRegistered<IContainerBuilderFinder>())
                        {
                            args.ObjectContainer.RegisterTypeAs<SimpleInjectorTestObjectResolver, ITestObjectResolver>();
                            args.ObjectContainer.RegisterTypeAs<ContainerBuilderFinder, IContainerBuilderFinder>();
                        }
                    }

                    // workaround for parallel execution issue - this should be rather a feature in BoDi?
                    args.ObjectContainer.Resolve<IContainerBuilderFinder>();
                }
            };

            runtimePluginEvents.CustomizeScenarioDependencies += (sender, args) =>
            {
                args.ObjectContainer.RegisterFactoryAs<Container>(() =>
                {
                    var containerBuilderFinder = args.ObjectContainer.Resolve<IContainerBuilderFinder>();
                    var createScenarioContainerBuilder = containerBuilderFinder.GetCreateScenarioContainerBuilder();
                    var container = createScenarioContainerBuilder();
                    RegisterSpecflowDependencies(args.ObjectContainer, container);
                    return container;
                });
            };
        }

        /// <summary>
        ///     Fix for https://github.com/gasparnagy/SpecFlow.Autofac/issues/11 Cannot resolve ScenarioInfo
        ///     Extracted from
        ///     https://github.com/techtalk/SpecFlow/blob/master/TechTalk.SpecFlow/Infrastructure/ITestObjectResolver.cs
        ///     The test objects might be dependent on particular SpecFlow infrastructure, therefore the implemented
        ///     resolution logic should support resolving the following objects (from the provided SpecFlow container):
        ///     <see cref="ScenarioContext" />, <see cref="FeatureContext" />, <see cref="TestThreadContext" /> and
        ///     <see cref="IObjectContainer" /> (to be able to resolve any other SpecFlow infrastructure). So basically
        ///     the resolution of these classes has to be forwarded to the original container.
        /// </summary>
        /// <param name="objectContainer">SpecFlow DI container.</param>
        /// <param name="container">SimpleInjector Container</param>
        private void RegisterSpecflowDependencies(
            IObjectContainer objectContainer,
            Container container)
        {
            container.Register(() => objectContainer, Lifestyle.Scoped);
            container.Register<ScenarioContext>(
                () =>
                {
                    var specflowContainer = container.GetInstance<IObjectContainer>();
                    var scenarioContext = specflowContainer.Resolve<ScenarioContext>();
                    return scenarioContext;
                },
                Lifestyle.Scoped);
            container.Register(
                () =>
                {
                    var specflowContainer = container.GetInstance<IObjectContainer>();
                    var scenarioContext = specflowContainer.Resolve<FeatureContext>();
                    return scenarioContext;
                },
                Lifestyle.Scoped);
            container.Register(
                () =>
                {
                    var specflowContainer = container.GetInstance<IObjectContainer>();
                    var scenarioContext = specflowContainer.Resolve<TestThreadContext>();
                    return scenarioContext;
                },
                Lifestyle.Scoped);
        }
    }
}
