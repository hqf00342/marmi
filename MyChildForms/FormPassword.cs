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
            //�����l��OK�{�^���������Ȃ��悤��
            buttonOK.Enabled = false;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            //���͂��Ȃ��Ƃ���OK�{�^���������Ȃ��悤��
            if (string.IsNullOrEmpty(textBox1.Text))
                buttonOK.Enabled = false;
            else
                buttonOK.Enabled = true;
        }
    }
}