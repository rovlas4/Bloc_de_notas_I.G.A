using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
        }

        private void abrirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Archivos de texto|*.txt";
            openFileDialog1.Title = "Abrir archivo";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                richTextBox1.Text = System.IO.File.ReadAllText(openFileDialog1.FileName);
            }
        }
    }
}
