using DependencyInjectionWorkshop.Adapter;
using DependencyInjectionWorkshop.Models;
using DependencyInjectionWorkshop.Repository;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        [Test]
        public void is_valid()
        {
            var profile = Substitute.For<IProfile>();
            var otp = Substitute.For<IOtp>();
            var hash = Substitute.For<IHash>();
            var notification = Substitute.For<INotification>();
            var failedCounter = Substitute.For<IFailedCounter>();
            var logger = Substitute.For<ILogger>();

            var authenticationService = new AuthenticationService(failedCounter, profile, hash, otp, logger, notification);

            otp.GetCurrentOtp("cash").ReturnsForAnyArgs("123456");
            profile.GetPassword("cash").ReturnsForAnyArgs("my hashed password");
            hash.GetHash("pw").ReturnsForAnyArgs("my hashed password");

            var isValid = authenticationService.Verify("cash", "pw", "123456");

            Assert.IsTrue(isValid);
        }
    }
}