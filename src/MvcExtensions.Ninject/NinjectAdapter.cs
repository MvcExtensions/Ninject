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
    using System.Diagnostics;

    using Microsoft.Practices.ServiceLocation;
    using Extension = global::Ninject.ResolutionExtensions;
    using IKernel = global::Ninject.IKernel;

    /// <summary>
    /// Defines an adapter class which is backed by Ninject <seealso cref="IKernel">Kernel</seealso>.
    /// </summary>
    public class NinjectAdapter : ServiceLocatorImplBase, IServiceRegistrar, IServiceInjector, IDisposable
    {
        private bool disposed;

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
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="NinjectAdapter"/> is reclaimed by garbage collection.
        /// </summary>
        [DebuggerStepThrough]
        ~NinjectAdapter()
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets the kernel.
        /// </summary>
        /// <value>The kernel.</value>
        public IKernel Kernel
        {
            get;
            private set;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        [DebuggerStepThrough]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Registers the type.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="implementationType">Type of the implementation.</param>
        /// <param name="lifetime">The lifetime of the service.</param>
        /// <returns></returns>
        public virtual IServiceRegistrar RegisterType(string key, Type serviceType, Type implementationType, LifetimeType lifetime)
        {
            Invariant.IsNotNull(serviceType, "serviceType");
            Invariant.IsNotNull(implementationType, "implementationType");

            var bindingExpression = Kernel.Bind(serviceType).To(implementationType);

            var expression = (lifetime == LifetimeType.PerRequest) ?
                             bindingExpression.InRequestScope() : 
                             ((lifetime == LifetimeType.Singleton) ?
                             bindingExpression.InSingletonScope() :
                             bindingExpression.InTransientScope());

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
        public virtual IServiceRegistrar RegisterInstance(string key, Type serviceType, object instance)
        {
            Invariant.IsNotNull(serviceType, "serviceType");
            Invariant.IsNotNull(instance, "instance");

            var bindingExpression = Kernel.Bind(serviceType).ToConstant(instance);

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
        public virtual void Inject(object instance)
        {
            if (instance != null)
            {
                Kernel.Inject(instance);
            }
        }

        /// <summary>
        /// Gets the matching instance for the given type and key.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        protected override object DoGetInstance(Type serviceType, string key)
        {
            return Extension.Get(Kernel, serviceType, key);
        }

        /// <summary>
        /// Gets all the instances for the given type.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns></returns>
        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            return Extension.GetAll(Kernel, serviceType);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        [DebuggerStepThrough]
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed && disposing)
            {
                Kernel.Dispose();
            }

            disposed = true;
        }
    }
}