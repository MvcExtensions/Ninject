#region Copyright
// Copyright (c) 2009 - 2010, Kazi Manzur Rashid <kazimanzurrashid@gmail.com>.
// This source is subject to the Microsoft Public License. 
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL. 
// All other rights reserved.
#endregion

namespace MvcExtensions.Ninject
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using global::Ninject;
    using global::Ninject.Parameters;
    using global::Ninject.Syntax;
    using global::Ninject.Web.Common;

    /// <summary>
    /// Defines an adapter class which is backed by Ninject <seealso cref="IKernel">Kernel</seealso>.
    /// </summary>
    public class NinjectAdapter : ContainerAdapter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NinjectAdapter"/> class.
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        public NinjectAdapter(IKernel kernel)
        {
            Invariant.IsNotNull(kernel, "kernel");

            Kernel = kernel;
        }

        /// <summary>
        /// Gets the kernel.
        /// </summary>
        /// <value>The kernel.</value>
        public IKernel Kernel { get; private set; }

        /// <summary>
        /// Registers the type.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="implementationType">Type of the implementation.</param>
        /// <param name="lifetime">The lifetime of the service.</param>
        /// <returns></returns>
        public override IServiceRegistrar RegisterType(Type serviceType, Type implementationType, LifetimeType lifetime)
        {
            Invariant.IsNotNull(serviceType, "serviceType");
            Invariant.IsNotNull(implementationType, "implementationType");

            IBindingWhenInNamedWithOrOnSyntax<object> expression = Kernel.Bind(serviceType).To(implementationType);

            switch (lifetime)
            {
                case LifetimeType.PerRequest:
                    expression.InRequestScope();
                    break;
                case LifetimeType.Singleton:
                    expression.InSingletonScope();
                    break;
                default:
                    expression.InTransientScope();
                    break;
            }

            return this;
        }

        /// <summary>
        /// Registers the instance.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="instance">The instance.</param>
        /// <returns></returns>
        public override IServiceRegistrar RegisterInstance(Type serviceType, object instance)
        {
            Invariant.IsNotNull(serviceType, "serviceType");
            Invariant.IsNotNull(instance, "instance");

            Kernel.Bind(serviceType).ToConstant(instance);

            return this;
        }

        /// <summary>
        /// Injects the matching dependences.
        /// </summary>
        /// <param name="instance">The instance.</param>
        public override void Inject(object instance)
        {
            if (instance != null)
            {
                Kernel.Inject(instance);
            }
        }

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns></returns>
        public override object GetService(Type serviceType)
        {
            var request = Kernel.CreateRequest(serviceType, null, new Parameter[0], true, true);
            return Kernel.Resolve(request).SingleOrDefault();
        }

        /// <summary>
        /// Gets the services.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns></returns>
        public override IEnumerable<object> GetServices(Type serviceType)
        {
            return Kernel.GetAll(serviceType);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        protected override void DisposeCore()
        {
            Kernel.Dispose();
        }
    }
}