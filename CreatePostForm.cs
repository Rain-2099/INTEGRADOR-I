using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace NOVAGRIDX
{
    public class CreatePostForm : Form
    {
        private readonly string[] categories =
        {
            "Character Design", "Concept Art", "Illustration", "3D Render", "Pixel Art",
            "Landscape", "Abstract", "Generative", "Asset Pack", "Mascot", "Logo / Branding", "Other"
        };

        private string type;
        private string imagePath;
        private Button btnArt;
        private Button btnReq;
        private Button btnPublish;
        private TextBox txtTitle;
        private TextBox txtPrice;
        private TextBox txtDesc;
        private ComboBox cmbCategory;
        private Label lblPrice;
        private Label lblFile;
        private Label lblError;

        public CreatePostForm()
        {
            string role = UserStore.CurrentUser.Role;
            bool canArt = role == "Artist" || role == "Both";
            bool canReq = role == "Buyer" || role == "Both";

            Text = "NOVAGRIDX - New post";
            ClientSize = new Size(520, 660);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Theme.Bg;
            ForeColor = Color.White;
            Font = Theme.Ui(10F, FontStyle.Regular);

            Label title = Theme.MakeLabel("NEW POST", 30, 18, 460, 34, 18F, FontStyle.Bold, Color.White);

            btnArt = Theme.MakeButton("ARTWORK FOR SALE", 30, 62, 225, 40, Theme.Cyan, true);
            btnArt.Enabled = canArt;
            btnArt.Click += delegate { SetType(PostStore.TypeArtwork); };

            btnReq = Theme.MakeButton("LOOKING TO HIRE", 265, 62, 225, 40, Theme.Violet, false);
            btnReq.Enabled = canReq;
            btnReq.Click += delegate { SetType(PostStore.TypeRequest); };

            string noteText = "";
            if (!canArt) noteText = "Buyer accounts can post hiring requests. Switch to Artist or Both in Profile to sell art.";
            if (!canReq) noteText = "Artist accounts can post artwork. Switch to Buyer or Both in Profile to hire artists.";
            Label note = Theme.MakeLabel(noteText, 30, 106, 460, 18, 8.5F, FontStyle.Regular, Theme.Muted);

            Label lblTitle = Theme.MakeLabel("Title", 30, 134, 460, 20, 10F, FontStyle.Regular, Theme.Muted);
            txtTitle = Box(30, 156, 460);
            txtTitle.MaxLength = 60;

            Label lblCategory = Theme.MakeLabel("Category", 30, 196, 460, 20, 10F, FontStyle.Regular, Theme.Muted);
            cmbCategory = new ComboBox();
            cmbCategory.Location = new Point(30, 218);
            cmbCategory.Size = new Size(460, 30);
            cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCategory.FlatStyle = FlatStyle.Flat;
            cmbCategory.BackColor = Theme.Card;
            cmbCategory.ForeColor = Color.White;
            cmbCategory.Font = Theme.Ui(11F, FontStyle.Regular);
            cmbCategory.Items.AddRange(categories);

            lblPrice = Theme.MakeLabel("Price (USD)", 30, 258, 460, 20, 10F, FontStyle.Regular, Theme.Muted);
            txtPrice = Box(30, 280, 460);
            txtPrice.MaxLength = 10;

            Label lblDesc = Theme.MakeLabel("Description (max. 240 characters)", 30, 320, 460, 20, 10F, FontStyle.Regular, Theme.Muted);
            txtDesc = Box(30, 342, 460);
            txtDesc.Multiline = true;
            txtDesc.WordWrap = true;
            txtDesc.MaxLength = 240;
            txtDesc.Size = new Size(460, 86);

            Label lblImage = Theme.MakeLabel("Cover image (optional, JPG or PNG)", 30, 438, 460, 20, 10F, FontStyle.Regular, Theme.Muted);
            Button btnBrowse = Theme.MakeButton("BROWSE...", 30, 460, 150, 34, Theme.Magenta, false);
            btnBrowse.Click += delegate { BrowseImage(); };
            lblFile = Theme.MakeLabel("No image selected", 192, 468, 298, 22, 9.5F, FontStyle.Regular, Theme.Muted);
            lblFile.AutoEllipsis = true;

            lblError = Theme.MakeLabel("", 30, 502, 460, 34, 10F, FontStyle.Regular, Theme.Danger);

            btnPublish = Theme.MakeButton("PUBLISH", 30, 540, 460, 44, Theme.Cyan, true);
            btnPublish.Click += delegate { Publish(); };

            Button cancel = Theme.MakeButton("CANCEL", 30, 592, 460, 40, Theme.Muted, false);
            cancel.Click += delegate
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            Controls.Add(title);
            Controls.Add(btnArt);
            Controls.Add(btnReq);
            Controls.Add(note);
            Controls.Add(lblTitle);
            Controls.Add(txtTitle);
            Controls.Add(lblCategory);
            Controls.Add(cmbCategory);
            Controls.Add(lblPrice);
            Controls.Add(txtPrice);
            Controls.Add(lblDesc);
            Controls.Add(txtDesc);
            Controls.Add(lblImage);
            Controls.Add(btnBrowse);
            Controls.Add(lblFile);
            Controls.Add(lblError);
            Controls.Add(btnPublish);
            Controls.Add(cancel);

            SetType(canArt ? PostStore.TypeArtwork : PostStore.TypeRequest);
        }

        private TextBox Box(int x, int y, int w)
        {
            TextBox t = new TextBox();
            t.Location = new Point(x, y);
            t.Size = new Size(w, 30);
            t.BackColor = Theme.Card;
            t.ForeColor = Color.White;
            t.BorderStyle = BorderStyle.FixedSingle;
            t.Font = Theme.Ui(11F, FontStyle.Regular);
            return t;
        }

        private void SetType(string t)
        {
            type = t;
            bool request = t == PostStore.TypeRequest;
            Color accent = request ? Theme.Violet : Theme.Cyan;

            Theme.StyleButton(btnArt, Theme.Cyan, !request);
            Theme.StyleButton(btnReq, Theme.Violet, request);
            Theme.StyleButton(btnPublish, accent, true);

            btnPublish.Text = request ? "PUBLISH REQUEST" : "PUBLISH ARTWORK";
            lblPrice.Text = request ? "Budget (USD)" : "Price (USD)";
        }

        private void BrowseImage()
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "Images (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";
                dlg.Title = "Choose a cover image";

                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                try
                {
                    if (new FileInfo(dlg.FileName).Length > 15 * 1024 * 1024)
                    {
                        lblError.Text = "The image is too large (max. 15 MB).";
                        return;
                    }

                    using (Image test = Image.FromFile(dlg.FileName))
                    {
                    }
                }
                catch (Exception)
                {
                    lblError.Text = "That file is not a valid image.";
                    return;
                }

                imagePath = dlg.FileName;
                lblFile.Text = Path.GetFileName(dlg.FileName);
                lblFile.ForeColor = Color.White;
                lblError.Text = "";
            }
        }

        private bool TryPrice(string text, out decimal price)
        {
            string clean = text.Replace("$", "").Trim();

            if (decimal.TryParse(clean, NumberStyles.Number, CultureInfo.CurrentCulture, out price)) return true;
            return decimal.TryParse(clean, NumberStyles.Number, CultureInfo.InvariantCulture, out price);
        }

        private void Publish()
        {
            string postTitle = UserStore.Clean(txtTitle.Text);
            string description = UserStore.Clean(txtDesc.Text);
            decimal price;

            if (postTitle.Length < 3)
            {
                lblError.Text = "The title must be at least 3 characters.";
                return;
            }

            if (cmbCategory.SelectedIndex < 0)
            {
                lblError.Text = "Choose a category.";
                return;
            }

            if (!TryPrice(txtPrice.Text, out price) || price <= 0m || price > 1000000m)
            {
                lblError.Text = "Enter a valid amount greater than 0.";
                return;
            }

            if (description.Length < 10)
            {
                lblError.Text = "Add a short description (min. 10 characters).";
                return;
            }

            string imageRef = "";

            if (imagePath != null)
            {
                try
                {
                    string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "posts");
                    Directory.CreateDirectory(dir);

                    string fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(imagePath).ToLower();
                    File.Copy(imagePath, Path.Combine(dir, fileName));
                    imageRef = "posts/" + fileName;
                }
                catch (Exception)
                {
                    lblError.Text = "Could not copy the image. Try another file.";
                    return;
                }
            }

            Post post = new Post
            {
                Id = Guid.NewGuid().ToString("N"),
                Author = UserStore.CurrentUser.Username,
                Type = type,
                Title = postTitle,
                Category = (string)cmbCategory.SelectedItem,
                Price = price,
                Description = description,
                ImageRef = imageRef,
                Created = DateTime.Now
            };

            if (!PostStore.Add(post))
            {
                lblError.Text = "Could not save the post. Try again.";
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
