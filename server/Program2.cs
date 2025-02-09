// using System.Net;
// using System.Net.Sockets;
// using System.Text;

// public class ClientData
// {
//     public TcpClient TcpClient { get; set; }
//     public NetworkStream NetworkStream { get; set; }
//     public string Name { get; set; }
//     public List<string> CommandHistory {get; set;}
// }

// public class Server2
// {
//     private static List<ClientData> clients = new List<ClientData>();

//     static void Main()
//     {
//         TcpListener server = new TcpListener(IPAddress.Any, 8080);
//         server.Start();

//         Console.WriteLine("Server started. Waiting for clients...");

//         while (true)
//         {
//             while (server.Pending())
//             {
//                 TcpClient newClient = server.AcceptTcpClient();

//                 ClientData newClientData = new ClientData
//                 {
//                     TcpClient = newClient,
//                     NetworkStream = newClient.GetStream(),
//                     Name = string.Empty,
//                     CommandHistory = new List<string>()
//                 };

//                 clients.Add(newClientData);

//                 ThreadPool.QueueUserWorkItem(HandleClient, newClientData);
//             }


//             // Check for incoming messages from existing clients
//             foreach (ClientData clientData in clients)
//             {
//                 if (clientData.NetworkStream.DataAvailable)
//                 {
//                     // Handle incoming messages for existing clients
//                     ThreadPool.QueueUserWorkItem(HandleClient, clientData);
//                 }
//                 // TODO: check if clients disconnected and remove it from the list of clients.
//             }

//             // You can add a delay to reduce CPU usage
//             Thread.Sleep(100);
//         }

//     }

//     static void HandleClient(object state)
//     {
//         if (state is ClientData clientData)
//         {
//             TcpClient tcpClient = clientData.TcpClient;
//             NetworkStream stream = clientData.NetworkStream;

//             Console.WriteLine("Handling: " + clientData.Name);

//             byte[] buffer = new byte[1024];
//             int bytesRead;

//             try
//             {
//                 // Wait for the initial message from the client
//                 bytesRead = stream.Read(buffer, 0, buffer.Length);
//                 string message = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();

//                 Console.WriteLine($"Received message: {message}");

//                 //If empty command history, we check for valid join command
//                 if(clientData.CommandHistory.Count == 0)
//                 {
//                     //Check command for new client
//                     if(message == "/join")
//                     {
//                         Console.WriteLine("Client wants to join our server - asking for name");

//                         // Respond with acknowledgment (you can add more logic here)
//                         byte[] responseBytes = Encoding.ASCII.GetBytes("OK\n");
//                         stream.Write(responseBytes, 0, responseBytes.Length);
//                         //Add command to client history
//                         clientData.CommandHistory.Add(message);
//                     }
//                     else
//                     {
//                         Console.WriteLine($"Unrecognized command received: {message}");
//                         string response = "Cannot join the server - please check command :)\n";
//                         byte[] responseBytes = Encoding.ASCII.GetBytes(response);
//                         stream.Write(responseBytes, 0, responseBytes.Length);
//                     }
//                 }
//                 else if(clientData.CommandHistory.Count == 1)
//                 {
//                     // var clientExists = clients.Select(c => c.Name == clientData.Name).First();
//                     // if(clientExists){
//                     //     //Respond with an error message
//                     //     byte[] responseBytes = Encoding.ASCII.GetBytes("Client with this name already exists");
//                     //     stream.Write(responseBytes, 0, responseBytes.Length);
//                     //     Console.WriteLine("Client with this name already exists");
//                     //     return;
//                     // }

//                     byte[] responseBytes = Encoding.ASCII.GetBytes("OK\n");
//                     stream.Write(responseBytes, 0, responseBytes.Length);

//                     Console.WriteLine($"Name received: {message}");
//                     //Save client name
//                     clientData.Name = message;
//                     //Save command in history
//                     clientData.CommandHistory.Add("Name received: " + clientData.Name);
//                 }
//                 else
//                 {
//                     if(message == "/leave")
//                     {
//                         //remove client from list according to command
//                         clients.RemoveAll(c => c.Name == clientData.Name); // TODO: client is guaranteed to have unique name
//                         tcpClient.Close();
//                         Console.WriteLine("Client disconnected.");
//                         foreach (var client in clients)
//                             Console.WriteLine(client.Name);

//                     }
//                     else if(message == "/get-list")
//                     {
//                         //Respond with the list of connected client names
//                         string clientList = string.Join(", ", clients.Select(client => client.Name));
//                         byte[] responseBytes = Encoding.ASCII.GetBytes(clientList + "\n");
//                         stream.Write(responseBytes, 0, responseBytes.Length);
//                         Console.WriteLine("The list was sent to the client.");
//                     }
//                     else
//                     {
//                         //Respond with an error message to other commands
//                         byte[] responseBytes = Encoding.ASCII.GetBytes("Invalid command\n");
//                         stream.Write(responseBytes, 0, responseBytes.Length);
//                         Console.WriteLine("Client sent an invalid command");
//                     }
//                 }
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Error: {ex.Message}");
//             }
//             finally
//             {

//             }
//         }
//     }
// }
