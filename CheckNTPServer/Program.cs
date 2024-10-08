using System.Net.Sockets;
using System.Net;

internal class Program
{
    private static void Main(string[] args)
    {
        string regFile = "registros.log";
        Console.WriteLine("Watchdog para la supervision de servicios NTP by Aimarmun 2024.");
        if (args.Length == 0)
        {
            Console.WriteLine("Se require al menos un argumento que indique el servidor NTP. Ejemplo:");
            Console.WriteLine("\tCheckNTPServer ntp.ccpamplona.tr");
            Console.WriteLine();
            Console.WriteLine($"Se admite un segundo argumento que sería el nombre del archivo de salida de registro, si no el nombre del archivo de registro sera {regFile}. Ejemplo:");
            Console.WriteLine("\tCheckNTPServer ntp.ccpamplona.tr registrosPamplona.log");
            System.Environment.Exit(1);
        }
        
        bool error = false;
        string msg = "";
        string ntpServer = args[0];

        if (args.Length > 1)
        {
            regFile = args[1];
            Console.WriteLine($"Se usara el archivo de registro {regFile}.");
        }

        File.AppendAllText(regFile, $"{DateTime.Now} Inicio de aplicación. Servidor {ntpServer}.{Environment.NewLine}");
        while (true)
        {
            var networkTime = GetNetworkTime(DateTimeOffset.Now.Offset);
            var nowTime = DateTime.Now;
            if (Math.Abs((networkTime - nowTime).TotalSeconds) >= 3)
            {
                msg = $"Cuidado! la diferencia entre fecha local y remota es demasiada! Local: {nowTime}, Remota: {networkTime}{Environment.NewLine}";
                Console.Write(msg);
                if (!error)
                {
                    File.AppendAllText(regFile, msg);
                }
                error = true;
            }
            else
            {
                msg = $"Local: {nowTime}, Remota: {networkTime} OK.{Environment.NewLine}";
                if (error)
                {
                    File.AppendAllText(regFile, $"La hora vuelve a coincidir{Environment.NewLine}");
                    File.AppendAllText(regFile, msg);
                }
                error = false;
                Console.Write(msg);
            }
            Thread.Sleep(1000);
        }

        DateTime GetNetworkTime(TimeSpan offset)
        {
            
            var ntpData = new byte[48];
            ntpData[0] = 0x1B; //LeapIndicator = 0 (no warning), VersionNum = 3 (IPv4 only), Mode = 3 (Client Mode)

            var addresses = Dns.GetHostEntry(ntpServer).AddressList;
            var ipEndPoint = new IPEndPoint(addresses[0], 123);
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            socket.Connect(ipEndPoint);
            socket.Send(ntpData);
            socket.Receive(ntpData);
            socket.Close();

            ulong intPart = (ulong)ntpData[40] << 24 | (ulong)ntpData[41] << 16 | (ulong)ntpData[42] << 8 | ntpData[43];
            ulong fractPart = (ulong)ntpData[44] << 24 | (ulong)ntpData[45] << 16 | (ulong)ntpData[46] << 8 | ntpData[47];

            var milliseconds = intPart * 1000 + fractPart * 1000 / 0x100000000L;
            var networkDateTime = new DateTime(1900, 1, 1).AddMilliseconds((long)milliseconds);

            return networkDateTime + offset;
        }
    }
}