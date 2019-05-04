using DependencyInjectionWorkshop.Adapter;
using DependencyInjectionWorkshop.Repository;

namespace DependencyInjectionWorkshop.Models
{
    public interface IAuthenticationService
    {
        bool Verify(string accountId, string password, string otp);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly IFailedCounter _failedCounter;
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IOtp _otpService;
        private readonly ILogger _logger;

        public AuthenticationService(IFailedCounter failedCounter,
                                     IProfile profile,
                                     IHash hash,
                                     IOtp otpService,
                                     ILogger logger)
        {
            _failedCounter = failedCounter;
            _profile = profile;
            _hash = hash;
            _otpService = otpService;
            _logger = logger;
        }

        public bool Verify(string accountId, string password, string otp)
        {
            if (_failedCounter.CheckAccountIsLocked(accountId))
            {
                throw new FailedTooManyTimeException(accountId);
            }

            var passwordFromDB = _profile.GetPassword(accountId);

            var hashPassword = _hash.GetHash(password);

            var currentOpt = _otpService.GetCurrentOtp(accountId);

            if (passwordFromDB == hashPassword && otp == currentOpt)
            {
                _failedCounter.Reset(accountId);

                return true;
            }
            else
            {
                _failedCounter.Add(accountId);

                var failedCount = _failedCounter.Get(accountId);

                _logger.Info($"AccountId - {accountId}, Failed Count - {failedCount}");

               return false;
            }
        }
    }
}