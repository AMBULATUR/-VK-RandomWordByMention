using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using CoreLibrary;



namespace MentionBot
{
    public partial class main : Form
    {
        static private List<string> BadWords = new List<string>();
        private Thread WorkThread = null;
        public main()
        {
            InitializeComponent();
        }

        private void AuthButton_Click(object sender, EventArgs e)
        {
            try
            {
                LoadBadWords();
                Core.VkAuth(LoginBox.Text, PasswordBox.Text);
                MessageBox.Show("Successful load");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void EnableButton_Click(object sender, EventArgs e)
        {
            if (EnableButton.Text == "Enable")
            {
                WorkThread = new Thread(()=>Work(QueryBox.Text));
                WorkThread.Start();
                EnableButton.Text = "Disable";
            }
            else
            {
                WorkThread.Abort();//Избежать Thread.Abort
                EnableButton.Text = "Enable";
            }
        }
        static private void Work(string query)
        {
            while (true)
            {
                Thread.Sleep(5000);
                try
                {
                    if(Core.FindMention(query))
                    Core.SendMessage(BadWords[new Random().Next(1, BadWords.Count - 1)]);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        private void LoadBadWords()
        {
            string UnSortedBW = File.ReadAllText("BadWords.txt");
            var BW = UnSortedBW.Split(',');
            foreach (var item in BW)
            {
                BadWords.Add(item);
            }
        }
    }

}
