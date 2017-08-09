using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace GksKatowiceBot.Helpers
{
    public class BaseDB
    {
        public static void AddToLog(string action)
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                cmd.CommandText = "INSERT INTO LogGKSKatowice (Tresc) VALUES ('" + action + " " + DateTime.Now.ToString() + "')";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                cmd.ExecuteNonQuery();

                sqlConnection1.Close();
            }
            catch (Exception ex)
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                cmd.CommandText = "INSERT INTO LogGKSKatowice (Tresc) VALUES ('" + "Błąd dodawania wiadomosci do Loga" + " " + DateTime.Now.ToString() + "')";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                cmd.ExecuteNonQuery();

                sqlConnection1.Close();
            }
        }
        public static void AddUser(string UserName, string UserId, string BotName, string BotId, string Url, byte flgTyp)
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                cmd.CommandText = "addUserGKSKatowice";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserName", UserName);
                cmd.Parameters.AddWithValue("@UserId", UserId);
                cmd.Parameters.AddWithValue("@BotName", BotName);
                cmd.Parameters.AddWithValue("@BotId", BotId);
                cmd.Parameters.AddWithValue("@UrlStr", "https://facebook.botframework.com");
                cmd.Parameters.AddWithValue("@flgTyp", flgTyp);
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                cmd.ExecuteNonQuery();

                sqlConnection1.Close();
            }
            catch (Exception ex)
            {
                AddToLog("Blad dodawania uzytkownika "+ex.ToString());
            }
        }
        public static object czyAdministrator(string UserId)
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                cmd.CommandText = "sprawdzCzyAdministrator";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@userId", UserId);
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                var rowsAffected = cmd.ExecuteScalar();

                sqlConnection1.Close();

                return rowsAffected;
            }
            catch (Exception ex)
            {
                AddToLog("Blad sprawdzania uzytkownika czy admnistrator "+ex.ToString());
                return null;
            }
        }
        public static void DeleteUser(Int64 UserId)
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                cmd.CommandText = "Update [dbo].[UserGKSKatowice] set flgDeleted=1 where UserId='" + UserId.ToString()+"'";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                cmd.ExecuteNonQuery();

                sqlConnection1.Close();
            }
            catch (Exception ex )
            {
                AddToLog("Blad usuwania uzytkownika: " + UserId+"  " + ex.Message);
            }
        }
        public static void AddWiadomoscPilka(List<System.Linq.IGrouping<string, string>> hrefList)
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                cmd.CommandText = "INSERT INTO [dbo].[WiadomosciGKSKatowicePilka] (Nazwa,DataUtw,Wiadomosc1,Wiadomosc2,Wiadomosc3,Wiadomosc4,Wiadomosc5) VALUES ('" + "" + "','" + DateTime.Now + "','" + hrefList[0].Key + "','" + hrefList[1].Key + "','" + hrefList[2].Key + "','" + hrefList[3].Key + "','" + hrefList[4].Key + "')";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                cmd.ExecuteNonQuery();

                sqlConnection1.Close();
            }
            catch (Exception ex)
            {
                AddToLog("Błąd dodawania wiadomości: " + ex.ToString());
            }
        }
        public static void AddWiadomoscSiatka(List<System.Linq.IGrouping<string, string>> hrefList)
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                cmd.CommandText = "INSERT INTO [dbo].[WiadomosciGKSKatowiceSiatka] (Nazwa,DataUtw,Wiadomosc1,Wiadomosc2,Wiadomosc3,Wiadomosc4,Wiadomosc5) VALUES ('" + "" + "','" + DateTime.Now + "','" + hrefList[0].Key + "','" + hrefList[1].Key + "','" + hrefList[2].Key + "','" + hrefList[3].Key + "','" + hrefList[4].Key + "')";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                cmd.ExecuteNonQuery();

                sqlConnection1.Close();
            }
            catch (Exception ex)
            {
                AddToLog("Błąd dodawania wiadomości AddWiadomoscSiatka: " + ex.ToString());
            }
        }
        public static void AddWiadomoscHokej(List<System.Linq.IGrouping<string, string>> hrefList)
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                cmd.CommandText = "INSERT INTO [dbo].[WiadomosciGKSKatowiceHokej] (Nazwa,DataUtw,Wiadomosc1,Wiadomosc2,Wiadomosc3) VALUES ('" + "" + "','" + DateTime.Now + "','" + hrefList[0].Key + "','" + hrefList[1].Key + "','" + hrefList[2].Key + "')";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                cmd.ExecuteNonQuery();

                sqlConnection1.Close();
            }
            catch (Exception ex)
            {
                AddToLog("Błąd dodawania wiadomości AddWiadomoscHokej: " + ex.ToString());
            }
        }
        public static byte czyPowiadomienia(string UserId)
        {
            byte returnValue = 0;
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                cmd.CommandText = "sprawdzCzyPowiadomieniaGKSKatowice";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@userId", UserId);
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                var rowsAffected = cmd.ExecuteScalar();

                sqlConnection1.Close();

                if (rowsAffected != null)
                {
                    returnValue = 1;
                }
                else
                {
                    returnValue = 0;
                }
                return returnValue;
            }
            catch (Exception ex)
            {
                AddToLog("Blad sprawdzania uzytkownika czy admnistrator " + ex.ToString());
                return returnValue;
            }
        }

        public static byte czyPrzeklenstwo(string Tekst)
        {
            byte returnValue = 0;
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                cmd.CommandText = "proceduraPrzeklenstwaGKSKatowice";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Tekst", Tekst.ToLower());
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                var rowsAffected = cmd.ExecuteScalar();

                sqlConnection1.Close();

                if (rowsAffected.ToString() !="0")
                {
                    returnValue = 1;
                }
                else
                {
                    returnValue = 0;
                }
                return returnValue;
            }
            catch (Exception ex)
            {
                AddToLog("Blad sprawdzania uzytkownika czy admnistrator " + ex.ToString());
                return returnValue;
            }
        }

        public static void ChangeNotification(string id, byte tryb)
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                cmd.CommandText = "Update [dbo].[UserGKSKatowice] SET flgDeleted = " + tryb + " where UserId=" + "'" + id + "'";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                cmd.ExecuteNonQuery();

                sqlConnection1.Close();
            }
            catch (Exception ex)
            {
                AddToLog("Błąd aktualizacji powiadomień: " + ex.ToString());
            }
        }
    }
}