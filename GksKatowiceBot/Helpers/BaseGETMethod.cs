using HtmlAgilityPack;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace GksKatowiceBot.Helpers
{
    public class BaseGETMethod
    {
        

          public static IList<Attachment> GetCardsAttachmentsHokej(ref List<IGrouping<string, string>> hrefList, bool newUser = false)
        {
            List<Attachment> list = new List<Attachment>();

            string urlAddress = "http://www.hokej.gkskatowice.eu/index";
            // string urlAddress = "http://www.orlenliga.pl/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                string data = readStream.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(data);

                string matchResultDivId = "carousel-inner";
                string xpath = String.Format("//div[@class='{0}']/div", matchResultDivId);
                var people = doc.DocumentNode.SelectNodes(xpath).Select(p => p.InnerHtml);
                string text = "";
                foreach (var person in people)
                {
                    text += person;
                }

                HtmlDocument doc2 = new HtmlDocument();

                doc2.LoadHtml(text);
                hrefList = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("href", "not found")).Where(p => p.Contains("/n/") || p.Contains("/video/") || p.Contains("/gallery/") || p.Contains("/blog/")).GroupBy(p => p.ToString())
                                  .ToList();

                var imgList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("src", "not found"))
                                  .ToList();

                var titleList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("alt", "not found"))
                                  .ToList();

                response.Close();
                readStream.Close();

                int index = hrefList.Count;

                DataTable dt = GetWiadomosciHokej();

                if (newUser == true)
                {
                    index = hrefList.Count;
                    if (dt.Rows.Count == 0)
                    {
                        //    AddWiadomosc(hrefList);
                    }
                }

                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        List<int> deleteList = new List<int>();
                        var listTemp = new List<System.Linq.IGrouping<string, string>>();
                        var imageListTemp = new List<string>();
                        var titleListTemp = new List<string>();

                        for (int i = 0; i < hrefList.Count; i++)
                        {
                            if (dt.Rows[dt.Rows.Count - 1]["Wiadomosc1"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc2"].ToString() != hrefList[i].Key &&
                                dt.Rows[dt.Rows.Count - 1]["Wiadomosc3"].ToString() != hrefList[i].Key)
                            {
                                listTemp.Add(hrefList[i]);
                                imageListTemp.Add(imgList[i]);
                                titleListTemp.Add(titleList[i].Replace("&quot;", ""));
                            }
                            listTemp2.Add(hrefList[i]);
                        }
                        hrefList = listTemp;
                        index = hrefList.Count;
                        imgList = imageListTemp;
                        titleList = titleListTemp;
                        //   AddWiadomosc(listTemp2);
                    }
                    else
                    {
                        index = hrefList.Count;
                        //   AddWiadomosc(hrefList);
                    }
                }

                for (int i = 0; i < index; i++)
                {
                    string link = "";
                    if (hrefList[i].Key.Contains("http"))
                    {
                        link = hrefList[i].Key;
                    }
                    else
                    {
                        link = "http://www.hokej.gkskatowice.eu" + hrefList[i].Key;
                        //link = "http://www.orlenliga.pl/" + hrefList[i].Key;
                    }

                    if (link.Contains("video"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Replace("&quot;", ""), "", "",
                        new CardImage(url: "http://www.hokej.gkskatowice.eu" + imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Oglądaj video", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                        if (link.Contains("gallery"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Replace("&quot;", ""), "", "",
                        new CardImage(url: "http://www.hokej.gkskatowice.eu" + imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Przeglądaj galerie", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Replace("&quot;", ""), "", "",
                        new CardImage(url: "http://www.hokej.gkskatowice.eu" + imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Więcej", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }

                    //  list.Add(new Microsoft.Bot.Connector.VideoCard(titleList[i], "", "",null)
                }
            }
            if (listTemp2.Count > 0)
            {
                hrefList = listTemp2;
            }

            return list;

        }

        public static IList<Attachment> GetCardsAttachmentsAkademia(ref List<IGrouping<string, string>> hrefList, bool newUser = false)
        {
            List<Attachment> list = new List<Attachment>();

            string urlAddress = "http://mloda-gieksa.pl/";
            // string urlAddress = "http://www.orlenliga.pl/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                string data = readStream.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(data);

                string matchResultDivId = "carousel-inner";
                string xpath = String.Format("//div[@class='{0}']/div", matchResultDivId);
                var people = doc.DocumentNode.SelectNodes(xpath).Select(p => p.InnerHtml);
                string text = "";
                foreach (var person in people)
                {
                    text += person;
                }

                HtmlDocument doc2 = new HtmlDocument();

                doc2.LoadHtml(text);
                hrefList = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("href", "not found")).Where(p => p.Contains("/n/") || p.Contains("/video/") || p.Contains("/gallery/") || p.Contains("/blog/")).GroupBy(p => p.ToString())
                                  .ToList();

                var imgList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("src", "not found"))
                                  .ToList();

                var titleList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("alt", "not found"))
                                  .ToList();

                response.Close();
                readStream.Close();

                int index = hrefList.Count;

                DataTable dt = GetWiadomosciHokej();

                if (newUser == true)
                {
                    index = hrefList.Count;
                    if (dt.Rows.Count == 0)
                    {
                        //    AddWiadomosc(hrefList);
                    }
                }

                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        List<int> deleteList = new List<int>();
                        var listTemp = new List<System.Linq.IGrouping<string, string>>();
                        var imageListTemp = new List<string>();
                        var titleListTemp = new List<string>();

                        for (int i = 0; i < hrefList.Count; i++)
                        {
                            if (dt.Rows[dt.Rows.Count - 1]["Wiadomosc1"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc2"].ToString() != hrefList[i].Key &&
                                dt.Rows[dt.Rows.Count - 1]["Wiadomosc3"].ToString() != hrefList[i].Key)
                            {
                                listTemp.Add(hrefList[i]);
                                imageListTemp.Add(imgList[i]);
                                titleListTemp.Add(titleList[i].Replace("&quot;", ""));
                            }
                            listTemp2.Add(hrefList[i]);
                        }
                        hrefList = listTemp;
                        index = hrefList.Count;
                        imgList = imageListTemp;
                        titleList = titleListTemp;
                        //   AddWiadomosc(listTemp2);
                    }
                    else
                    {
                        index = hrefList.Count;
                        //   AddWiadomosc(hrefList);
                    }
                }

                for (int i = 0; i < index; i++)
                {
                    string link = "";
                    if (hrefList[i].Key.Contains("http"))
                    {
                        link = hrefList[i].Key;
                    }
                    else
                    {
                        link = "http://mloda-gieksa.pl/" + hrefList[i].Key;
                        //link = "http://www.orlenliga.pl/" + hrefList[i].Key;
                    }

                    if (link.Contains("video"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Replace("&quot;", ""), "", "",
                        new CardImage(url: "http://mloda-gieksa.pl/" + imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Oglądaj video", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                        if (link.Contains("gallery"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Replace("&quot;", ""), "", "",
                        new CardImage(url: "http://mloda-gieksa.pl/" + imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Przeglądaj galerie", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Replace("&quot;", ""), "", "",
                        new CardImage(url: "http://mloda-gieksa.pl/" + imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Więcej", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }

                    //  list.Add(new Microsoft.Bot.Connector.VideoCard(titleList[i], "", "",null)
                }
            }
            if (listTemp2.Count > 0)
            {
                hrefList = listTemp2;
            }

            return list;

        }

        public static IList<Attachment> GetCardsAttachmentsStrefaGieksy(ref List<IGrouping<string, string>> hrefList, bool newUser = false)
        {
            List<Attachment> list = new List<Attachment>();

            string urlAddress = "http://strefagieksy.pl/";
            // string urlAddress = "http://www.orlenliga.pl/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                string data = readStream.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(data);

                string matchResultDivId = "homepage-slider";
                string xpath = String.Format("//div[@id='{0}']/div", matchResultDivId);
                var people = doc.DocumentNode.SelectNodes(xpath).Select(p => p.InnerHtml);
                string text = "";
                foreach (var person in people)
                {
                    text += person;
                }

                HtmlDocument doc2 = new HtmlDocument();

                doc2.LoadHtml(text);
                hrefList = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("href", "not found")).Where(p => p.Contains("/n/") || p.Contains("/video/") || p.Contains("/gallery/") || p.Contains("/blog/")).GroupBy(p => p.ToString())
                                  .ToList();

                var imgList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("src", "not found"))
                                  .ToList();

                var titleList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("alt", "not found"))
                                  .ToList();

                response.Close();
                readStream.Close();

                int index = hrefList.Count;

                DataTable dt = GetWiadomosciHokej();

                if (newUser == true)
                {
                    index = hrefList.Count;
                    if (dt.Rows.Count == 0)
                    {
                        //    AddWiadomosc(hrefList);
                    }
                }

                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        List<int> deleteList = new List<int>();
                        var listTemp = new List<System.Linq.IGrouping<string, string>>();
                        var imageListTemp = new List<string>();
                        var titleListTemp = new List<string>();

                        for (int i = 0; i < hrefList.Count; i++)
                        {
                            if (dt.Rows[dt.Rows.Count - 1]["Wiadomosc1"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc2"].ToString() != hrefList[i].Key &&
                                dt.Rows[dt.Rows.Count - 1]["Wiadomosc3"].ToString() != hrefList[i].Key)
                            {
                                listTemp.Add(hrefList[i]);
                                imageListTemp.Add(imgList[i]);
                                titleListTemp.Add(titleList[i].Replace("&quot;", ""));
                            }
                            listTemp2.Add(hrefList[i]);
                        }
                        hrefList = listTemp;
                        index = hrefList.Count;
                        imgList = imageListTemp;
                        titleList = titleListTemp;
                        //   AddWiadomosc(listTemp2);
                    }
                    else
                    {
                        index = hrefList.Count;
                        //   AddWiadomosc(hrefList);
                    }
                }

                for (int i = 0; i < index; i++)
                {
                    string link = "";
                    if (hrefList[i].Key.Contains("http"))
                    {
                        link = hrefList[i].Key;
                    }
                    else
                    {
                        link = "http://strefagieksy.pl/" + hrefList[i].Key;
                        //link = "http://www.orlenliga.pl/" + hrefList[i].Key;
                    }

                    if (link.Contains("video"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Replace("&quot;", ""), "", "",
                        new CardImage(url: "http://strefagieksy.pl/" + imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Oglądaj video", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                        if (link.Contains("gallery"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Replace("&quot;", ""), "", "",
                        new CardImage(url: "http://strefagieksy.pl/" + imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Przeglądaj galerie", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Replace("&quot;", ""), "", "",
                        new CardImage(url: "http://strefagieksy.pl/" + imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Więcej", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }

                    //  list.Add(new Microsoft.Bot.Connector.VideoCard(titleList[i], "", "",null)
                }
            }
            if (listTemp2.Count > 0)
            {
                hrefList = listTemp2;
            }

            return list;

        }


        public static IList<Attachment> GetCardsAttachments(ref List<IGrouping<string, string>> hrefList, bool newUser = false)
        {
            List<Attachment> list = new List<Attachment>();

            string urlAddress = "http://www.gkskatowice.eu/index";
            // string urlAddress = "http://www.orlenliga.pl/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                string data = readStream.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(data);

                string matchResultDivId = "carousel-inner";
                string xpath = String.Format("//div[@class='{0}']/div", matchResultDivId);
                var people = doc.DocumentNode.SelectNodes(xpath).Select(p => p.InnerHtml);
                string text = "";
                foreach (var person in people)
                {
                    text += person;
                }

                HtmlDocument doc2 = new HtmlDocument();

                doc2.LoadHtml(text);
                hrefList = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("href", "not found")).Where(p => p.Contains("/n/") || p.Contains("/video/") || p.Contains("/gallery/") || p.Contains("/blog/")).GroupBy(p => p.ToString())
                                  .ToList();

                var imgList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("src", "not found"))
                                  .ToList();

                var titleList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("alt", "not found"))
                                  .ToList();

                response.Close();
                readStream.Close();

                int index = 5;

                DataTable dt = GetWiadomosciPilka();

                if (newUser == true)
                {
                    index = 5;
                    if (dt.Rows.Count == 0)
                    {
                        //    AddWiadomosc(hrefList);
                    }
                }

                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        List<int> deleteList = new List<int>();
                        var listTemp = new List<System.Linq.IGrouping<string, string>>();
                        var imageListTemp = new List<string>();
                        var titleListTemp = new List<string>();

                        for (int i = 0; i < hrefList.Count; i++)
                        {
                            if (dt.Rows[dt.Rows.Count - 1]["Wiadomosc1"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc2"].ToString() != hrefList[i].Key &&
                                dt.Rows[dt.Rows.Count - 1]["Wiadomosc3"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc4"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc5"].ToString() != hrefList[i].Key
                            )
                            {
                                listTemp.Add(hrefList[i]);
                                imageListTemp.Add(imgList[i]);
                                titleListTemp.Add(titleList[i].Replace("&quot;", ""));
                            }
                            listTemp2.Add(hrefList[i]);
                        }
                        hrefList = listTemp;
                        index = hrefList.Count;
                        imgList = imageListTemp;
                        titleList = titleListTemp;
                        //   AddWiadomosc(listTemp2);
                    }
                    else
                    {
                        index = 5;
                        //   AddWiadomosc(hrefList);
                    }
                }

                for (int i = 0; i < index; i++)
                {
                    string link = "";
                    if (hrefList[i].Key.Contains("http"))
                    {
                        link = hrefList[i].Key;
                    }
                    else
                    {
                        link = "http://www.gkskatowice.eu" + hrefList[i].Key;
                        //link = "http://www.orlenliga.pl/" + hrefList[i].Key;
                    }

                    if (link.Contains("video"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Replace("&quot;", ""), "", "",
                        new CardImage(url: "http://www.gkskatowice.eu" + imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Oglądaj video", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                        if (link.Contains("gallery"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Replace("&quot;", ""), "", "",
                        new CardImage(url: "http://www.gkskatowice.eu" + imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Przeglądaj galerie", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Replace("&quot;", ""), "", "",
                        new CardImage(url: "http://www.gkskatowice.eu" + imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Więcej", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }

                    //  list.Add(new Microsoft.Bot.Connector.VideoCard(titleList[i], "", "",null)
                }
            }
            if (listTemp2.Count > 0)
            {
                hrefList = listTemp2;
            }

            return list;
            //return new List<Attachment>()
            //{
            //    for(int i=0;i<5;i++)
            //    GetHeroCard(
            //        "Azure Storage",
            //        "Massively scalable cloud storage for your applications",
            //        "Store and help protect your data. Get durable, highly available data storage across the globe and pay only for what you use.",
            //        new CardImage(url: "https://acom.azurecomcdn.net/80C57D/cdn/mediahandler/docarticles/dpsmedia-prod/azure.microsoft.com/en-us/documentation/articles/storage-introduction/20160801042915/storage-concepts.png"),
            //        new CardAction(ActionTypes.OpenUrl, "Learn more", value: "https://azure.microsoft.com/en-us/services/storage/")),
            //  //  GetThumbnailCard(
            //    //    "DocumentDB",
            //    //    "Blazing fast, planet-scale NoSQL",
            //    //    "NoSQL service for highly available, globally distributed apps—take full advantage of SQL and JavaScript over document and key-value data without the hassles of on-premises or virtual machine-based cloud database options.",
            //    //    new CardImage(url: "https://sec.ch9.ms/ch9/29f4/beb4b953-ab91-4a31-b16a-71fb6d6829f4/WhatisAzureDocumentDB_960.jpg"),
            //    //    new CardAction(ActionTypes.OpenUrl, "Learn more", value: "https://azure.microsoft.com/en-us/services/documentdb/")),
            //    //GetHeroCard(
            //    //    "Azure Functions",
            //    //    "Process events with serverless code",
            //    //    "Azure Functions is a serverless event driven experience that extends the existing Azure App Service platform. These nano-services can scale based on demand and you pay only for the resources you consume.",
            //    //    new CardImage(url: "https://azurecomcdn.azureedge.net/cvt-8636d9bb8d979834d655a5d39d1b4e86b12956a2bcfdb8beb04730b6daac1b86/images/page/services/functions/azure-functions-screenshot.png"),
            //    //    new CardAction(ActionTypes.OpenUrl, "Learn more", value: "https://azure.microsoft.com/en-us/services/functions/")),
            //    //GetThumbnailCard(
            //    //    "Cognitive Services",
            //    //    "Build powerful intelligence into your applications to enable natural and contextual interactions",
            //    //    "Enable natural and contextual interaction with tools that augment users' experiences using the power of machine-based intelligence. Tap into an ever-growing collection of powerful artificial intelligence algorithms for vision, speech, language, and knowledge.",
            //    //    new CardImage(url: "https://azurecomcdn.azureedge.net/cvt-8636d9bb8d979834d655a5d39d1b4e86b12956a2bcfdb8beb04730b6daac1b86/images/page/services/functions/azure-functions-screenshot.png"),
            //    //    new CardAction(ActionTypes.OpenUrl, "Learn more", value: "https://azure.microsoft.com/en-us/services/functions/")),

            //};
        }
        public static IList<Attachment> GetCardsAttachmentsOrlenLiga(ref List<IGrouping<string, string>> hrefList, bool newUser = false)
        {
            List<Attachment> list = new List<Attachment>();

            // string urlAddress = "http://www.plusliga.pl";
            string urlAddress = "http://siatkowka.gkskatowice.eu/index";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                string data = readStream.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(data);

                string matchResultDivId = "carousel-inner";
                string xpath = String.Format("//div[@class='{0}']/div", matchResultDivId);
                var people = doc.DocumentNode.SelectNodes(xpath).Select(p => p.InnerHtml);
                string text = "";
                foreach (var person in people)
                {
                    text += person;
                }

                HtmlDocument doc2 = new HtmlDocument();

                doc2.LoadHtml(text);
                hrefList = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("href", "not found")).Where(p => p.Contains("/n/") || p.Contains("/video/") || p.Contains("/gallery/")).GroupBy(p => p.ToString())
                                  .ToList();

                var imgList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("src", "not found"))
                                  .ToList();

                var titleList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("alt", "not found"))
                                  .ToList();

                response.Close();
                readStream.Close();

                int index = 5;

                DataTable dt = GetWiadomosciSiatka();

                if (newUser == true)
                {
                    index = 5;
                    if (dt.Rows.Count == 0)
                    {
                        //    AddWiadomosc(hrefList);
                    }
                }

                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        List<int> deleteList = new List<int>();
                        var listTemp = new List<System.Linq.IGrouping<string, string>>();
                        var imageListTemp = new List<string>();
                        var titleListTemp = new List<string>();

                        for (int i = 0; i < hrefList.Count; i++)
                        {
                            if (dt.Rows[dt.Rows.Count - 1]["Wiadomosc1"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc2"].ToString() != hrefList[i].Key &&
                                dt.Rows[dt.Rows.Count - 1]["Wiadomosc3"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc4"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc5"].ToString() != hrefList[i].Key
                            )
                            {
                                listTemp.Add(hrefList[i]);
                                imageListTemp.Add(imgList[i]);
                                titleListTemp.Add(titleList[i]);
                            }
                            listTemp2.Add(hrefList[i]);
                        }
                        hrefList = listTemp;
                        index = hrefList.Count;
                        imgList = imageListTemp;
                        titleList = titleListTemp;
                        //   AddWiadomosc(listTemp2);
                    }
                    else
                    {
                        index = 5;
                        //   AddWiadomosc(hrefList);
                    }
                }

                for (int i = 0; i < index; i++)
                {
                    string link = "";
                    if (hrefList[i].Key.Contains("http"))
                    {
                        link = hrefList[i].Key;
                    }
                    else
                    {
                        //   link = "http://plusliga.pl" + hrefList[i].Key;
                        link = "http://siatkowka.gkskatowice.eu" + hrefList[i].Key;
                    }
                    if (link.Contains("video"))
                    {
                        list.Add(GetHeroCard(
                         titleList[i], "", "",
                        new CardImage(url: "http://siatkowka.gkskatowice.eu" + imgList[i]),
                         new CardAction(ActionTypes.OpenUrl, "Oglądaj video", value: link),
                         new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                        if (link.Contains("gallery"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i], "", "",
                        new CardImage(url: "http://siatkowka.gkskatowice.eu" + imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Przeglądaj galerie", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Replace("&quot;", ""), "", "",
                        new CardImage(url: "http://siatkowka.gkskatowice.eu" + imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Więcej", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }


                    //  list.Add(new Microsoft.Bot.Connector.VideoCard(titleList[i], "", "",null)
                }
            }
            if (listTemp2.Count > 0)
            {
                hrefList = listTemp2;
            }
            return list;
            //return new List<Attachment>()
            //{
            //    for(int i=0;i<5;i++)
            //    GetHeroCard(
            //        "Azure Storage",
            //        "Massively scalable cloud storage for your applications",
            //        "Store and help protect your data. Get durable, highly available data storage across the globe and pay only for what you use.",
            //        new CardImage(url: "https://acom.azurecomcdn.net/80C57D/cdn/mediahandler/docarticles/dpsmedia-prod/azure.microsoft.com/en-us/documentation/articles/storage-introduction/20160801042915/storage-concepts.png"),
            //        new CardAction(ActionTypes.OpenUrl, "Learn more", value: "https://azure.microsoft.com/en-us/services/storage/")),
            //  //  GetThumbnailCard(
            //    //    "DocumentDB",
            //    //    "Blazing fast, planet-scale NoSQL",
            //    //    "NoSQL service for highly available, globally distributed apps—take full advantage of SQL and JavaScript over document and key-value data without the hassles of on-premises or virtual machine-based cloud database options.",
            //    //    new CardImage(url: "https://sec.ch9.ms/ch9/29f4/beb4b953-ab91-4a31-b16a-71fb6d6829f4/WhatisAzureDocumentDB_960.jpg"),
            //    //    new CardAction(ActionTypes.OpenUrl, "Learn more", value: "https://azure.microsoft.com/en-us/services/documentdb/")),
            //    //GetHeroCard(
            //    //    "Azure Functions",
            //    //    "Process events with serverless code",
            //    //    "Azure Functions is a serverless event driven experience that extends the existing Azure App Service platform. These nano-services can scale based on demand and you pay only for the resources you consume.",
            //    //    new CardImage(url: "https://azurecomcdn.azureedge.net/cvt-8636d9bb8d979834d655a5d39d1b4e86b12956a2bcfdb8beb04730b6daac1b86/images/page/services/functions/azure-functions-screenshot.png"),
            //    //    new CardAction(ActionTypes.OpenUrl, "Learn more", value: "https://azure.microsoft.com/en-us/services/functions/")),
            //    //GetThumbnailCard(
            //    //    "Cognitive Services",
            //    //    "Build powerful intelligence into your applications to enable natural and contextual interactions",
            //    //    "Enable natural and contextual interaction with tools that augment users' experiences using the power of machine-based intelligence. Tap into an ever-growing collection of powerful artificial intelligence algorithms for vision, speech, language, and knowledge.",
            //    //    new CardImage(url: "https://azurecomcdn.azureedge.net/cvt-8636d9bb8d979834d655a5d39d1b4e86b12956a2bcfdb8beb04730b6daac1b86/images/page/services/functions/azure-functions-screenshot.png"),
            //    //    new CardAction(ActionTypes.OpenUrl, "Learn more", value: "https://azure.microsoft.com/en-us/services/functions/")),

            //};
        }


        public static IList<Attachment> GetCardsAttachmentsFoto()
        {
            List<Attachment> list = new List<Attachment>();
           // list.Add(GetHeroCard(
           // "", "", "",
           //new CardImage(url: "http://tomasoft.pl/pub/GKSKatowice/przeklenstwo.jpg"),
           //null,null)
           //            // new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
           //             );
            list.Add(new Attachment()
            {
                ContentUrl = "http://tomasoft.pl/pub/GKSKatowice/przeklenstwo.jpg",
                ContentType = "image/jpg",
                Name = "przeklenstwo.jpg"
            });
            return list;
        }

        public static IList<Attachment> GetCardsAttachmentsPowitanie()
        {
            List<Attachment> list = new List<Attachment>();
            // list.Add(GetHeroCard(
            // "", "", "",
            //new CardImage(url: "http://tomasoft.pl/pub/GKSKatowice/powitanie.jpg"),
            //null,null)
            //            // new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
            //             );
            list.Add(new Attachment()
            {
                ContentUrl = "http://tomasoft.pl/pub/GKSKatowice/powitanie.jpg",
                ContentType = "image/jpg",
                Name = "przeklenstwo.jpg"
            });
            return list;
        }

        public static IList<Attachment> GetCardsAttachmentsFotoKibice()
        {
            List<Attachment> list = new List<Attachment>();
            // list.Add(GetHeroCard(
            // "", "", "",
            //new CardImage(url: "http://tomasoft.pl/pub/GKSKatowice/przeklenstwo.jpg"),
            //null,null)
            //            // new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
            //             );
            list.Add(new Attachment()
            {
                ContentUrl = "http://tomasoft.pl/pub/GKSKatowice/kibice.jpg",
                ContentType = "image/jpg",
                Name = "kibice.jpg"
            });
            return list;
        }

        public static IList<Attachment> GetCardsAttachmentsFotoGieksiak()
        {
            List<Attachment> list = new List<Attachment>();
            // list.Add(GetHeroCard(
            // "", "", "",
            //new CardImage(url: "http://tomasoft.pl/pub/GKSKatowice/przeklenstwo.jpg"),
            //null,null)
            //            // new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
            //             );
            list.Add(new Attachment()
            {
                ContentUrl = "http://tomasoft.pl/pub/GKSKatowice/gieksiak.jpg",
                ContentType = "image/jpg",
                Name = "kibice.jpg"
            });
            return list;
        }

        public static IList<Attachment> GetCardsAttachmentsGaleria(ref List<IGrouping<string, string>> hrefList, bool newUser = false)
        {
            List<Attachment> list = new List<Attachment>();

            string urlAddress = "http://www.gkskatowice.eu/index";
            // string urlAddress = "http://www.orlenliga.pl/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                string data = readStream.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(data);

                string matchResultDivId = "carousel-inner";
                string xpath = String.Format("//div[@class='{0}']/div", matchResultDivId);
                var people = doc.DocumentNode.SelectNodes(xpath).Select(p => p.InnerHtml);
                string text = "";
                foreach (var person in people)
                {
                    text += person;
                }

                HtmlDocument doc2 = new HtmlDocument();

                doc2.LoadHtml(text);
                hrefList = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("href", "not found")).Where(p => p.Contains("/n/") || p.Contains("/video/") || p.Contains("/gallery/") || p.Contains("/blog/")).GroupBy(p => p.ToString())
                                  .ToList();

                var imgList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("src", "not found"))
                                  .ToList();

                var titleList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("alt", "not found"))
                                  .ToList();

                response.Close();
                readStream.Close();

                int index = 5;

                DataTable dt = GetWiadomosciPilka();

                if (newUser == true)
                {
                    index = 5;
                    if (dt.Rows.Count == 0)
                    {
                        //    AddWiadomosc(hrefList);
                    }
                }

                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        List<int> deleteList = new List<int>();
                        var listTemp = new List<System.Linq.IGrouping<string, string>>();
                        var imageListTemp = new List<string>();
                        var titleListTemp = new List<string>();

                        for (int i = 0; i < hrefList.Count; i++)
                        {
                            if (dt.Rows[dt.Rows.Count - 1]["Wiadomosc1"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc2"].ToString() != hrefList[i].Key &&
                                dt.Rows[dt.Rows.Count - 1]["Wiadomosc3"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc4"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc5"].ToString() != hrefList[i].Key
                            )
                            {
                                listTemp.Add(hrefList[i]);
                                imageListTemp.Add("http://www.gkskatowice.eu" + imgList[i]);
                                titleListTemp.Add(titleList[i].Replace("&quot;", ""));
                            }
                            listTemp2.Add(hrefList[i]);
                        }
                        hrefList = listTemp;
                        index = hrefList.Count;
                        imgList = imageListTemp;
                        titleList = titleListTemp;
                        //   AddWiadomosc(listTemp2);
                    }
                    else
                    {
                        index = 5;
                        //   AddWiadomosc(hrefList);
                    }
                }

                for (int i = 0; i < index; i++)
                {
                    string link = "";
                    if (hrefList[i].Key.Contains("http"))
                    {
                        link = hrefList[i].Key;
                    }
                    else
                    {
                        link = "http://www.gkskatowice.eu" + hrefList[i].Key;
                        //link = "http://www.orlenliga.pl/" + hrefList[i].Key;
                    }

                    if (link.Contains("video"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Replace("&quot;", ""), "", "",
                        new CardImage(url: imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Oglądaj video", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                        if (link.Contains("gallery"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Replace("&quot;", ""), "", "",
                        new CardImage(url: imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Przeglądaj galerie", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Replace("&quot;", ""), "", "",
                        new CardImage(url: "http://www.gkskatowice.eu" + imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Więcej", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }

                    //  list.Add(new Microsoft.Bot.Connector.VideoCard(titleList[i], "", "",null)
                }
            }
            if (listTemp2.Count > 0)
            {
                hrefList = listTemp2;
            }

            return list;
            //return new List<Attachment>()
            //{
            //    for(int i=0;i<5;i++)
            //    GetHeroCard(
            //        "Azure Storage",
            //        "Massively scalable cloud storage for your applications",
            //        "Store and help protect your data. Get durable, highly available data storage across the globe and pay only for what you use.",
            //        new CardImage(url: "https://acom.azurecomcdn.net/80C57D/cdn/mediahandler/docarticles/dpsmedia-prod/azure.microsoft.com/en-us/documentation/articles/storage-introduction/20160801042915/storage-concepts.png"),
            //        new CardAction(ActionTypes.OpenUrl, "Learn more", value: "https://azure.microsoft.com/en-us/services/storage/")),
            //  //  GetThumbnailCard(
            //    //    "DocumentDB",
            //    //    "Blazing fast, planet-scale NoSQL",
            //    //    "NoSQL service for highly available, globally distributed apps—take full advantage of SQL and JavaScript over document and key-value data without the hassles of on-premises or virtual machine-based cloud database options.",
            //    //    new CardImage(url: "https://sec.ch9.ms/ch9/29f4/beb4b953-ab91-4a31-b16a-71fb6d6829f4/WhatisAzureDocumentDB_960.jpg"),
            //    //    new CardAction(ActionTypes.OpenUrl, "Learn more", value: "https://azure.microsoft.com/en-us/services/documentdb/")),
            //    //GetHeroCard(
            //    //    "Azure Functions",
            //    //    "Process events with serverless code",
            //    //    "Azure Functions is a serverless event driven experience that extends the existing Azure App Service platform. These nano-services can scale based on demand and you pay only for the resources you consume.",
            //    //    new CardImage(url: "https://azurecomcdn.azureedge.net/cvt-8636d9bb8d979834d655a5d39d1b4e86b12956a2bcfdb8beb04730b6daac1b86/images/page/services/functions/azure-functions-screenshot.png"),
            //    //    new CardAction(ActionTypes.OpenUrl, "Learn more", value: "https://azure.microsoft.com/en-us/services/functions/")),
            //    //GetThumbnailCard(
            //    //    "Cognitive Services",
            //    //    "Build powerful intelligence into your applications to enable natural and contextual interactions",
            //    //    "Enable natural and contextual interaction with tools that augment users' experiences using the power of machine-based intelligence. Tap into an ever-growing collection of powerful artificial intelligence algorithms for vision, speech, language, and knowledge.",
            //    //    new CardImage(url: "https://azurecomcdn.azureedge.net/cvt-8636d9bb8d979834d655a5d39d1b4e86b12956a2bcfdb8beb04730b6daac1b86/images/page/services/functions/azure-functions-screenshot.png"),
            //    //    new CardAction(ActionTypes.OpenUrl, "Learn more", value: "https://azure.microsoft.com/en-us/services/functions/")),

            //};
        }




        public static IList<Attachment> GetCardsAttachmentsExtra(ref List<IGrouping<string, string>> hrefList, bool newUser = false,string urlAddress="",string tytul="",string imgLink="")
        {
            List<Attachment> list = new List<Attachment>();

          //  string urlAddress = "http://www.gkskatowice.eu/index";
        //    // string urlAddress = "http://www.orlenliga.pl/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                        list.Add(GetHeroCard(
                        tytul, "", "",
                        new CardImage(url: imgLink),
                        new CardAction(ActionTypes.OpenUrl, "Zobacz", value: urlAddress),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + urlAddress))
                        );

            }
            if (listTemp2.Count > 0)
            {
                hrefList = listTemp2;
            }

            return list;
            
        }

        public static IList<Attachment> GetCardsAttachmentsBilety(ref List<IGrouping<string, string>> hrefList, bool newUser = false, string urlAddress = "", string tytul = "")
        {
            List<Attachment> list = new List<Attachment>();

                list.Add(GetHeroCard(
"Piłka nożna - bilety", "", "",
new CardImage(url: "http://gkskatowice.eu/uploads/assets/images/x2.jpg"),
new CardAction(ActionTypes.OpenUrl, "Zobacz", value: "http://gkskatowice.eu/page/bilety"),
new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + "http://gkskatowice.eu/page/bilety"))
);
                list.Add(GetHeroCard(
"Siatkówka - bilety", "", "",
new CardImage(url: "http://siatkowka.gkskatowice.eu/uploads/assets/images/bilety_cennik(1).jpg"),
new CardAction(ActionTypes.OpenUrl, "Zobacz", value: "http://siatkowka.gkskatowice.eu/page/bilety"),
new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + "http://siatkowka.gkskatowice.eu/page/bilety"))
);
            list.Add(GetHeroCard(
"Hokej - bilety", "", "",
new CardImage(url: "http://hokej.gkskatowice.eu/uploads/assets/images/cennik_bilety.png"),
new CardAction(ActionTypes.OpenUrl, "Zobacz", value: "http://hokej.gkskatowice.eu/page/bilety"),
new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + "http://hokej.gkskatowice.eu/page/bilety"))
);



            return list;

        }

        public static IList<Attachment> GetCardsAttachmentsKarnety(ref List<IGrouping<string, string>> hrefList, bool newUser = false, string urlAddress = "", string tytul = "")
        {
            List<Attachment> list = new List<Attachment>();

            list.Add(GetHeroCard(
            "Piłka nożna - karnety", "", "",
            new CardImage(url: "http://gkskatowice.eu/uploads/assets/images/CENNIK_KARNETOW_WIOSNA_2017(2).jpg"),
            new CardAction(ActionTypes.OpenUrl, "Zobacz", value: "http://gkskatowice.eu/page/karnety"),
            new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + "http://gkskatowice.eu/page/karnety"))
            );
            list.Add(GetHeroCard(
"Siatkówka - karnety", "", "",
new CardImage(url: "http://siatkowka.gkskatowice.eu/uploads/assets/images/nowycennikkarnetow(1).jpg"),
new CardAction(ActionTypes.OpenUrl, "Zobacz", value: "http://siatkowka.gkskatowice.eu/page/karnety"),
new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + "http://siatkowka.gkskatowice.eu/page/karnety"))
);
            list.Add(GetHeroCard(
"Hokej - karnety", "", "",
new CardImage(url: "http://hokej.gkskatowice.eu/uploads/assets/images/najnowszycennik.jpg"),
new CardAction(ActionTypes.OpenUrl, "Zobacz", value: "http://hokej.gkskatowice.eu/page/karnety"),
new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + "http://hokej.gkskatowice.eu/page/karnety"))
);

            return list;

        }

        public static IList<Attachment> GetCardsAttachmentsTabelaTerminarz(ref List<IGrouping<string, string>> hrefList, bool newUser = false, string urlAddress = "", string tytul = "")
        {
            List<Attachment> list = new List<Attachment>();

            list.Add(GetHeroCard(
            "Piłka nożna - tabela i terminarz", "", "",
            new CardImage(url: ""),
            new CardAction(ActionTypes.OpenUrl, "Zobacz", value: "http://gkskatowice.eu/page/tabela-terminarz"),
            new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + "http://gkskatowice.eu/page/tabela-terminarz"))
            );
            list.Add(GetHeroCard(
"Siatkówka - tabela i terminarz", "", "",
new CardImage(url: ""),
new CardAction(ActionTypes.OpenUrl, "Zobacz", value: "http://siatkowka.gkskatowice.eu/page/tabela-terminarz"),
new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + "http://siatkowka.gkskatowice.eu/page/tabela-terminarz"))
);
            list.Add(GetHeroCard(
"Hokej - tabela i terminarz", "", "",
new CardImage(url: ""),
new CardAction(ActionTypes.OpenUrl, "Zobacz", value: "http://hokej.gkskatowice.eu/page/tabela-terminarz"),
new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + "http://hokej.gkskatowice.eu/page/tabela-terminarz"))
);

            return list;

        }

        public static IList<Attachment> GetCardsAttachmentsSklad(ref List<IGrouping<string, string>> hrefList, bool newUser = false, string urlAddress = "", string tytul = "")
        {
            List<Attachment> list = new List<Attachment>();

            list.Add(GetHeroCard(
            "Piłka nożna - skład ", "", "",
            new CardImage(url: ""),
            new CardAction(ActionTypes.OpenUrl, "Zobacz", value: "http://gkskatowice.eu/page/i-zesp"),
            new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + "http://gkskatowice.eu/page/i-zesp "))
            );
            list.Add(GetHeroCard(
"Siatkówka - skład", "", "",
new CardImage(url: ""),
new CardAction(ActionTypes.OpenUrl, "Zobacz", value: "http://siatkowka.gkskatowice.eu/page/i-zespol"),
new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + "http://siatkowka.gkskatowice.eu/page/i-zespol"))
);
            list.Add(GetHeroCard(
"Hokej - skład", "", "",
new CardImage(url: ""),
new CardAction(ActionTypes.OpenUrl, "Zobacz", value: "http://hokej.gkskatowice.eu/page/i-zespol "),
new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + "http://hokej.gkskatowice.eu/page/i-zespol"))
);

            return list;

        }

        private static Attachment GetHeroCard(string title, string subtitle, string text, CardImage cardImage, CardAction cardAction, CardAction cardAction2)
        {
            if (cardAction2 != null)
            {
                var heroCard = new HeroCard
                {
                    Title = title,
                    Subtitle = subtitle,
                    Text = text,
                    Images = new List<CardImage>() { cardImage },
                    Buttons = new List<CardAction>() { cardAction, cardAction2 },
                };

                return heroCard.ToAttachment();
            }
            else
            {
                var heroCard = new HeroCard
                {
                    Title = title,
                    Subtitle = subtitle,
                    Text = text,
                    Images = new List<CardImage>() { cardImage },
                    Buttons = new List<CardAction>() { cardAction },
                };

                return heroCard.ToAttachment();
            }
        }

  
        public static DataTable GetWiadomosciPilka()
        {
            DataTable dt = new DataTable();

            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();

                cmd.CommandText = "SELECT * FROM [dbo].[WiadomosciGKSKatowicePilka]";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                sqlConnection1.Close();
                return dt;
            }
            catch
            {
                BaseDB.AddToLog("Błąd pobierania wiadomości");
                return null;
            }
        }


        public static DataTable GetWiadomosciHokej()
        {
            DataTable dt = new DataTable();

            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();

                cmd.CommandText = "SELECT * FROM [dbo].[WiadomosciGKSKatowiceHokej]";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                sqlConnection1.Close();
                return dt;
            }
            catch
            {
                BaseDB.AddToLog("Błąd pobierania wiadomości");
                return null;
            }
        }
        public static DataTable GetUser()
        {
            DataTable dt = new DataTable();

            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();

                cmd.CommandText = "SELECT * FROM [dbo].[UserGKSKatowice] where flgDeleted=0";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                sqlConnection1.Close();
                return dt;
            }
            catch
            {
                BaseDB.AddToLog("Błąd pobierania użytkowników");
                return null;
            }
        }

        public static DataTable GetWiadomosciSiatka()
        {
            DataTable dt = new DataTable();

            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();

                cmd.CommandText = "SELECT * FROM [dbo].[WiadomosciGKSKatowiceSiatka]";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                sqlConnection1.Close();
                return dt;
            }
            catch
            {
                BaseDB.AddToLog("Błąd pobierania wiadomości Orlen");
                return null;
            }
        }

        public static IList<Attachment> GetCardsAttachmentsGallery(ref List<IGrouping<string, string>> hrefList, bool newUser = false,byte rodzajStrony=0)
        {
            List<Attachment> list = new List<Attachment>();

            string urlAddress = "http://www.gkskatowice.eu/index";

            switch (rodzajStrony)
            {
                case 1:
                    urlAddress = "http://siatkowka.gkskatowice.eu/index";
                    break;
                case 2:
                    urlAddress = "http://hokej.gkskatowice.eu/index";
                    break;
            }

            // string urlAddress = "http://www.orlenliga.pl/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                string data = readStream.ReadToEnd();

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(data);

                string matchResultDivId = "box-href";
                string xpath = String.Format("//li[@class='{0}']", matchResultDivId);

                var people = doc.DocumentNode.SelectNodes(xpath).Select(p => p.InnerHtml); ;
                string text = "";
                foreach (var person in people)
                {
                    if (person.Contains("camera-icon"))
                    {
                        text += person;

                    }
                }

                HtmlDocument doc2 = new HtmlDocument();

                doc2.LoadHtml(text);

                hrefList = doc2.DocumentNode.SelectNodes("//a")
               .Select(p => p.GetAttributeValue("href", "not found")).Where(p => p.Contains("/media/") || p.Contains("/video/") || p.Contains("/gallery/") || p.Contains("/blog/")).GroupBy(p => p.ToString())
               .ToList();

                var imgList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("src", "not found")).Where(p => p.Contains("/uploads/"))
                                  .ToList();



                var titleList = doc2.DocumentNode.SelectNodes("//h4").Select(p => p.ChildNodes)
                                  .ToList();

                response.Close();
                readStream.Close();

                int index = 5;


                for (int i = 0; i < index; i++)
                {
                    string link = "";
                    if (hrefList[i].Key.Contains("http"))
                    {
                        link = hrefList[i].Key;
                    }
                    else
                    {
                        if (rodzajStrony == 0)
                        {
                            link = "http://www.gkskatowice.eu" + hrefList[i].Key;
                        }
                        else if (rodzajStrony == 1)
                        {
                            link= "http://siatkowka.gkskatowice.eu"+ hrefList[i].Key;
                        }
                        else if (rodzajStrony == 2)
                        {
                            link = "http://hokej.gkskatowice.eu" + hrefList[i].Key;
                        }
                    }

                    list.Add(GetHeroCard(
                    titleList[i][0].InnerHtml.Replace("&quot;", ""), "", "",
                    new CardImage(url: urlAddress.Replace("/index","")+ imgList[i]),
                    new CardAction(ActionTypes.OpenUrl, "Przeglądaj galerie", value: link),
                    new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                    );

                }
            }
            return list;

        }
        public static IList<Attachment> GetCardsAttachmentsVideo(ref List<IGrouping<string, string>> hrefList, bool newUser = false,byte rodzajStrony=0)
        {
            List<Attachment> list = new List<Attachment>();

            string urlAddress = "http://www.gkskatowice.eu/index";

            switch (rodzajStrony)
            {
                case 1:
                    urlAddress = "http://siatkowka.gkskatowice.eu/index";
                    break;
                case 2:
                    urlAddress = "http://hokej.gkskatowice.eu/index";
                    break;
            }

            // string urlAddress = "http://www.orlenliga.pl/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                string data = readStream.ReadToEnd();

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(data);

                string matchResultDivId = "box-href";
                string xpath = String.Format("//li[@class='{0}']", matchResultDivId);

                var people = doc.DocumentNode.SelectNodes(xpath).Select(p => p.InnerHtml); ;
                string text = "";
                foreach (var person in people)
                {
                    if (person.Contains("video-icon"))
                    {
                        text += person;

                    }
                }

                HtmlDocument doc2 = new HtmlDocument();

                doc2.LoadHtml(text);

                hrefList = doc2.DocumentNode.SelectNodes("//a")
               .Select(p => p.GetAttributeValue("href", "not found")).Where(p => p.Contains("/media/") || p.Contains("/video/") || p.Contains("/gallery/") || p.Contains("/blog/")).GroupBy(p => p.ToString())
               .ToList();

                var imgList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("src", "not found")).Where(p => p.Contains("/uploads/"))
                                  .ToList();



                var titleList = doc2.DocumentNode.SelectNodes("//h4").Select(p => p.ChildNodes)
                                  .ToList();

                response.Close();
                readStream.Close();

                int index = 5;


                for (int i = 0; i < index; i++)
                {
                    string link = "";
                    if (hrefList[i].Key.Contains("http"))
                    {
                        link = hrefList[i].Key;
                    }
                    else
                    {
                        if (rodzajStrony == 0)
                        {
                            link = "http://www.gkskatowice.eu" + hrefList[i].Key;
                        }
                        else if (rodzajStrony == 1)
                        {
                            link = "http://siatkowka.gkskatowice.eu" + hrefList[i].Key;
                        }
                        else if (rodzajStrony == 2)
                        {
                            link = "http://hokej.gkskatowice.eu" + hrefList[i].Key;
                        }
                        //link = "http://www.orlenliga.pl/" + hrefList[i].Key;
                    }

                    list.Add(GetHeroCard(
                    titleList[i][0].InnerHtml.Replace("&quot;", ""), "", "",
                    new CardImage(url: urlAddress.Replace("/index","") + imgList[i]),
                    new CardAction(ActionTypes.OpenUrl, "Oglądaj", value: link),
                    new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                    );

                }
            }
            return list;

        }

    }
}