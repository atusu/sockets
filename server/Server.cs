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
        foreach (var client in clients) 
        {
            if (client.Name == name) 
                return true;
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

        if(client.ClientState == ClientState.INIT) 
        {
            SendToClient(stream, message == "/join" ? "OK" : "ERR: you cannot join server using this command.");
            if (message == "/join") 
                client.ClientState = ClientState.JOINED;
            
            return;
        }

        if(client.ClientState == ClientState.JOINED) 
        {
            if (ClientAlreadyExists(clients, message)) 
            {
                SendToClient(stream, "ERR: name is already on server");
                return;
            }

            Console.WriteLine($"[server] Client {message} is now connected to the server.");
            client.Name = message;
            client.ClientState = ClientState.CONNECTED;
            SendToClient(stream, "OK");
            return;
        }
        
        if (client.ClientState == ClientState.GET_FILE_DETAILS)
        {
            string?[] parts = message.Split(' ');

            if (parts.Count() > 2)
            {
                SendToClient(stream, "ERR: invalid command");
                return;
            }
                
            if (!long.TryParse(parts[0], out var size) || size <= 0)
            {
                SendToClient(stream, "ERR: size must be a positive number");
                return;
            }
                
            if (parts[1].Length != 32)
            {
                SendToClient(stream, "ERR: invalid hash");
                return;
            }
                
            var file = client.SharedFiles.FirstOrDefault(f => f.Size == null && string.IsNullOrEmpty(f.Hash));
            var hash = parts[1];
                
            file.Size = size;
            file.Hash = hash;
            client.ClientState = ClientState.CONNECTED;
            SendToClient(stream, "OK");
            return;
        }

        if(client.ClientState == ClientState.CONNECTED)
        {
            if (message == "/leave") 
            {
                client.Close();
                clients.RemoveAll(c => c.Name == client.Name);
                Console.WriteLine("[server] Client disconnected: " + client.Name);
                return;
            }

            if(message == "/get-list") 
            {
                string clientList = string.Join(", ", clients.Select(c => c.Name)) + "\n";
                SendToClient(stream, clientList);
                Console.WriteLine("[server] The list was sent to the client.");
                return;
            }

            if (message.StartsWith("/share"))
            {
                if (!message.Contains(' '))
                {
                    SendToClient(stream, "ERR: incorrect command");
                    return;
                }
                
                var fileName = message.Substring(7);
                
                if (string.IsNullOrEmpty(fileName))
                {
                    SendToClient(stream, "ERR: no file provided");
                    return;
                }

                if (client.SharedFiles.Any(f => f.Name == fileName))
                {
                    SendToClient(stream, "ERR: file already shared");
                    return;
                }
                
                var newFile = new File { Name = fileName, Size = null, Hash = null };
                client.SharedFiles.Add(newFile);
                client.ClientState = ClientState.GET_FILE_DETAILS;
                SendToClient(stream, "OK");
                return;
            }
            
            if (message.StartsWith("/unshare"))
            {
                if (!message.Contains(' '))
                {
                    SendToClient(stream, "ERR: incorrect command");
                    return;
                }
                
                var fileName = message.Substring(9);
                
                if (string.IsNullOrEmpty(fileName))
                {
                    SendToClient(stream, "ERR: no file provided");
                    return;
                }
                
                client.SharedFiles.RemoveAll(f => f.Name == fileName);
                SendToClient(stream, "OK");
                return;
            }
            
            if (message.StartsWith("/list-files"))
            {
                if (!message.Contains(' '))
                {
                    SendToClient(stream, "ERR: incorrect command");
                    return;
                }
                
                var userName = message.Substring(12);

                if (string.IsNullOrEmpty(userName))
                {
                    SendToClient(stream, "ERR: no user provided");
                    return;
                }

                var identifiedClient = clients.FirstOrDefault(c => c.Name == userName);
                if (identifiedClient == null)
                {
                    SendToClient(stream, "ERR: no such user on server");
                    return;
                }

                if (identifiedClient.SharedFiles.Count == 0)
                {
                    SendToClient(stream, $"INFO: user {userName} shared no files");
                    return;
                }

                string filesList = string.Join("\n", identifiedClient.GetSharedFiles().Select(file =>
                    $"({file.Name}, {file.Size}, {file.Hash})")) + "\n";
                
                SendToClient(stream, filesList);
                return;
            }

            
            SendToClient(stream, "Invalid command, please check spelling!");
        }
    }
}

