using System.Net.Sockets;
using System.Net;

internal class Program
{
    public static string RegFile { get; set; } = "registros.log";
    public static string? NtpServer { get; set; }
    public static int MaxMillisOfDiff { get; set; } = 3000;

    private static void Main(string[] args)
    {
        
        Console.WriteLine("Watchdog para la supervision de servicios NTP by Aimarmun 2024.");

        ReadAndValidateArguments(args);

        WriteConfigInfile();

        bool error = false;
        string msg = "";

        while (true)
        {
            try
            {
                var networkTime = GetNetworkTime(DateTimeOffset.Now.Offset, 3000);
                var nowTime = DateTime.Now;
                var diff = Math.Abs((networkTime - nowTime).TotalMilliseconds);
                if (diff >= MaxMillisOfDiff)
                {
                    msg = $"Cuidado! Se ha pasado el umbral permitido! Local: {nowTime}, Remota: {networkTime}. Diferencia: {diff} milisegundos.{Environment.NewLine}";
                    Console.Write(msg);
                    if (!error)
                    {
                        File.AppendAllText(RegFile, msg);
                        File.AppendAllLines(RegFile, new[] { "Se volverá a escribir otro mensaje cuando se vuelva al umbral aceptable" });
                    }
                    error = true;
                }
                else
                {
                    msg = $"Local: {nowTime}, Remota: {networkTime} OK. {diff}ms.{Environment.NewLine}";
                    if (error)
                    {
                        File.AppendAllText(RegFile, $"La hora vuelve a mantener el umbral aceptable.{Environment.NewLine}");
                        File.AppendAllText(RegFile, msg);
                    }
                    error = false;
                    Console.Write(msg);
                }
            }
            catch (Exception e) 
            {
                msg = $"Error en la recuperación de hora: {e.Message}";
                Console.WriteLine (msg);
                File.AppendAllText(RegFile, $"{DateTime.Now} {msg}{Environment.NewLine}");
            }
            Thread.Sleep(1000);

        }

    }

    private static void WriteConfigInfile()
    {
        var lines = new[]
        {
            $"Inicio de la aplicación {DateTime.Now}.",
            "Configuración:",
            $"  Servidor: {NtpServer}",
            $"  Umbral aceptable: {MaxMillisOfDiff}ms"
        };
        File.AppendAllLines(RegFile, lines);
    }

    public static void ReadAndValidateArguments(string[] args)
    {
        if (args.Length == 0) PrintHelpAndExit();
        if (args.LastOrDefault(a => a.Equals("-h")) != null) PrintHelpAndExit();

        NtpServer = args.LastOrDefault(a => a.StartsWith("-s"))?.Substring(2);

        if (string.IsNullOrEmpty(NtpServer)) PrintHelpAndExit();

        RegFile = args.LastOrDefault(a => a.StartsWith("-r"))?.Substring(2) ?? RegFile;

        var maxMillisOfDiff = args.LastOrDefault(a => a.StartsWith("-d"))?.Substring(2);

        if (!string.IsNullOrEmpty(maxMillisOfDiff))
        {
            int result;
            if (!int.TryParse(maxMillisOfDiff, out result)) PrintHelpAndExit();
            if (result < 1) PrintHelpAndExit();
            MaxMillisOfDiff = result;
        }
    }

    private static void PrintHelpAndExit()
    {
        var lines = new[]
        {
            "Argumentos soportados:",
            "  -s<servidor>    Indica el servidor de hora a supervisar [obligatorio]",
            "",
            "  -r<archivo>     Indica el archivo donde se escribiran los registros de la aplicación [opcional]. Por defecto \"registros.log\"",
            "",
            "  -d<m.segundos>  Umbral para dar por bueno el dato de la hora, superado este umbral se escribirá una incidencia en el registro. Tambien se escribirá en el registro si se ha vuelto al umbral aceptable.[opcional]. Por defecto 3000ms. El valor tiene que ser mayor de 1",
            "",
            "  -h              Muestra esta ayuda.",
            "",
            "Se require al menos el argumento \"-s\" que indica el servidor NTP. Ejemplo:",
            "  >CheckNTPServer.exe -sntp.tuhora.com",
            "",
            "Ejemplo utilizando todas las opciones:",
            "  >CheckNTPServer.exe -sntp.tuhora.com -rRegistroTuHora.log -d1000"
        };

        foreach(string line in lines)
        {
            Console.WriteLine(line);
        }
   
        Environment.Exit(1);
    }

    //public static DateTime GetNetworkTime(TimeSpan offset)
    //{

    //    var ntpData = new byte[48];
    //    ntpData[0] = 0x1B; //LeapIndicator = 0 (no warning), VersionNum = 3 (IPv4 only), Mode = 3 (Client Mode)

    //    var addresses = Dns.GetHostEntry(NtpServer ?? "").AddressList;
    //    var ipEndPoint = new IPEndPoint(addresses[0], 123);
    //    var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

    //    socket.Connect(ipEndPoint);
    //    socket.Send(ntpData);
    //    socket.Receive(ntpData);
    //    socket.Close();

    //    ulong intPart = (ulong)ntpData[40] << 24 | (ulong)ntpData[41] << 16 | (ulong)ntpData[42] << 8 | ntpData[43];
    //    ulong fractPart = (ulong)ntpData[44] << 24 | (ulong)ntpData[45] << 16 | (ulong)ntpData[46] << 8 | ntpData[47];

    //    var milliseconds = intPart * 1000 + fractPart * 1000 / 0x100000000L;
    //    var networkDateTime = new DateTime(1900, 1, 1).AddMilliseconds((long)milliseconds);

    //    return networkDateTime + offset;
    //}

    public static DateTime GetNetworkTime(TimeSpan offset, int timeoutMilliseconds)
    {
        var ntpData = new byte[48];
        ntpData[0] = 0x1B; // LeapIndicator = 0 (no warning), VersionNum = 3 (IPv4 only), Mode = 3 (Client Mode)

        var addresses = Dns.GetHostEntry(NtpServer ?? "").AddressList;
        var ipEndPoint = new IPEndPoint(addresses[0], 123);

        using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(timeoutMilliseconds);

            var task = Task.Run(() =>
            {
                socket.Connect(ipEndPoint);
                socket.Send(ntpData);
                socket.Receive(ntpData);
            }, cts.Token);

            try
            {
                task.Wait(cts.Token);
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException("La operación superó el tiempo de respuesta esperado.");
            }
        }

        ulong intPart = (ulong)ntpData[40] << 24 | (ulong)ntpData[41] << 16 | (ulong)ntpData[42] << 8 | ntpData[43];
        ulong fractPart = (ulong)ntpData[44] << 24 | (ulong)ntpData[45] << 16 | (ulong)ntpData[46] << 8 | ntpData[47];

        var milliseconds = intPart * 1000 + fractPart * 1000 / 0x100000000L;
        var networkDateTime = new DateTime(1900, 1, 1).AddMilliseconds((long)milliseconds);

        return networkDateTime + offset;
    }
}