namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly ProfileRepo _profileRepo = new ProfileRepo();
        private readonly Sha256Adapter _sha256Adapter = new Sha256Adapter();
        private readonly OptService _optService = new OptService();
        private readonly FailedCounter _failedCounter = new FailedCounter();
        private readonly NlogAdapter _nlogAdapter = new NlogAdapter();
        private readonly SlackAdapter _slackAdapter = new SlackAdapter();

        public bool Verify(string accountId, string password, string otp)
        {
            _failedCounter.CheckAccountIsLocked(accountId);

            var passwordFromDB = _profileRepo.GetPasswordFromDb(accountId);

            var hashPassword = _sha256Adapter.GetHashPassword(password);

            var currentOpt = _optService.GetCurrentOpt(accountId);

            if (passwordFromDB == hashPassword && otp == currentOpt)
            {
                _failedCounter.ResetFailedCounter(accountId);

                return true;
            }
            else
            {
                _failedCounter.AddFailedCounter(accountId);

                var failedCount = _failedCounter.GetFailedCount(accountId);

                _nlogAdapter.LogFailedCount($"AccountId - {accountId}, Failed Count - {failedCount}");

                _slackAdapter.Notify($"AccountId - {accountId} verify failed");

                return false;
            }
        }
    }
}