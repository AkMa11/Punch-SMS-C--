using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PunchSmsConsole
{
    class SendSMS
    {
        public static bool processComplete = true;
        private static MySqlConnection conn;
        private static String date, logDate, s_id, dir = null;
        private static ArrayList studentDirectionList = new ArrayList();
        public static ArrayList studentInfoList = new ArrayList();
        private static String zero = "00000", zeroSix = "000000";
        private static int zeroLength = 5, zeroSixLength = 6;
        public SendSMS()
        {
            processComplete = true;
            getTodaysDate();
            databaseConnection();
            getStudentIdAndDirectionFromLogDatabase();
            putAllStudentInfoIntoList();
            sendingSMS();
            clearListsForNextLogs();
        }

        private void clearListsForNextLogs()
        {
            studentInfoList.Clear();
            studentDirectionList.Clear();
        }

        private void sendingSMS()
        {
            foreach (string obj2 in studentInfoList)
            {
                processComplete = false;
                String sep = "^";
                String[] separateText = obj2.Split(sep.ToCharArray());
                String stud_id = separateText[0].Trim();
                String in_out = separateText[1].Trim();
                String p_d_t = separateText[2].Trim();
                String s_name = separateText[3].Trim();
                String p_no = separateText[4].Trim();


                if (p_no.Equals("NULL"))
                {
                    Console.WriteLine("Phone number for "
                + stud_id + " " + s_name + " is not in the database.");
                }
                else
                {

                    smsScript(in_out, p_d_t, s_name, p_no);
                }
                updateStudentReportInLogDatabase(p_d_t);
            }
            processComplete = true;
        }

        private void updateStudentReportInLogDatabase(string p_d_t)
        {
            MySqlCommand cmd3 = new MySqlCommand("UPDATE " + logDate + " SET report ='1' WHERE punch_date_time = '" + p_d_t + "'", conn);
            cmd3.ExecuteNonQuery();
            Console.WriteLine("Log database Updated!");
        }

        private void smsScript(string in_out, string p_d_t, string s_name, string p_no)
        {
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(
                    "http://tra.bulksmshyderabad.co.in/websms/"
                    + "sendsms.aspx?userid=Uniquetutorials&password=tu1234566&"
                    + "sender=UNQTUT&mobileno=" + Uri.EscapeUriString(p_no)
                    + "&msg=" + Uri.EscapeUriString(s_name + " has " + in_out
                            + " The Unique Tutorials on " + p_d_t));
            myRequest.Method = "GET";
            var resp = (HttpWebResponse)myRequest.GetResponse();
            var result = new StreamReader(resp.GetResponseStream()).ReadToEnd();
            Console.WriteLine(s_name + " has " + in_out
                            + " The Unique Tutorials on " + p_d_t);
            Console.WriteLine(result);
            Console.WriteLine("SMS sent!");
        }

        private static void putAllStudentInfoIntoList()
        {
            foreach (string obj in studentDirectionList)
            {
                String sep = "^";
                String[] separateText = obj.Split(sep.ToCharArray());
                String stud_id = separateText[0].Trim();
                String in_out = separateText[1].Trim();
                String p_d_t = separateText[2].Trim();

                MySqlCommand cmd2 = new MySqlCommand("SELECT p_no, s_name FROM main_details_1 WHERE s_id = " + stud_id, conn);
                MySqlDataReader reader = cmd2.ExecuteReader();
                while (reader.Read())
                {
                    studentInfoList.Add(stud_id + "^" + in_out + "^"
                                + p_d_t + "^" + reader.GetString("s_name") + "^"
                                + reader.GetString("p_no"));
                }
                reader.Close();
            }
        }

        private static void getStudentIdAndDirectionFromLogDatabase()
        {
            MySqlCommand cmd = new MySqlCommand("SELECT student_id, direction, punch_date_time"
                + " FROM " + logDate + " WHERE report = '0'", conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                s_id = reader.GetString("student_id");
                s_id = s_id.StartsWith(zeroSix) ? s_id.Substring(zeroSixLength) : s_id;
                s_id = s_id.StartsWith(zero) ? s_id.Substring(zeroLength) : s_id;
                dir = reader.GetString("direction");
                dir = dir.Replace("0", "reached at");
                dir = dir.Replace("1", "departed from");
                studentDirectionList.Add(s_id + "^" + dir + "^" + reader.GetString("punch_date_time"));
            }
            reader.Close();
        }

        private static void databaseConnection()
        {
            string connectionString = "Server=localhost;port=3306;username=rootuser;password=rootuser; database =utdb;";
            conn = new MySqlConnection(connectionString);
            try
            {
                conn.Open();
                Console.WriteLine("Connection success");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static string getTodaysDate()
        {
            string dtf = DateTime.Now.ToString("dd-MM-yyyy");
            string dtfLog = DateTime.Now.ToString("MM_yyyy");
            date = dtf + ".txt";
            logDate = "log_" + dtfLog;
            return date;
        }
    }
}
