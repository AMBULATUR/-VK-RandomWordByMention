using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VkNet.AudioBypassService.Extensions;
using VkNet;
using Microsoft.Extensions.DependencyInjection;
using VkNet.Model;
using VkNet.Enums.Filters;
using System.Collections;
using Microsoft.Extensions.Logging;
using VkNet.NLog.Extensions.Logging.Extensions;
using VkNet.NLog.Extensions.Logging;

namespace MentionBot
{
    public partial class main : Form
    {
        MentionCore MCobj = null;
        public main()
        {
            InitializeComponent();

        }

        private void EnableButton_Click(object sender, EventArgs e)
        {
            MCobj = new MentionCore(LoginBox.Text, PasswordBox.Text);
            MCobj.PreLoad();
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            MCobj.FindMention(IdBox.Text);
        }
    }

    class MentionCore
    {

        private string vkLogin;
        private string vkPassword;
        public MentionCore(string login, string password)
        {
            vkLogin = login;
            vkPassword = password;
        }
        private long? idChat;
        private long? messageId;
        private List<string> BadWords = new List<string>();
        ServiceCollection services = new ServiceCollection();
        VkApi api = null;


        public void PreLoad()
        {
            LoadBadWords();
            VkAuth();
        }
        private void VkAuth()
        {
            services.AddAudioBypass();

            #region Logger
            //// Регистрация логгера
            //services.AddSingleton<ILoggerFactory, LoggerFactory>();
            //services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            //services.AddLogging(builder =>
            //{
            //    builder.ClearProviders();
            //    builder.SetMinimumLevel(LogLevel.Trace);
            //    builder.AddNLog(new NLogProviderOptions
            //    {
            //        CaptureMessageProperties = true,
            //        CaptureMessageTemplates = true
            //    });
            //});
            //NLog.LogManager.LoadConfiguration("nlog.config");

            #endregion

            api = new VkApi(services);

            try
            {
                api.Authorize(new ApiAuthParams
                {
                    Login = vkLogin,
                    Password = vkPassword
                });
                MessageBox.Show("Success log in");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
        public void FindMention(string id)
        {

         


           var MentionList = api.Messages.Search(new VkNet.Model.RequestParams.MessagesSearchParams
            {
               
                Query = id,
                PreviewLength = 0,
                Offset = 0,
                Count = 3,
                Extended = true,
            });
            idChat = MentionList.Items[0].PeerId - 2000000000;
           messageId = MentionList.Items[0].Id;
            //if (MentionList.Items[0].ReadState == VkNet.Enums.MessageReadState.Unreaded)
            //{
                SendMessage();
            //}
            

        }
        public void FindConversation(string ConversationName)
        {
           var a = api.Messages.GetConversations(new VkNet.Model.RequestParams.GetConversationsParams {
                Count=30,
                
            });
            foreach (var item in a.Items)
            {
                try
                {
                    if (item.Conversation.ChatSettings.Title == ConversationName)
                    {
                        idChat = item.LastMessage.PeerId - 2000000000;
                    }
                }
                catch
                { }
                break;

            }
        }
        public void SendMessage()
        {
            List<long> messages = new List<long>() { (long)messageId };
            api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams
            {
                ForwardMessages = messages,
                ChatId = idChat,
                Message = BadWords[new Random().Next(1,BadWords.Count-1)],
                RandomId = api.GetHashCode() * DateTime.Now.Millisecond
            });
        }



    }

}
