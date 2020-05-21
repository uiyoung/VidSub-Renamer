using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VidSubRenamer
{
    public partial class PreferenceForm : Form
    {
        public PreferenceForm()
        {
            InitializeComponent();
        }

        private void PreferenceForm_DragEnter(object sender, DragEventArgs e)
        {
            MessageBox.Show("hi");
        }

        private void PreferenceForm_DragDrop(object sender, DragEventArgs e)
        {
            MessageBox.Show("hi");
        }
    }
}
