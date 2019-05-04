using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        public bool Verify(string accountId, string password, string otp)
        {
            CheckAccountIsLocked(accountId, new HttpClient
            {
                BaseAddress = new Uri("http://joey.com/")
            });

            var passwordFromDB = GetPasswordFromDb(accountId);

            var hashPassword = GetHashPassword(password);

            var currentOpt = GetCurrentOpt(accountId);

            if (passwordFromDB == hashPassword && otp == currentOpt)
            {
                ResetFailedCounter(accountId);

                return true;
            }
            else
            {
                AddFailedCounter(accountId);

                var failedCount = GetFailedCount(accountId);

                LogFailedCount($"AccountId - {accountId}, Failed Count - {failedCount}");

                Notify($"AccountId - {accountId} verify failed");

                return false;
            }
        }

        private static void Notify(string message)
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(repo =>
            {
            }, "my channel", message, "my bot name");
        }

        private static void LogFailedCount(string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }

        private static Task<int> GetFailedCount(string accountId)
        {
            var getFailedCountResponse = new HttpClient
            {
                BaseAddress = new Uri("http://joey.com/")
            }.PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;
            getFailedCountResponse.EnsureSuccessStatusCode();

            var failedCount = getFailedCountResponse.Content.ReadAsAsync<int>();
            return failedCount;
        }

        private static void AddFailedCounter(string accountId)
        {
            var addFailedCountResponse = new HttpClient
            {
                BaseAddress = new Uri("http://joey.com/")
            }.PostAsJsonAsync("api/failedCounter/Add", accountId).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        private static void ResetFailedCounter(string accountId)
        {
            var resetResponse = new HttpClient
            {
                BaseAddress = new Uri("http://joey.com/")
            }.PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        private static string GetCurrentOpt(string accountId)
        {
            string currentOpt;
            var otpResponse = new HttpClient
            {
                BaseAddress = new Uri("http://joey.com/")
            }.PostAsJsonAsync("api/otps", accountId).Result;
            if (otpResponse.IsSuccessStatusCode)
            {
                currentOpt = otpResponse.Content.ReadAsAsync<string>().Result;
            }
            else
            {
                throw new Exception($"web api error, accountId:{accountId}");
            }

            return currentOpt;
        }

        private static string GetHashPassword(string password)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            var hashPassword = hash.ToString();
            return hashPassword;
        }

        private static string GetPasswordFromDb(string accountId)
        {
            string passwordFromDB;
            using (var connection = new SqlConnection("my connection string"))
            {
                passwordFromDB = connection.Query<string>("spGetUserPassword", new
                                           {
                                               Id = accountId
                                           }, commandType: CommandType.StoredProcedure)
                                           .SingleOrDefault();
            }

            return passwordFromDB;
        }

        private static void CheckAccountIsLocked(string accountId, HttpClient httpClient)
        {
            var isLockedResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", accountId).Result;
            isLockedResponse.EnsureSuccessStatusCode();

            var isLock = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            if (isLock)
            {
                throw new FailedTooManyTimeException(accountId);
            }
        }
    }

    public class FailedTooManyTimeException : Exception
    {
        public FailedTooManyTimeException(string accountId)
        {
            
        }
    }
}