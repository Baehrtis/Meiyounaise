using System.Collections.Generic;
using System.Linq;
using Discord.WebSocket;

namespace Meiyounaise.Core.Data
{
    public class UserAccounts
    {
        private static List<UserAccount> accounts;

        private static readonly string AccountsFile = Utilities.DataPath + "accounts.json";
        static UserAccounts()
        {
            if (DataStorage.SaveExists(AccountsFile))
            {
                accounts = DataStorage.LoadUsers(AccountsFile).ToList();
            }
            else
            {
                accounts = new List<UserAccount>();
                SaveAccounts();
            }
        }

        public static void SaveAccounts()
        {
            DataStorage.SaveUsers(accounts,AccountsFile);
        }

        public static UserAccount GetAccount(SocketUser user)
        {
            return GetOrCreateAccount(user.Id);
        }

        private static UserAccount GetOrCreateAccount(ulong id)
        {
            var result = from a in accounts
                where a.Id == id
                select a;
            var account = result.FirstOrDefault();
            if (account == null) account = CreateUserAccount(id);
            return account;
        }

        private static UserAccount CreateUserAccount(ulong id)
        {
            var newAccount = new UserAccount()
            {
                Id=id
            };
            accounts.Add(newAccount);
            SaveAccounts();
            return newAccount;
        }
    }
}
