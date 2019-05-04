using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using Dapper;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        public bool Verify(string accountId, string password, string otp)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://joey.com/")
            };
            var isLockedResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", accountId).Result;
            isLockedResponse.EnsureSuccessStatusCode();

            var isLock = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            if (isLock)
            {
                throw new FailedTooManyTimeException(accountId);
            }
            
            string passwordFromDB;
            using (var connection = new SqlConnection("my connection string"))
            {
                passwordFromDB = connection.Query<string>("spGetUserPassword", new
                                       {
                                           Id = accountId
                                       }, commandType: CommandType.StoredProcedure)
                                       .SingleOrDefault();
            }

            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            var hashPassword = hash.ToString();

            string currentOpt;
            var otpResponse = httpClient.PostAsJsonAsync("api/otps", accountId).Result;
            if (otpResponse.IsSuccessStatusCode)
            {
                currentOpt = otpResponse.Content.ReadAsAsync<string>().Result;
            }
            else
            {
                throw new Exception($"web api error, accountId:{accountId}");
            }

            if (passwordFromDB == hashPassword && otp == currentOpt)
            {
                var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
                resetResponse.EnsureSuccessStatusCode();

                return true;
            }
            else
            {
                var addFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", accountId).Result;
                addFailedCountResponse.EnsureSuccessStatusCode();
                
                var getFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;
                getFailedCountResponse.EnsureSuccessStatusCode();

                var failedCount = getFailedCountResponse.Content.ReadAsAsync<int>();

                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Info($"AccountId - {accountId}, Failed Count - {failedCount}");

                string message = $"{accountId} failed";
                var slackClient = new SlackClient("my api token");
                slackClient.PostMessage(repo =>
                {
                }, "my channel", message, "my bot name");

                return false;
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