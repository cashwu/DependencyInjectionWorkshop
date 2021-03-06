using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace DependencyInjectionWorkshop.Repository
{
    public class ProfileRepo : IProfile
    {
        public string GetPassword(string accountId)
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
    }
}