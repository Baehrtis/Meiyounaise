using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace Meiyounaise.Core
{
    public class RemindService
    {
        private ConcurrentDictionary<ulong, List<Reminder>> reminders;
        private DiscordSocketClient discordClient;
        private double DELAY = 60;
        private Timer _timer;

        public RemindService(DiscordSocketClient client)
        {
            discordClient = client;
            ReminderStorage.InitializeLoader();
            var loadedReminders = ReminderStorage.LoadReminders();
            if (loadedReminders != null)
            {
                reminders = loadedReminders;
                Task.Factory.StartNew(() => { InitializeTimers(); });
            }
        }

        public async Task InitializeTimers()
        {
            _timer = new Timer(async _ =>
             {
                 foreach (var user in reminders.ToArray())
                 {
                     List<Reminder> itemsToRemove = new List<Reminder>();
                     foreach (var reminder in user.Value)
                     {
                         if (reminder.TimeToRemind.CompareTo(DateTime.UtcNow) <= 0)
                         {
                             var userToRemind = discordClient.GetUser(user.Key);
                             await (await userToRemind.GetOrCreateDMChannelAsync()).SendMessageAsync($":alarm_clock: **Reminder:** {reminder.Content}");
                             itemsToRemove.Add(reminder);
                         }
                     }

                     foreach (var remove in itemsToRemove)
                     {
                         user.Value.Remove(remove);
                     }

                     reminders.TryUpdate(user.Key, user.Value, null);
                 }
                 ReminderStorage.SaveReminders(reminders);
             }, null, TimeSpan.FromSeconds(DELAY), TimeSpan.Zero);
        }

        public async Task SetReminder(SocketCommandContext Context, double time, string message)
        {
            try
            {
                List<Reminder> dataList = new List<Reminder>();
                if (reminders.ContainsKey(Context.User.Id))
                {
                    reminders.TryGetValue(Context.User.Id, out dataList);
                }
                
                Reminder data = new Reminder
                {
                    TimeToRemind = DateTime.UtcNow.AddSeconds(time),
                    Content = message
                };
                dataList.Add(data); //_punishLogs.AddOrUpdate(Context.Guild.Id, str, (key, oldValue) => str);
                reminders.AddOrUpdate(Context.User.Id, dataList,
                    (key, oldValue) => dataList);
                ChangeToClosestInterval();
                ReminderStorage.SaveReminders(reminders);
                await Context.Channel.SendMessageAsync($":white_check_mark: Successfully set Reminder. I will remind you to `{data.Content}` in `{time}`!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void ChangeToClosestInterval()
        {
            double timeToUpdate = Double.PositiveInfinity;
            foreach (var user in reminders)
            {
                foreach (var reminder in user.Value)
                {
                    var delta = reminder.TimeToRemind.Subtract(DateTime.UtcNow).TotalSeconds;
                    if (delta < 0)
                        delta = 0;
                    if (timeToUpdate > delta)
                        timeToUpdate = delta;
                }
            }

            if (Double.IsPositiveInfinity(timeToUpdate))
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                Console.WriteLine($"TIMER HAS BEEN HALTED!");
            }
            else
            {
                _timer.Change(TimeSpan.FromSeconds(timeToUpdate), TimeSpan.FromSeconds(timeToUpdate));
                Console.WriteLine($"CHANGED TIMER INTERVAL TO: {timeToUpdate}");
            }
        }

    }
    public class Reminder
    {
        public string Content { get; set; }
        public DateTime TimeToRemind { get; set; }
    }
}
