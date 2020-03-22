using System.Data;
using System.Threading.Tasks;
using Grains.Account;
using Npgsql;
using Dapper;

namespace Grains.Repositories
{
    public interface IAccountRepository
    {
        Task<AccountModel> GetAccount(long accountId);
        Task UpdateAccount(AccountModel account);
        Task CreateAccount(AccountModel account);
    }

    public class AccountRepository : IAccountRepository
    {
        private string _connectionString;
        private IDbConnection Connection
        {
            get
            {
                return new NpgsqlConnection(_connectionString);
            }
        }

        public AccountRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        
        public async Task<AccountModel> GetAccount(long accountId)
        {
            using (IDbConnection conn = Connection)
            {
                return await conn.QueryFirstOrDefaultAsync<AccountModel>(
                    @"
                        select * from account
                        where Id=@accountId
                    ",
                    new {  accountId });
            }
        }

        public async Task CreateAccount(AccountModel account)
        {
            using (IDbConnection conn = Connection)
            {
                await conn.ExecuteAsync(
                    @"
                        insert into Account(Id, Version, Balance, Modified) 
                        values (@Id, @Version, @Balance, @Modified)
                    ",
                    account);
            }
        }

        public async Task UpdateAccount(AccountModel account)
        {
            using (IDbConnection conn = Connection)
            {
                await conn.ExecuteAsync(
                    @"
                        update Account 
                        set Version=@Version, Balance=@Balance, Modified=@Modified
                        where Id=@Id
                    ",
                    account);
            }
        }
    }
}