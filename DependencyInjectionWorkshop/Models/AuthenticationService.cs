﻿using System;
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
            string dbPassword;
            using (var connection = new SqlConnection("my connection string"))
            {
                dbPassword = connection.Query<string>("spGetUserPassword", new
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
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://joey.com/")
            };
            var response = httpClient.PostAsJsonAsync("api/otps", accountId).Result;
            if (response.IsSuccessStatusCode)
            {
                currentOpt = response.Content.ReadAsAsync<string>().Result;
            }
            else
            {
                throw new Exception($"web api error, accountId:{accountId}");
            }

            if (dbPassword == hashPassword && otp == currentOpt)
            {
                return true;
            }
            else
            {
                var slackClient = new SlackClient("my api token");
                slackClient.PostMessage(repo =>
                {
                }, "my channel", "my message", "my bot name");

                return false;
            }
        }
    }
}