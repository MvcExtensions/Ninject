#region Copyright
// Copyright (c) 2009 - 2010, Kazi Manzur Rashid <kazimanzurrashid@gmail.com>.
// This source is subject to the Microsoft Public License. 
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL. 
// All other rights reserved.
#endregion

namespace MvcExtensions.Ninject.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Web.Mvc;

    using Moq;
    using Xunit;
    using Xunit.Extensions;

    using IBindingMetadata = global::Ninject.Planning.Bindings.IBindingMetadata;
    using IBindingNamedWithOrOnSyntax = global::Ninject.Syntax.IBindingNamedWithOrOnSyntax<object>;
    using IBindingToSyntax = global::Ninject.Syntax.IBindingToSyntax<object>;
    using IBindingWhenInNamedWithOrOnSyntax = global::Ninject.Syntax.IBindingWhenInNamedWithOrOnSyntax<object>;
    using IKernel = global::Ninject.IKernel;
    using IParameter = global::Ninject.Parameters.IParameter;
    using IRequest = global::Ninject.Activation.IRequest;
    using IContext = global::Ninject.Activation.IContext;
    using global::Ninject.Web.Common;

    public class NinjectAdapterTests
    {
        private readonly Mock<IKernel> kernel;
        private readonly NinjectAdapter adapter;

        public NinjectAdapterTests()
        {
            kernel = new Mock<IKernel>();
            adapter = new NinjectAdapter(kernel.Object);
        }

        [Fact]
        public void Dispose_should_also_dispose_kernel()
        {
            kernel.Setup(c => c.Dispose());

            adapter.Dispose();

            kernel.VerifyAll();
        }

        [Theory]
        [InlineData(LifetimeType.Transient)]
        [InlineData(LifetimeType.Singleton)]
        [InlineData(LifetimeType.PerRequest)]
        public void Should_be_able_to_register(LifetimeType lifetime)
        {
            var bindingName = new Mock<IBindingNamedWithOrOnSyntax>();

            var bindingWhen = new Mock<IBindingWhenInNamedWithOrOnSyntax>();

            if (lifetime == LifetimeType.Transient)
            {
                bindingWhen.Setup(b => b.InTransientScope()).Returns(bindingName.Object).Verifiable();
            }
            else if (lifetime == LifetimeType.Transient)
            {
                bindingWhen.Setup(b => b.InSingletonScope()).Returns(bindingName.Object).Verifiable();
            }
            else if (lifetime == LifetimeType.PerRequest)
            {
                var func = (Func<IContext, object>)Delegate.CreateDelegate(typeof(Func<IContext, object>), typeof(RequestScopeExtensionMethod).GetMethod("GetScope", BindingFlags.NonPublic | BindingFlags.Static));
                bindingWhen.Setup(b => b.InScope(func)).Returns(bindingName.Object).Verifiable();
            }

            var bindingTo = new Mock<IBindingToSyntax>();

            bindingTo.Setup(b => b.To(It.IsAny<Type>())).Returns(bindingWhen.Object);
            kernel.Setup(k => k.Bind(It.IsAny<Type>())).Returns(bindingTo.Object);

            adapter.RegisterType(typeof(object), typeof(object), lifetime);

            bindingWhen.Verify();
        }

        [Fact]
        public void Should_be_able_to_register_instance()
        {
            var bindingTo = new Mock<IBindingToSyntax>();

            kernel.Setup(k => k.Bind(It.IsAny<Type>())).Returns(bindingTo.Object);

            adapter.RegisterInstance(typeof(DummyObject), new DummyObject());

            bindingTo.Verify();
        }

        [Fact]
        public void Should_be_able_to_inject()
        {
            var dummy = new DummyObject();

            kernel.Setup(c => c.Inject(It.IsAny<object>()));

            adapter.Inject(dummy);

            kernel.VerifyAll();
        }

        [Fact]
        public void Should_be_able_to_get_service_by_type()
        {
            SetupResolve();

            adapter.GetService<DummyObject>();

            kernel.VerifyAll();
        }

        [Fact]
        public void Should_be_able_to_get_services()
        {
            SetupResolve();

            adapter.GetServices(typeof(DummyObject));

            kernel.VerifyAll();
        }

        private void SetupResolve()
        {
            kernel.Setup(k => k.CreateRequest(It.IsAny<Type>(), It.IsAny<Func<IBindingMetadata, bool>>(), It.IsAny<IEnumerable<IParameter>>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(new Mock<IRequest>().Object);
            kernel.Setup(k => k.Resolve(It.IsAny<IRequest>())).Returns(new[] { new DummyObject() });
        }

        private class DummyObject
        {
        }
    }
}