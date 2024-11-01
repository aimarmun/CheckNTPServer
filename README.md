# CheckNTPServer, una aplicación para la comproboación de un servidor NTP
Con CheckNTPserver puedes comprobar el funcionamiento de un servidor NTP comparando la hora local con la hora del servidor NTP.
Es una aplicación de consola muy sencilla que muestra los resultados por pantalla y por archivo de registro, mostrando la diferencia entre tu máquina local y el servidor NTP.
```cmd
Watchdog para la supervision de servicios NTP by Aimarmun 2024.
[01/11/2024 18:55:30] Inicio de la aplicación.
       Configuración:
          Servidor: time.windows.com
          Umbral aceptable: 3000ms
Local: 01/11/2024 18:55:30, Remota: 01/11/2024 18:55:27 OK. 2833ms.
Local: 01/11/2024 18:55:31, Remota: 01/11/2024 18:55:28 OK. 2841ms.
Local: 01/11/2024 18:55:32, Remota: 01/11/2024 18:55:29 OK. 2832ms.
Local: 01/11/2024 18:55:33, Remota: 01/11/2024 18:55:30 OK. 2831ms.
Local: 01/11/2024 18:55:34, Remota: 01/11/2024 18:55:31 OK. 2831ms.
Local: 01/11/2024 18:55:35, Remota: 01/11/2024 18:55:32 OK. 2844ms.
[01/11/2024 18:55:36] Error en la recuperación de hora: Host desconocido.
[01/11/2024 18:55:37] Error en la recuperación de hora: Host desconocido.
[01/11/2024 18:55:38] Error en la recuperación de hora: Host desconocido.
[01/11/2024 18:55:39] Error en la recuperación de hora: Host desconocido.
[01/11/2024 18:55:40] Error en la recuperación de hora: Host desconocido.
[01/11/2024 18:55:41] Error en la recuperación de hora: Host desconocido.
[01/11/2024 18:55:42] Error en la recuperación de hora: Host desconocido.
[01/11/2024 18:55:43] Error en la recuperación de hora: Host desconocido.
Local: 01/11/2024 18:55:44, Remota: 01/11/2024 18:55:42 OK. 2832ms.
[01/11/2024 18:55:44] Se ha recuperado de errores previos
Local: 01/11/2024 18:55:46, Remota: 01/11/2024 18:55:43 OK. 2832ms.
Local: 01/11/2024 18:55:47, Remota: 01/11/2024 18:55:44 OK. 2858ms.
Local: 01/11/2024 18:55:48, Remota: 01/11/2024 18:55:45 OK. 2838ms.
Local: 01/11/2024 18:55:49, Remota: 01/11/2024 18:55:46 OK. 2833ms.
```
Es muy parecido a la utilidad "ping", pero para servidores NTP.
En el archivo de registro generado solo se guardan los fallos y las recuperaciones:
```
[01/11/2024 18:55:30] Inicio de la aplicación.
       Configuración:
          Servidor: time.windows.com
          Umbral aceptable: 3000ms
[01/11/2024 18:55:36] Error en la recuperación de hora: Host desconocido.
[01/11/2024 18:55:37] Error en la recuperación de hora: Host desconocido.
[01/11/2024 18:55:38] Error en la recuperación de hora: Host desconocido.
[01/11/2024 18:55:39] Error en la recuperación de hora: Host desconocido.
[01/11/2024 18:55:40] Error en la recuperación de hora: Host desconocido.
[01/11/2024 18:55:41] Error en la recuperación de hora: Host desconocido.
[01/11/2024 18:55:42] Error en la recuperación de hora: Host desconocido.
[01/11/2024 18:55:43] Error en la recuperación de hora: Host desconocido.
[01/11/2024 18:55:44] Se ha recuperado de errores previos
```
## Argumentos admitidos
```cmd
  -s<servidor>      Indica el servidor de hora a supervisar [obligatorio]

  -r<archivo>       Indica el archivo donde se escribiran los registros de la aplicación [opcional]. Por defecto "registros.log"

  -d<milisegundos>  Umbral para dar por bueno el dato de la hora, superado este umbral se escribirá una incidencia en el registro. Tambien se escribirá en el registro si se ha vuelto al umbral aceptable.[opcional]. Por defecto 3000ms. El valor tiene que ser mayor de 1

  -h                Muestra esta ayuda.

Se require al menos el argumento "-s" que indica el servidor NTP. Ejemplo:
  >CheckNTPServer.exe -sntp.tuhora.com

Ejemplo utilizando todas las opciones:
  >CheckNTPServer.exe -sntp.tuhora.com -rRegistroTuHora.log -d1000
```
Esta solución está basada en este [post](https://stackoverflow.com/questions/1193955/how-to-query-an-ntp-server-using-c#:~:text=This%20is%20a%20optimized%20version%20of%20the%20function%20which%20removes%20dependency%20on%20BitConverter%20function%20and%20makes%20it%20compatible%20with%20NETMF%20(.NET%20Micro%20Framework)
) de GonzaloG en [stackoverflow](https://stackoverflow.com/questions/1193955/how-to-query-an-ntp-server-using-c#:~:text=This%20is%20a%20optimized%20version%20of%20the%20function%20which%20removes%20dependency%20on%20BitConverter%20function%20and%20makes%20it%20compatible%20with%20NETMF%20(.NET%20Micro%20Framework)
)
