using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Discord.WebSocket;

namespace Meiyounaise.Core.Data
{
    public class UserAccounts
    {
        private static List<UserAccount> accounts;

        private static string accountsFile = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).Replace(@"bin\Debug\netcoreapp2.1", @"Data\accounts.json");
        static UserAccounts()
        {
            if (DataStorage.SaveExists(accountsFile))
            {
                accounts = DataStorage.LoadUsers(accountsFile).ToList();
            }
            else
            {
                accounts = new List<UserAccount>();
                SaveAccounts();
            }

        }

        public static void SaveAccounts()
        {
            DataStorage.SaveUsers(accounts,accountsFile);
        }

        public static UserAccount GetAccount(SocketUser user)
        {
            return GetOrCreateAccount(user.Id);
        }

        private static UserAccount GetOrCreateAccount(ulong id)
        {
            var result = from a in accounts
                where a.ID == id
                select a;
            var account = result.FirstOrDefault();
            if (account == null) account = CreateUserAccount(id);
            return account;
        }

        private static UserAccount CreateUserAccount(ulong id)
        {
            var newAccount = new UserAccount()
            {
                ID=id
            };
            accounts.Add(newAccount);
            SaveAccounts();
            return newAccount;
        }
    }
}
