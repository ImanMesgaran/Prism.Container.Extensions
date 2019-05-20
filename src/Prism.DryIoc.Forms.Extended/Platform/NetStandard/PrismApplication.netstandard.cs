using System;
using System.Threading.Tasks;
using Prism.Behaviors;
using Prism.DryIoc.Extensions;
using Prism.Ioc;
using Prism.Logging;
using Xamarin.Forms.Internals;

namespace Prism.DryIoc
{
    public abstract partial class PrismApplication
    {
        public override void Initialize()
        {
            Logger = new ConsoleLoggingService();

            AppDomain.CurrentDomain.UnhandledException += AppDomain_UnhandledException;

            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            base.Initialize();

            if (Container.IsRegistered<IPageBehaviorFactoryOptions>())
            {
                ((IContainerRegistry)Container).RegisterSingleton<IPageBehaviorFactoryOptions, DefaultPageBehaviorFactoryOptions>();
            }

            Logger = Container.Resolve<ILogger>();
            Log.Listeners.Add(Container.Resolve<FormsLogListener>());
        }
    }
}