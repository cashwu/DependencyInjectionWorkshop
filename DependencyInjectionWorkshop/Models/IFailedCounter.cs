using System.Threading.Tasks;

public interface IFailedCounter
{
    void Reset(string accountId);
    void Add(string accountId);
    int Get(string accountId);
    bool CheckAccountIsLocked(string accountId);
}