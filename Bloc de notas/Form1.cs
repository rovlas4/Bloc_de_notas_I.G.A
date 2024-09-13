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

            emojis = new Dictionary<string, string>
            {
                { ":NOAUTORIZO:", @"C:\Users\ojeda\OneDrive\Imágenes\EMOGIS\GHSTCMjaUAARiMY.png" },
                { ":EZ:", @"C:\Users\ojeda\OneDrive\Imágenes\EMOGIS\st,small,507x507-pad,600x600,f8f8f8.u3.png" },
                { ":CINEMA:", @"C:\Users\ojeda\OneDrive\Imágenes\EMOGIS\1366_2000.png" },
                { ":xdd:", @"C:\Users\ojeda\OneDrive\Imágenes\EMOGIS\png-transparent-emoji-emote-emoticon-emoticons-xd-emoticons-icon.png" },
                { ":looking:", @"C:\Users\ojeda\OneDrive\Imágenes\EMOGIS\0f89b98b793bd2aedd16ad3484128f02.png" }
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
                try
                {
                    Image emojiImage = Image.FromFile(imagePath);

                    // Ajustar el tamaño de la imagen al tamaño del texto
                    int textHeight = (int)richTextBox.Font.GetHeight();
                    Image resizedImage = new Bitmap(emojiImage, new Size(textHeight, textHeight));

                    Clipboard.SetImage(resizedImage); // Poner la imagen redimensionada en el portapapeles
                    richTextBox.Paste();  // Reemplazar el texto con la imagen
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar la imagen: {ex.Message}");
                }

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
                RichTextBox richTextBox = (RichTextBox)tabControl1.SelectedTab.Controls[0];
                string fileContent = File.ReadAllText(opf.FileName);

                // Reemplazar los textos de los emojis con sus imágenes correspondientes
                richTextBox.Text = fileContent;
                foreach (var emoji in emojis)
                {
                    ReplaceTextWithImage(richTextBox, emoji.Key, emoji.Value);
                }

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

                // Reemplazar las imágenes con las palabras clave en el RichTextBox
                ReplaceImagesWithText(currentRichTextBox);

                // Guardar el texto con palabras clave en el archivo
                File.WriteAllText(sfd.FileName, currentRichTextBox.Text);

                // Actualizar la pestaña con el nombre del archivo guardado
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

            // Reemplazar directamente las imágenes en el RichTextBox con las palabras clave
            ReplaceImagesWithText(currentRichTextBox);

            // Guardar el texto actualizado con las palabras clave
            if (tabControl1.SelectedTab.Tag != null && !string.IsNullOrEmpty(tabControl1.SelectedTab.Tag.ToString()))
            {
                File.WriteAllText(tabControl1.SelectedTab.Tag.ToString(), currentRichTextBox.Text);
            }
            else
            {
                guardarComoToolStripMenuItem_Click(sender, e);
            }
        }

        private void ReplaceImagesWithText(RichTextBox richTextBox)
        {
            // Iteramos desde el final al principio para evitar desajustes en los índices
            for (int i = richTextBox.TextLength - 1; i >= 0; i--)
            {
                richTextBox.Select(i, 1); // Seleccionamos un carácter

                // Verificamos si es una imagen en el RTF
                if (richTextBox.SelectedRtf.Contains(@"\pict"))
                {
                    // Buscar la palabra clave correspondiente a la imagen
                    foreach (var emoji in emojis)
                    {
                        string imageRtf = GetImageRtf(emoji.Value); // Obtenemos el RTF de la imagen
                        if (richTextBox.SelectedRtf.Contains(imageRtf))
                        {
                            // Reemplazamos la imagen por la palabra clave
                            richTextBox.Select(i, 1);  // Seleccionamos la imagen
                            richTextBox.SelectedText = emoji.Key;  // Reemplazamos la imagen por el texto
                            break;
                        }
                    }
                }
            }
        }

        private void cerrarPestañaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl1.TabPages.RemoveAt(tabControl1.SelectedIndex);
        }

        // Método auxiliar para obtener el RTF de una imagen a partir de su ruta
        private string GetImageRtf(string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                MessageBox.Show($"El archivo de imagen no fue encontrado: {imagePath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

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

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}