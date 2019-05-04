using System.Threading.Tasks;

public interface IFailedCounter
{
    void Reset(string accountId);
    void Add(string accountId);
    Task<int> Get(string accountId);
    void CheckAccountIsLocked(string accountId);
}