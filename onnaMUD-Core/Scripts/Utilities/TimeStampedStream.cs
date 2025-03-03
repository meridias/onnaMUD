using onnaMUD;
using onnaMUD.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//this is for overriding the standard Write and Writeline methods so we can add timestamps automatically for the server classes since the log files will
//be written to as soon as they start
class TimeStampedStream : StreamWriter
{
    //private string logFile;
    //private StreamWriter stampedStream;
    //private bool fileCreated = false;

    public TimeStampedStream(string logFileName) : base(logFileName)//  Stream stream, string logFileName) : base(stream)//  string logFile)
    {
        //fileCreated = true;
        //logFile = logFileName;


    }

    public override void WriteLine(string message)
    {

        base.WriteLine(String.Format("{0} {1}", DateTime.Now.ToString("HH:mm:ss"), message));
        //SendToConsole(message);
    }

    public override void Write(string message)
    {
        base.Write(String.Format("{0} {1}", DateTime.Now.ToString("HH:mm:ss"), message));
    }

    private void SendToConsole(string message)
    {
        if (ServerConsole.newConsole != null)
        {
            ServerConsole.newConsole.WriteLine(message);
        }


    }
}
