using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Meiyounaise.Core;
namespace Meiyounaise.Core
{
    public static class ReminderStorage
    {
        private static JsonSerializer _jSerializer = new JsonSerializer();

        public static void InitializeLoader()
        {
            _jSerializer.Converters.Add(new JavaScriptDateTimeConverter());
            _jSerializer.NullValueHandling = NullValueHandling.Ignore;
        }

        public static void SaveReminders(ConcurrentDictionary<ulong, List<Reminder>> modlogsDict)
        {
            using (StreamWriter sw = File.CreateText(Utilities.DataPath + @"Reminders.json"))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    _jSerializer.Serialize(writer, modlogsDict);
                }
            }
        }

        public static ConcurrentDictionary<ulong, List<Reminder>> LoadReminders()
        {
            if (File.Exists(Utilities.DataPath + "Reminders.json"))
            {
                using (StreamReader sr = File.OpenText(Utilities.DataPath + @"Reminders.json"))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        var temp = _jSerializer.Deserialize<ConcurrentDictionary<ulong, List<Reminder>>>(reader);
                        return temp;
                    }
                }
            }
            else
            {
                File.Create("Reminders.json").Dispose();
            }
            return null;
        }
    }
}
