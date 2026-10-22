using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace NOVAGRIDX
{
    public class Post
    {
        public string Id { get; set; }
        public string Author { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string ImageRef { get; set; }
        public decimal Price { get; set; }
        public DateTime Created { get; set; }

        public bool IsRequest
        {
            get { return Type == PostStore.TypeRequest; }
        }

        public bool IsSeed
        {
            get { return Id != null && Id.StartsWith("seed-"); }
        }

        public Color Accent
        {
            get { return IsRequest ? Theme.Violet : Theme.Cyan; }
        }
    }

    internal static class FileDb
    {
        public static string PathOf(string name)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, name);
        }

        public static List<string> ReadLines(string path)
        {
            List<string> list = new List<string>();

            for (int attempt = 0; attempt < 3; attempt++)
            {
                try
                {
                    if (!File.Exists(path)) return list;

                    using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (line.Length > 0) list.Add(line);
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

        public static bool Append(string path, string line)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(line + Environment.NewLine);

            for (int attempt = 0; attempt < 3; attempt++)
            {
                try
                {
                    using (FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
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

        public static bool WriteAll(string path, List<string> lines)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string line in lines) sb.Append(line).Append(Environment.NewLine);
            byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());

            for (int attempt = 0; attempt < 3; attempt++)
            {
                try
                {
                    using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
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
    }

    public static class PostStore
    {
        public const string TypeArtwork = "Artwork";
        public const string TypeRequest = "Request";

        private static readonly string FilePath = FileDb.PathOf("posts.txt");

        private static string Encode(string text)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(text ?? ""));
        }

        private static string Decode(string text)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(text));
        }

        private static Post Seed(int number, string type, string title, string author, string category, decimal price, string image, string description)
        {
            return new Post
            {
                Id = "seed-" + number,
                Type = type,
                Title = title,
                Author = author,
                Category = category,
                Price = price,
                ImageRef = image,
                Description = description,
                Created = new DateTime(2026, 1, 1).AddDays(number)
            };
        }

        private static List<Post> BuildSeeds()
        {
            List<Post> list = new List<Post>();

            list.Add(Seed(1, TypeArtwork, "Neon Samurai", "KaiZero", "Character Design", 120.00m, "neon_samurai", "Digital illustration of a samurai walking through a neon-drenched megacity."));
            list.Add(Seed(2, TypeArtwork, "Chrome Dreams", "LyraVolt", "3D Render", 85.50m, "chrome_dreams", "A metallic landscape with futuristic reflections and cold light."));
            list.Add(Seed(3, TypeArtwork, "Ghost in the Grid", "N0va", "Generative", 200.00m, "ghost_in_the_grid", "Generative art inspired by neural networks and digital glitch."));
            list.Add(Seed(4, TypeArtwork, "Synth Alley", "PixelWraith", "Pixel Art", 60.00m, "synth_alley", "Pixel art of a cyberpunk alley under the rain."));
            list.Add(Seed(5, TypeArtwork, "Data Bloom", "Echo7", "Abstract", 150.75m, "data_bloom", "An abstract composition blending flowers and streams of data."));
            list.Add(Seed(6, TypeArtwork, "Night Runner", "Vex", "Concept Art", 95.00m, "night_runner", "Concept art of an underground courier on an aerial highway."));
            list.Add(Seed(7, TypeArtwork, "City Drift", "Mako", "Landscape", 110.00m, "city_drift", "A skyline at dusk with drifting neon signs and low fog."));
            list.Add(Seed(8, TypeArtwork, "Glitch Garden", "Sable", "Asset Pack", 75.00m, "glitch_garden", "A pack of glitch textures and neon brushes for digital painters."));

            list.Add(Seed(9, TypeRequest, "Mascot for my indie game", "NeonCollector", "Mascot", 150.00m, "", "I need a cute cyberpunk mascot with a full reference sheet for my pixel platformer."));
            list.Add(Seed(10, TypeRequest, "Synthwave album cover", "BeatRunner", "Illustration", 200.00m, "", "Looking for an artist to design the cover of my debut synthwave EP. Neon city vibes."));
            list.Add(Seed(11, TypeRequest, "Logo for a podcast", "GridCast", "Logo / Branding", 90.00m, "", "We need a bold cyan and magenta logo for a tech podcast about digital culture."));
            list.Add(Seed(12, TypeRequest, "Character sheet for my RPG", "DiceNomad", "Character Design", 120.00m, "", "Searching for a character designer to illustrate three heroes for a tabletop campaign."));

            return list;
        }

        private static List<Post> ReadFile()
        {
            List<Post> list = new List<Post>();

            foreach (string line in FileDb.ReadLines(FilePath))
            {
                string[] p = line.Split('|');
                if (p.Length != 9) continue;
                if (p[2] != TypeArtwork && p[2] != TypeRequest) continue;

                long ticks;
                decimal price;
                if (!long.TryParse(p[3], out ticks)) continue;
                if (ticks <= 0 || ticks > DateTime.MaxValue.Ticks) continue;
                if (!decimal.TryParse(p[4], NumberStyles.Number, CultureInfo.InvariantCulture, out price)) continue;

                string title;
                string category;
                string description;

                try
                {
                    title = Decode(p[6]);
                    category = Decode(p[7]);
                    description = Decode(p[8]);
                }
                catch (FormatException)
                {
                    continue;
                }

                list.Add(new Post
                {
                    Id = p[0],
                    Author = p[1],
                    Type = p[2],
                    Created = new DateTime(ticks),
                    Price = price,
                    ImageRef = p[5],
                    Title = title,
                    Category = category,
                    Description = description
                });
            }

            return list;
        }

        public static List<Post> GetAll()
        {
            List<Post> list = ReadFile();
            list.AddRange(BuildSeeds());
            list.Sort(delegate (Post a, Post b) { return b.Created.CompareTo(a.Created); });
            return list;
        }

        public static Post Find(string id)
        {
            foreach (Post p in GetAll())
            {
                if (p.Id == id) return p;
            }

            return null;
        }

        public static bool Add(Post post)
        {
            string line = string.Join("|", new string[]
            {
                post.Id,
                post.Author,
                post.Type,
                post.Created.Ticks.ToString(CultureInfo.InvariantCulture),
                post.Price.ToString("0.00", CultureInfo.InvariantCulture),
                post.ImageRef ?? "",
                Encode(post.Title),
                Encode(post.Category),
                Encode(post.Description)
            });

            return FileDb.Append(FilePath, line);
        }

        public static bool Delete(string id, string author)
        {
            List<string> kept = new List<string>();
            bool removed = false;

            foreach (string line in FileDb.ReadLines(FilePath))
            {
                string[] p = line.Split('|');
                if (p.Length == 9 && p[0] == id && string.Equals(p[1], author, StringComparison.OrdinalIgnoreCase))
                {
                    removed = true;
                    continue;
                }

                kept.Add(line);
            }

            if (!removed) return false;
            return FileDb.WriteAll(FilePath, kept);
        }
    }
}
