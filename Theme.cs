using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace NOVAGRIDX
{
    public static class Theme
    {
        public static readonly Color Bg = Color.FromArgb(15, 17, 23);
        public static readonly Color Card = Color.FromArgb(28, 31, 43);
        public static readonly Color Cyan = Color.FromArgb(0, 229, 255);
        public static readonly Color Magenta = Color.FromArgb(255, 64, 200);
        public static readonly Color Amber = Color.FromArgb(255, 170, 40);
        public static readonly Color Violet = Color.FromArgb(150, 90, 255);
        public static readonly Color Muted = Color.FromArgb(170, 175, 190);
        public static readonly Color Danger = Color.FromArgb(255, 80, 110);
        public static readonly Color[] Accents = { Cyan, Magenta, Amber, Violet };

        private static readonly Dictionary<string, Image> cache = new Dictionary<string, Image>();

        public static Font Ui(float size, FontStyle style)
        {
            return new Font("Segoe UI", size, style);
        }

        public static Color Mix(Color a, Color b, float t)
        {
            return Color.FromArgb((int)(a.R * t + b.R * (1 - t)), (int)(a.G * t + b.G * (1 - t)), (int)(a.B * t + b.B * (1 - t)));
        }

        public static Label MakeLabel(string text, int x, int y, int w, int h, float size, FontStyle style, Color color)
        {
            Label l = new Label();
            l.Text = text;
            l.Location = new Point(x, y);
            l.Size = new Size(w, h);
            l.AutoSize = false;
            l.Font = Ui(size, style);
            l.ForeColor = color;
            l.BackColor = Color.Transparent;
            return l;
        }

        public static Label MakeBadge(string text, Color color, int x, int y, int w)
        {
            Label l = new Label();
            l.Text = text;
            l.Location = new Point(x, y);
            l.Size = new Size(w, 22);
            l.AutoSize = false;
            l.TextAlign = ContentAlignment.MiddleCenter;
            l.Font = Ui(8.5F, FontStyle.Bold);
            l.BackColor = color;
            l.ForeColor = Bg;
            return l;
        }

        public static void StyleButton(Button b, Color color, bool filled)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.BorderColor = color;
            b.UseVisualStyleBackColor = false;
            b.BackColor = filled ? color : Card;
            b.ForeColor = filled ? Bg : color;
        }

        public static Button MakeButton(string text, int x, int y, int w, int h, Color color, bool filled)
        {
            Button b = new Button();
            b.Text = text;
            b.Location = new Point(x, y);
            b.Size = new Size(w, h);
            StyleButton(b, color, filled);
            b.Font = Ui(10.5F, FontStyle.Bold);
            b.Cursor = Cursors.Hand;
            return b;
        }

        private static Image Fit(Image img)
        {
            int max = 1400;
            if (img.Width <= max && img.Height <= max) return new Bitmap(img);

            float scale = Math.Min((float)max / img.Width, (float)max / img.Height);
            int w = Math.Max(1, (int)(img.Width * scale));
            int h = Math.Max(1, (int)(img.Height * scale));

            Bitmap bmp = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(img, 0, 0, w, h);
            }
            return bmp;
        }

        public static Image LoadImage(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            Image cached;
            if (cache.TryGetValue(name, out cached)) return cached;

            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
            string rel = name.Replace('/', Path.DirectorySeparatorChar);
            List<string> paths = new List<string>();

            if (Path.HasExtension(rel))
            {
                paths.Add(Path.Combine(dir, rel));
            }
            else
            {
                string[] exts = { ".jpg", ".jpeg", ".png" };
                foreach (string ext in exts) paths.Add(Path.Combine(dir, rel + ext));
            }

            foreach (string path in paths)
            {
                if (!File.Exists(path)) continue;

                try
                {
                    byte[] data = File.ReadAllBytes(path);
                    using (MemoryStream ms = new MemoryStream(data))
                    using (Image img = Image.FromStream(ms))
                    {
                        Image result = Fit(img);
                        cache[name] = result;
                        return result;
                    }
                }
                catch
                {
                }
            }

            return null;
        }

        public static void DrawCover(Graphics g, Image img, Rectangle r)
        {
            float scale = Math.Max((float)r.Width / img.Width, (float)r.Height / img.Height);
            float sw = r.Width / scale;
            float sh = r.Height / scale;
            float sx = (img.Width - sw) / 2f;
            float sy = (img.Height - sh) / 2f;
            g.DrawImage(img, r, sx, sy, sw, sh, GraphicsUnit.Pixel);
        }

        public static void DrawGrid(Graphics g, Rectangle r, Color accent)
        {
            using (Pen p = new Pen(Color.FromArgb(28, accent), 1f))
            {
                for (int x = r.Left; x < r.Right; x += 24) g.DrawLine(p, x, r.Top, x, r.Bottom);
                for (int y = r.Top; y < r.Bottom; y += 24) g.DrawLine(p, r.Left, y, r.Right, y);
            }
        }

        public static void DrawPlaceholder(Graphics g, Rectangle r, Color accent)
        {
            using (LinearGradientBrush br = new LinearGradientBrush(r, Mix(accent, Bg, 0.30f), Color.FromArgb(18, 20, 30), 45f))
            {
                g.FillRectangle(br, r);
            }
            DrawGrid(g, r, accent);
        }

        public static void DrawNeonBorder(Graphics g, int width, int height, Color color)
        {
            using (Pen glow = new Pen(Color.FromArgb(50, color), 6f))
            {
                g.DrawRectangle(glow, 2, 2, width - 5, height - 5);
            }
            using (Pen line = new Pen(color, 2f))
            {
                g.DrawRectangle(line, 1, 1, width - 3, height - 3);
            }
        }
    }

    public class NeonPanel : Panel
    {
        public Color BorderColor { get; set; }
        public Image Picture { get; set; }
        public Rectangle PictureBounds { get; set; }

        public NeonPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BorderColor = Theme.Cyan;
            BackColor = Theme.Card;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            if (PictureBounds.Width > 0 && PictureBounds.Height > 0)
            {
                if (Picture != null) Theme.DrawCover(g, Picture, PictureBounds);
                else Theme.DrawPlaceholder(g, PictureBounds, BorderColor);
            }

            Theme.DrawNeonBorder(g, Width, Height, BorderColor);
        }
    }

    public class RoleSelector : Panel
    {
        private readonly string[] roles = { "Artist", "Buyer", "Both" };
        private readonly Button[] buttons = new Button[3];
        private string role = "Buyer";

        public string Role
        {
            get { return role; }
            set
            {
                role = value;
                Apply();
            }
        }

        public RoleSelector()
        {
            Size = new Size(380, 44);
            BackColor = Theme.Bg;

            for (int i = 0; i < 3; i++)
            {
                int index = i;
                Button b = Theme.MakeButton(roles[i].ToUpper(), i * 130, 0, 120, 44, Theme.Cyan, false);
                b.Click += delegate { Role = roles[index]; };
                buttons[i] = b;
                Controls.Add(b);
            }

            Apply();
        }

        private void Apply()
        {
            for (int i = 0; i < 3; i++)
            {
                bool selected = roles[i] == role;
                buttons[i].BackColor = selected ? Theme.Cyan : Theme.Card;
                buttons[i].ForeColor = selected ? Theme.Bg : Theme.Muted;
                buttons[i].FlatAppearance.BorderColor = selected ? Theme.Cyan : Color.FromArgb(60, 65, 85);
            }
        }
    }
}
