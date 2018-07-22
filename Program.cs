using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace PunchSmsConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Timer timer = new Timer(5000);
            timer.AutoReset = true;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(myTimer);
            timer.Start();

            Console.WriteLine("----------SMS service has started----------");
            
            Console.ReadLine();
		}
        

        public static void myTimer(object sender, System.Timers.ElapsedEventArgs e)
        {
            FileStream ostrm;
            StreamWriter writer;
            TextWriter oldOut = Console.Out;
            string date = DateTime.Now.ToString("dd-MM-yyyy");
            try
            {
                ostrm = new FileStream("E:\\LogFolder\\" + date + ".txt", FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                writer = new StreamWriter(ostrm);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot open " + date + ".txt for writing");
                Console.WriteLine(ex.Message);
                return;
            }
            Console.SetOut(writer);
            Console.WriteLine(DateTime.Now.ToString() + ": " + "----------File Read service has started----------");
            try
            {
                ReadFile readFile = new ReadFile();

                if (SendSMS.processComplete == true)
                {

                    SendSMS sendSMS = new SendSMS();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured: " + ex);
            }

            Console.WriteLine(DateTime.Now.ToString() + ": " + "==========Timer ticked==========");

            Console.SetOut(oldOut);
            writer.Close();
            ostrm.Close();
        }
    }
}
