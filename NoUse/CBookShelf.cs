using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Marmi
{
    public partial class CBookShelf : UserControl
    {
        public FlowLayoutPanel flowPanel { get { return flowLayoutPanel1; } }
        public Panel leftPanel { get { return splitContainer1.Panel1; } }

        public CBookShelf()
        {
            InitializeComponent();
        }
    }
}