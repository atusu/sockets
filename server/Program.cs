using System;
using server;

public class Program
{
    static void Main(string[] args)
    {
        int port = 8080;
        if (args.Length > 0)
        {
            port = Int32.Parse(args[0]);
        }
        Console.WriteLine($"Starting server on port: {port}");
        var server = new Server(port);
        server.ServerInit();
    }
}