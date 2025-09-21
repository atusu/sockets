using System.Net;
using System.Net.Sockets;
using System.Text;

namespace server;
public class Server {
    private static int SLEEP_TIME = 1000;
    public List<IClientConnection> clients {get; set;}
    public int port {get;set;}

    public Server(int port){
        this.port = port;
        clients = new List<IClientConnection>();
    }

    public void ServerInit()
    {
        TcpListener server = new TcpListener(IPAddress.Any, port);
        server.Start();

        Console.WriteLine("[server] Server started.");
        MainLoopSequencialServer(server);
    }

    private void MainLoopSequencialServer(TcpListener server)
    {
        while (true)
        {
            DateTime start = DateTime.Now;
            Console.WriteLine("[server] Waiting for clients..."); 

            if (server.Pending()) {
                var tcpClient = server.AcceptTcpClient();
                clients.Add(new ClientConnection(tcpClient));
                Console.WriteLine("[server] Added new client :)");
            }

            Console.WriteLine($"[server] Checking existing clients... (Total: {clients.Count()})");
            for(var i=0 ; i < clients.Count(); i++)
            {
                if(!clients[i].IsConnected()) {
                    clients.RemoveAt(i);
                    i--;
                    continue;
                }
                HandleClient(clients[i]);
            }
            
            Thread.Sleep(SLEEP_TIME);
        }
    }
    
    private void SendToClient(Stream stream, string message)
    {
        // the \n here is needed fot the nc test so we get the message on a new line always
        message = message[message.Count()-1].ToString() == "\n" ? message : message + "\n";
        var responseBytes = Encoding.ASCII.GetBytes(message);
        stream.Write(responseBytes, 0, responseBytes.Length);
    }

    private string Receive(Stream stream)
    {
        byte[] acknowledgmentBytes = new byte[1024];
        int bytesRead = stream.Read(acknowledgmentBytes, 0, acknowledgmentBytes.Length);
        string acknowledgment = Encoding.ASCII.GetString(acknowledgmentBytes, 0, bytesRead);
        return acknowledgment;
    }

    public bool ClientAlreadyExists(List<IClientConnection> clients, string name)
    {
        foreach (var client in clients) {
            if (client.Name == name) {
                return true;
            }
        }
        return false;
    }

    public void HandleClient(IClientConnection client)
    {
        if (!client.DataAvailable())
            return;
        
        var stream = client.GetStream();
        string message = Receive(stream);
        // some clients end a \n as well, like for example the netcat client (integration test). We trim it.
        message = message[message.Count()-1].ToString() == "\n" ? message.Substring(0, message.Count()-1) : message;
        Console.WriteLine($"[server] Handling client message: {message}");
        client.CommandHistory.Add(message);

        if(client.ClientState == ClientState.INIT) {
            SendToClient(stream, message == "/join" ? "OK" : "ERR: you cannot join server using this command.");
            if (message == "/join") {
                client.ClientState = ClientState.JOINED;
            }
            return;
        }

        if(client.ClientState == ClientState.JOINED) {
            if (ClientAlreadyExists(clients, message)) {
                SendToClient(stream, "ERR: name is already on server");
                return;
            }

            Console.WriteLine($"[server] Client {message} is now connected to the server.");
            client.Name = message;
            client.ClientState = ClientState.CONNECTED;
            SendToClient(stream, "OK");
            return;
        }

        if(client.ClientState == ClientState.CONNECTED){
            if (message == "/leave") {
                client.Close();
                clients.RemoveAll(c => c.Name == client.Name);
                Console.WriteLine("[server] Client disconnected: " + client.Name);
                return;
            }

            if(message == "/get-list") {
                string clientList = string.Join(", ", clients.Select(client => client.Name)) + "\n";
                SendToClient(stream, clientList);
                Console.WriteLine("[server] The list was sent to the client.");
                return;
            }

            SendToClient(stream, "Invalid command, please check spelling!");
            return;
        }
    }
}

