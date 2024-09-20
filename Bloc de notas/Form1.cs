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
        private Dictionary<TabPage, List<Tuple<int, string>>> imagenesInsertadasPorPestana = new Dictionary<TabPage, List<Tuple<int, string>>>();
        private Dictionary<TabPage, string> archivosAbiertos = new Dictionary<TabPage, string>();
        private Dictionary<TabPage, bool> cambiosNoGuardados = new Dictionary<TabPage, bool>();
        private bool isUpdatingText = false;

        Dictionary<string, string> emojis = new Dictionary<string, string>
            {
                { ":NOAUTORIZO:", @"images\5.png" },
                { ":EZ:", @"images\1x.png" },
                { ":CINEMA:", @"images\2.png" },
                { ":xdd:", @"images\3.png" },
                { ":looking:", @"images\4.png" }
            };


        public Form1()
        {
            InitializeComponent();
            RichTextBox richTextBox = new RichTextBox();
            richTextBox.Dock = DockStyle.Fill;
            richTextBox.Font = new Font("Consolas", 14);
            richTextBox.TextChanged += richTextBox1_TextChanged;

            tabControl1.TabPages[0].Controls.Add(richTextBox);
            tabControl1.TabPages[0].Tag = "";
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
                string filePath = opf.FileName;
                string fileName = Path.GetFileName(filePath);

                TabPage nuevaPestana = new TabPage(fileName);
                RichTextBox richTextBox = new RichTextBox { Dock = DockStyle.Fill };
                richTextBox.Font = new Font("Consolas", 14);
                richTextBox.TextChanged += richTextBox1_TextChanged;
                nuevaPestana.Controls.Add(richTextBox);
                tabControl1.TabPages.Add(nuevaPestana);
                tabControl1.SelectedTab = nuevaPestana;

                CargarTextoYReemplazarConImagenes(filePath, richTextBox);

                archivosAbiertos[nuevaPestana] = filePath;
                cambiosNoGuardados[nuevaPestana] = false;
                archivoToolStripMenuItem.DropDownItems[2].Enabled = true;
            }
        }
        private string textoOriginal;
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            RichTextBox richTextBox = sender as RichTextBox;
            if (richTextBox != null && !isUpdatingText)
            {
                if (textoOriginal != richTextBox.Text)
                {
                    isUpdatingText = true;
                    int caretPosition = richTextBox.SelectionStart;
                    //ActualizarListaEmojis(richTextBox.Text, caretPosition);
                    ReemplazarTextoConEmojis();
                    TabPage pestanaActual = tabControl1.SelectedTab;
                    if (pestanaActual != null)
                    {
                        cambiosNoGuardados[pestanaActual] = true;
                    }
                    richTextBox.SelectionStart = caretPosition;
                    textoOriginal = richTextBox.Text;
                    isUpdatingText = false;
                }
            }
        }

        private void ReemplazarTextoConEmojis()
        {
            RichTextBox richTextBox = ObtenerRichTextBoxActivo();
            if (richTextBox == null) return;

            isUpdatingText = true;
            int caretPosition = richTextBox.SelectionStart;

            foreach (var item in emojis)
            {
                string emojiKey = item.Key;
                string imagePath = item.Value;

                int startIndex = 0;

                while ((startIndex = richTextBox.Text.IndexOf(emojiKey, startIndex)) != -1)
                {
                    richTextBox.Select(startIndex, emojiKey.Length);
                    richTextBox.SelectedText = ""; // Elimina el texto del atajo
                    InsertEmojiImage(imagePath);
                    AgregarEmojiALaLista(startIndex, emojiKey);
                    startIndex += 1;
                }
            }

            richTextBox.SelectionStart = caretPosition;
            isUpdatingText = false;
        }

        private void AgregarEmojiALaLista(int posicion, string emoji)
        {
            TabPage pestanaActual = tabControl1.SelectedTab;
            if (pestanaActual != null && !imagenesInsertadasPorPestana.ContainsKey(pestanaActual))
            {
                imagenesInsertadasPorPestana[pestanaActual] = new List<Tuple<int, string>>();
            }
            imagenesInsertadasPorPestana[pestanaActual].Add(new Tuple<int, string>(posicion, emoji));
        }

        private void InsertEmojiImage(string imagePath)
        {
            RichTextBox richTextBox = ObtenerRichTextBoxActivo();
            if (richTextBox != null)
            {
                try
                {
                    // Cargar la imagen desde el archivo
                    using (Image image = Image.FromFile(imagePath))
                    {
                        // Ajustar el tamaño de la imagen
                        Image resizedImage = new Bitmap(image, new Size(24, 24));

                        // Copiar la imagen al portapapeles
                        Clipboard.SetImage(resizedImage);

                        // Insertar la imagen en el RichTextBox
                        richTextBox.Paste();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al insertar la imagen: " + ex.Message);
                }
            }
        }
        private RichTextBox ObtenerRichTextBoxActivo()
        {
            TabPage pestanaActual = tabControl1.SelectedTab;
            return pestanaActual != null ? pestanaActual.Controls.OfType<RichTextBox>().FirstOrDefault() : null;
        }

        private void CargarTextoYReemplazarConImagenes(string filePath, RichTextBox richTextBox)
        {
            string contenido = File.ReadAllText(filePath);
            richTextBox.Text = contenido;
            ReemplazarCarasConImagenes(richTextBox); // Reemplaza las combinaciones con imágenes
        }

        private void ReemplazarCarasConImagenes(RichTextBox richTextBox)
        {
            if (isUpdatingText) return;
            isUpdatingText = true;

            int cursorPosition = richTextBox.SelectionStart;

            // Reemplazar las combinaciones de caracteres con imágenes
            InsertarImagenSiCoincide(richTextBox, ":)", @"C:\Users\anais\OneDrive\Documentos\5° semestre ISI\Interfaces\blocDeNotasFinal\feliz.png", 24, 24);
            InsertarImagenSiCoincide(richTextBox, "<3", @"C:\Users\anais\OneDrive\Documentos\5° semestre ISI\Interfaces\blocDeNotasFinal\corazon.png", 24, 24);
            InsertarImagenSiCoincide(richTextBox, "$$p", @"C:\Users\anais\OneDrive\Documentos\5° semestre ISI\Interfaces\blocDeNotasFinal\Money.png", 24, 24);
            InsertarImagenSiCoincide(richTextBox, ":NoteNest", @"C:\Users\anais\OneDrive\Documentos\5° semestre ISI\Interfaces\blocDeNotasFinal\nest.png", 24, 24);
            InsertarImagenSiCoincide(richTextBox, ":P", @"C:\Users\anais\OneDrive\Documentos\5° semestre ISI\Interfaces\blocDeNotasFinal\lengua.png", 24, 24);

            richTextBox.SelectionStart = cursorPosition;
            isUpdatingText = false;
        }

        private void InsertarImagenSiCoincide(RichTextBox richTextBox, string atajo, string rutaImagen, int width, int height)
        {
            TabPage pestanaActual = tabControl1.SelectedTab;
            if (pestanaActual == null) return;

            if (!imagenesInsertadasPorPestana.ContainsKey(pestanaActual))
            {
                imagenesInsertadasPorPestana[pestanaActual] = new List<Tuple<int, string>>();
            }

            List<Tuple<int, string>> imagenesInsertadas = imagenesInsertadasPorPestana[pestanaActual];
            richTextBox.SuspendLayout();

            // Obtener el contenido del RichTextBox
            string textoActual = richTextBox.Text;

            int startIndex = 0;
            int index = textoActual.IndexOf(atajo, startIndex, StringComparison.OrdinalIgnoreCase); // Case-insensitive search

            // Evitar realizar cambios mientras el texto se está modificando
            isUpdatingText = true;

            while (index != -1 && index < richTextBox.Text.Length)
            {
                // Asegurarse de que el índice sea válido
                if (index >= 0 && index + atajo.Length <= richTextBox.Text.Length)
                {
                    richTextBox.Select(index, atajo.Length); // Seleccionar el texto que se va a reemplazar

                    try
                    {
                        using (Image imagenOriginal = Image.FromFile(rutaImagen))
                        {
                            using (Image imagenEscalada = new Bitmap(imagenOriginal, new Size(width, height)))
                            {
                                Clipboard.SetImage(imagenEscalada);
                                richTextBox.Paste();
                            }
                        }

                        // Agregar la imagen insertada al historial
                        imagenesInsertadas.Add(new Tuple<int, string>(index, atajo));

                        // Ajustar el startIndex después del reemplazo
                        startIndex = index + 1; // Incrementa después de la imagen
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al cargar la imagen: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                // Volver a buscar el atajo en el texto después del último reemplazo
                index = richTextBox.Text.IndexOf(atajo, startIndex, StringComparison.OrdinalIgnoreCase); // Case-insensitive search
            }

            // Restaurar el estado del texto
            isUpdatingText = false;
            richTextBox.ResumeLayout();
        }

        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void guardarComoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabPage pestanaActual = tabControl1.SelectedTab;
            if (pestanaActual != null && pestanaActual.Controls[0] is RichTextBox rtb)
            {
                GuardarComoArchivo(rtb);
            }
        }

        private void GuardarComoArchivo(RichTextBox richTextBox)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveFileDialog.FileName;

                    // Obtén la lista de imágenes insertadas para la pestaña actual
                    TabPage pestañaActual = tabControl1.SelectedTab;
                    List<Tuple<int, string>> imagenesInsertadas = imagenesInsertadasPorPestana.ContainsKey(pestañaActual)
                        ? imagenesInsertadasPorPestana[pestañaActual]
                        : new List<Tuple<int, string>>();

                    // Reemplazar imágenes con atajos antes de guardar
                    StringBuilder contenidoConAtajos = new StringBuilder(richTextBox.Text);
                    string textoConAtajos = ReemplazarImagenesPorAtajos(contenidoConAtajos, imagenesInsertadas);

                    using (StreamWriter writer = new StreamWriter(filePath, false))
                    {
                        writer.Write(textoConAtajos);
                    }

                    // Actualizar la pestaña con el nuevo nombre de archivo

                    if (pestañaActual != null)
                    {

                        archivosAbiertos[pestañaActual] = filePath;
                        cambiosNoGuardados[pestañaActual] = false;
                        pestañaActual.Text = Path.GetFileName(filePath);
                    }
                    archivoToolStripMenuItem.DropDownItems[2].Enabled = true;
                }
            }
        }

        private string ReemplazarImagenesPorAtajos(StringBuilder contenidoConAtajos, List<Tuple<int, string>> imagenesInsertadas)
        {
            // Procesar en orden inverso para evitar problemas con los índices
            for (int i = imagenesInsertadas.Count - 1; i >= 0; i--)
            {
                var img = imagenesInsertadas[i];

                // Asegurarse de que el índice sea válido
                if (img.Item1 >= 0 && img.Item1 < contenidoConAtajos.Length)
                {
                    // Determinar el tamaño del marcador de imagen
                    int longitudMarcador = 1; // Asumir longitud de marcador por defecto
                                              // Asegúrate de ajustar esto si los marcadores son más largos
                                              // (e.g., longitudMarcador = img.Item2.Length)

                    // Verificar el índice de eliminación
                    if (img.Item1 + longitudMarcador <= contenidoConAtajos.Length)
                    {
                        // Eliminar el marcador de imagen
                        contenidoConAtajos.Remove(img.Item1, longitudMarcador);

                        // Insertar el atajo en el mismo lugar
                        contenidoConAtajos.Insert(img.Item1, img.Item2);
                    }
                }
            }

            return contenidoConAtajos.ToString();
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
            richTextBox.TextChanged += richTextBox1_TextChanged;

            nuevaPestana.Controls.Add(richTextBox);

            tabControl1.SelectedTab = nuevaPestana;

            archivoToolStripMenuItem.DropDownItems[1].Enabled = true;
            archivosAbiertos[nuevaPestana] = null;
            cambiosNoGuardados[nuevaPestana] = false;

            imagenesInsertadasPorPestana[nuevaPestana] = new List<Tuple<int, string>>();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab != null && tabControl1.SelectedTab.Tag == null)
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
            TabPage pestanaActual = tabControl1.SelectedTab;
            if (pestanaActual != null && pestanaActual.Controls[0] is RichTextBox rtb)
            {
                if (archivosAbiertos.ContainsKey(pestanaActual) && archivosAbiertos[pestanaActual] != null)
                {
                    // Guardar el archivo con el nuevo contenido
                    string filePath = archivosAbiertos[pestanaActual];
                    GuardarComoTextoPlano(filePath, rtb);
                    cambiosNoGuardados[pestanaActual] = false;
                    pestanaActual.Text = Path.GetFileName(filePath);
                    MessageBox.Show("Archivo guardado correctamente.", "Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Guardar el archivo como un nuevo archivo
                    GuardarComoArchivo(rtb);
                }
            }
        }

        private void GuardarComoTextoPlano(string filePath, RichTextBox richTextBox)
        {
            // Obtén la lista de imágenes insertadas para la pestaña actual
            TabPage pestañaActual = tabControl1.SelectedTab;
            List<Tuple<int, string>> imagenesInsertadas = imagenesInsertadasPorPestana.ContainsKey(pestañaActual)
                ? imagenesInsertadasPorPestana[pestañaActual]
                : new List<Tuple<int, string>>();

            // Reemplazar imágenes con atajos antes de guardar
            StringBuilder contenidoConAtajos = new StringBuilder(richTextBox.Text);
            string textoConAtajos = ReemplazarImagenesPorAtajos(contenidoConAtajos, imagenesInsertadas);

            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                writer.Write(textoConAtajos);
            }
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