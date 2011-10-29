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
    
    using global::Ninject;
    using global::Ninject.Syntax;

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
        /// <param name="key">The key.</param>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="implementationType">Type of the implementation.</param>
        /// <param name="lifetime">The lifetime of the service.</param>
        /// <returns></returns>
        public override IServiceRegistrar RegisterType(string key, Type serviceType, Type implementationType, LifetimeType lifetime)
        {
            Invariant.IsNotNull(serviceType, "serviceType");
            Invariant.IsNotNull(implementationType, "implementationType");

            IBindingWhenInNamedWithOrOnSyntax<object> bindingExpression = Kernel.Bind(serviceType).To(implementationType);

            IBindingNamedWithOrOnSyntax<object> expression = (lifetime == LifetimeType.PerRequest)
                                                                 ? bindingExpression.InRequestScope()
                                                                 : ((lifetime == LifetimeType.Singleton)
                                                                        ? bindingExpression.InSingletonScope()
                                                                        : bindingExpression.InTransientScope());

            if (!string.IsNullOrEmpty(key))
            {
                expression.Named(key);
            }

            return this;
        }

        /// <summary>
        /// Registers the instance.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="instance">The instance.</param>
        /// <returns></returns>
        public override IServiceRegistrar RegisterInstance(string key, Type serviceType, object instance)
        {
            Invariant.IsNotNull(serviceType, "serviceType");
            Invariant.IsNotNull(instance, "instance");

            IBindingWhenInNamedWithOrOnSyntax<object> bindingExpression = Kernel.Bind(serviceType).ToConstant(instance);

            if (!string.IsNullOrEmpty(key))
            {
                bindingExpression.Named(key);
            }

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
        /// <param name="key">The key.</param>
        /// <returns></returns>
        protected override object DoGetService(Type serviceType, string key)
        {
            return Kernel.Get(serviceType, key);
        }

        /// <summary>
        /// Gets the services.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns></returns>
        protected override IEnumerable<object> DoGetServices(Type serviceType)
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