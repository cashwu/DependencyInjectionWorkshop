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
        private const string DefaultAccountId = "cash";
        private const string DefaultHashPassword = "my hashed password";
        private const string DefaultOtp = "123456";
        private const string DefaultPassword = "pw";
        private const int DefaultFailedCount = 99;
        private IProfile _profile;
        private IOtp _otpService;
        private IHash _hash;
        private INotification _notification;
        private IFailedCounter _failedCounter;
        private ILogger _logger;
        private AuthenticationService _authenticationService;

        [SetUp]
        public void Setup()
        {
            _profile = Substitute.For<IProfile>();
            _otpService = Substitute.For<IOtp>();
            _hash = Substitute.For<IHash>();
            _notification = Substitute.For<INotification>();
            _failedCounter = Substitute.For<IFailedCounter>();
            _logger = Substitute.For<ILogger>();

            _authenticationService = new AuthenticationService(_failedCounter, _profile, _hash, _otpService, _logger, _notification);
        }

        [Test]
        public void is_valid()
        {
            var isValid = WhenValid();
            ShouldBeValid(isValid);
        }

        [Test]
        public void is_invalid_when_wrong_otp()
        {
            var isValid = WhenInvalid();
            ShouldBeInvalid(isValid);
        }

        [Test]
        public void notify_user_when_invalid()
        {
            WhenInvalid();
            ShouldNotifyUser();
        }

        [Test]
        public void log_account_failed_count_when_invalid()
        {
            GivenFailedCount(DefaultFailedCount);
            WhenInvalid();
            
            LogShouldContains(DefaultAccountId, DefaultFailedCount); 
        }

        private void LogShouldContains(string accountId, int failedCount)
        {
            _logger.Received(1).Info(Arg.Is<string>(m => m.Contains(accountId) && m.Contains(failedCount.ToString())));
        }

        private void GivenFailedCount(int failedCount)
        {
            _failedCounter.Get(DefaultAccountId).ReturnsForAnyArgs(failedCount);
        }

        private bool WhenValid()
        {
            GivenPassword(DefaultAccountId, DefaultHashPassword);
            GivenHash(DefaultPassword, DefaultHashPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);

            var isValid = WhenVerify(DefaultAccountId, DefaultPassword, DefaultOtp);
            return isValid;
        }

        private bool WhenInvalid()
        {
            GivenPassword(DefaultAccountId, DefaultHashPassword);
            GivenHash(DefaultPassword, DefaultHashPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);

            return WhenVerify(DefaultAccountId, DefaultPassword, "wrong otp");
        }

        private void ShouldNotifyUser()
        {
            _notification.Received(1).PushMessage(Arg.Any<string>());
        }

        private static void ShouldBeInvalid(bool isValid)
        {
            Assert.IsFalse(isValid);
        }

        private bool WhenVerify(string accountId, string password, string otp)
        {
            return _authenticationService.Verify(accountId, password, otp);
        }

        private void ShouldBeValid(bool isValid)
        {
            Assert.IsTrue(isValid);
        }

        private void GivenOtp(string accountId, string otp)
        {
            _otpService.GetCurrentOtp(accountId).ReturnsForAnyArgs(otp);
        }

        private void GivenHash(string password, string hashPassword)
        {
            _hash.GetHash(password).ReturnsForAnyArgs(hashPassword);
        }

        private void GivenPassword(string accountId, string password)
        {
            _profile.GetPassword(accountId).ReturnsForAnyArgs(password);
        }
    }
}