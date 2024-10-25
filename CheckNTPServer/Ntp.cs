using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CheckNTPServer
{
    internal class Ntp
    {
        /// <summary>
        /// Función original sacada de: https://stackoverflow.com/questions/1193955/how-to-query-an-ntp-server-using-c#:~:text=This%20is%20a%20optimized%20version%20of%20the%20function%20which%20removes%20dependency%20on%20BitConverter%20function%20and%20makes%20it%20compatible%20with%20NETMF%20(.NET%20Micro%20Framework)
        /// </summary>
        /// <param name="ntpServer">Dirección del servidor NTP</param>
        /// <param name="offset">Diferencia horaria con servidor</param>
        /// <param name="timeoutMilliseconds">Superado este tiempo, si no se recibe respuesta, se cancela la petición.</param>
        /// <returns></returns>
        /// <exception cref="TimeoutException"></exception>
        public static DateTime GetNetworkTime(string ntpServer, TimeSpan offset, int timeoutMilliseconds)
        {
            var ntpData = new byte[48];
            ntpData[0] = 0x1B; // LeapIndicator = 0 (no warning), VersionNum = 3 (IPv4 only), Mode = 3 (Client Mode)

            var addresses = Dns.GetHostEntry(ntpServer ?? "").AddressList;
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
}
