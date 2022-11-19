using System;
using System.Windows.Forms;

namespace Marmi
{
    public partial class FormPassword : Form
    {
        public FormPassword()
        {
            InitializeComponent();
        }

        public string PasswordText
        {
            get { return textBox1.Text; }
            set { textBox1.Text = value; }
        }

        private void FormPassword_Load(object sender, EventArgs e)
        {
            //初期値でOKボタンを押せないように
            buttonOK.Enabled = false;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            //入力がないときはOKボタンを押せないように
            if (string.IsNullOrEmpty(textBox1.Text))
                buttonOK.Enabled = false;
            else
                buttonOK.Enabled = true;
        }
    }
}