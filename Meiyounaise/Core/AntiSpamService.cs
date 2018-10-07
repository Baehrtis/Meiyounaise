using System;
using System.Collections.Generic;
using System.Threading;
using Discord.WebSocket;

namespace Meiyounaise.Core
{
    public class AntiSpamService
    {
        private static readonly Dictionary<User, DateTime> Users = new Dictionary<User, DateTime>();
        public static void RateLimitUser(SocketMessage m)
        {
            var user = new User(m.Author.Id);
            user.Timeout += ClientTimedOut;
            Users.TryAdd(user, DateTime.Now.AddMilliseconds(2500));
        }

        private static void ClientTimedOut(User sender)
        {
            Users.Remove(sender);

            sender.Timer.Dispose();
        }

        public static bool ContainsUser(ulong id)
        {
            foreach (var user in Users)
            {
                if (user.Key.Id==id)
                {
                    return true;
                }
            }
            return false;
        }

        public static TimeSpan GetTimeForUser(ulong id)
        {
            User temp=null;
            foreach (var user in Users)
            {
                if (user.Key.Id == id)
                {
                    temp = user.Key;
                }
            }

            if (temp == null)
            {
                return TimeSpan.Zero;
            }
            return Users[temp] - DateTime.Now;
        }
    }

    public class User
    {
        public ulong Id;
        public Timer Timer;
        public event Action<User> Timeout;

        public User(ulong id)
        {
            Id = id;
            Timer = new Timer(timeoutHandler,null,2500,0);
        }
        public void timeoutHandler(object data)
        {
            Timeout?.Invoke(this);
        }
    }
}