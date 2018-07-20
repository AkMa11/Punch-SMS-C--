using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.IO;

namespace PunchSmsConsole
{
    public class ReadFile
    {
        private static String date, logDate, filePath = @"c:\logs\";
        private static MySqlConnection conn;
        private static String selectDbAndTable = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'utdb' AND table_name = 'log_07_2018'; ";
        private static String createTableQuery = "CREATE TABLE log_07_2018 (student_id VARCHAR(10), punch_date_time VARCHAR(25), direction VARCHAR(1), report VARCHAR(1))";
        private static String getDateTime = "SELECT punch_date_time FROM ";
        private static bool tableConfirmation, logConfirmation;
        private static ArrayList date_time_list = new ArrayList();
        public ReadFile()
        {
            getTodaysDate();
            databaseConnection();
            checkIfTableExistsAndCreateNewIfItDoesnt();
            readLogFile();
        }

        private void checkIfTableExistsAndCreateNewIfItDoesnt()
        {
            MySqlCommand cmd = new MySqlCommand(selectDbAndTable, conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int count = reader.GetInt32(0);
                if (count == 0)
                {
                    Console.WriteLine("Creating table...");
                    tableConfirmation = false;
                }
                else if (count == 1)
                {
                    tableConfirmation = true;
                }
            }
            reader.Close();

            createMonthLogTable();

            getDateAndTimeFromDatabaseAndPutThemInList();


        }

        private void getDateAndTimeFromDatabaseAndPutThemInList()
        {

            if (tableConfirmation == true)
            {
                MySqlCommand cmd3 = new MySqlCommand(getDateTime + logDate, conn);
                MySqlDataReader reader = cmd3.ExecuteReader();
                while (reader.Read())
                {
                    date_time_list.Add(reader.GetString("punch_date_time"));
                }
                reader.Close();
            }
        }

        private void createMonthLogTable()
        {
            if (tableConfirmation == false)
            {
                MySqlCommand cmd2 = new MySqlCommand(createTableQuery, conn);
                cmd2.ExecuteNonQuery();
                Console.WriteLine("Table created");
                tableConfirmation = true;
            }
        }

        private void databaseConnection()
        {
            string connectionString = "Server=localhost;port=3306;username=rootuser;password=rootuser;database=utdb;";
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

        private void readLogFile()
        {
            if (!File.Exists(filePath + date))
            {
                Console.WriteLine("No logs today.");
            }
            else
            {
                String[] readText = File.ReadAllLines(filePath + date);
                foreach (string s in readText)
                {
                    if (s.Length != 0)
                    {
                        String[] separateText = s.Split(',');
                        String s_id = separateText[0].Trim();
                        String date_time = separateText[1].Trim();
                        String in_out = separateText[2].Trim();

                        checkingIfLogEntryExistsInDatabase(s_id, date_time, in_out);
                    }
                }
                if (logConfirmation)
                {
                    Console.WriteLine("No new entries.");
                }
            }
        }

        private void checkingIfLogEntryExistsInDatabase(String s_id, String date_time, String in_out)
        {
            logConfirmation = date_time_list.Contains(date_time);

            if (!logConfirmation)
            {
                sendingLogToMonthlyDatabase(s_id, date_time, in_out);
            }
        }

        private void sendingLogToMonthlyDatabase(String s_id, String date_time, String in_out)
        {
            Console.WriteLine("New entry detected.");
            MySqlCommand comm = new MySqlCommand("insert into " + logDate
                    + "(student_id, punch_date_time, direction, report)"
                    + " values (?s_id, ?date_time, ?in_out, ?report)", conn);
            comm.Parameters.AddWithValue("?s_id", s_id);
            comm.Parameters.AddWithValue("?date_time", date_time);
            comm.Parameters.AddWithValue("?in_out", in_out);
            comm.Parameters.AddWithValue("?report", "0");
            comm.ExecuteNonQuery();
            Console.WriteLine("Entry append success.");
        }

        private string getTodaysDate()
        {
            string dtf = DateTime.Now.ToString("dd-MM-yyyy");
            string dtfLog = DateTime.Now.ToString("MM_yyyy");
            date = dtf + ".txt";
            logDate = "log_" + dtfLog;
            return date;
        }
    }
}
