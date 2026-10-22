using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace NOVAGRIDX
{
    public class UserProfile
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Bio { get; set; }
    }

    public static class UserStore
    {
        private static readonly string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "users.txt");

        public static UserProfile CurrentUser { get; set; }

        private static string Hash(string text)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
                return BitConverter.ToString(bytes).Replace("-", "");
            }
        }

        public static string Clean(string text)
        {
            if (text == null) return "";
            return text.Replace("|", " ").Replace("\r", " ").Replace("\n", " ").Trim();
        }

        private static string ValidRole(string role)
        {
            if (role == "Artist" || role == "Buyer" || role == "Both") return role;
            return "Buyer";
        }

        private static List<string[]> ReadAll()
        {
            List<string[]> list = new List<string[]>();
            if (!File.Exists(FilePath)) return list;

            foreach (string line in File.ReadAllLines(FilePath))
            {
                string[] parts = line.Split('|');
                if (parts.Length != 5) continue;
                list.Add(parts);
            }

            return list;
        }

        private static void WriteAll(List<string[]> users)
        {
            List<string> lines = new List<string>();
            foreach (string[] u in users) lines.Add(string.Join("|", u));
            File.WriteAllLines(FilePath, lines.ToArray());
        }

        public static bool Register(string username, string email, string password, string role, out string error)
        {
            error = "";

            foreach (string[] u in ReadAll())
            {
                if (string.Equals(u[0], username, StringComparison.OrdinalIgnoreCase))
                {
                    error = "That username is already taken.";
                    return false;
                }

                if (string.Equals(u[1], email, StringComparison.OrdinalIgnoreCase))
                {
                    error = "That email is already registered.";
                    return false;
                }
            }

            string line = string.Join("|", new string[] { username, email, Hash(password), ValidRole(role), "" });
            File.AppendAllText(FilePath, line + Environment.NewLine);
            return true;
        }

        public static bool Validate(string identifier, string password, out UserProfile profile)
        {
            profile = null;
            string hash = Hash(password);

            foreach (string[] u in ReadAll())
            {
                bool idMatch = string.Equals(u[0], identifier, StringComparison.OrdinalIgnoreCase)
                            || string.Equals(u[1], identifier, StringComparison.OrdinalIgnoreCase);

                if (idMatch && u[2] == hash)
                {
                    profile = new UserProfile { Username = u[0], Email = u[1], Role = u[3], Bio = u[4] };
                    return true;
                }
            }

            return false;
        }

        public static bool UpdateProfile(string username, string role, string bio)
        {
            List<string[]> users = ReadAll();

            foreach (string[] u in users)
            {
                if (string.Equals(u[0], username, StringComparison.OrdinalIgnoreCase))
                {
                    u[3] = ValidRole(role);
                    u[4] = Clean(bio);
                    WriteAll(users);
                    return true;
                }
            }

            return false;
        }
    }
}
