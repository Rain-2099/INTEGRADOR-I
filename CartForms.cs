using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace NOVAGRIDX
{
    public class CartForm : Form
    {
        private readonly string user;
        private readonly List<Post> items = new List<Post>();
        private Panel list;
        private Label lblTotal;
        private Button btnCheckout;

        public CartForm()
        {
            user = UserStore.CurrentUser.Username;

            Text = "NOVAGRIDX - Cart";
            ClientSize = new Size(660, 620);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Theme.Bg;
            ForeColor = Color.White;
            Font = Theme.Ui(10F, FontStyle.Regular);

            Label title = Theme.MakeLabel("YOUR CART", 30, 20, 400, 36, 20F, FontStyle.Bold, Theme.Cyan);

            list = new Panel();
            list.Location = new Point(30, 70);
            list.Size = new Size(600, 410);
            list.AutoScroll = true;
            list.BackColor = Theme.Bg;

            lblTotal = Theme.MakeLabel("", 30, 490, 600, 34, 16F, FontStyle.Bold, Color.White);
            lblTotal.TextAlign = ContentAlignment.MiddleRight;

            btnCheckout = Theme.MakeButton("CHECKOUT", 30, 536, 290, 46, Theme.Cyan, true);
            btnCheckout.Click += delegate { Checkout(); };

            Button close = Theme.MakeButton("CLOSE", 340, 536, 290, 46, Theme.Magenta, false);
            close.Click += delegate { Close(); };

            Controls.Add(title);
            Controls.Add(list);
            Controls.Add(lblTotal);
            Controls.Add(btnCheckout);
            Controls.Add(close);

            Rebuild();
        }

        private void Rebuild()
        {
            List<Control> old = new List<Control>();
            foreach (Control c in list.Controls) old.Add(c);
            list.Controls.Clear();
            foreach (Control c in old) c.Dispose();

            items.Clear();
            decimal total = 0m;

            foreach (string id in CartStore.GetIds(user))
            {
                Post p = PostStore.Find(id);
                if (p == null || p.IsRequest)
                {
                    CartStore.Remove(user, id);
                    continue;
                }

                items.Add(p);
            }

            int y = 0;
            foreach (Post p in items)
            {
                list.Controls.Add(BuildRow(p, y));
                y += 98;
                total += p.Price;
            }

            if (items.Count == 0)
            {
                list.Controls.Add(Theme.MakeLabel("Your cart is empty. Browse the feed and add some artwork.", 0, 20, 570, 30, 11F, FontStyle.Regular, Theme.Muted));
            }

            lblTotal.Text = "TOTAL  $" + total.ToString("N2");
            btnCheckout.Enabled = items.Count > 0;
        }

        private Control BuildRow(Post p, int y)
        {
            NeonPanel row = new NeonPanel();
            row.Location = new Point(0, y);
            row.Size = new Size(570, 88);
            row.BorderColor = p.Accent;
            row.PictureBounds = new Rectangle(3, 3, 120, 82);
            row.Picture = Theme.LoadImage(p.ImageRef);

            Label title = Theme.MakeLabel(p.Title, 136, 10, 300, 26, 12F, FontStyle.Bold, Color.White);
            title.AutoEllipsis = true;
            Label author = Theme.MakeLabel("@" + p.Author, 136, 38, 300, 20, 9.5F, FontStyle.Regular, Theme.Muted);
            Label category = Theme.MakeLabel(p.Category.ToUpper(), 136, 60, 300, 20, 8.5F, FontStyle.Bold, p.Accent);

            Label price = Theme.MakeLabel("$" + p.Price.ToString("N2"), 440, 12, 115, 30, 14F, FontStyle.Bold, p.Accent);
            price.TextAlign = ContentAlignment.MiddleRight;

            Button remove = Theme.MakeButton("REMOVE", 455, 50, 100, 28, Theme.Danger, false);
            remove.Font = Theme.Ui(9F, FontStyle.Bold);
            remove.Click += delegate
            {
                CartStore.Remove(user, p.Id);
                BeginInvoke(new MethodInvoker(Rebuild));
            };

            row.Controls.Add(title);
            row.Controls.Add(author);
            row.Controls.Add(category);
            row.Controls.Add(price);
            row.Controls.Add(remove);

            return row;
        }

        private void Checkout()
        {
            if (items.Count == 0) return;

            decimal total = 0m;
            foreach (Post p in items)
            {
                total += p.Price;
                ChatStore.Send(user, p.Author, "Hi! I just purchased \"" + p.Title + "\" ($" + p.Price.ToString("N2") + "). Looking forward to it!");
            }

            CartStore.Checkout(user, items);

            MessageBox.Show("Order placed. Total: $" + total.ToString("N2") + Environment.NewLine + "Each artist received a message in your chats.", "NOVAGRIDX", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
    }
}
