using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace MyCalculator
{
    public static class Dependencies
    {
        public static Container CreateContainerBuilder()
        {
            var builder = new Container();
            builder.Options.DefaultScopedLifestyle = new ThreadScopedLifestyle();

            builder.Register<ICalculator, Calculator>(Lifestyle.Singleton);

            return builder;
        }
    }
}
