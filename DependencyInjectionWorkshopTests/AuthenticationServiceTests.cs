﻿using DependencyInjectionWorkshop.Adapter;
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
        private IAuthenticationService _authentication;

        [SetUp]
        public void Setup()
        {
            _profile = Substitute.For<IProfile>();
            _otpService = Substitute.For<IOtp>();
            _hash = Substitute.For<IHash>();
            _notification = Substitute.For<INotification>();
            _failedCounter = Substitute.For<IFailedCounter>();
            _logger = Substitute.For<ILogger>();

            var authenticationService = new AuthenticationService(_failedCounter, _profile, _hash, _otpService, _logger);
            _authentication = new NotificationDecorator(authenticationService, _notification);
            
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

        [Test]
        public void reset_failed_count_when_valid()
        {
            WhenValid();
            ShouldResetFailedCounter();
        }

        [Test]
        public void add_failed_when_invalid()
        {
            WhenInvalid();
            ShouldAddFailedCount();
        }

        [Test]
        public void account_is_locked()
        {
            WhenAccountIsLock();
            ShouldThrowException();
        }

        private void WhenAccountIsLock()
        {
            _failedCounter.CheckAccountIsLocked(DefaultAccountId).ReturnsForAnyArgs(true);
        }

        private void ShouldThrowException()
        {
            TestDelegate action = () => _authentication.Verify(DefaultAccountId, DefaultPassword, DefaultOtp);
            Assert.Throws<FailedTooManyTimeException>(action);
        }

        private void ShouldResetFailedCounter()
        {
            _failedCounter.Received(1).Reset(Arg.Any<string>());
        }

        private void ShouldAddFailedCount()
        {
            _failedCounter.Received(1).Add(Arg.Any<string>());
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
            return _authentication.Verify(accountId, password, otp);
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