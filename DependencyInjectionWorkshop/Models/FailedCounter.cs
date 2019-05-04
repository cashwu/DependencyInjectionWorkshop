using System;
using System.Net.Http;
using System.Threading.Tasks;
using DependencyInjectionWorkshop.Exceptions;
using DependencyInjectionWorkshop.Models;

public class FailedCounter : IFailedCounter
{
    public void Reset(string accountId)
    {
        var resetResponse = new HttpClient
                {
                    BaseAddress = new Uri("http://joey.com/")
                }.PostAsJsonAsync("api/failedCounter/Reset", accountId)
                 .Result;
        resetResponse.EnsureSuccessStatusCode();
    }

    public void Add(string accountId)
    {
        var addFailedCountResponse = new HttpClient
                {
                    BaseAddress = new Uri("http://joey.com/")
                }.PostAsJsonAsync("api/failedCounter/Add", accountId)
                 .Result;
        addFailedCountResponse.EnsureSuccessStatusCode();
    }

    public Task<int> Get(string accountId)
    {
        var getFailedCountResponse = new HttpClient
                {
                    BaseAddress = new Uri("http://joey.com/")
                }.PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId)
                 .Result;
        getFailedCountResponse.EnsureSuccessStatusCode();

        var failedCount = getFailedCountResponse.Content.ReadAsAsync<int>();
        return failedCount;
    }

    public void CheckAccountIsLocked(string accountId)
    {
        var isLockedResponse = new HttpClient
        {
            BaseAddress = new Uri("http://joey.com/")
        }.PostAsJsonAsync("api/failedCounter/IsLocked", accountId).Result;
        isLockedResponse.EnsureSuccessStatusCode();

        var isLock = isLockedResponse.Content.ReadAsAsync<bool>().Result;
        if (isLock)
        {
            throw new FailedTooManyTimeException(accountId);
        }
    }
}