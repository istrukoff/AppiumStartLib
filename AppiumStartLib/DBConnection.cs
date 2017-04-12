using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using NLog;

namespace AppiumStartLib
{
    public static class ADBBase
    {
        private static string server;
        public static string Server { get { return server; } }

        private static int port { get; set; }
        public static int Port { get { return port; } }

        private static string dbName = string.Empty;
        public static string DBName
        {
            get { return dbName; }
            set { dbName = value; }
        }

        private static string user;
        public static string User { get { return user; } }

        private static string password;
        public static string Password { get { return password; } }

        private static MySqlConnection connection = null;
        public static MySqlConnection Connection
        {
            get { return connection; }
        }

        [STAThread]
        public static bool Connect()
        {
            Logger log = LogManager.GetCurrentClassLogger();
            log.Info("Подключение к базе данных");

            bool result = true;

            if (String.IsNullOrEmpty(dbName))
                result = false;

            server = "ovz1.akolomiec.znog6.vps.myjino.ru";
            port = 49283;
            dbName = "adb";
            user = "root";
            password = "KRUS56_ak+";

            string connstring = string.Format("Server={0}; Port={1}; database={2}; UID={3}; password={4}; charset=utf8", server, port, dbName, user, password);
            connection = new MySqlConnection(connstring);
            connection.Open();
            result = true;

            return result;
        }

        // добавление устройства
        [STAThread]
        public static bool DeviceAdd(string dev_id, string adb_status)
        {
            bool result = true;

            ADBBase.Connect();
            string cmdtext = string.Format("INSERT INTO devices (dev_id, adb_status, used, appium_port, bootstrap_port, pid) VALUES ('{0}', '{1}', 0, 0, 0, 0)", dev_id, adb_status);
            MySqlCommand cmd = new MySqlCommand(cmdtext, connection);
            cmd.ExecuteNonQuery();
            ADBBase.Close();

            return result;
        }

        // удаление устройства по id
        [STAThread]
        public static bool DeviceRemove(int id)
        {
            bool result = true;

            ADBBase.Connect();
            string cmdtext = string.Format("DELETE FROM devices WHERE devices.id={0}", id);
            MySqlCommand cmd = new MySqlCommand(cmdtext, connection);
            cmd.ExecuteNonQuery();
            ADBBase.Close();

            return result;
        }

        // список всех устройств
        [STAThread]
        public static List<Device> getDevicesList()
        {
            List<Device> result = new List<Device>();

            ADBBase.Connect();
            string cmdtext = "SELECT d.id as id, d.dev_id as dev_id, d.adb_status as adb_status, d.used as used, d.appium_port as appium_port, d.bootstrap_port as bootstrap_port, d.pid as pid FROM devices as d";
            MySqlCommand cmd = new MySqlCommand(cmdtext, connection);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new Device(int.Parse(reader["id"].ToString()),
                    reader["dev_id"].ToString(),
                    reader["adb_status"].ToString(),
                    bool.Parse(reader["used"].ToString()),
                    int.Parse(reader["appium_port"].ToString()),
                    int.Parse(reader["bootstrap_port"].ToString()),
                    int.Parse(reader["pid"].ToString())));
            }
            ADBBase.Close();

            return result;
        }

        // список всех устройств по статусу
        [STAThread]
        public static List<Device> getDevicesList(string adb_status)
        {
            List<Device> result = new List<Device>();

            ADBBase.Connect();
            string cmdtext = string.Format("SELECT d.id as id, d.dev_id as dev_id, d.adb_status as adb_status, d.used as used, d.appium_port as appium_port, d.bootstrap_port as bootstrap_port, d.pid as pid FROM devices as d WHERE d.adb_status='{0}'", adb_status);
            MySqlCommand cmd = new MySqlCommand(cmdtext, connection);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new Device(int.Parse(reader["id"].ToString()),
                    reader["dev_id"].ToString(),
                    reader["adb_status"].ToString(),
                    bool.Parse(reader["used"].ToString()),
                    int.Parse(reader["appium_port"].ToString()),
                    int.Parse(reader["bootstrap_port"].ToString()),
                    int.Parse(reader["pid"].ToString())));
            }
            ADBBase.Close();

            return result;
        }

        // список всех устройств по статусу и занятости
        [STAThread]
        public static List<Device> getDevicesList(string adb_status, int used)
        {
            List<Device> result = new List<Device>();

            ADBBase.Connect();
            string cmdtext = string.Format("SELECT d.id as id, d.dev_id as dev_id, d.adb_status as adb_status, d.used as used, d.appium_port as appium_port, d.bootstrap_port as bootstrap_port, d.pid as pid FROM devices as d WHERE d.adb_status='{0}' AND d.used={1}", adb_status, used);
            MySqlCommand cmd = new MySqlCommand(cmdtext, connection);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new Device(int.Parse(reader["id"].ToString()),
                    reader["dev_id"].ToString(),
                    reader["adb_status"].ToString(),
                    bool.Parse(reader["used"].ToString()),
                    int.Parse(reader["appium_port"].ToString()),
                    int.Parse(reader["bootstrap_port"].ToString()),
                    int.Parse(reader["pid"].ToString())));
            }
            ADBBase.Close();

            return result;
        }

        // получить первое свободное устройство
        [STAThread]
        public static Device getFirstFreeDevice()
        {
            Device result;

            ADBBase.Connect();
            string cmdtext = string.Format("SELECT d.id as id, d.dev_id as dev_id, d.adb_status as adb_status, d.used as used, d.appium_port as appium_port, d.bootstrap_port as bootstrap_port, d.pid as pid FROM devices as d WHERE d.adb_status='device' AND d.used=0 LIMIT 1");
            MySqlCommand cmd = new MySqlCommand(cmdtext, connection);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                result = new Device(int.Parse(reader["id"].ToString()),
                            reader["dev_id"].ToString(),
                            reader["adb_status"].ToString(),
                            bool.Parse(reader["used"].ToString()),
                            int.Parse(reader["appium_port"].ToString()),
                            int.Parse(reader["bootstrap_port"].ToString()),
                            int.Parse(reader["pid"].ToString()));
                reader.Close();
            }
            else
            {
                result = new Device(-1, "", "", true, 0, 0, -1);
            }
            ADBBase.Close();

            return result;
        }

        // получить первое свободное устройство на указанном компьютере
        [STAThread]
        public static Device getFirstFreeDevice(string machine)
        {
            Device result;

            ADBBase.Connect();
            string cmdtext = string.Format("SELECT d.id as id, d.dev_id as dev_id, d.adb_status as adb_status, d.used as used, d.appium_port as appium_port, d.bootstrap_port as bootstrap_port, d.pid as pid FROM devices as d WHERE machine='{0}' AND d.adb_status='device' AND d.used=0 LIMIT 1", machine);
            MySqlCommand cmd = new MySqlCommand(cmdtext, connection);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                result = new Device(int.Parse(reader["id"].ToString()),
                            reader["dev_id"].ToString(),
                            reader["adb_status"].ToString(),
                            bool.Parse(reader["used"].ToString()),
                            int.Parse(reader["appium_port"].ToString()),
                            int.Parse(reader["bootstrap_port"].ToString()),
                            int.Parse(reader["pid"].ToString()));
                reader.Close();
            }
            else
            {
                result = new Device(-1, "", "", true, 0, 0, -1);
            }
            ADBBase.Close();

            return result;
        }

        // изменяем статус занятости устройства
        [STAThread]
        public static void DeviceUsed(int id, string task)
        {
            ADBBase.Connect();
            string cmdtext = string.Format("UPDATE devices SET used={1} WHERE devices.id={0}; INSERT INTO devices_activities (id_device, starttime, task) VALUES ({0}, now(), '{2}');", id, 1, task);
            MySqlCommand cmd = new MySqlCommand(cmdtext, connection);
            cmd.ExecuteNonQuery();
            ADBBase.Close();
        }

        // для выбранного устройства записываем порты, которые использует запущенный Appium
        [STAThread]
        public static void AppiumPortsSave(int id, int appium_port, int bootsrap_port, int pid)
        {
            ADBBase.Connect();
            string cmdtext = string.Format("UPDATE devices SET appium_port={1}, bootstrap_port={2}, pid={3} WHERE devices.id={0};", id, appium_port, bootsrap_port, pid);
            MySqlCommand cmd = new MySqlCommand(cmdtext, connection);
            cmd.ExecuteNonQuery();
            ADBBase.Close();
        }

        // обнуляем поля после остановки Appium
        [STAThread]
        public static void AppiumStop(int device_id, int pid, string status)
        {
            ADBBase.Connect();
            string cmdtext = string.Format("UPDATE devices SET used=0, appium_port=0, bootstrap_port=0, pid=-1 WHERE devices.pid={0}; UPDATE devices_activities AS d_a SET d_a.endtime=now(), d_a.status='{2}' WHERE d_a.id_device={1} AND endtime is null", pid, device_id, status);
            MySqlCommand cmd = new MySqlCommand(cmdtext, connection);
            cmd.ExecuteNonQuery();
            ADBBase.Close();
        }

        // получить следующие по порядку номера для портов Appium
        [STAThread]
        public static List<int> getFreeAppiumPorts()
        {
            List<int> result = new List<int>();

            ADBBase.Connect();
            string cmdtext = string.Format("SELECT (MAX(d.appium_port) + 1) as appium_port, (MAX(d.bootstrap_port) + 1) as bootstrap_port FROM devices as d");
            MySqlCommand cmd = new MySqlCommand(cmdtext, connection);
            MySqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            result.Add(int.Parse(reader["appium_port"].ToString()));
            result.Add(int.Parse(reader["bootstrap_port"].ToString()));
            ADBBase.Close();

            return result;
        }

        public static void Close()
        {
            connection.Close();
        }
    }
}