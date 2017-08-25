using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Web.Http;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Data.SqlClient;
using System.Data;
using Newtonsoft.Json.Linq;
using Parameters;
using GksKatowiceBot.Helpers;
using System.Json;

namespace GksKatowiceBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            try
            {
                if (activity.Type == ActivityTypes.Message)
                {

                    if (BaseDB.czyAdministrator(activity.From.Id) != null && (((activity.Text != null && activity.Text.IndexOf("!!!") == 0) || (activity.Attachments != null && activity.Attachments.Count > 0))))
                    {
                        WebClient client = new WebClient();


                        if (activity.Text.ToUpper().Contains("POWIADOMIENIA"))
                        {
                            CreateMessagePowiadomienia(activity.From.Id);
                        }

                        else
                        {
                            if (activity.Attachments != null)
                            {
                                //Uri uri = new Uri(activity.Attachments[0].ContentUrl);
                                string filename = activity.Attachments[0].ContentUrl.Substring(activity.Attachments[0].ContentUrl.Length - 4, 3).Replace(".", "");


                                //  WebClient client = new WebClient();
                                client.Credentials = new NetworkCredential("serwer1606926", "Tomason1910");
                                client.BaseAddress = "ftp://serwer1606926.home.pl/public_html/pub/";


                                byte[] data;
                                using (WebClient client2 = new WebClient())
                                {
                                    data = client2.DownloadData(activity.Attachments[0].ContentUrl);
                                }
                                if (activity.Attachments[0].ContentType.Contains("image")) client.UploadData(filename + ".png", data); //since the baseaddress
                                else if (activity.Attachments[0].ContentType.Contains("video")) client.UploadData(filename + ".mp4", data);
                            }
                            CreateMessage(activity.Attachments, activity.Text == null ? "" : activity.Text.Replace("!!!", ""), activity.From.Id);
                        }
                    }
                    else
                    {
                        string komenda = "";
                        if (activity.ChannelData != null)
                        {
                            try
                            {
                                var stuff = JsonConvert.DeserializeObject<ClassHelpers.RootObject>(activity.ChannelData.ToString());
                                //komenda = stuff.message.quick_reply.payload;
                     //           BaseDB.AddToLog("Przesłąny json " + activity.ChannelData.ToString());
                                komenda = stuff.message.quick_reply.payload;

               //                 BaseDB.AddToLog(komenda);

                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        var toReply = activity.CreateReply(String.Empty);
                        var connectorNew = new ConnectorClient(new Uri(activity.ServiceUrl));
                        toReply.Type = ActivityTypes.Typing;
                        await connectorNew.Conversations.SendToConversationAsync(toReply);



                        MicrosoftAppCredentials.TrustServiceUrl(@"https://facebook.botframework.com", DateTime.MaxValue);

                        if (komenda == "DEVELOPER_DEFINED_PAYLOAD_Hokej" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_Hokej" || activity.Text.ToLower() == "hokej")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            //        BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                                       {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "noności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualności",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_HokejAktualnosci",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                 //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Galeria",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_HokejGaleria",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "GieKSa TV",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_HokejVideo",
                                //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                },
                                                                       }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            message.Attachments = BaseGETMethod.GetCardsAttachmentsHokej(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        else
                            if (komenda == "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna" || activity.Text.ToLower() == "piłka nożna" || activity.Text.ToLower() == "pilka nozna")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            //        BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",


                                buttons = new dynamic[]
                            {
                            new
                        {
                                type = "web_url",
                                url = "https://petersfancyapparel.com/classic_white_tshirt",
                                title = "Wyniki",
                                webview_height_ratio = "compact"
                            }
                            },

                                quick_replies = new dynamic[]
                                   {
                                //new
                                //{oh
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualności",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_NoznaAktualnosci",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                 //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Galeria",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_NoznaGaleria",
                               //       image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "GieKSa TV",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_NoznaVideo",
                                //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                },
                                                                   }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();

                            message.Attachments = BaseGETMethod.GetCardsAttachments(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        else
                                if (komenda == "DEVELOPER_DEFINED_PAYLOAD_Siatkowka" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_Siatkowka" || activity.Text.ToLower() == "siatkowka" || activity.Text.ToLower() == "siatkówka")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            //     BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",


                                buttons = new dynamic[]
                            {
                            new
                            {
                                type = "web_url",
                                url = "https://petersfancyapparel.com/classic_white_tshirt",
                                title = "Wyniki",
                                webview_height_ratio = "compact"
                            }
                            },

                                quick_replies = new dynamic[]
                               {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualności",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_SiatkowkaAktualnosci",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                 //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Galeria",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_SiatkowkaGaleria",
                         //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "GieKSa TV",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_SiatkowkaVideo",
                                //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                },
                                                                                                                               }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();

                            message.Attachments = BaseGETMethod.GetCardsAttachmentsOrlenLiga(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        else
                                    if (activity.Text == "USER_DEFINED_PAYLOAD")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();

                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                            {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Piłka nożna",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Siatkówka",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = "Hokej",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.List;
                            message.Attachments = BaseGETMethod.GetCardsAttachmentsPowitanie();
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();

                            message.Text = @"Jestem Twoim asystentem do kontaktu z GKS-em Katowice. Co jakiś czas powiadomię Cię o tym, co dzieje się w Klubie.";
                            await connector.Conversations.SendToConversationAsync((Activity)message);
                            message.Attachments = null;

                            Thread.Sleep(500);

                            // message.Attachments = GetCardsAttachments(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);

                            message.Text = @"Współpraca między nami jest bardzo prosta. Wydajesz mi polecenia, a ja za Ciebie wykonuję całą robotę. Zaznacz tylko w rozwijanym menu lub skorzystaj z podpowiedzi co dokładnie Cię interesuje, a ja automatycznie połączę Cię z aktualnościami.
";

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        else
                                    if (activity.Text == "DEVELOPER_DEFINED_PAYLOAD_HELP")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();

                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                            {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Piłka nożna",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Siatkówka",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = "Hokej",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            message.Attachments = BaseGETMethod.GetCardsAttachmentsPowitanie();
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                            message.Attachments = null;

                            message.Text = @"Jestem Twoim asystentem do kontaktu z GKS-em Katowice. Co jakiś czas powiadomię Cię o tym, co dzieje się w Klubie.
";
                            // message.Attachments = GetCardsAttachments(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);

                            message.Text = @"Współpraca między nami jest bardzo prosta. Wydajesz mi polecenia, a ja za Ciebie wykonuję całą robotę. Zaznacz tylko w rozwijanym menu lub skorzystaj z podpowiedzi co dokładnie Cię interesuje, a ja automatycznie połączę Cię z aktualnościami.
";

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }

                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_Pilka_NoznaAktualnosci" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_Pilka_NoznaAktualnosci")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();

                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                            {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualności",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_NoznaAktualnosci",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //    image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Galeria",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_NoznaGaleria",
                                //    image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = "GieKSa TV",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_NoznaVideo",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            //message.Text = "Wybierz jedną z opcji";
                            message.Attachments = BaseGETMethod.GetCardsAttachments(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);

                        }
                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_Pilka_NoznaGaleria")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();

                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                            {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualności",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_NoznaAktualnosci",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //    image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Galeria",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_NoznaGaleria",
                                //    image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = "GieKSa TV",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_NoznaVideo",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            //message.Text = "Wybierz jedną z opcji";
                            message.Attachments = BaseGETMethod.GetCardsAttachmentsGallery(ref hrefList, true, 0);

                            await connector.Conversations.SendToConversationAsync((Activity)message);

                        }
                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_Pilka_NoznaVideo")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();

                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                            {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualności",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_NoznaAktualnosci",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                 //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Galeria",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_NoznaGaleria",
                                 //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = "GieKSa TV",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_NoznaVideo",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            //message.Text = "Wybierz jedną z opcji";
                            message.Attachments = BaseGETMethod.GetCardsAttachmentsVideo(ref hrefList, true, 0);

                            await connector.Conversations.SendToConversationAsync((Activity)message);

                        }
                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_HokejAktualnosci")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();

                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                            {
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualności",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_HokejAktualności",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //    image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Galeria",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_HokejGaleria",
                           //         image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = "GieKSa TV",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_HokejVideo",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            //message.Text = "Wybierz jedną z opcji";
                            message.Attachments = BaseGETMethod.GetCardsAttachmentsHokej(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);

                        }
                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_HokejGaleria")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();

                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                quick_replies = new dynamic[]
                            {
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualności",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_HokejAktualnosci",
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Galeria",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_HokejGaleria",
                           //         image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = "GieKSa TV",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_HokejVideo",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            //message.Text = "Wybierz jedną z opcji";
                            message.Attachments = BaseGETMethod.GetCardsAttachmentsGallery(ref hrefList, true, 2);

                            await connector.Conversations.SendToConversationAsync((Activity)message);

                        }
                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_HokejVideo")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();

                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                            {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualności",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_HokejAktualnosci",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                 //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Galeria",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_HokejGaleria",
                               //     image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = "GieKSa TV",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_HokejVideo",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            //message.Text = "Wybierz jedną z opcji";
                            message.Attachments = BaseGETMethod.GetCardsAttachmentsVideo(ref hrefList, true, 2);

                            await connector.Conversations.SendToConversationAsync((Activity)message);

                        }
                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_SiatkowkaAktualnosci")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();

                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                            {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualnosci",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_SiatkowkaAktualnosci",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //    image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Galeria",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_SiatkowkaGaleria",
                                   // image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = "GieKSa TV",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_SiatkowkaVideo",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            //message.Text = "Wybierz jedną z opcji";
                            message.Attachments = BaseGETMethod.GetCardsAttachmentsOrlenLiga(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);

                        }

                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_POWIADOMIENIA" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_POWIADOMIENIA" || activity.Text == "Powiadomienia")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            // BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            //BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            byte czyPowiadomienia = BaseDB.czyPowiadomienia(userAccount.Id);
                            if (czyPowiadomienia == 1)
                            {
                                message.Text = "Opcja automatycznych, codziennych powiadomień o aktualnościach  jest włączona. Jeśli nie chcesz otrzymywać powiadomień  możesz je wyłączyć lub zmienić częstotliwość ich otrzymywania. Wybierz jedną z opcji";
                                message.ChannelData = JObject.FromObject(new
                                {
                                    notification_type = "REGULAR",
                                    //buttons = new dynamic[]
                                    // {
                                    //     new
                                    //     {
                                    //    type ="postback",
                                    //    title="Tytul",
                                    //    vslue = "tytul",
                                    //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                    //     }
                                    // },
                                    quick_replies = new dynamic[]
                                           {
                                                                new
                                {
                                    content_type = "text",
                                    title = "Trzy razy w tygodniu",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Powiadomienia3WTygodniu",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                                                                                               new
                                {
                                    content_type = "text",
                                    title = "Raz w tygodniu",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_PowiadomieniaRazWTygodniu",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                                                                                new
                                {
                                    content_type = "text",
                                    title = "Wyłącz powiadomienia",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_PowiadomieniaWylacz",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                           }
                                });
                            }
                            else if (czyPowiadomienia == 2)
                            {
                                message.Text = "Opcja automatycznych, powiadomień o aktualnościach otrzymywanych trzy razy w tygodniu jest włączona. Jeśli nie chcesz otrzymywać powiadomień  możesz je wyłączyć lub zmienić częstotliwość ich otrzymywania. Wybierz jedną z opcji";
                                message.ChannelData = JObject.FromObject(new
                                {
                                    notification_type = "REGULAR",
                                    //buttons = new dynamic[]
                                    // {
                                    //     new
                                    //     {
                                    //    type ="postback",
                                    //    title="Tytul",
                                    //    vslue = "tytul",
                                    //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                    //     }
                                    // },
                                    quick_replies = new dynamic[]
                                           {
                                                                               new
                                {
                                    content_type = "text",
                                    title = "Codziennie",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_PowiadomieniaCodziennie",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                                                                                               new
                                {
                                    content_type = "text",
                                    title = "Raz w tygodniu",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_PowiadomieniaRazWTygodniu",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                                                                                new
                                {
                                    content_type = "text",
                                    title = "Wyłącz powiadomienia",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_PowiadomieniaWylacz",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },

                                           }
                                });
                            }
                            else if (czyPowiadomienia == 3)
                            {
                                message.Text = "Opcja automatycznych, powiadomień o aktualnościach przesyłanych raz w tygodniu jest włączona. Jeśli nie chcesz otrzymywać powiadomień  możesz je wyłączyć lub zmienić częstotliwość ich otrzymywania. Wybierz jedną z opcji";
                                message.ChannelData = JObject.FromObject(new
                                {
                                    notification_type = "REGULAR",
                                    //buttons = new dynamic[]
                                    // {
                                    //     new
                                    //     {
                                    //    type ="postback",
                                    //    title="Tytul",
                                    //    vslue = "tytul",
                                    //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                    //     }
                                    // },
                                    quick_replies = new dynamic[]
           {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualności",
                                ////       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                //},
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Wyłącz",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_PowiadomieniaWylacz",
                                //    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                // //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Codziennie",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_PowiadomieniaCodziennie",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                                                new
                                {
                                    content_type = "text",
                                    title = "Trzy razy w tygodniu",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Powiadomienia3WTygodniu",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                                                                                new
                                {
                                    content_type = "text",
                                    title = "Wyłącz powiadomienia",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_PowiadomieniaWylacz",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },

           }
                                });
                            }
                            else if (czyPowiadomienia == 4)
                            {
                                message.Text = "Opcja automatycznych, powiadomień o aktualnościach jest wyłączona. Jeśli chcesz otrzymywać powiadomienia możesz je włączyć.";
                                message.ChannelData = JObject.FromObject(new
                                {
                                    notification_type = "REGULAR",
                                    //buttons = new dynamic[]
                                    // {
                                    //     new
                                    //     {
                                    //    type ="postback",
                                    //    title="Tytul",
                                    //    vslue = "tytul",
                                    //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                    //     }
                                    // },
                                    quick_replies = new dynamic[]
           {
             new
                                {
                                    content_type = "text",
                                    title = "Codziennie",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_PowiadomieniaCodziennie",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                                                new
                                {
                                    content_type = "text",
                                    title = "Trzy razy w tygodniu",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Powiadomienia3WTygodniu",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                                                                                               new
                                {
                                    content_type = "text",
                                    title = "Raz w tygodniu",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_PowiadomieniaRazWTygodniu",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },

           }
                                });
                            }
                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            //    message.Attachments = BaseGETMethod.GetCardsAttachmentsNajnowsze(ref hrefList, true);
                            //     message.Text = "W kazdej chwili możesz włączyć lub wyłączyć otrzymywanie powiadomień na swojego Messengera. Co chcesz zrobić z powiadomieniami? ";
                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }

                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_PowiadomieniaCodziennie" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_PowiadomieniaCodziennie" || activity.Text == "Wyłącz")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            // BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            //BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                            {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Piłka nożna",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Siatkówka",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = "Hokej",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                            });
                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            //  message.Attachments = BaseGETMethod.GetCardsAttachmentsNajnowsze(ref hrefList, true);
                            message.Text = "Zmieniłem ustawienia powiadomień. Otrzymasz powiadomienia codziennie po godzinie 18.";
                            BaseDB.ChangeNotification(userAccount.Id, 1);
                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }

                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_Powiadomienia3WTygodniu" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_Powiadomienia3WTygodniu" || activity.Text == "Wyłącz")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            // BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            //BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                            {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Piłka nożna",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Siatkówka",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = "Hokej",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                            });
                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            //  message.Attachments = BaseGETMethod.GetCardsAttachmentsNajnowsze(ref hrefList, true);
                            message.Text = "Zmieniłem ustawienia powiadomień. Otrzymasz powiadomienia w poniedziałki, środy i soboty po godzinie 18.";
                            BaseDB.ChangeNotification(userAccount.Id, 2);
                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }


                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_PowiadomieniaRazWTygodniu" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_PowiadomieniaRazWTygodniu" || activity.Text == "Wyłącz")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            // BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            //BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                            {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Piłka nożna",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Siatkówka",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = "Hokej",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                            });
                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            //  message.Attachments = BaseGETMethod.GetCardsAttachmentsNajnowsze(ref hrefList, true);
                            message.Text = "Zmieniłem ustawienia powiadomień. Otrzymasz powiadomienia w każdy poniedziałek po godzinie 18.";
                            BaseDB.ChangeNotification(userAccount.Id, 3);
                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_PowiadomieniaWylacz" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_PowiadomieniaWylacz" || activity.Text == "Wyłącz")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            // BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            //BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                            {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Piłka nożna",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Siatkówka",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = "Hokej",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                            });
                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            //  message.Attachments = BaseGETMethod.GetCardsAttachmentsNajnowsze(ref hrefList, true);
                            message.Text = "Zmieniłem ustawienia powiadomień";
                            BaseDB.ChangeNotification(userAccount.Id, 4);
                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }


                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_SiatkowkaGaleria")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();

                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                            {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualnosci",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_SiatkowkaAktualnosci",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //    image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Galeria",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_SiatkowkaGaleria",
                                   // image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = "GieKSa TV",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_SiatkowkaVideo",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            //message.Text = "Wybierz jedną z opcji";
                            message.Attachments = BaseGETMethod.GetCardsAttachmentsGallery(ref hrefList, true, 1);

                            await connector.Conversations.SendToConversationAsync((Activity)message);

                        }
                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_SiatkowkaVideo")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();

                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                            {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualności",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_SiatkowkaAktualnosci",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //    image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Galeria",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_SiatkowkaGaleria",
                                  //  image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = "GieKSa TV",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_SiatkowkaVideo",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            //message.Text = "Wybierz jedną z opcji";
                            message.Attachments = BaseGETMethod.GetCardsAttachmentsVideo(ref hrefList, true, 1);

                            await connector.Conversations.SendToConversationAsync((Activity)message);

                        }

                        else
                        {
                            if (BaseDB.czyPrzeklenstwo(activity.Text) == 1)
                            {
                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();

                                message.ChannelData = JObject.FromObject(new
                                {
                                    notification_type = "REGULAR",
                                    //buttons = new dynamic[]
                                    // {
                                    //     new
                                    //     {
                                    //    type ="postback",
                                    //    title="Tytul",
                                    //    vslue = "tytul",
                                    //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                    //     }
                                    // },
                                    quick_replies = new dynamic[]
                                {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Piłka nożna",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                   // image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Siatkówka",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                           //         image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = "Hokej",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                               }
                                });


                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                                List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                                // message.Text = "Wybierz jedną z opcji";
                                message.Attachments = BaseGETMethod.GetCardsAttachmentsFoto();

                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }
                            else if (activity.Text.ToUpper().Contains("KARTA KIBICA"))
                            {
                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();

                                //     message.ChannelData = JObject.FromObject(new
                                //     {
                                //         notification_type = "REGULAR",
                                //         //buttons = new dynamic[]
                                //         // {
                                //         //     new
                                //         //     {
                                //         //    type ="postback",
                                //         //    title="Tytul",
                                //         //    vslue = "tytul",
                                //         //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //         //     }
                                //         // },
                                //         quick_replies = new dynamic[]
                                //     {
                                //     //new
                                //     //{
                                //     //    content_type = "text",
                                //     //    title = "Aktualności",
                                //     //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //     //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //     //},
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Piłka nożna",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna",
                                //         //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //        // image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                //     },
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Siatkówka",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                                ////         image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //     },                                new
                                //     {
                                //         content_type = "text",
                                //         title = "Hokej",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                //     //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //     },
                                //                                    }
                                //     });


                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                                List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                                message.Text = "";
                                message.Attachments = Helpers.BaseGETMethod.GetCardsAttachmentsExtra(ref hrefList, true, "http://gkskatowice.eu/page/karta-kibica", "Informacje o karcie kibica", "http://gkskatowice.eu/uploads/assets/images/karta.png");

                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }


                            else if (activity.Text.ToUpper().Contains("KIBICE") || activity.Text.ToUpper().Contains("KIBIC") || activity.Text.ToUpper().Contains("FANI"))
                            {
                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();

                                message.ChannelData = JObject.FromObject(new
                                {
                                    notification_type = "REGULAR",
                                    //buttons = new dynamic[]
                                    // {
                                    //     new
                                    //     {
                                    //    type ="postback",
                                    //    title="Tytul",
                                    //    vslue = "tytul",
                                    //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                    //     }
                                    // },
                                    quick_replies = new dynamic[]
                                {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Piłka nożna",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                   // image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Siatkówka",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                           //         image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = "Hokej",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                               }
                                });


                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                                List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                                // message.Text = "Wybierz jedną z opcji";
                                message.Attachments = BaseGETMethod.GetCardsAttachmentsFotoKibice();

                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }
                            else if (activity.Text.ToUpper().Contains("GIEKSIK"))
                            {
                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();

                                message.ChannelData = JObject.FromObject(new
                                {
                                    notification_type = "REGULAR",
                                    //buttons = new dynamic[]
                                    // {
                                    //     new
                                    //     {
                                    //    type ="postback",
                                    //    title="Tytul",
                                    //    vslue = "tytul",
                                    //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                    //     }
                                    // },
                                    quick_replies = new dynamic[]
                                {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Piłka nożna",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                   // image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Siatkówka",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                           //         image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = "Hokej",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                               }
                                });


                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                                List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                                // message.Text = "Wybierz jedną z opcji";
                                message.Attachments = BaseGETMethod.GetCardsAttachmentsFotoGieksiak();

                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }
                            else if (activity.Text.ToUpper().Contains("SKLEP") || activity.Text.ToUpper().Contains("GADŻETY") || activity.Text.ToUpper().Contains("STREFA GIEKSY"))
                            {
                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();

                                //     message.ChannelData = JObject.FromObject(new
                                //     {
                                //         notification_type = "REGULAR",
                                //         //buttons = new dynamic[]
                                //         // {
                                //         //     new
                                //         //     {
                                //         //    type ="postback",
                                //         //    title="Tytul",
                                //         //    vslue = "tytul",
                                //         //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //         //     }
                                //         // },
                                //         quick_replies = new dynamic[]
                                //     {
                                //     //new
                                //     //{
                                //     //    content_type = "text",
                                //     //    title = "Aktualności",
                                //     //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //     //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //     //},
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Piłka nożna",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna",
                                //         //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //        // image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                //     },
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Siatkówka",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                                ////         image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //     },                                new
                                //     {
                                //         content_type = "text",
                                //         title = "Hokej",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                //     //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //     },
                                //                                    }
                                //     });


                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                                List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                                message.Text = "";
                                message.Attachments = Helpers.BaseGETMethod.GetCardsAttachmentsExtra(ref hrefList, true, "http://strefagieksy.pl/", "Sklep GieKSy", "http://strefagieksy.pl/img/sklep-kibica-gieksy-katowice-ul-slowackiego-x7-logo-1430559269.jpg");

                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }
                            else if (activity.Text.ToUpper().Contains("FUCHS"))
                            {
                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();

                                //     message.ChannelData = JObject.FromObject(new
                                //     {
                                //         notification_type = "REGULAR",
                                //         //buttons = new dynamic[]
                                //         // {
                                //         //     new
                                //         //     {
                                //         //    type ="postback",
                                //         //    title="Tytul",
                                //         //    vslue = "tytul",
                                //         //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //         //     }
                                //         // },
                                //         quick_replies = new dynamic[]
                                //     {
                                //     //new
                                //     //{
                                //     //    content_type = "text",
                                //     //    title = "Aktualności",
                                //     //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //     //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //     //},
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Piłka nożna",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna",
                                //         //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //        // image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                //     },
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Siatkówka",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                                ////         image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //     },                                new
                                //     {
                                //         content_type = "text",
                                //         title = "Hokej",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                //     //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //     },
                                //                                    }
                                //     });


                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                                List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                                message.Text = "";
                                message.Attachments = Helpers.BaseGETMethod.GetCardsAttachmentsExtra(ref hrefList, true, "https://www.fuchs.com/pl/pl/", "Strona firmowa", "");

                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }

                            else if (activity.Text.ToUpper().Contains("AASA"))
                            {
                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();

                                //     message.ChannelData = JObject.FromObject(new
                                //     {
                                //         notification_type = "REGULAR",
                                //         //buttons = new dynamic[]
                                //         // {
                                //         //     new
                                //         //     {
                                //         //    type ="postback",
                                //         //    title="Tytul",
                                //         //    vslue = "tytul",
                                //         //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //         //     }
                                //         // },
                                //         quick_replies = new dynamic[]
                                //     {
                                //     //new
                                //     //{
                                //     //    content_type = "text",
                                //     //    title = "Aktualności",
                                //     //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //     //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //     //},
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Piłka nożna",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna",
                                //         //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //        // image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                //     },
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Siatkówka",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                                ////         image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //     },                                new
                                //     {
                                //         content_type = "text",
                                //         title = "Hokej",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                //     //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //     },
                                //                                    }
                                //     });


                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                                List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                                message.Text = "";
                                message.Attachments = Helpers.BaseGETMethod.GetCardsAttachmentsExtra(ref hrefList, true, "https://www.aasapolska.pl", "Aasa Polska", "https://www.aasapolska.pl/images/front/logo.png");

                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }

                            else if (activity.Text.ToUpper().Contains("NORD"))
                            {
                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();

                                //     message.ChannelData = JObject.FromObject(new
                                //     {
                                //         notification_type = "REGULAR",
                                //         //buttons = new dynamic[]
                                //         // {
                                //         //     new
                                //         //     {
                                //         //    type ="postback",
                                //         //    title="Tytul",
                                //         //    vslue = "tytul",
                                //         //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //         //     }
                                //         // },
                                //         quick_replies = new dynamic[]
                                //     {
                                //     //new
                                //     //{
                                //     //    content_type = "text",
                                //     //    title = "Aktualności",
                                //     //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //     //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //     //},
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Piłka nożna",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna",
                                //         //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //        // image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                //     },
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Siatkówka",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                                ////         image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //     },                                new
                                //     {
                                //         content_type = "text",
                                //         title = "Hokej",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                //     //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //     },
                                //                                    }
                                //     });


                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                                List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                                message.Text = "";
                                message.Attachments = Helpers.BaseGETMethod.GetCardsAttachmentsExtra(ref hrefList, true, "http://www.np.com.pl/", "Strona firmowa", "http://www.np.com.pl/logo.jpg");

                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }
                            else if (activity.Text.ToUpper().Contains("GTL"))
                            {
                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();

                                //     message.ChannelData = JObject.FromObject(new
                                //     {
                                //         notification_type = "REGULAR",
                                //         //buttons = new dynamic[]
                                //         // {
                                //         //     new
                                //         //     {
                                //         //    type ="postback",
                                //         //    title="Tytul",
                                //         //    vslue = "tytul",
                                //         //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //         //     }
                                //         // },
                                //         quick_replies = new dynamic[]
                                //     {
                                //     //new
                                //     //{
                                //     //    content_type = "text",
                                //     //    title = "Aktualności",
                                //     //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //     //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //     //},
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Piłka nożna",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna",
                                //         //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //        // image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                //     },
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Siatkówka",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                                ////         image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //     },                                new
                                //     {
                                //         content_type = "text",
                                //         title = "Hokej",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                //     //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //     },
                                //                                    }
                                //     });


                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                                List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                                message.Text = "";
                                message.Attachments = Helpers.BaseGETMethod.GetCardsAttachmentsExtra(ref hrefList, true, "https://gtl.com.pl/", "Strona firmowa", "");

                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }
                            else if (activity.Text.ToUpper().Contains("FORTUNA"))
                            {
                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();

                                //     message.ChannelData = JObject.FromObject(new
                                //     {
                                //         notification_type = "REGULAR",
                                //         //buttons = new dynamic[]
                                //         // {
                                //         //     new
                                //         //     {
                                //         //    type ="postback",
                                //         //    title="Tytul",
                                //         //    vslue = "tytul",
                                //         //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //         //     }
                                //         // },
                                //         quick_replies = new dynamic[]
                                //     {
                                //     //new
                                //     //{
                                //     //    content_type = "text",
                                //     //    title = "Aktualności",
                                //     //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //     //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //     //},
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Piłka nożna",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna",
                                //         //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //        // image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                //     },
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Siatkówka",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                                ////         image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //     },                                new
                                //     {
                                //         content_type = "text",
                                //         title = "Hokej",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                //     //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //     },
                                //                                    }
                                //     });


                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                                List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                                message.Text = "";
                                message.Attachments = Helpers.BaseGETMethod.GetCardsAttachmentsExtra(ref hrefList, true, "https://www.efortuna.pl/pl/rejestracja/bonus/kampanie/katowice_gks/x100_bez_ryzyka.html?clickid=EGFW48871128&affid=75723", "Zakłady bukmarcherski - Fortuna", "http://fortuna.bieguliczny.pl/public/uploads/fortuna_web.jpg");

                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }

                            else if (activity.Text.ToUpper().Contains("MŁODA GIEKSA") || activity.Text.ToUpper().Contains("AKADEMIA"))
                            {
                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();

                                //     message.ChannelData = JObject.FromObject(new
                                //     {
                                //         notification_type = "REGULAR",
                                //         //buttons = new dynamic[]
                                //         // {
                                //         //     new
                                //         //     {
                                //         //    type ="postback",
                                //         //    title="Tytul",
                                //         //    vslue = "tytul",
                                //         //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //         //     }
                                //         // },
                                //         quick_replies = new dynamic[]
                                //     {
                                //     //new
                                //     //{
                                //     //    content_type = "text",
                                //     //    title = "Aktualności",
                                //     //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //     //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //     //},
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Piłka nożna",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna",
                                //         //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //        // image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                //     },
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Siatkówka",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                                ////         image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //     },                                new
                                //     {
                                //         content_type = "text",
                                //         title = "Hokej",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                //     //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //     },
                                //                                    }
                                //     });


                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                                List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                                message.Text = "";
                                message.Attachments = Helpers.BaseGETMethod.GetCardsAttachmentsExtra(ref hrefList, true, "http://mloda-gieksa.pl/", "Akademia piłkarska Młoda GieKSa", "http://mloda-gieksa.pl/sgfx/logo.png");

                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }
                            else if (activity.Text.ToUpper().Contains("KATOWICE") || activity.Text.ToUpper().Contains("MIASTO") || activity.Text.ToUpper().Contains("PREZYDENT") || activity.Text.ToUpper().Contains("MARCIN KRUPA"))
                            {
                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();

                                //     message.ChannelData = JObject.FromObject(new
                                //     {
                                //         notification_type = "REGULAR",
                                //         //buttons = new dynamic[]
                                //         // {
                                //         //     new
                                //         //     {
                                //         //    type ="postback",
                                //         //    title="Tytul",
                                //         //    vslue = "tytul",
                                //         //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //         //     }
                                //         // },
                                //         quick_replies = new dynamic[]
                                //     {
                                //     //new
                                //     //{
                                //     //    content_type = "text",
                                //     //    title = "Aktualności",
                                //     //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //     //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //     //},
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Piłka nożna",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna",
                                //         //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //        // image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                //     },
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Siatkówka",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                                ////         image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //     },                                new
                                //     {
                                //         content_type = "text",
                                //         title = "Hokej",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                //     //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //     },
                                //                                    }
                                //     });


                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                                List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                                message.Text = "";
                                message.Attachments = Helpers.BaseGETMethod.GetCardsAttachmentsExtra(ref hrefList, true, "http://www.katowice.eu/", "Strona internetowa miasta Katowice", "");

                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }
                            else if (activity.Text.ToUpper().Contains("ZAGRAJ NA BUKOWEJ"))
                            {
                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();

                                //     message.ChannelData = JObject.FromObject(new
                                //     {
                                //         notification_type = "REGULAR",
                                //         //buttons = new dynamic[]
                                //         // {
                                //         //     new
                                //         //     {
                                //         //    type ="postback",
                                //         //    title="Tytul",
                                //         //    vslue = "tytul",
                                //         //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //         //     }
                                //         // },
                                //         quick_replies = new dynamic[]
                                //     {
                                //     //new
                                //     //{
                                //     //    content_type = "text",
                                //     //    title = "Aktualności",
                                //     //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //     //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //     //},
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Piłka nożna",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna",
                                //         //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //        // image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                //     },
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Siatkówka",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                                ////         image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //     },                                new
                                //     {
                                //         content_type = "text",
                                //         title = "Hokej",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                //     //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //     },
                                //                                    }
                                //     });


                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                                List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                                message.Text = "";
                                message.Attachments = Helpers.BaseGETMethod.GetCardsAttachmentsExtra(ref hrefList, true, "http://zagrajnabukowej.pl/", "Zagraj na Bukowej", "http://zagrajnabukowej.pl/wp-content/uploads/2017/03/stronaWWW.jpg");

                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }
                            else if (activity.Text.ToUpper().Contains("TAURON"))
                            {
                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();

                                //     message.ChannelData = JObject.FromObject(new
                                //     {
                                //         notification_type = "REGULAR",
                                //         //buttons = new dynamic[]
                                //         // {
                                //         //     new
                                //         //     {
                                //         //    type ="postback",
                                //         //    title="Tytul",
                                //         //    vslue = "tytul",
                                //         //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //         //     }
                                //         // },
                                //         quick_replies = new dynamic[]
                                //     {
                                //     //new
                                //     //{
                                //     //    content_type = "text",
                                //     //    title = "Aktualności",
                                //     //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //     //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //     //},
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Piłka nożna",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna",
                                //         //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //        // image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                //     },
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Siatkówka",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                                ////         image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //     },                                new
                                //     {
                                //         content_type = "text",
                                //         title = "Hokej",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                //     //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //     },
                                //                                    }
                                //     });


                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                                List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                                message.Text = "";
                                message.Attachments = Helpers.BaseGETMethod.GetCardsAttachmentsExtra(ref hrefList, true, "http://www.tauron.pl", "Tauron - strona internetowa", "https://www.tauron.pl/-/-/media/Layout/logo_109x109.ashx");

                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }
                            else if (activity.Text.ToUpper().Contains("BILET") || activity.Text.ToUpper().Contains("BILETY") || activity.Text.ToUpper().Contains("BILECIKI") || activity.Text.ToUpper().Contains("WEJŚCIÓWKI"))
                            {
                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();

                                //     message.ChannelData = JObject.FromObject(new
                                //     {
                                //         notification_type = "REGULAR",
                                //         //buttons = new dynamic[]
                                //         // {
                                //         //     new
                                //         //     {
                                //         //    type ="postback",
                                //         //    title="Tytul",
                                //         //    vslue = "tytul",
                                //         //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //         //     }
                                //         // },
                                //         quick_replies = new dynamic[]
                                //     {
                                //     //new
                                //     //{
                                //     //    content_type = "text",
                                //     //    title = "Aktualności",
                                //     //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //     //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //     //},
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Piłka nożna",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna",
                                //         //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //        // image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                //     },
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Siatkówka",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                                ////         image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //     },                                new
                                //     {
                                //         content_type = "text",
                                //         title = "Hokej",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                //     //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //     },
                                //                                    }
                                //     });


                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                                List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                                message.Text = "";
                                message.Attachments = Helpers.BaseGETMethod.GetCardsAttachmentsBilety(ref hrefList, true, "http://strefagieksy.pl/", "Sklep GieKSy");

                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }
                            else if (activity.Text.ToUpper().Contains("KARNET") || activity.Text.ToUpper().Contains("KARNETY"))
                            {
                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();

                                //     message.ChannelData = JObject.FromObject(new
                                //     {
                                //         notification_type = "REGULAR",
                                //         //buttons = new dynamic[]
                                //         // {
                                //         //     new
                                //         //     {
                                //         //    type ="postback",
                                //         //    title="Tytul",
                                //         //    vslue = "tytul",
                                //         //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //         //     }
                                //         // },
                                //         quick_replies = new dynamic[]
                                //     {
                                //     //new
                                //     //{
                                //     //    content_type = "text",
                                //     //    title = "Aktualności",
                                //     //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //     //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //     //},
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Piłka nożna",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna",
                                //         //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //        // image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                //     },
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Siatkówka",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                                ////         image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //     },                                new
                                //     {
                                //         content_type = "text",
                                //         title = "Hokej",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                //     //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //     },
                                //                                    }
                                //     });


                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                                List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                                message.Text = "";
                                message.Attachments = Helpers.BaseGETMethod.GetCardsAttachmentsKarnety(ref hrefList, true, "http://strefagieksy.pl/", "Sklep GieKSy");

                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }
                            else if (activity.Text.ToUpper().Contains("TABELA") || activity.Text.ToUpper().Contains("TERMINARZ") || activity.Text.ToUpper().Contains("WYNIK") || activity.Text.ToUpper().Contains("WYNIKI"))
                            {
                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();

                                //     message.ChannelData = JObject.FromObject(new
                                //     {
                                //         notification_type = "REGULAR",
                                //         //buttons = new dynamic[]
                                //         // {
                                //         //     new
                                //         //     {
                                //         //    type ="postback",
                                //         //    title="Tytul",
                                //         //    vslue = "tytul",
                                //         //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //         //     }
                                //         // },
                                //         quick_replies = new dynamic[]
                                //     {
                                //     //new
                                //     //{
                                //     //    content_type = "text",
                                //     //    title = "Aktualności",
                                //     //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //     //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //     //},
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Piłka nożna",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna",
                                //         //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //        // image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                //     },
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Siatkówka",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                                ////         image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //     },                                new
                                //     {
                                //         content_type = "text",
                                //         title = "Hokej",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                //     //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //     },
                                //                                    }
                                //     });


                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                                List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                                message.Text = "";
                                message.Attachments = Helpers.BaseGETMethod.GetCardsAttachmentsTabelaTerminarz(ref hrefList, true, "http://strefagieksy.pl/", "Sklep GieKSy");

                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }
                            else if (activity.Text.ToUpper().Contains("SKŁAD") || activity.Text.ToUpper().Contains("DRUŻYNA") || activity.Text.ToUpper().Contains("ZESPÓŁ") || activity.Text.ToUpper().Contains("ZAWODNICY"))
                            {
                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();

                                //     message.ChannelData = JObject.FromObject(new
                                //     {
                                //         notification_type = "REGULAR",
                                //         //buttons = new dynamic[]
                                //         // {
                                //         //     new
                                //         //     {
                                //         //    type ="postback",
                                //         //    title="Tytul",
                                //         //    vslue = "tytul",
                                //         //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //         //     }
                                //         // },
                                //         quick_replies = new dynamic[]
                                //     {
                                //     //new
                                //     //{
                                //     //    content_type = "text",
                                //     //    title = "Aktualności",
                                //     //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //     //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //     //},
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Piłka nożna",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna",
                                //         //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //        // image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                //     },
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Siatkówka",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                                ////         image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //     },                                new
                                //     {
                                //         content_type = "text",
                                //         title = "Hokej",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                //     //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //     },
                                //                                    }
                                //     });


                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                                List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                                message.Text = "";
                                message.Attachments = Helpers.BaseGETMethod.GetCardsAttachmentsSklad(ref hrefList, true, "http://strefagieksy.pl/", "Sklep GieKSy");

                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }
                            else if (activity.Text.ToUpper().Contains("ZARZĄD") || activity.Text.ToUpper().Contains("PREZES") || activity.Text.ToUpper().Contains("PREZESI") || activity.Text.ToUpper().Contains("WŁADZE"))
                            {
                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();

                                //     message.ChannelData = JObject.FromObject(new
                                //     {
                                //         notification_type = "REGULAR",
                                //         //buttons = new dynamic[]
                                //         // {
                                //         //     new
                                //         //     {
                                //         //    type ="postback",
                                //         //    title="Tytul",
                                //         //    vslue = "tytul",
                                //         //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //         //     }
                                //         // },
                                //         quick_replies = new dynamic[]
                                //     {
                                //     //new
                                //     //{
                                //     //    content_type = "text",
                                //     //    title = "Aktualności",
                                //     //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //     //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //     //},
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Piłka nożna",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna",
                                //         //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //        // image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                //     },
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Siatkówka",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                                ////         image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //     },                                new
                                //     {
                                //         content_type = "text",
                                //         title = "Hokej",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                //     //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //     },
                                //                                    }
                                //     });


                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                                List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                                message.Text = "";
                                message.Attachments = Helpers.BaseGETMethod.GetCardsAttachmentsExtra(ref hrefList, true, "http://gkskatowice.eu/page/ludzie-gieksy", "Ludzie GieKSy");

                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }
                            else if (activity.Text.ToUpper().Contains("JUNIORZY") || activity.Text.ToUpper().Contains("AKADEMIA") || activity.Text.ToUpper().Contains("MŁODZIEŻ") || activity.Text.ToUpper().Contains("MŁODA GIEKSA"))
                            {
                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();

                                //     message.ChannelData = JObject.FromObject(new
                                //     {
                                //         notification_type = "REGULAR",
                                //         //buttons = new dynamic[]
                                //         // {
                                //         //     new
                                //         //     {
                                //         //    type ="postback",
                                //         //    title="Tytul",
                                //         //    vslue = "tytul",
                                //         //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //         //     }
                                //         // },
                                //         quick_replies = new dynamic[]
                                //     {
                                //     //new
                                //     //{
                                //     //    content_type = "text",
                                //     //    title = "Aktualności",
                                //     //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //     //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //     //},
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Piłka nożna",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna",
                                //         //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //        // image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                //     },
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Siatkówka",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                                ////         image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //     },                                new
                                //     {
                                //         content_type = "text",
                                //         title = "Hokej",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                //     //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //     },
                                //                                    }
                                //     });


                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                                List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                                message.Text = "";
                                message.Attachments = Helpers.BaseGETMethod.GetCardsAttachmentsExtra(ref hrefList, true, "http://mloda-gieksa.pl/", "Akademia GieKSy", "http://mloda-gieksa.pl/sgfx/logo.png");

                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }
                            else if (activity.Text.ToUpper().Contains("KONTAKT") || activity.Text.ToUpper().Contains("TELEFON") || activity.Text.ToUpper().Contains("ADRES"))
                            {
                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();

                                //     message.ChannelData = JObject.FromObject(new
                                //     {
                                //         notification_type = "REGULAR",
                                //         //buttons = new dynamic[]
                                //         // {
                                //         //     new
                                //         //     {
                                //         //    type ="postback",
                                //         //    title="Tytul",
                                //         //    vslue = "tytul",
                                //         //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //         //     }
                                //         // },
                                //         quick_replies = new dynamic[]
                                //     {
                                //     //new
                                //     //{
                                //     //    content_type = "text",
                                //     //    title = "Aktualności",
                                //     //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //     //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //     //},
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Piłka nożna",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna",
                                //         //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //        // image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                //     },
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Siatkówka",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                                ////         image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //     },                                new
                                //     {
                                //         content_type = "text",
                                //         title = "Hokej",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                //     //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //     },
                                //                                    }
                                //     });


                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                                List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                                message.Text = "";
                                message.Attachments = Helpers.BaseGETMethod.GetCardsAttachmentsExtra(ref hrefList, true, "http://gkskatowice.eu/page/kontakt", "Kontakt");

                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }

                            else if (activity.Text.ToUpper().Contains("SZACHY"))
                            {
                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();

                                //     message.ChannelData = JObject.FromObject(new
                                //     {
                                //         notification_type = "REGULAR",
                                //         //buttons = new dynamic[]
                                //         // {
                                //         //     new
                                //         //     {
                                //         //    type ="postback",
                                //         //    title="Tytul",
                                //         //    vslue = "tytul",
                                //         //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //         //     }
                                //         // },
                                //         quick_replies = new dynamic[]
                                //     {
                                //     //new
                                //     //{
                                //     //    content_type = "text",
                                //     //    title = "Aktualności",
                                //     //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //     //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //     //},
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Piłka nożna",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna",
                                //         //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //        // image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                //     },
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Siatkówka",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                                ////         image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //     },                                new
                                //     {
                                //         content_type = "text",
                                //         title = "Hokej",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                //     //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //     },
                                //                                    }
                                //     });


                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                                List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                                message.Text = "";
                                message.Attachments = Helpers.BaseGETMethod.GetCardsAttachmentsExtra(ref hrefList, true, "http://hetmankatowice.pl/", "Klub Szachowy HETMAN w Katowicach");

                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }

                            else if (activity.Text.ToUpper().Contains("PIŁKARKI") || activity.Text.ToUpper().Contains("KOBIETY") || activity.Text.ToUpper().Contains("ŻEŃSKI"))
                            {
                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();

                                //     message.ChannelData = JObject.FromObject(new
                                //     {
                                //         notification_type = "REGULAR",
                                //         //buttons = new dynamic[]
                                //         // {
                                //         //     new
                                //         //     {
                                //         //    type ="postback",
                                //         //    title="Tytul",
                                //         //    vslue = "tytul",
                                //         //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //         //     }
                                //         // },
                                //         quick_replies = new dynamic[]
                                //     {
                                //     //new
                                //     //{
                                //     //    content_type = "text",
                                //     //    title = "Aktualności",
                                //     //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //     //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //     //},
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Piłka nożna",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna",
                                //         //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //        // image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                //     },
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Siatkówka",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                                ////         image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //     },                                new
                                //     {
                                //         content_type = "text",
                                //         title = "Hokej",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                //     //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //     },
                                //                                    }
                                //     });


                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                                List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                                message.Text = "";
                                message.Attachments = Helpers.BaseGETMethod.GetCardsAttachmentsExtra(ref hrefList, true, "http://gkskatowice.eu/page/tabela-terminarz-kobiet", "Rozgrywki Kobiet");

                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }

                            else if (activity.Text.ToUpper().Contains("GALERIA LIBERO") || activity.Text.ToUpper().Contains("LIBER"))
                            {
                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();

                                //     message.ChannelData = JObject.FromObject(new
                                //     {
                                //         notification_type = "REGULAR",
                                //         //buttons = new dynamic[]
                                //         // {
                                //         //     new
                                //         //     {
                                //         //    type ="postback",
                                //         //    title="Tytul",
                                //         //    vslue = "tytul",
                                //         //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //         //     }
                                //         // },
                                //         quick_replies = new dynamic[]
                                //     {
                                //     //new
                                //     //{
                                //     //    content_type = "text",
                                //     //    title = "Aktualności",
                                //     //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //     //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //     //},
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Piłka nożna",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna",
                                //         //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //        // image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                //     },
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Siatkówka",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                                ////         image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //     },                                new
                                //     {
                                //         content_type = "text",
                                //         title = "Hokej",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                //     //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //     },
                                //                                    }
                                //     });


                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                                List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                                message.Text = "";
                                message.Attachments = Helpers.BaseGETMethod.GetCardsAttachmentsExtra(ref hrefList, true, "http://galerialibero.pl/", "Galeria Libero");

                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }


                            else if (activity.Text.ToUpper().Contains("AUTOGRAFY") || activity.Text.ToUpper().Contains("FANKARTY") || activity.Text.ToUpper().Contains("FAN KARTY") || activity.Text.ToUpper().Contains("KARTY ZAWODNIKÓW"))
                            {
                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();

                                //     message.ChannelData = JObject.FromObject(new
                                //     {
                                //         notification_type = "REGULAR",
                                //         //buttons = new dynamic[]
                                //         // {
                                //         //     new
                                //         //     {
                                //         //    type ="postback",
                                //         //    title="Tytul",
                                //         //    vslue = "tytul",
                                //         //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //         //     }
                                //         // },
                                //         quick_replies = new dynamic[]
                                //     {
                                //     //new
                                //     //{
                                //     //    content_type = "text",
                                //     //    title = "Aktualności",
                                //     //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //     //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //     //},
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Piłka nożna",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna",
                                //         //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //        // image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                //     },
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Siatkówka",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                                ////         image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //     },                                new
                                //     {
                                //         content_type = "text",
                                //         title = "Hokej",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                //     //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //     },
                                //                                    }
                                //     });


                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                                List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                                message.Text = "Zapraszamy do kontaktu pod adresem: marketing@gkskatowice.eu ";
                                //message.Attachments = 

                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }


                            else
                            {
                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                             //   BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                                BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();

                                message.ChannelData = JObject.FromObject(new
                                {
                                    notification_type = "REGULAR",
                                    //buttons = new dynamic[]
                                    // {
                                    //     new
                                    //     {
                                    //    type ="postback",
                                    //    title="Tytul",
                                    //    vslue = "tytul",
                                    //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                    //     }
                                    // },
                                    quick_replies = new dynamic[]
                                {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Piłka nożna",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                   // image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Siatkówka",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                           //         image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = "Hokej",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                               }
                                });


                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                                List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                                message.Text = "Wybierz jedną z opcji";
                                // message.Attachments = BaseGETMethod.GetCardsAttachmentsGallery(ref hrefList, true);

                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }
                        }
                    }
                }

                else
                {
                    HandleSystemMessage(activity);
                }
            }
            catch (Exception ex)
            {
                BaseDB.AddToLog("Wysylanie wiadomosci: " + ex.ToString());
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        public async static void CreateMessage(IList<Attachment> foto, string wiadomosc, string fromId)
        {
            try
            {
                BaseDB.AddToLog("Wywołanie metody CreateMessage");

                string uzytkownik = "";
                DataTable dt = BaseGETMethod.GetUser(1);

                try
                {
                    MicrosoftAppCredentials.TrustServiceUrl(@"https://facebook.botframework.com", DateTime.MaxValue);

                    IMessageActivity message = Activity.CreateMessageActivity();
                    message.ChannelData = JObject.FromObject(new
                    {
                        notification_type = "REGULAR",
                        quick_replies = new dynamic[]
                            {
                               new
                        {
                                    content_type = "text",
                                    title = "Piłka nożna",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna",
                                  //  image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                new
                        {
                                    content_type = "text",
                                    title = "Siatkówka",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                                   // image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                },                                new
                        {
                                    content_type = "text",
                                    title = "Hokej",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                   // image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                    });

                    message.AttachmentLayout = null;

                    if (foto != null && foto.Count > 0)
                    {
                        string filename = foto[0].ContentUrl.Substring(foto[0].ContentUrl.Length - 4, 3).Replace(".", "");

                        if (foto[0].ContentType.Contains("image")) foto[0].ContentUrl = "http://serwer1606926.home.pl/pub/" + filename + ".png";//since the baseaddress
                        else if (foto[0].ContentType.Contains("video")) foto[0].ContentUrl = "http://serwer1606926.home.pl/pub/" + filename + ".mp4";

                        //foto[0].ContentUrl = "http://serwer1606926.home.pl/pub/" + filename + ".png";

                        message.Attachments = foto;
                    }


                    //var list = new List<Attachment>();
                    //if (foto != null)
                    //{
                    //    for (int i = 0; i < foto.Count; i++)
                    //    {
                    //        list.Add(GetHeroCard(
                    //       foto[i].ContentUrl, "", "",
                    //       new CardImage(url: foto[i].ContentUrl),
                    //       new CardAction(ActionTypes.OpenUrl, "", value: ""),
                    //       new CardAction(ActionTypes.OpenUrl, "", value: "https://www.facebook.com/sharer/sharer.php?u=" + "")));
                    //    }
                    //}

                    message.Text = wiadomosc;
                    int i = 0;
                    while (i <= dt.Rows.Count)
                    {
                        var listaUzytkownikow = dt.AsEnumerable().Skip(i).Take(50).ToList();
                        SednMessage(listaUzytkownikow, message, fromId);
                        i += 50;
                    }
                }
                catch (Exception ex)
                {
                    BaseDB.AddToLog("Błąd wysyłania wiadomości do: " + uzytkownik + " " + ex.ToString());
                }
            }
            catch (Exception ex)
            {
                BaseDB.AddToLog("Błąd wysłania wiadomosci: " + ex.ToString());
            }
        }



        public async static void CreateMessagePowiadomienia(string fromId)
        {
            try
            {
                BaseDB.AddToLog("Wywołanie metody CreateMessage");

                string uzytkownik = "";
                DataTable dt = BaseGETMethod.GetUser(4);

                try
                {
                    MicrosoftAppCredentials.TrustServiceUrl(@"https://facebook.botframework.com", DateTime.MaxValue);

                    IMessageActivity message = Activity.CreateMessageActivity();
                    message.ChannelData = JObject.FromObject(new
                    {
                        notification_type = "REGULAR",
                        quick_replies = new dynamic[]
                            {
                                                                                                               new
                                {
                                    content_type = "text",
                                    title = "Codziennie",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_PowiadomieniaCodziennie",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                                                                        new
                                {
                                    content_type = "text",
                                    title = "Trzy razy w tygodniu",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Powiadomienia3WTygodniu",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                                                                                               new
                                {
                                    content_type = "text",
                                    title = "Raz w tygodniu",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_PowiadomieniaRazWTygodniu",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                                                                                new
                                {
                                    content_type = "text",
                                    title = "Wyłącz powiadomienia",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_PowiadomieniaWylacz",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                                           }
                    });

                    message.AttachmentLayout = null;


                        message.Attachments = BaseGETMethod.GetCardsAttachmentsFotoPowiadomienia();

                    int i = 0;
                    while (i <= dt.Rows.Count)
                    {
                        var listaUzytkownikow = dt.AsEnumerable().Skip(i).Take(50).ToList();
                        SednMessage(listaUzytkownikow, message, fromId);
                        i += 50;
                    }
                }
                catch (Exception ex)
                {
                    BaseDB.AddToLog("Błąd wysyłania wiadomości do: " + uzytkownik + " " + ex.ToString());
                }
            }
            catch (Exception ex)
            {
                BaseDB.AddToLog("Błąd wysłania wiadomosci: " + ex.ToString());
            }
        }

        public async static void SednMessage(List<DataRow> dt, IMessageActivity message, string fromId)
        {
            foreach (DataRow dr in dt)
            {
                string uzytkownik = "";
                try
                {
                    if (fromId != dr["UserId"].ToString())
                    {
                        var userAccount = new ChannelAccount(name: dr["UserName"].ToString(), id: dr["UserId"].ToString());
                        uzytkownik = userAccount.Name;
                        var botAccount = new ChannelAccount(name: dr["BotName"].ToString(), id: dr["BotId"].ToString());
                        var connector = new ConnectorClient(new Uri(dr["Url"].ToString()), "73267226-823f-46b0-8303-2e866165a3b2", "k6XBDCgzL5452YDhS3PPLsL");
                        var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                        message.From = botAccount;
                        message.Recipient = userAccount;
                        message.Conversation = new ConversationAccount(id: conversationId.Id, isGroup: false);
                        //await connector.Conversations.SendToConversationAsync((Activity)message).ConfigureAwait(false);

                        var returne = await connector.Conversations.SendToConversationAsync((Activity)message);
                    }
                }
                catch (Exception ex)
                {
                    BaseDB.AddToLog("Błąd wysyłania wiadomości do: " + uzytkownik + " " + ex.ToString());
                }
            }
        }

        public static void CallToChildThread()
        {
            try
            {
                Thread.Sleep(5000);
            }

            catch (ThreadAbortException e)
            {
                Console.WriteLine("Thread Abort Exception");
            }
            finally
            {
                Console.WriteLine("Couldn't catch the Thread Exception");
            }
        }






        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                BaseDB.DeleteUser(Convert.ToInt64(message.From.Id));
            }
            else
                if (message.Type == ActivityTypes.ConversationUpdate)
            {
            }
            else
                    if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
            }
            else
                        if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else
                            if (message.Type == ActivityTypes.Ping)
            {
            }
            else
                                if (message.Type == ActivityTypes.Typing)
            {
            }
            return null;
        }







    }
}
