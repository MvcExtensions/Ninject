#region Copyright
// Copyright (c) 2009 - 2010, Kazi Manzur Rashid <kazimanzurrashid@gmail.com>.
// This source is subject to the Microsoft Public License. 
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL. 
// All other rights reserved.
#endregion

namespace MvcExtensions.Ninject
{
    using System;
    using System.Linq;
    using System.Web;
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;
    using IKernel = global::Ninject.IKernel;
    using IModule = global::Ninject.Modules.INinjectModule;
    using Kernel = global::Ninject.StandardKernel;

    /// <summary>
    /// Defines a <seealso cref="Bootstrapper">Bootstrapper</seealso> which is backed by <seealso cref="NinjectAdapter"/>.
    /// </summary>
    public class NinjectBootstrapper : Bootstrapper
    {
        private static readonly Type moduleType = typeof(IModule);

        /// <summary>
        /// Initializes a new instance of the <see cref="NinjectBootstrapper"/> class.
        /// </summary>
        /// <param name="buildManager">The build manager.</param>
        /// <param name="bootstrapperTasks">The bootstrapper tasks.</param>
        /// <param name="perRequestTasks">The per request tasks.</param>
        public NinjectBootstrapper(IBuildManager buildManager, IBootstrapperTasksRegistry bootstrapperTasks, IPerRequestTasksRegistry perRequestTasks) 
            : base(buildManager, bootstrapperTasks, perRequestTasks)
        {
        }

        /// <summary>
        /// Starts this bootstapper
        /// </summary>
        public static void Run()
        {
            Current = new NinjectBootstrapper(BuildManagerWrapper.Current, BootstrapperTasksRegistry.Current, PerRequestTasksRegistry.Current);
            DynamicModuleUtility.RegisterModule(typeof(BootstrapperModule));
        }

        /// <summary>
        /// Creates the container adapter.
        /// </summary>
        /// <returns></returns>
        protected override ContainerAdapter CreateAdapter()
        {
            IKernel kernel = new Kernel();

            kernel.Bind<HttpContextBase>().ToMethod(c => new HttpContextWrapper(HttpContext.Current)).InTransientScope();

            return new NinjectAdapter(kernel);
        }

        /// <summary>
        /// Loads the container specific modules.
        /// </summary>
        protected override void LoadModules()
        {
            IKernel kernel = ((NinjectAdapter)Adapter).Kernel;

            BuildManager.ConcreteTypes
                        .Where(type => moduleType.IsAssignableFrom(type) && type.HasDefaultConstructor())
                        .Except(kernel.GetModules().Select(c => c.GetType()))
                        .Each(type => kernel.Load(new[] { Activator.CreateInstance(type) as IModule }));
        }
    }
}