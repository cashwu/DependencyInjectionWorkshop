using DependencyInjectionWorkshop.Adapter;
using DependencyInjectionWorkshop.Repository;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IFailedCounter _failedCounter;
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IOtp _otpService;
        private readonly ILogger _logger;
        private readonly INotification _notification;

        public AuthenticationService(IFailedCounter failedCounter,
                                     IProfile profile,
                                     IHash hash,
                                     IOtp otpService,
                                     ILogger logger,
                                     INotification notification)
        {
            _failedCounter = failedCounter;
            _profile = profile;
            _hash = hash;
            _otpService = otpService;
            _logger = logger;
            _notification = notification;
        }

        // public AuthenticationService()
        // {
        //     _failedCounter = new FailedCounter();
        //     _profile = new ProfileRepo();
        //     _hash = new Sha256Adapter();
        //     _optService = new OptService();
        //     _logger = new NlogAdapter();
        //     _notification = new SlackAdapter();
        // }

        public bool Verify(string accountId, string password, string otp)
        {
            _failedCounter.CheckAccountIsLocked(accountId);

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

                _notification.PushMessage($"AccountId - {accountId} verify failed");

                return false;
            }
        }
    }
}