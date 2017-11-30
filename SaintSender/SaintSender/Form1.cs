using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.IO;
using System.Threading;

namespace SaintSender
{
    public partial class Form1 : Form
    {
        static List<Mail> mails;
        static List<Mail> newMails;
        static System.Windows.Forms.Timer myTimer = new System.Windows.Forms.Timer();

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_LoadAsync(object sender, EventArgs e)
        {
            await GetAllOnlineEmails();

            myTimer.Tick += new EventHandler(TimerEventProcessorAsync);

            myTimer.Interval = 5000;
            myTimer.Start();
        }

        private async void TimerEventProcessorAsync(Object myObject, EventArgs myEventArgs)
        {
            await GetNewMailsAsync();
            if (mails.Count != newMails.Count)
            {
                button2.BackColor = Color.GreenYellow;
            }

        }

        private async void dataGridView1_CellClickAsync(object sender, DataGridViewCellEventArgs e)
        {
            //todo implement a real solution
            try
            {
                if (dataGridView1.CurrentCell.ColumnIndex != 0)
                {
                    string id = mails[e.RowIndex].id;
                    string content = await Logic.GetMessageAsync(id);
                    webBrowser1.DocumentText = content;
                }
            }
            catch (Exception){ }
        }

        private void BackupButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                bool asd = (bool)dataGridView1.Rows[i].Cells[0].Value;
                if (asd)
                {
                    mails[i].SerializeAsync();
                }
            }
        }

        private void RestoreButton_Click(object sender, EventArgs e)
        {
            mails = Logic.restoreAllMails();
            FillDataGridWithEmails();

        }

        private async void GetAllEmailsButton_ClickAsync(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            await GetAllOnlineEmails();
            button2.BackColor = Color.WhiteSmoke;
        }

        private async Task GetAllOnlineEmails()
        {
            mails = await Task.Run(Logic.GetAllMailAsync);

            FillDataGridWithEmails();
        }

        private void FillDataGridWithEmails()
        {
            dataGridView1.Rows.Clear();
            foreach (var item in mails)
            {
                dataGridView1.Rows.Add(false, item.from, item.subject, item.snippet, item.date);
            }
        }

        private async Task GetNewMailsAsync()
        {
            newMails = await Task.Run(Logic.GetAllMailAsync);
        }

        private void SendMailButton_Click(object sender, EventArgs e)
        {
            Form dlg1 = new EmailSender();
            dlg1.ShowDialog();
        }

        private void ReplyButton_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentCell != null)
            {
                string fromAddress = (string) dataGridView1.SelectedCells[1].Value;
                string subject = (string)dataGridView1.SelectedCells[2].Value;
                Form dlg1 = new EmailSender(fromAddress, subject);
                dlg1.ShowDialog();
            }
        }

        private async void DeleteButton_ClickAsync(object sender, EventArgs e)
        {
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                bool asd = (bool)dataGridView1.Rows[i].Cells[0].Value;
                if (asd)
                {
                    Logic.DeleteMessage(mails[i].id);
                }
            }
            await GetAllOnlineEmails();
        }
    }
}
