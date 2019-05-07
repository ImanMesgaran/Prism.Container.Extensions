﻿using DryIoc;
using Prism.Ioc;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Prism.DryIoc.Extensions.Tests")]
namespace Prism.DryIoc
{
    public partial class ExtendedPrismDryIocContainer : IContainerExtension<IContainer>, IExtendedContainerRegistry
    {
        private static IContainerExtension<IContainer> _current;
        public static IContainerExtension<IContainer> Current
        {
            get
            {
                if(_current is null)
                {
                    _current = new ExtendedPrismDryIocContainer();
                }

                return _current;
            }
        }

        internal static void Reset()
        {
            if(_current != null && _current is ExtendedPrismDryIocContainer ext && !(ext.Instance?.IsDisposed ?? true))
            {
                ext.Instance.Dispose();
            }
            
            _current = null;
        }

        public static IContainerExtension Create(Rules rules) =>
            new ExtendedPrismDryIocContainer(rules);

        public static IContainerExtension Create(IContainer container) => 
            new ExtendedPrismDryIocContainer(container);

        public IContainer Instance { get; private set; }

        public ExtendedPrismDryIocContainer() 
            : this(CreateContainerRules())
        {
        }

        public ExtendedPrismDryIocContainer(Rules rules) 
            : this(new global::DryIoc.Container(rules))
        {
        }

        public ExtendedPrismDryIocContainer(IContainer container)
        {
            if(_current != null)
            {
                Trace.WriteLine($"{nameof(ExtendedPrismDryIocContainer)} has already been initialized. Note that you may lose any service registrations that have already been made");
            }

            _current = this;
            Instance = container;
            Splat.Locator.SetLocator(this);
        }

        private static Rules CreateContainerRules() => Rules.Default.WithAutoConcreteTypeResolution()
                                                                    .With(Made.Of(FactoryMethod.ConstructorWithResolvableArguments))
                                                                    .WithDefaultIfAlreadyRegistered(IfAlreadyRegistered.Replace);

        public void FinalizeExtension() { }

        public IContainerRegistry RegisterInstance(Type type, object instance)
        {
            Instance.UseInstance(type, instance);
            return this;
        }

        public IContainerRegistry RegisterInstance(Type type, object instance, string name)
        {
            Instance.UseInstance(type, instance, serviceKey: name);
            return this;
        }

        public IContainerRegistry RegisterSingleton(Type from, Type to)
        {
            Instance.Register(from, to, Reuse.Singleton);
            return this;
        }

        public IContainerRegistry RegisterSingleton(Type from, Type to, string name)
        {
            Instance.Register(from, to, Reuse.Singleton, serviceKey: name);
            return this;
        }

        public IContainerRegistry Register(Type from, Type to)
        {
            Instance.Register(from, to);
            return this;
        }

        public IContainerRegistry Register(Type from, Type to, string name)
        {
            Instance.Register(from, to, serviceKey: name);
            return this;
        }

        public IContainerRegistry RegisterMany(Type implementingType, params Type[] serviceTypes)
        {
            if(serviceTypes.Length == 0)
            {
                serviceTypes = implementingType.GetInterfaces();
            }

            Instance.RegisterMany(serviceTypes, implementingType, Reuse.Transient);
            return this;
        }

        public IContainerRegistry RegisterManySingleton(Type implementingType, params Type[] serviceTypes)
        {
            if (serviceTypes.Length == 0)
            {
                serviceTypes = implementingType.GetInterfaces();
            }

            Instance.RegisterMany(serviceTypes, implementingType, Reuse.Singleton);
            return this;
        }

        public object Resolve(Type type)
        {
            return Instance.Resolve(type);
        }

        public object Resolve(Type type, string name)
        {
            return Instance.Resolve(type, serviceKey: name);
        }

        public object Resolve(Type type, params (Type Type, object Instance)[] parameters)
        {
            return Instance.Resolve(type, args: parameters.Select(p => p.Instance).ToArray());
        }

        public object Resolve(Type type, string name, params (Type Type, object Instance)[] parameters)
        {
            return Instance.Resolve(type, name, args: parameters.Select(p => p.Instance).ToArray());
        }

        public bool IsRegistered(Type type)
        {
            return Instance.IsRegistered(type);
        }

        public bool IsRegistered(Type type, string name)
        {
            return Instance.IsRegistered(type, name);
        }

        public IContainerRegistry Register<T>(Func<T> factoryMethod)
        {
            Instance.Register(typeof(T), made: Made.Of(() => factoryMethod()));
            return this;
        }

        public IContainerRegistry Register<T>(Func<IContainerProvider, T> factoryMethod)
        {
            Instance.Register(typeof(T), made: Made.Of(() => factoryMethod(this)));
            return this;
        }

        public IContainerRegistry Register<T>(Func<IServiceProvider, T> factoryMethod)
        {
            Instance.Register(typeof(T), made: Made.Of(() => factoryMethod(this)));
            return this;
        }

        public IContainerRegistry Register(Type serviceType, Func<IServiceProvider, object> factoryMethod)
        {
            Instance.Register(serviceType, made: Made.Of(() => factoryMethod(this)));
            return this;
        }

        public IContainerRegistry RegisterSingleton<T>(Func<T> factoryMethod)
        {
            Instance.Register(typeof(T), Reuse.Singleton, made: Made.Of(() => factoryMethod.Invoke()));
            return this;
        }

        public IContainerRegistry RegisterSingleton<T>(Func<IContainerProvider, T> factoryMethod)
        {
            Instance.Register(typeof(T), Reuse.Singleton, made: Made.Of(() => factoryMethod(this)));
            return this;
        }

        public IContainerRegistry RegisterSingleton<T>(Func<IServiceProvider, T> factoryMethod)
        {
            Instance.Register(typeof(T), Reuse.Singleton, made: Made.Of(() => factoryMethod(this)));
            return this;
        }

        public object GetService(Type serviceType) => Resolve(serviceType);
    }
}
