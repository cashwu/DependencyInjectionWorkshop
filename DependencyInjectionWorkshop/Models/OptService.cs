using System;
using System.Net.Http;

internal class OptService
{
    public string GetCurrentOpt(string accountId)
    {
        string currentOpt;
        var otpResponse = new HttpClient
                {
                    BaseAddress = new Uri("http://joey.com/")
                }.PostAsJsonAsync("api/otps", accountId)
                 .Result;
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
}