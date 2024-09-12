using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Bloc_de_notas
{
    public partial class Form1 : Form
    {
        Dictionary<string, string> emojis;

        public Form1()
        {
            InitializeComponent();
            System.Windows.Forms.TextBox textBox = new System.Windows.Forms.TextBox();
            textBox.Dock = DockStyle.Fill;
            textBox.Font = new Font("Consolas", 14);
            textBox.Multiline = true;

            textBox.TextChanged += TextBox_TextChanged;

            tabControl1.TabPages[0].Controls.Add(textBox);
            tabControl1.TabPages[0].Tag = "";

            emojis = new Dictionary<string, string>
            {
                { "(°_°)", "💀" },
                { "<:P", "🥵" },
                { "B)", "😎" },
                { ":)", "🌚" },
                { ":chocolate:", "🍫" }
            };
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.TextBox textBox = (System.Windows.Forms.TextBox)sender;
            foreach (var emoji in emojis)
            {
                ReplaceTextWithImage(textBox, emoji.Key, emoji.Value);
            }
        }

        private void ReplaceTextWithImage(System.Windows.Forms.TextBox textBox, string emojiText, string emoji)
        {
            int index = textBox.Text.IndexOf(emojiText);
            while (index != -1)
            {
                textBox.Select(index, emojiText.Length);
                Clipboard.SetText(emoji);
                textBox.Paste();

                index = textBox.Text.IndexOf(emojiText, index + 1);
            }
        }

        private void abrirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog opf = new OpenFileDialog();
            opf.Filter = "Archivos de texto|*.txt";
            opf.Title = "Abrir archivo";

            if (opf.ShowDialog() == DialogResult.OK)
            {
                crearPestaña();
                ((System.Windows.Forms.TextBox)tabControl1.SelectedTab.Controls[0]).Text = File.ReadAllText(opf.FileName);
                tabControl1.SelectedTab.Tag = opf.FileName;
                tabControl1.SelectedTab.Text = Path.GetFileName(opf.FileName);
                archivoToolStripMenuItem.DropDownItems[2].Enabled = true;
            }
        }

        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void guardarComoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Archivos de texto|*.txt";
            sfd.Title = "Guardar archivo";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                System.Windows.Forms.TextBox currentTextBox = (System.Windows.Forms.TextBox)tabControl1.SelectedTab.Controls[0];

                string textWithEmoticons = ConvertImagesToText(currentTextBox);

                File.WriteAllText(sfd.FileName, textWithEmoticons);

                tabControl1.SelectedTab.Tag = sfd.FileName;
                tabControl1.SelectedTab.Text = Path.GetFileName(sfd.FileName);
                archivoToolStripMenuItem.DropDownItems[2].Enabled = true;
            }
        }


        private void nuevaPestañaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            crearPestaña();
        }

        private void crearPestaña()
        {
            TabPage nuevaPestana = new TabPage("Nueva Pestaña");
            nuevaPestana.Tag = "";
            tabControl1.TabPages.Add(nuevaPestana);

            System.Windows.Forms.TextBox newTextBox = new System.Windows.Forms.TextBox();
            newTextBox.Dock = DockStyle.Fill;
            newTextBox.Font = new Font("Consolas", 14);
            newTextBox.Multiline = true;
            newTextBox.TextChanged += TextBox_TextChanged;

            nuevaPestana.Controls.Add(newTextBox);

            tabControl1.SelectedTab = nuevaPestana;

            archivoToolStripMenuItem.DropDownItems[1].Enabled = true;
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab != null && tabControl1.SelectedTab.Tag.Equals(""))
            {
                archivoToolStripMenuItem.DropDownItems[2].Enabled = false;
                archivoToolStripMenuItem.DropDownItems[1].Enabled = true;
            }
            else if (tabControl1.SelectedTab != null)
            {
                archivoToolStripMenuItem.DropDownItems[2].Enabled = true;
            }
            else
            {
                archivoToolStripMenuItem.DropDownItems[1].Enabled = false;
                archivoToolStripMenuItem.DropDownItems[2].Enabled = false;
            }
        }

        private void guardarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.TextBox currentTextBox = (System.Windows.Forms.TextBox)tabControl1.SelectedTab.Controls[0];

            string textWithEmoticons = ConvertImagesToText(currentTextBox);

            File.WriteAllText(tabControl1.SelectedTab.Tag.ToString(), textWithEmoticons);
        }

        private void cerrarPestañaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl1.TabPages.RemoveAt(tabControl1.SelectedIndex);
        }

        private string ConvertImagesToText(System.Windows.Forms.TextBox textBox)
        {
            string content = textBox.Text;

            foreach (var emoji in emojis)
            {
                content = content.Replace(emoji.Value, emoji.Key);
            }

            return content;
        }
    }
}
