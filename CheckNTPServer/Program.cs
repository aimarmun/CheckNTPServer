using System.Net.Sockets;
using System.Net;
using CheckNTPServer;
using System.ComponentModel;

internal class Program
{
    /// <summary>
    /// Archivo de registros, por defecto registros.log
    /// </summary>
    public static string RegFile { get; set; } = "registros.log";
    
    /// <summary>
    /// Dirección del servidor NTP
    /// </summary>
    public static string NtpServer { get; set; } = string.Empty;

    /// <summary>
    /// Umbral de fallo en milisegundos. Si se supera la diferencia de este umbral entre el equipo local y el servidor, se marcará como fallo. Por defecto 3000ms.
    /// </summary>
    public static int MaxMillisOfDiff { get; set; } = 3000;

    /// <summary>
    /// Escribe los archivos de registros y la salida de consola.
    /// </summary>
    public static Log Log { get; set; } = new(RegFile);

    /// <summary>
    /// Indica si hubo erroes anteriores
    /// </summary>
    public static bool HasPrevErrors { get; set; } = false;

    private static void Main(string[] args)
    {
        
        Console.WriteLine("Watchdog para la supervision de servicios NTP by Aimarmun 2024.");

        ReadAndValidateArguments(args);

        Log = new Log(RegFile);

        WriteConfigInfile();

        bool error = false;
        string msg = "";

        while (true)
        {
            try
            {
                var networkTime = Ntp.GetNetworkTime(NtpServer, DateTimeOffset.Now.Offset, 3000);
                var nowTime = DateTime.Now;
                var diff = Math.Round(Math.Abs((networkTime - nowTime).TotalMilliseconds), 0);
                if (diff >= MaxMillisOfDiff)
                {
                    msg = $"Cuidado! Se ha pasado el umbral permitido! Local: {nowTime}, Remota: {networkTime}. Diferencia: {diff} milisegundos.";
                    Console.WriteLine(msg);
                    if (!error)
                    {
                        Log.Write(msg, false);
                        Log.Write("Se volverá a escribir otro mensaje cuando se vuelva al umbral aceptable", false);
                    }
                    error = true;
                }
                else
                {
                    msg = $"Local: {nowTime}, Remota: {networkTime} OK. {diff}ms.";
                    if (error)
                    {
                        Log.Write($"La hora vuelve a mantener el umbral aceptable.", false);
                        Log.Write(msg, false);
                    }
                    error = false;
                    Console.WriteLine(msg);
                }
                if (HasPrevErrors)
                {
                    HasPrevErrors = false;
                    Log.Write("Se ha recuperado de errores previos");
                }
            }
            catch (Exception e) 
            {
                msg = $"Error en la recuperación de hora: {e.Message}";
                Log.Write(msg);
                HasPrevErrors = true;
            }
            Thread.Sleep(1000);

        }

    }

    private static void WriteConfigInfile()
    {
        var lines = new[]
        {
            $"Inicio de la aplicación.",
            "       Configuración:",
            $"          Servidor: {NtpServer}",
            $"          Umbral aceptable: {MaxMillisOfDiff}ms"
        };
        Log.Write(lines);
    }

    public static void ReadAndValidateArguments(string[] args)
    {
        if (args.Length == 0) PrintHelpAndExit();
        if (args.LastOrDefault(a => a.Equals("-h")) != null) PrintHelpAndExit();

        NtpServer = args.LastOrDefault(a => a.StartsWith("-s"))?.Substring(2) ?? "";

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
            "  -s<servidor>      Indica el servidor de hora a supervisar [obligatorio]",
            "",
            "  -r<archivo>       Indica el archivo donde se escribiran los registros de la aplicación [opcional]. Por defecto \"registros.log\"",
            "",
            "  -d<milisegundos>  Umbral para dar por bueno el dato de la hora, superado este umbral se escribirá una incidencia en el registro. Tambien se escribirá en el registro si se ha vuelto al umbral aceptable.[opcional]. Por defecto 3000ms. El valor tiene que ser mayor de 1",
            "",
            "  -h                Muestra esta ayuda.",
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
}