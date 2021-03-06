using System.Text;

internal class Sha256Adapter
{
    public string GetHashPassword(string password)
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
}