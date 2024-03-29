﻿using System.Data;
using System.Threading.Tasks;
using Account.Grains.ReadModelWriter;
using Dapper;
using Npgsql;

namespace Account.Grains.Repositories
{
    public interface IAccountRepository
    {
        Task<AccountModel> GetAccount(long accountId);
        Task UpdateAccount(AccountModel account);
        Task CreateAccount(AccountModel account);
    }

    public class AccountRepository : IAccountRepository
    {
        private readonly string _connectionString;
        private IDbConnection Connection => new NpgsqlConnection(_connectionString);

        public AccountRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<AccountModel> GetAccount(long accountId)
        {
            using var conn = Connection;
            return await conn.QueryFirstOrDefaultAsync<AccountModel>(
                @"
                        select * from account
                        where Id=@accountId
                    ",
                new { accountId });
        }

        public async Task CreateAccount(AccountModel account)
        {
            using var conn = Connection;
            await conn.ExecuteAsync(
                @"
                        insert into Account(Id, Version, Balance, Modified) 
                        values (@Id, @Version, @Balance, @Modified)
                    ",
                account);
        }

        public async Task UpdateAccount(AccountModel account)
        {
            using var conn = Connection;
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
