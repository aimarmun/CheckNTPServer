using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckNTPServer
{
    internal class Log
    {
        public string PathFile { get; }

        public Log(string pathFile)
        {
            PathFile = pathFile;
        }
        public void Write(string message, bool writeInConsole = true)
        {
            var finalMsg = $"[{DateTime.Now}] {message}";
            if(writeInConsole) Console.WriteLine(finalMsg);
            try
            {
                File.AppendAllLines(PathFile, finalMsg.Split(Environment.NewLine));
            }
            catch (Exception ex)
            {
                throw new Exception("Ocurrio un problema al intentar escribir en el archivo", ex);
            }
        }

        public void Write(string[] message, bool writeInConsole = true)
        {
            if(message.Length < 1) return;

            string[] finalMsg = new string[message.Length];

            Array.Copy(message, finalMsg, message.Length);

            finalMsg[0] = $"[{DateTime.Now}] {finalMsg[0]}";

            if (writeInConsole)
            {
                foreach (string line in finalMsg)
                {
                    Console.WriteLine(line);
                }
            }
            try
            {
                File.AppendAllLines(PathFile, finalMsg);
            }
            catch (Exception ex)
            {
                throw new Exception("Ocurrio un problema al intentar escribir en el archivo", ex);
            }
        }
    }
}
