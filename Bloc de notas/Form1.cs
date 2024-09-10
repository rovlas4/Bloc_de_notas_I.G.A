using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bloc_de_notas
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            RichTextBox richTextBox = new RichTextBox();
            richTextBox.Dock = DockStyle.Fill;

            tabControl1.TabPages[0].Controls.Add(richTextBox);
            tabControl1.TabPages[0].Tag = "";
        }

        private void abrirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog opf = new OpenFileDialog();
            opf.Filter = "Archivos de texto|*.txt";
            opf.Title = "Abrir archivo";

            if (opf.ShowDialog() == DialogResult.OK)
            {
                ((RichTextBox)tabControl1.SelectedTab.Controls[0]).Text = File.ReadAllText(opf.FileName);
                tabControl1.SelectedTab.Tag = opf.FileName;
                tabControl1.SelectedTab.Text = Path.GetFileName(opf.FileName);
                archivoToolStripMenuItem.DropDownItems[1].Enabled = true;
            }
        }

        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void guardarComoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Archivos de texto (*.txt)|*.txt";
            sfd.Title = "Guardar archivo";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(sfd.FileName, ((RichTextBox)tabControl1.SelectedTab.Controls[0]).Text);
                tabControl1.SelectedTab.Tag = sfd.FileName;
                tabControl1.SelectedTab.Text = Path.GetFileName(sfd.FileName);
                archivoToolStripMenuItem.DropDownItems[1].Enabled = true;
            }
        }

        private void nuevaPestañaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabPage nuevaPestana = new TabPage("Nueva Pestaña");
            nuevaPestana.Tag = "";
            tabControl1.TabPages.Add(nuevaPestana);

            RichTextBox richTextBox = new RichTextBox();
            richTextBox.Dock = DockStyle.Fill;

            nuevaPestana.Controls.Add(richTextBox);

            tabControl1.SelectedTab = nuevaPestana;
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab != null && tabControl1.SelectedTab.Tag.Equals(""))
            {
                archivoToolStripMenuItem.DropDownItems[1].Enabled = false;
            }
            else
            {
                archivoToolStripMenuItem.DropDownItems[1].Enabled = true;
            }
        }

        private void guardarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            File.WriteAllText(tabControl1.SelectedTab.Tag.ToString(), ((RichTextBox)tabControl1.SelectedTab.Controls[0]).Text);
        }
    }
}
