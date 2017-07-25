using GksKatowiceBot.Helpers;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace GksKatowiceBot.Controllers
{
    public class ThreadClass
    {
        public async static void SendThreadMessage(List<DataRow> dt,IList<Attachment> items)
        {
            try
            {
              


                    string uzytkownik = "";
                    Int64 uzytkownikId = 0;

                    if (items.Count > 0)
                    {
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

                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            message.Attachments = items;
                            foreach(DataRow dr in dt)
                            {
                                try
                                {
                                    var userAccount = new ChannelAccount(name: dr["UserName"].ToString(), id: dr["UserId"].ToString());
                                    uzytkownik = userAccount.Name;
                                    uzytkownikId = Convert.ToInt64(userAccount.Id);
                                    var botAccount = new ChannelAccount(name: dr["BotName"].ToString(), id: dr["BotId"].ToString());
                                    var connector = new ConnectorClient(new Uri(dr["Url"].ToString()), "73267226-823f-46b0-8303-2e866165a3b2", "k6XBDCgzL5452YDhS3PPLsL");
                                    var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                    message.From = botAccount;
                                    message.Recipient = userAccount;
                                    message.Conversation = new ConversationAccount(id: conversationId.Id, isGroup: false);
                                    await connector.Conversations.SendToConversationAsync((Activity)message).ConfigureAwait(false);

                                }
                                catch (Exception ex)
                                {                                    
                                    BaseDB.AddToLog("Błąd wysyłania wiadomości do: " + uzytkownik + " " + ex.ToString());
                                    BaseDB.DeleteUser(uzytkownikId);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            BaseDB.AddToLog("Błąd wysyłania wiadomości do: " + uzytkownik + " " + ex.ToString());
                        }
                }
            }
            catch (Exception ex)
            {
                BaseDB.AddToLog("Błąd wysłania wiadomosci: " + ex.ToString());
            }
        }
    }
}