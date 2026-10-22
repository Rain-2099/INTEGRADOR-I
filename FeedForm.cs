using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NOVAGRIDX
{
    public class HeroPanel : Panel
    {
        public Image Banner { get; set; }

        public HeroPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Theme.Bg;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle r = ClientRectangle;
            if (r.Width < 2 || r.Height < 2) return;

            if (Banner != null)
            {
                Theme.DrawCover(g, Banner, r);
            }
            else
            {
                using (LinearGradientBrush br = new LinearGradientBrush(r, Color.FromArgb(10, 45, 65), Color.FromArgb(75, 15, 85), 0f))
                {
                    g.FillRectangle(br, r);
                }

                Theme.DrawGrid(g, r, Theme.Cyan);

                int i = 0;
                for (int x = 0; x < r.Width; x += 46)
                {
                    int h = 70 + ((x / 46 * 37) % 7) * 22;
                    Rectangle b = new Rectangle(x, r.Height - h, 40, h);
                    using (SolidBrush sb = new SolidBrush(Color.FromArgb(210, 10, 12, 20)))
                    {
                        g.FillRectangle(sb, b);
                    }
                    using (Pen p = new Pen(Theme.Accents[i % 4], 2f))
                    {
                        g.DrawLine(p, b.Left, b.Top, b.Right, b.Top);
                    }
                    i++;
                }
            }

            using (LinearGradientBrush shade = new LinearGradientBrush(r, Color.FromArgb(235, 15, 17, 23), Color.FromArgb(40, 15, 17, 23), 0f))
            {
                g.FillRectangle(shade, r);
            }

            Theme.DrawNeonBorder(g, Width, Height, Theme.Magenta);
        }
    }

    public class CarouselPanel : Panel
    {
        private readonly List<Post> posts;
        private int index;
        private Timer timer;

        public Action<Post> PostClicked;

        public CarouselPanel(List<Post> items)
        {
            posts = items;
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Theme.Card;
            Cursor = Cursors.Hand;

            timer = new Timer();
            timer.Interval = 4500;
            timer.Tick += delegate { Step(1); };
            if (posts.Count > 1) timer.Start();

            MouseEnter += delegate { timer.Stop(); };
            MouseLeave += delegate { if (posts.Count > 1) timer.Start(); };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && timer != null)
            {
                timer.Stop();
                timer.Dispose();
                timer = null;
            }

            base.Dispose(disposing);
        }

        private void Step(int direction)
        {
            if (posts.Count == 0) return;
            index = (index + direction + posts.Count) % posts.Count;
            Invalidate();
        }

        private Rectangle LeftArrow
        {
            get { return new Rectangle(12, Height / 2 - 20, 40, 40); }
        }

        private Rectangle RightArrow
        {
            get { return new Rectangle(Width - 52, Height / 2 - 20, 40, 40); }
        }

        private Rectangle DotRect(int i)
        {
            int total = posts.Count * 18;
            int x0 = Width - 18 - total;
            return new Rectangle(x0 + i * 18, Height - 32, 18, 18);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (posts.Count == 0) return;

            if (posts.Count > 1)
            {
                if (LeftArrow.Contains(e.Location))
                {
                    Step(-1);
                    return;
                }

                if (RightArrow.Contains(e.Location))
                {
                    Step(1);
                    return;
                }

                for (int i = 0; i < posts.Count; i++)
                {
                    if (DotRect(i).Contains(e.Location))
                    {
                        index = i;
                        Invalidate();
                        return;
                    }
                }
            }

            if (PostClicked != null) PostClicked(posts[index]);
        }

        private static void DrawText(Graphics g, string text, Font font, Brush brush, Rectangle area)
        {
            using (StringFormat sf = new StringFormat())
            {
                sf.Trimming = StringTrimming.EllipsisCharacter;
                sf.FormatFlags = StringFormatFlags.NoWrap;
                g.DrawString(text, font, brush, area, sf);
            }
        }

        private void DrawArrow(Graphics g, Rectangle r, string symbol)
        {
            using (SolidBrush fill = new SolidBrush(Color.FromArgb(170, 15, 17, 23)))
            {
                g.FillEllipse(fill, r);
            }
            using (Font f = Theme.Ui(15F, FontStyle.Bold))
            using (StringFormat sf = new StringFormat())
            {
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                g.DrawString(symbol, f, Brushes.White, r, sf);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle r = ClientRectangle;
            if (r.Width < 80 || r.Height < 120) return;

            if (posts.Count == 0)
            {
                Theme.DrawPlaceholder(g, r, Theme.Cyan);
                using (Font f = Theme.Ui(14F, FontStyle.Bold))
                using (StringFormat sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                    g.DrawString("No posts yet", f, Brushes.White, r, sf);
                }
                Theme.DrawNeonBorder(g, Width, Height, Theme.Cyan);
                return;
            }

            Post p = posts[index];
            Color accent = p.Accent;
            Image img = Theme.LoadImage(p.ImageRef);

            if (img != null) Theme.DrawCover(g, img, r);
            else Theme.DrawPlaceholder(g, r, accent);

            int shadeTop = r.Height * 40 / 100;
            Rectangle shade = new Rectangle(0, shadeTop, r.Width, r.Height - shadeTop);
            using (LinearGradientBrush br = new LinearGradientBrush(shade, Color.FromArgb(0, 10, 12, 20), Color.FromArgb(240, 10, 12, 20), 90f))
            {
                g.FillRectangle(br, shade);
            }

            Rectangle badge = new Rectangle(18, r.Height - 118, 150, 22);
            using (SolidBrush fill = new SolidBrush(accent))
            {
                g.FillRectangle(fill, badge);
            }
            using (SolidBrush dark = new SolidBrush(Theme.Bg))
            using (Font f = Theme.Ui(8.5F, FontStyle.Bold))
            using (StringFormat sf = new StringFormat())
            {
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                g.DrawString(p.IsRequest ? "LOOKING TO HIRE" : "ARTWORK FOR SALE", f, dark, badge, sf);
            }

            using (Font f = Theme.Ui(17F, FontStyle.Bold))
            {
                DrawText(g, p.Title, f, Brushes.White, new Rectangle(18, r.Height - 90, r.Width - 36, 30));
            }

            using (Font f = Theme.Ui(10F, FontStyle.Regular))
            using (SolidBrush soft = new SolidBrush(Color.FromArgb(205, 210, 225)))
            {
                DrawText(g, "@" + p.Author + "   |   " + p.Category, f, soft, new Rectangle(18, r.Height - 58, r.Width - 36, 20));
            }

            using (Font f = Theme.Ui(13F, FontStyle.Bold))
            using (SolidBrush accentBrush = new SolidBrush(accent))
            {
                DrawText(g, (p.IsRequest ? "Budget $" : "$") + p.Price.ToString("N2"), f, accentBrush, new Rectangle(18, r.Height - 34, 200, 24));
            }

            if (posts.Count > 1)
            {
                DrawArrow(g, LeftArrow, "<");
                DrawArrow(g, RightArrow, ">");

                for (int i = 0; i < posts.Count; i++)
                {
                    Rectangle d = DotRect(i);
                    Rectangle dot = new Rectangle(d.X + 5, d.Y + 5, 8, 8);
                    Color c = i == index ? accent : Color.FromArgb(120, 255, 255, 255);
                    using (SolidBrush fill = new SolidBrush(c))
                    {
                        g.FillEllipse(fill, dot);
                    }
                }
            }

            Theme.DrawNeonBorder(g, Width, Height, accent);
        }
    }

    public class FeedForm : Form
    {
        private const string HintText = "Search artworks, artists, requests...";

        private FlowLayoutPanel flow;
        private Panel header;
        private Panel toolbar;
        private Label navHome;
        private Label navExplore;
        private Label navMessages;
        private Label navProfile;
        private Label lblUser;
        private Button btnLogout;
        private Button btnGo;
        private Button btnCart;
        private Button btnNew;
        private Button chipAll;
        private Button chipArt;
        private Button chipReq;
        private TextBox txtSearch;
        private bool hintShown;
        private string view = "home";
        private string filter = "all";
        private string query = "";
        private readonly List<Control> wide = new List<Control>();

        public bool LogOut { get; private set; }

        public FeedForm()
        {
            Text = "NOVAGRIDX - Explore";
            Size = new Size(1100, 780);
            MinimumSize = new Size(1000, 640);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Theme.Bg;
            ForeColor = Color.White;
            Font = Theme.Ui(10F, FontStyle.Regular);

            flow = new FlowLayoutPanel();
            flow.Dock = DockStyle.Fill;
            flow.AutoScroll = true;
            flow.WrapContents = true;
            flow.FlowDirection = FlowDirection.LeftToRight;
            flow.Padding = new Padding(20);
            flow.BackColor = Theme.Bg;
            flow.Resize += delegate { FitWide(); };

            BuildHeader();
            BuildToolbar();

            Controls.Add(flow);
            Controls.Add(toolbar);
            Controls.Add(header);

            RefreshUser();
            UpdateCart();
            SetHint();
            ShowHome();
        }

        private Label NavLabel(string text, int x)
        {
            Label l = Theme.MakeLabel(text, x, 26, 90, 24, 10.5F, FontStyle.Bold, Color.White);
            l.Cursor = Cursors.Hand;
            return l;
        }

        private void BuildHeader()
        {
            header = new Panel();
            header.Dock = DockStyle.Top;
            header.Height = 70;
            header.BackColor = Theme.Card;

            Label logo = new Label();
            logo.Text = "NOVAGRIDX";
            logo.Font = Theme.Ui(20F, FontStyle.Bold);
            logo.ForeColor = Theme.Cyan;
            logo.BackColor = Color.Transparent;
            logo.AutoSize = true;
            logo.Location = new Point(25, 15);

            navHome = NavLabel("HOME", 270);
            navHome.Click += delegate
            {
                ClearSearch();
                ShowHome();
            };

            navExplore = NavLabel("EXPLORE", 370);
            navExplore.Click += delegate
            {
                ClearSearch();
                ShowExplore();
            };

            navMessages = NavLabel("MESSAGES", 470);
            navMessages.Click += delegate { OpenMessages(); };

            navProfile = NavLabel("PROFILE", 570);
            navProfile.Click += delegate { OpenProfile(); };

            lblUser = new Label();
            lblUser.Font = Theme.Ui(10.5F, FontStyle.Bold);
            lblUser.ForeColor = Theme.Magenta;
            lblUser.BackColor = Color.Transparent;
            lblUser.AutoSize = true;
            lblUser.Cursor = Cursors.Hand;
            lblUser.Click += delegate { OpenProfile(); };

            btnLogout = Theme.MakeButton("Log out", 0, 17, 110, 36, Theme.Cyan, false);
            btnLogout.Click += delegate
            {
                LogOut = true;
                Close();
            };

            header.Controls.Add(logo);
            header.Controls.Add(navHome);
            header.Controls.Add(navExplore);
            header.Controls.Add(navMessages);
            header.Controls.Add(navProfile);
            header.Controls.Add(lblUser);
            header.Controls.Add(btnLogout);

            header.Paint += delegate (object sender, PaintEventArgs pe)
            {
                using (Pen p = new Pen(Theme.Cyan, 2f))
                {
                    pe.Graphics.DrawLine(p, 0, header.Height - 1, header.Width, header.Height - 1);
                }
            };

            header.Resize += delegate
            {
                PositionBars();
                header.Invalidate();
            };
        }

        private void BuildToolbar()
        {
            toolbar = new Panel();
            toolbar.Dock = DockStyle.Top;
            toolbar.Height = 58;
            toolbar.BackColor = Theme.Bg;

            txtSearch = new TextBox();
            txtSearch.Location = new Point(25, 14);
            txtSearch.Size = new Size(290, 30);
            txtSearch.BackColor = Theme.Card;
            txtSearch.ForeColor = Color.White;
            txtSearch.BorderStyle = BorderStyle.FixedSingle;
            txtSearch.Font = Theme.Ui(11F, FontStyle.Regular);
            txtSearch.GotFocus += delegate
            {
                if (!hintShown) return;
                hintShown = false;
                txtSearch.Text = "";
                txtSearch.ForeColor = Color.White;
            };
            txtSearch.LostFocus += delegate
            {
                if (txtSearch.Text.Trim().Length == 0) SetHint();
            };
            txtSearch.KeyDown += delegate (object sender, KeyEventArgs ke)
            {
                if (ke.KeyCode != Keys.Enter) return;
                ke.SuppressKeyPress = true;
                DoSearch();
            };

            btnGo = Theme.MakeButton("GO", 322, 13, 56, 32, Theme.Cyan, true);
            btnGo.Click += delegate { DoSearch(); };

            chipAll = Theme.MakeButton("ALL", 398, 13, 60, 32, Theme.Magenta, false);
            chipAll.Click += delegate { SetFilter("all"); };

            chipArt = Theme.MakeButton("ARTWORKS", 466, 13, 104, 32, Theme.Cyan, false);
            chipArt.Click += delegate { SetFilter("art"); };

            chipReq = Theme.MakeButton("REQUESTS", 578, 13, 104, 32, Theme.Violet, false);
            chipReq.Click += delegate { SetFilter("req"); };

            btnCart = Theme.MakeButton("CART (0)", 0, 12, 110, 34, Theme.Cyan, false);
            btnCart.Click += delegate { OpenCart(); };

            btnNew = Theme.MakeButton("+ NEW POST", 0, 12, 140, 34, Theme.Magenta, true);
            btnNew.Click += delegate { OpenCreate(); };

            toolbar.Controls.Add(txtSearch);
            toolbar.Controls.Add(btnGo);
            toolbar.Controls.Add(chipAll);
            toolbar.Controls.Add(chipArt);
            toolbar.Controls.Add(chipReq);
            toolbar.Controls.Add(btnCart);
            toolbar.Controls.Add(btnNew);

            toolbar.Paint += delegate (object sender, PaintEventArgs pe)
            {
                using (Pen p = new Pen(Color.FromArgb(45, 50, 68), 1f))
                {
                    pe.Graphics.DrawLine(p, 0, toolbar.Height - 1, toolbar.Width, toolbar.Height - 1);
                }
            };

            toolbar.Resize += delegate
            {
                PositionBars();
                toolbar.Invalidate();
            };
        }

        private void PositionBars()
        {
            btnLogout.Location = new Point(header.Width - btnLogout.Width - 25, 17);
            lblUser.Location = new Point(btnLogout.Left - lblUser.Width - 20, 25);
            btnNew.Location = new Point(toolbar.Width - btnNew.Width - 25, 12);
            btnCart.Location = new Point(btnNew.Left - btnCart.Width - 10, 12);
        }

        private void SetHint()
        {
            hintShown = true;
            txtSearch.Text = HintText;
            txtSearch.ForeColor = Theme.Muted;
        }

        private void ClearSearch()
        {
            query = "";
            filter = "all";
            SetHint();
        }

        private void DoSearch()
        {
            query = hintShown ? "" : txtSearch.Text.Trim();
            ShowExplore();
        }

        private void SetFilter(string value)
        {
            filter = value;
            ShowExplore();
        }

        private void RefreshUser()
        {
            UserProfile u = UserStore.CurrentUser;
            lblUser.Text = "@" + u.Username + "  |  " + u.Role.ToUpper();
            PositionBars();
        }

        private void UpdateCart()
        {
            btnCart.Text = "CART (" + CartStore.Count(UserStore.CurrentUser.Username) + ")";
        }

        private void UpdateNav()
        {
            navHome.ForeColor = view == "home" ? Theme.Cyan : Color.White;
            navExplore.ForeColor = view == "explore" ? Theme.Cyan : Color.White;
        }

        private void UpdateChips()
        {
            bool explore = view == "explore";
            Theme.StyleButton(chipAll, Theme.Magenta, explore && filter == "all");
            Theme.StyleButton(chipArt, Theme.Cyan, explore && filter == "art");
            Theme.StyleButton(chipReq, Theme.Violet, explore && filter == "req");
        }

        private void OpenProfile()
        {
            using (ProfileForm profile = new ProfileForm())
            {
                profile.ShowDialog(this);
            }
            RefreshUser();
        }

        private void OpenMessages()
        {
            using (ChatForm chat = new ChatForm(null))
            {
                chat.ShowDialog(this);
            }
        }

        private void OpenCart()
        {
            using (CartForm cart = new CartForm())
            {
                cart.ShowDialog(this);
            }
            UpdateCart();
        }

        private void OpenCreate()
        {
            bool created;
            using (CreatePostForm form = new CreatePostForm())
            {
                created = form.ShowDialog(this) == DialogResult.OK;
            }

            if (created) ReloadView();
        }

        private void OpenPost(Post post)
        {
            bool deleted;
            using (DetalleForm detail = new DetalleForm(post))
            {
                detail.ShowDialog(this);
                deleted = detail.Deleted;
            }

            UpdateCart();
            if (deleted) BeginInvoke(new MethodInvoker(ReloadView));
        }

        private void ReloadView()
        {
            if (view == "home") ShowHome();
            else ShowExplore();
        }

        private void ClearFlow()
        {
            List<Control> old = new List<Control>();
            foreach (Control c in flow.Controls) old.Add(c);
            flow.Controls.Clear();
            foreach (Control c in old) c.Dispose();
            wide.Clear();
            flow.AutoScrollPosition = new Point(0, 0);
        }

        private void FitWide()
        {
            int w = Math.Max(400, flow.ClientSize.Width - flow.Padding.Horizontal - SystemInformation.VerticalScrollBarWidth - 24);
            foreach (Control c in wide) c.Width = w;
        }

        private void AddTitle(string text, Color color)
        {
            Label l = Theme.MakeLabel(text, 0, 0, 800, 34, 14F, FontStyle.Bold, color);
            l.Margin = new Padding(12, 10, 12, 6);
            l.TextAlign = ContentAlignment.MiddleLeft;
            flow.Controls.Add(l);
            flow.SetFlowBreak(l, true);
            wide.Add(l);
        }

        private void AddNote(string text)
        {
            Label l = Theme.MakeLabel(text, 0, 0, 800, 40, 11F, FontStyle.Regular, Theme.Muted);
            l.Margin = new Padding(12, 6, 12, 6);
            flow.Controls.Add(l);
            flow.SetFlowBreak(l, true);
            wide.Add(l);
        }

        private void AddCards(List<Post> source, bool requests, int max)
        {
            Control last = null;
            int count = 0;

            foreach (Post p in source)
            {
                if (p.IsRequest != requests) continue;
                if (count >= max) break;

                last = CreateCard(p);
                flow.Controls.Add(last);
                count++;
            }

            if (last != null) flow.SetFlowBreak(last, true);
            else AddNote(requests ? "No requests yet. Be the first to post one." : "No artwork yet. Be the first to post some.");
        }

        private List<Post> CarouselPosts(List<Post> all)
        {
            List<Post> art = new List<Post>();
            List<Post> req = new List<Post>();

            foreach (Post p in all)
            {
                if (p.IsRequest && req.Count < 3) req.Add(p);
                else if (!p.IsRequest && art.Count < 3) art.Add(p);
            }

            List<Post> result = new List<Post>();
            for (int i = 0; i < 3; i++)
            {
                if (i < art.Count) result.Add(art[i]);
                if (i < req.Count) result.Add(req[i]);
            }

            return result;
        }

        private HeroPanel BuildHero(List<Post> all)
        {
            HeroPanel hero = new HeroPanel();
            hero.Size = new Size(900, 330);
            hero.Margin = new Padding(12, 12, 12, 24);
            hero.Banner = Theme.LoadImage("banner");

            Label headline = Theme.MakeLabel("DISCOVER HUMAN" + Environment.NewLine + "CREATIVITY." + Environment.NewLine + "AUTHENTIC ART.", 40, 34, 420, 140, 24F, FontStyle.Bold, Color.White);
            Label sub = Theme.MakeLabel("Buy original digital art, or post a request and hire an artist.", 42, 184, 400, 44, 11F, FontStyle.Regular, Theme.Muted);

            Button start = Theme.MakeButton("START COLLECTING", 42, 244, 220, 44, Theme.Magenta, false);
            start.Click += delegate
            {
                filter = "all";
                query = "";
                SetHint();
                BeginInvoke(new MethodInvoker(ShowExplore));
            };

            CarouselPanel carousel = new CarouselPanel(CarouselPosts(all));
            carousel.Size = new Size(440, 290);
            carousel.Location = new Point(hero.Width - carousel.Width - 24, 20);
            carousel.PostClicked = OpenPost;

            hero.Resize += delegate { carousel.Location = new Point(hero.Width - carousel.Width - 24, 20); };

            hero.Controls.Add(headline);
            hero.Controls.Add(sub);
            hero.Controls.Add(start);
            hero.Controls.Add(carousel);

            return hero;
        }

        private void ShowHome()
        {
            view = "home";
            UpdateNav();
            UpdateChips();

            List<Post> all = PostStore.GetAll();

            flow.SuspendLayout();
            ClearFlow();

            HeroPanel hero = BuildHero(all);
            flow.Controls.Add(hero);
            flow.SetFlowBreak(hero, true);
            wide.Add(hero);

            AddTitle("LATEST ARTWORKS", Theme.Cyan);
            AddCards(all, false, 6);

            AddTitle("OPEN REQUESTS - BUYERS LOOKING TO HIRE", Theme.Violet);
            AddCards(all, true, 6);

            FitWide();
            flow.ResumeLayout(true);
        }

        private List<Post> Filtered()
        {
            List<Post> result = new List<Post>();
            string[] tokens = query.ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (Post p in PostStore.GetAll())
            {
                if (filter == "art" && p.IsRequest) continue;
                if (filter == "req" && !p.IsRequest) continue;

                string text = (p.Title + " " + p.Author + " " + p.Category + " " + p.Description).ToLower();
                bool match = true;

                foreach (string token in tokens)
                {
                    if (!text.Contains(token))
                    {
                        match = false;
                        break;
                    }
                }

                if (match) result.Add(p);
            }

            return result;
        }

        private void ShowExplore()
        {
            view = "explore";
            UpdateNav();
            UpdateChips();

            List<Post> results = Filtered();

            string name = "ALL POSTS";
            Color color = Theme.Magenta;

            if (filter == "art")
            {
                name = "ARTWORKS FOR SALE";
                color = Theme.Cyan;
            }
            else if (filter == "req")
            {
                name = "REQUESTS - BUYERS LOOKING TO HIRE";
                color = Theme.Violet;
            }

            string heading = query.Length > 0 ? name + "  -  RESULTS FOR \"" + query + "\"" : name;
            heading += "  (" + results.Count + ")";

            flow.SuspendLayout();
            ClearFlow();

            AddTitle(heading, color);

            if (results.Count == 0)
            {
                AddNote("Nothing found. Try another search or filter.");
            }
            else
            {
                foreach (Post p in results) flow.Controls.Add(CreateCard(p));
            }

            FitWide();
            flow.ResumeLayout(true);
        }

        private Panel CreateCard(Post post)
        {
            Color accent = post.Accent;

            NeonPanel card = new NeonPanel();
            card.Size = new Size(290, 340);
            card.Margin = new Padding(12);
            card.BorderColor = accent;
            card.PictureBounds = new Rectangle(3, 3, 284, 170);
            card.Picture = Theme.LoadImage(post.ImageRef);

            Label badge = Theme.MakeBadge(post.IsRequest ? "HIRING" : "ARTWORK", accent, 12, 12, post.IsRequest ? 70 : 84);
            Label category = Theme.MakeLabel(post.Category.ToUpper(), 16, 182, 258, 20, 8.5F, FontStyle.Bold, accent);
            Label title = Theme.MakeLabel(post.Title, 16, 203, 258, 30, 14F, FontStyle.Bold, Color.White);
            title.AutoEllipsis = true;
            Label author = Theme.MakeLabel((post.IsRequest ? "Posted by @" : "@") + post.Author, 16, 234, 258, 22, 10F, FontStyle.Regular, Theme.Muted);
            author.AutoEllipsis = true;
            Label price = Theme.MakeLabel((post.IsRequest ? "Budget $" : "$") + post.Price.ToString("N2"), 16, 256, 258, 30, 14F, FontStyle.Bold, accent);

            Button view = Theme.MakeButton(post.IsRequest ? "VIEW REQUEST" : "VIEW ARTWORK", 16, 292, 258, 38, accent, true);
            view.Click += delegate { OpenPost(post); };

            card.Controls.Add(badge);
            card.Controls.Add(category);
            card.Controls.Add(title);
            card.Controls.Add(author);
            card.Controls.Add(price);
            card.Controls.Add(view);

            return card;
        }
    }
}
