using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace NOVAGRIDX
{
    public class ChatMessage
    {
        public string From { get; set; }
        public string To { get; set; }
        public DateTime Time { get; set; }
        public string Text { get; set; }
    }

    public static class ChatStore
    {
        private static readonly string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "chats.txt");

        private static bool Same(string a, string b)
        {
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }

        private static List<string[]> ReadLines()
        {
            List<string[]> list = new List<string[]>();

            for (int attempt = 0; attempt < 3; attempt++)
            {
                try
                {
                    if (!File.Exists(FilePath)) return list;

                    using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            string[] parts = line.Split('|');
                            if (parts.Length == 4) list.Add(parts);
                        }
                    }

                    return list;
                }
                catch (IOException)
                {
                    list.Clear();
                    Thread.Sleep(40);
                }
            }

            return list;
        }

        private static List<ChatMessage> ReadAll()
        {
            List<ChatMessage> list = new List<ChatMessage>();

            foreach (string[] p in ReadLines())
            {
                long ticks;
                if (!long.TryParse(p[2], out ticks)) continue;
                if (ticks <= 0 || ticks > DateTime.MaxValue.Ticks) continue;

                string text;
                try
                {
                    text = Encoding.UTF8.GetString(Convert.FromBase64String(p[3]));
                }
                catch (FormatException)
                {
                    continue;
                }

                list.Add(new ChatMessage { From = p[0], To = p[1], Time = new DateTime(ticks), Text = text });
            }

            return list;
        }

        public static bool Send(string from, string to, string text)
        {
            string line = from + "|" + to + "|" + DateTime.Now.Ticks + "|" + Convert.ToBase64String(Encoding.UTF8.GetBytes(text)) + Environment.NewLine;
            byte[] bytes = Encoding.UTF8.GetBytes(line);

            for (int attempt = 0; attempt < 3; attempt++)
            {
                try
                {
                    using (FileStream fs = new FileStream(FilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    {
                        fs.Write(bytes, 0, bytes.Length);
                    }

                    return true;
                }
                catch (IOException)
                {
                    Thread.Sleep(40);
                }
            }

            return false;
        }

        public static List<ChatMessage> GetConversation(string a, string b)
        {
            List<ChatMessage> result = new List<ChatMessage>();

            foreach (ChatMessage m in ReadAll())
            {
                if ((Same(m.From, a) && Same(m.To, b)) || (Same(m.From, b) && Same(m.To, a)))
                {
                    result.Add(m);
                }
            }

            result.Sort(delegate (ChatMessage x, ChatMessage y) { return x.Time.CompareTo(y.Time); });
            return result;
        }

        public static List<string> GetPeers(string user)
        {
            Dictionary<string, DateTime> last = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

            foreach (ChatMessage m in ReadAll())
            {
                string other = null;
                if (Same(m.From, user)) other = m.To;
                else if (Same(m.To, user)) other = m.From;
                if (other == null) continue;

                DateTime t;
                if (!last.TryGetValue(other, out t) || m.Time > t) last[other] = m.Time;
            }

            List<KeyValuePair<string, DateTime>> items = new List<KeyValuePair<string, DateTime>>(last);
            items.Sort(delegate (KeyValuePair<string, DateTime> x, KeyValuePair<string, DateTime> y) { return y.Value.CompareTo(x.Value); });

            List<string> result = new List<string>();
            foreach (KeyValuePair<string, DateTime> kv in items) result.Add(kv.Key);
            return result;
        }
    }
}
