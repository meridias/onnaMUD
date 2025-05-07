using onnaMUD.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//this is for the console output for errors since we don't want to create a streamwriter file unless we actually have something we need to write to it
class TimeStampedTextWriter : TextWriter
{
    private bool isFileCreated = false;
    private StreamWriter logStream;
    private string logFile;

    public override Encoding Encoding
    {
        get { return Encoding.ASCII; }
    }

    public TimeStampedTextWriter()//string logFileName)
    {
        //logFile = logFileName;

    }

    public override void WriteLine(string message)
    {
        /*   if (!isFileCreated)
           {
               logStream = File.CreateText(logFile);
               logStream.AutoFlush = true;
               isFileCreated = true;
           }
           logStream.WriteLine(String.Format("{0} {1}", DateTime.Now.ToString("HH:mm:ss"), message));
           ServerConsole.newConsole.WriteLine(message);*/
        Console.WriteLine($"***ERROR***{message}");//this will send this error message to the currently redirected console output (which is the logFile)
        
        //logStream.WriteLine($"***ERROR***{message}");
    }

    public override void Write(string message)
    {
        if (!isFileCreated)
        {
            logStream = File.CreateText(logFile);
            logStream.AutoFlush = true;
            isFileCreated = true;
        }
        logStream.Write(String.Format("{0} {1}", DateTime.Now.ToString("HH:mm:ss"), message));
        ServerConsole.newConsole.WriteLine(message);
    }



}
