using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using NLog;
using OSGTools;

namespace AppiumStartLib
{
    public class Appium
    {
        public Appium(string machine, string task)
        {
            // получить первое свободное подключенное устройство
            Device d;
            //ADBBase.DBName = "adb";
            d = ADBBase.getFirstFreeDevice(machine);
            if (d.id > 0)
            {
                // занимаем его и записываем время старта
                ADBBase.DeviceUsed(d.id, task);
                // создаём объект Appium, выбирая свободный порт appium и свободный порт bootsrap
                List<int> ports = ADBBase.getFreeAppiumPorts();
                appium_path = @"C:\Program Files\Appium\node.exe";
                appium_address = "127.0.0.1";
                appium_port = ports[0];
                bootstrap_port = ports[1];
                chromedriver_port = 9516;
                selendroid_port = 8082;
                device_id = d.id;
                device = d.dev_id;
                pid = -1;
                //appium_connstring = String.Format("\"C:\\Program Files\\Appium\\node_modules\\appium\\bin\\appium.js\" --address {0} -p {1} --chromedriver-port {2} --bootstrap-port {3} --selendroid-port {4} --no-reset --log-level info --local-timezone -U {5}",
                //    appium_address, appium_port, chromedriver_port, bootstrap_port, selendroid_port, device);
                appium_connstring = String.Format("\"C:\\Program Files\\Appium\\node_modules\\appium\\bin\\appium.js\" --address {0} -p {1} --chromedriver-port {2} --bootstrap-port {3} --selendroid-port {4} --no-reset --local-timezone -U {5}",
                    appium_address, appium_port, chromedriver_port, bootstrap_port, selendroid_port, device);
            }
            else
            {
                device_id = -1;
            }
        }

        private string appium_connstring = "";
        public string Appium_ConnString
        {
            get { return appium_connstring; }
        }
        public string appium_path { get; set; }
        public string appium_address { get; set; }
        public int appium_port { get; set; }
        public int bootstrap_port { get; set; }
        public int chromedriver_port { get; set; }
        public int selendroid_port { get; set; }
        public int device_id { get; set; }
        public string device { get; set; }
        public int pid { get; set; }

        [STAThread]
        public bool AppiumStart()
        {
            Logger log = LogManager.GetCurrentClassLogger();

            //ADBBase.DBName = "adb";

            // запускаем Appium и запоминаем pid запущенного процесса Appium
            log.Info("Запуск Appium");
            log.Info("Строка запуска:{0}", Appium_ConnString);
            Process p = new Process();
            p.StartInfo.FileName = appium_path;
            p.StartInfo.Arguments = Appium_ConnString;

            p.Start();

            // записываем в таблицу номера портов и pid процесса
            pid = p.Id;
            log.Info("ID запущенного процесса Appium:{0}", pid);
            ADBBase.AppiumPortsSave(device_id, appium_port, bootstrap_port, pid);
            log.Info("Успешная запись параметров Appium");

            return true;
        }

        [STAThread]
        public bool AppiumStop(string status)
        {
            bool result = false;
            Logger log = LogManager.GetCurrentClassLogger();

            try
            {
                //ADBBase.DBName = "adb";
                //
                log.Info("Завершение процесса Appium с ID: {0}", pid);
                Functions.KillProcessTree(pid);
                //
                ADBBase.AppiumStop(device_id, pid, status);
                log.Info("Успешное завершение Appium");
                result = true;
            }
            catch
            {
                result = false;
            }

            return result;
        }
    }
}