using System;
using System.Windows.Forms;

namespace Marmi
{
    public partial class PasswordForm : Form
    {
        public PasswordForm()
        {
            InitializeComponent();
        }

        public string PasswordText => textBox1.Text;

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            //入力がないときはOKボタンを無効
            buttonOK.Enabled = !string.IsNullOrEmpty(textBox1.Text);
        }

        public void ClearText() => textBox1.Text = string.Empty;
    }
}