using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bloc_de_notas
{
    public partial class Form1 : Form
    {
        Dictionary<string, string> emojis;

        public Form1()
        {
            InitializeComponent();
            RichTextBox richTextBox = new RichTextBox();
            richTextBox.Dock = DockStyle.Fill;
            richTextBox.Font = new Font("Consolas", 14);
            richTextBox.TextChanged += RichTextBox_TextChanged;

            tabControl1.TabPages[0].Controls.Add(richTextBox);
            tabControl1.TabPages[0].Tag = "";

            // Diccionario de emojis
            emojis = new Dictionary<string, string>
            {
                { ":NOAUTORIZO:", @"images\5.png" },
                { ":EZ:", @"images\1x.png" },
                { ":CINEMA:", @"images\2.png" },
                { ":xdd:", @"images\3.png" },
                { ":looking:", @"images\4.png" }
            };
        }

        private void RichTextBox_TextChanged(object sender, EventArgs e)
        {
            RichTextBox richTextBox = (RichTextBox)sender;
            foreach (var emoji in emojis)
            {
                ReplaceTextWithImage(richTextBox, emoji.Key, emoji.Value);
            }
        }

        private void ReplaceTextWithImage(RichTextBox richTextBox, string emojiText, string imagePath)
        {
            int index = richTextBox.Text.IndexOf(emojiText);
            while (index != -1)
            {
                richTextBox.Select(index, emojiText.Length);
                Clipboard.SetImage(Image.FromFile(imagePath)); // Carga la imagen desde el archivo
                richTextBox.Paste();  // Reemplaza el texto con la imagen

                index = richTextBox.Text.IndexOf(emojiText, index + 1);
            }
        }

        // El resto de tu código permanece igual
        private void abrirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog opf = new OpenFileDialog();
            opf.Filter = "Archivos de texto|*.txt|RTF|*.rtf";
            opf.Title = "Abrir archivo";

            if (opf.ShowDialog() == DialogResult.OK)
            {
                crearPestaña();
                ((RichTextBox)tabControl1.SelectedTab.Controls[0]).Text = File.ReadAllText(opf.FileName);
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
            sfd.Filter = "Archivos de texto (*.txt)|*.txt|RTF|*.rtf";
            sfd.Title = "Guardar archivo";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                RichTextBox currentRichTextBox = (RichTextBox)tabControl1.SelectedTab.Controls[0];

                // Convertir las imágenes a texto antes de guardar
                string textWithEmojis = ConvertImagesToText(currentRichTextBox);

                // Guardar el texto con emojis en el archivo
                File.WriteAllText(sfd.FileName, textWithEmojis);

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

            RichTextBox richTextBox = new RichTextBox();
            richTextBox.Dock = DockStyle.Fill;
            richTextBox.Font = new Font("Consolas", 14);
            richTextBox.TextChanged += RichTextBox_TextChanged;

            nuevaPestana.Controls.Add(richTextBox);

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
            RichTextBox currentRichTextBox = (RichTextBox)tabControl1.SelectedTab.Controls[0];

            // Convertir las imágenes a texto antes de guardar
            string textWithEmojis = ConvertImagesToText(currentRichTextBox);

            // Guardar el texto con emojis en el archivo
            File.WriteAllText(tabControl1.SelectedTab.Tag.ToString(), textWithEmojis);
        }

        private void cerrarPestañaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl1.TabPages.RemoveAt(tabControl1.SelectedIndex);
        }

        private string ConvertImagesToText(RichTextBox richTextBox)
        {
            // Convertimos el contenido de la RichTextBox a RTF
            string rtfContent = richTextBox.Rtf;

            foreach (var emoji in emojis)
            {
                // Buscamos el código RTF de la imagen correspondiente y lo reemplazamos con el texto del emoji
                string imageRtf = GetImageRtf(emoji.Value);
                if (!string.IsNullOrEmpty(imageRtf))
                {
                    rtfContent = rtfContent.Replace(imageRtf, emoji.Key);
                }
            }

            return rtfContent;
        }

        // Método auxiliar para obtener el RTF de una imagen a partir de su ruta
        private string GetImageRtf(string imagePath)
        {
            using (var img = Image.FromFile(imagePath))
            {
                string temp;
                Clipboard.SetImage(img);
                RichTextBox tempRichTextBox = new RichTextBox();
                tempRichTextBox.Font = new Font("Consolas", 14);
                tempRichTextBox.Paste();
                temp = tempRichTextBox.Rtf;

                temp = temp.Replace(@"{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang3082{\fonttbl{\f0\fnil\fcharset0 Consolas; } {\f1\fnil Consolas; } }" + Environment.NewLine + 
                                    @"{\*\generator Riched20 10.0.22621}\viewkind4\uc1" + Environment.NewLine +
                                    @"\pard\f0\fs29", "");
                temp = temp.Replace(@"\par" + Environment.NewLine +
                                    "}", "");
                temp = temp.Trim();
                return temp;
            }
        }

    }
}
