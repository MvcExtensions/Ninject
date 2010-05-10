#region Copyright
// Copyright (c) 2009 - 2010, Kazi Manzur Rashid <kazimanzurrashid@gmail.com>.
// This source is subject to the Microsoft Public License. 
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL. 
// All other rights reserved.
#endregion

namespace MvcExtensions.Ninject.Tests
{
    using Xunit;

    public class NinjectMvcApplicationTests
    {
        [Fact]
        public void Should_be_able_to_create_bootstrapper()
        {
            var application = new NinjectMvcApplication();

            Assert.NotNull(application.Bootstrapper);
            Assert.IsType<NinjectBootstrapper>(application.Bootstrapper);
        }
    }
}