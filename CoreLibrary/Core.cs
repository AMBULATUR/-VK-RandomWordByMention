using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using VkNet;
using VkNet.AudioBypassService.Extensions;
using VkNet.Model;
using VkNet.NLog.Extensions.Logging;
using VkNet.NLog.Extensions.Logging.Extensions;

namespace CoreLibrary
{
    public class Core
    {
        static private long? idChat;
        static private long? messageId;
        static ServiceCollection services = new ServiceCollection();
        static public VkApi api = null;
        static private long? randomId = 0;

        /// <summary>
        /// Авторизация
        /// </summary>
        /// <param name="vkLogin">Логин</param>
        /// <param name="vkPassword">Пароль</param>
        /// <param name="LoggingEnabled">Логгирование действий библиотеки VkNet. По умолчанию - выключено</param>
        public static void VkAuth(string vkLogin, string vkPassword, bool LoggingEnabled = false)
        {
            services.AddAudioBypass();
            #region Logger
            // Регистрация логгера
            if (LoggingEnabled)
            {
                services.AddSingleton<ILoggerFactory, LoggerFactory>();
                services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
                services.AddLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.SetMinimumLevel(LogLevel.Trace);
                    builder.AddNLog(new NLogProviderOptions
                    {
                        CaptureMessageProperties = true,
                        CaptureMessageTemplates = true
                    });
                });
                NLog.LogManager.LoadConfiguration("nlog.config");
            }
            #endregion

            api = new VkApi(services);
            api.Authorize(new ApiAuthParams
            {
                Login = vkLogin,
                Password = vkPassword
            });
        }
        /// <summary>
        /// Поиск упоминания заданной строки
        /// </summary>
        /// <param name="query">Искомая строка</param>
        public static bool FindMention(string query)
        {
            var MentionList = api.Messages.Search(new VkNet.Model.RequestParams.MessagesSearchParams
            {
                Query = query,
                PreviewLength = 0,
                Offset = 0,
                Count = 3,
                Extended = true,
            });
            idChat = MentionList.Items[0].PeerId - 2000000000;
            messageId = MentionList.Items[0].Id;
            if (randomId == MentionList.Items[0].RandomId)
            {
                return false;
            }
            else
            {
                randomId = MentionList.Items[0].RandomId;
                return true;
            }
        }
        /// <summary>
        /// Ответ на сообщение крайнему пользователю инициализированным методом FindMention(string query)
        /// </summary>
        /// <param name="message">Ответное сообщение</param>
        public static void SendMessage(string message)
        {
            List<long> messages = new List<long>() { (long)messageId };
            api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams
            {
              //  ForwardMessages = messages, //Переслать сообщение пользователя вызвавшего бота
                ChatId = idChat,
                Message = message,
                RandomId = api.GetHashCode() * DateTime.Now.Millisecond
            });
        }
    }
}
