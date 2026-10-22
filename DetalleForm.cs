using System;
using System.Drawing;
using System.Windows.Forms;

namespace NOVAGRIDX
{
    public class DetalleForm : Form
    {
        public bool Deleted { get; private set; }

        public DetalleForm(Post post)
        {
            UserProfile me = UserStore.CurrentUser;
            bool own = string.Equals(me.Username, post.Author, StringComparison.OrdinalIgnoreCase);
            bool canBuy = me.Role == "Buyer" || me.Role == "Both";
            Color accent = post.Accent;

            Text = post.Title;
            ClientSize = new Size(540, 660);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Theme.Bg;
            ForeColor = Color.White;
            Font = Theme.Ui(10F, FontStyle.Regular);

            NeonPanel card = new NeonPanel();
            card.Location = new Point(20, 20);
            card.Size = new Size(500, 620);
            card.BorderColor = accent;
            card.PictureBounds = new Rectangle(3, 3, 494, 240);
            card.Picture = Theme.LoadImage(post.ImageRef);

            Label badge = Theme.MakeBadge(post.IsRequest ? "LOOKING TO HIRE" : "ARTWORK FOR SALE", accent, 16, 16, 150);
            Label category = Theme.MakeLabel(post.Category.ToUpper(), 20, 256, 460, 20, 9F, FontStyle.Bold, accent);
            Label title = Theme.MakeLabel(post.Title, 20, 278, 460, 40, 20F, FontStyle.Bold, Color.White);
            title.AutoEllipsis = true;
            Label author = Theme.MakeLabel((post.IsRequest ? "Posted by @" : "by @") + post.Author, 20, 322, 460, 24, 10.5F, FontStyle.Regular, Theme.Muted);
            Label description = Theme.MakeLabel(post.Description, 20, 356, 460, 84, 10.5F, FontStyle.Regular, Color.White);
            Label price = Theme.MakeLabel((post.IsRequest ? "BUDGET  $" : "$") + post.Price.ToString("N2"), 20, 450, 460, 44, 22F, FontStyle.Bold, accent);

            card.Controls.Add(badge);
            card.Controls.Add(category);
            card.Controls.Add(title);
            card.Controls.Add(author);
            card.Controls.Add(description);
            card.Controls.Add(price);

            if (!own)
            {
                if (post.IsRequest)
                {
                    Button proposal = Theme.MakeButton("SEND PROPOSAL", 20, 510, 460, 44, accent, true);
                    proposal.Click += delegate { OpenChat(post.Author); };
                    card.Controls.Add(proposal);
                }
                else
                {
                    Button cart = Theme.MakeButton("ADD TO CART", 20, 510, 225, 44, accent, true);
                    cart.Click += delegate { AddToCart(post, canBuy, cart); };

                    Button chat = Theme.MakeButton("CHAT WITH ARTIST", 255, 510, 225, 44, Theme.Magenta, false);
                    chat.Click += delegate { OpenChat(post.Author); };

                    card.Controls.Add(cart);
                    card.Controls.Add(chat);
                }
            }
            else if (!post.IsSeed)
            {
                Button delete = Theme.MakeButton("DELETE POST", 20, 510, 460, 44, Theme.Danger, false);
                delete.Click += delegate { DeletePost(post); };
                card.Controls.Add(delete);
            }

            Button close = Theme.MakeButton("CLOSE", 20, 564, 460, 44, Theme.Muted, false);
            close.Click += delegate { Close(); };
            card.Controls.Add(close);

            Controls.Add(card);
        }

        private void OpenChat(string artist)
        {
            using (ChatForm chat = new ChatForm(artist))
            {
                chat.ShowDialog(this);
            }
        }

        private void AddToCart(Post post, bool canBuy, Button button)
        {
            if (!canBuy)
            {
                MessageBox.Show("Switch your account type to Buyer or Both in your Profile to purchase artwork.", "NOVAGRIDX", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string user = UserStore.CurrentUser.Username;

            if (CartStore.Has(user, post.Id))
            {
                MessageBox.Show("This artwork is already in your cart.", "NOVAGRIDX", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (CartStore.Add(user, post.Id))
            {
                button.Text = "IN CART";
                MessageBox.Show("Added to your cart.", "NOVAGRIDX", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Could not update your cart. Try again.", "NOVAGRIDX", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DeletePost(Post post)
        {
            DialogResult answer = MessageBox.Show("Delete this post? This cannot be undone.", "NOVAGRIDX", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer != DialogResult.Yes) return;

            if (PostStore.Delete(post.Id, UserStore.CurrentUser.Username))
            {
                Deleted = true;
                Close();
            }
            else
            {
                MessageBox.Show("Could not delete the post. Try again.", "NOVAGRIDX", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
