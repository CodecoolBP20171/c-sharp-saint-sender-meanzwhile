using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaintSender
{
    public partial class EmailSender : Form
    {
        bool isReply = false;

        public EmailSender()
        {
            InitializeComponent();
        }
        public EmailSender(string from, string subject)
        {
            InitializeComponent();

            isReply = true;
            textBox1.Text = from;
            textBox2.Text = "RE: " + subject;
        }

        private void EmailSender_Load(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(label1, "YourAddressHere@domain.tld");
        }

        private void SendButton_Click(object sender, EventArgs e)
        {

            string addressTo = textBox1.Text;
            string subject = textBox2.Text;
            string text = textBox3.Text;
            
            if (Validator.ValidateEmailAddress(addressTo))
            {
                Logic.SendMail(addressTo, subject, text);
                MessageBox.Show("Email successfully sent.");
                this.Close();
            }
            else
            {
                MessageBox.Show("This is not a valid e-mail address.");
            }

        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void EmailSender_Shown(object sender, EventArgs e)
        {
            if (isReply)
            {
                textBox3.Focus();
            }
        }
    }
}
