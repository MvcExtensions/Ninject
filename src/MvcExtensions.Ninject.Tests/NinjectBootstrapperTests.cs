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
    using Ninject;

    using Moq;
    using Xunit;

    using IBindingNamedWithOrOnSyntax = global::Ninject.Syntax.IBindingNamedWithOrOnSyntax<object>;
    using IBindingToSyntax = global::Ninject.Syntax.IBindingToSyntax<object>;
    using IBindingWhenInNamedWithOrOnSyntax = global::Ninject.Syntax.IBindingWhenInNamedWithOrOnSyntax<object>;
    using IKernel = global::Ninject.IKernel;
    using IModule = global::Ninject.Modules.INinjectModule;
    using IContext = global::Ninject.Activation.IContext;
    using global::Ninject.Web.Common;

    public class NinjectBootstrapperTests
    {
        [Fact]
        public void Should_be_able_to_create_container()
        {
            var buildManager = new Mock<IBuildManager>();
            buildManager.SetupGet(bm => bm.Assemblies).Returns(new[] { GetType().Assembly });

            var bootstrapper = new NinjectBootstrapper(buildManager.Object, new Mock<IBootstrapperTasksRegistry>().Object, new Mock<IPerRequestTasksRegistry>().Object);

            Assert.IsType<NinjectAdapter>(bootstrapper.Adapter);
        }

        [Fact]
        public void Should_be_able_to_load_modules()
        {
            var buildManager = new Mock<IBuildManager>();
            buildManager.SetupGet(bm => bm.ConcreteTypes).Returns(new[] { typeof(DummyModule) });

            var bindingName = new Mock<IBindingNamedWithOrOnSyntax>();
            var bindingWhen = new Mock<IBindingWhenInNamedWithOrOnSyntax>();

            bindingWhen.Setup(b => b.InTransientScope()).Returns(bindingName.Object);
            bindingWhen.Setup(b => b.InSingletonScope()).Returns(bindingName.Object);

            var func = (Func<IContext, object>)Delegate.CreateDelegate(typeof(Func<IContext, object>), typeof(RequestScopeExtensionMethod).GetMethod("GetScope", BindingFlags.NonPublic | BindingFlags.Static));
            bindingWhen.Setup(b => b.InScope(func)).Returns(bindingName.Object).Verifiable();

            var bindingTo = new Mock<IBindingToSyntax>();
            bindingTo.Setup(b => b.To(It.IsAny<Type>())).Returns(bindingWhen.Object);
            bindingTo.Setup(b => b.ToConstant(It.IsAny<object>())).Returns(bindingWhen.Object);

            var kernel = new Mock<IKernel>();
            kernel.Setup(k => k.Bind(It.IsAny<Type>())).Returns(bindingTo.Object);

            kernel.Setup(k => k.Load(It.IsAny<IEnumerable<IModule>>())).Verifiable();

            var bootstrapper = new NinjectBootstrapperTestDouble(kernel.Object, buildManager.Object, new Mock<IBootstrapperTasksRegistry>().Object, new Mock<IPerRequestTasksRegistry>().Object);

            Assert.IsType<NinjectAdapter>(bootstrapper.Adapter);

            kernel.Verify();
        }

        private sealed class DummyModule : IModule
        {
            public void OnVerifyRequiredModules()
            {
                throw new NotImplementedException();
            }

            public string Name
            {
                get;
                private set;
            }

            public IKernel Kernel
            {
                get;
                private set;
            }

            public void OnLoad(IKernel kernel)
            {
            }

            public void OnUnload(IKernel kernel)
            {
            }
        }

        private sealed class NinjectBootstrapperTestDouble : NinjectBootstrapper
        {
            private readonly IKernel kernel;

            public NinjectBootstrapperTestDouble(IKernel kernel, IBuildManager buildManager, IBootstrapperTasksRegistry bootstrapperTasksRegistry, IPerRequestTasksRegistry perRequestTasksRegistry) : base(buildManager, bootstrapperTasksRegistry, perRequestTasksRegistry)
            {
                this.kernel = kernel;
            }

            protected override ContainerAdapter CreateAdapter()
            {
                return new NinjectAdapter(kernel);
            }
        }
    }
}