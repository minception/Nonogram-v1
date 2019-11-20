using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nonogram
{
    public partial class FormNew : Form
    {
        public FormNew()
        {
            InitializeComponent();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void buttonCreate_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            sizeX = (int)setX.Value;
            sizeY = (int)setY.Value;
            Close();
        }
        public int sizeX { get; set; }
        public int sizeY { get; set; }
    }
}
