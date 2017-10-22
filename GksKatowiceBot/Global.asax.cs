using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using System.Timers;
using GksKatowiceBot.Helpers;
using Microsoft.Bot.Connector;
using System.Data;
using System.Threading;

namespace GksKatowiceBot
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);


            Helpers.BaseDB.AddToLog("Wywołanie metody Application_Start");
           // Controllers.ThreadClass.SendThreadMessage();
            var aTimer = new System.Timers.Timer();
            aTimer.Interval = 3 * 60 * 1000;

            aTimer.Elapsed += OnTimedEvent;
            aTimer.Start();
        }
        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if (DateTime.UtcNow.Hour == 16 && (DateTime.UtcNow.Minute > 0 && DateTime.UtcNow.Minute <= 3))
            {
                Helpers.BaseDB.AddToLog("Wywołanie metody SendThreadMessage");

                DataTable dt = new DataTable();

                if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)
                {
                    dt = BaseGETMethod.GetUser(3);
                }
                else if (DateTime.Now.DayOfWeek == DayOfWeek.Wednesday || DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
                {
                    dt = BaseGETMethod.GetUser(2);
                }
                else
                {
                    dt = BaseGETMethod.GetUser(1);
                }

                Helpers.BaseDB.AddToLog("Liczba pobranych użytkowników do wysłania: " + dt.Rows.Count);

                List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                List<IGrouping<string, string>> hrefList2 = new List<IGrouping<string, string>>();
                List<IGrouping<string, string>> hreflist3 = new List<IGrouping<string, string>>();
                List<IGrouping<string, string>> hreflist4 = new List<IGrouping<string, string>>();
                var items = BaseGETMethod.GetCardsAttachments(ref hrefList);
                hreflist3 = hrefList;
                var items2 = BaseGETMethod.GetCardsAttachmentsOrlenLiga(ref hrefList2);
                var items4 = BaseGETMethod.GetCardsAttachmentsHokej(ref hreflist4);

                var items3 = new List<Attachment>();

                //if(items.Count>0)
                //{
                //    items3.Add(items[0]);
                //}

                if (items.Count > 0)
                {
                    items3.Add(items[0]);
                }
                if (items2.Count > 0)
                {
                    items3.Add(items2[0]);
                }
                if (items4.Count > 0)
                {
                    items3.Add(items4[0]);
                }

                if (items3.Count < 3)
                {
                    if (items.Count >= 2)
                    {
                        if (items3.Count == 2)
                        {
                            items3.Insert(1, items[1]);
                        }
                        else if (items3.Count == 1)
                        {
                            items3.Add(items[1]);
                            if (items.Count == 3)
                            {
                                items3.Add(items[2]);
                            }
                        }
                    }
                    else if (items2.Count >= 2)
                    {
                        items3.Add(items2[1]);
                    }
                }

                items = items3;

                int i= 0;
                while (i <= dt.Rows.Count)
                {
                    var listaUzytkownikow = dt.AsEnumerable().Skip(i).Take(50).ToList();
                    Controllers.ThreadClass.SendThreadMessage(listaUzytkownikow, items);
                    i += 50;
                    Thread.Sleep(1000);
                }
                BaseDB.AddToLog("Wysyłanie zostało zakończone");
                BaseDB.AddWiadomoscPilka(hreflist3);
                BaseDB.AddWiadomoscSiatka(hrefList2);
                BaseDB.AddWiadomoscHokej(hreflist4);
            }
            else
            {

            }
        }
    }
}
