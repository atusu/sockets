using System.Dynamic;
using System.Net.Sockets;
namespace server;

public class ClientData
{
    public required TcpClient TcpClient { get; set; }
    public string? Name { get; set; }
    public List<string> CommandHistory { get; set; } = new List<string>();

    public bool IsConnected()
    {
        try
        {
            return !(TcpClient.Client.Poll(1, SelectMode.SelectRead) && TcpClient.Client.Available == 0);
        }
        catch
        {
            return false; // If an exception occurs, the client is disconnected.
        }
    }

    public bool IsNewConnection() {
        return CommandHistory.Count == 0;
    }
}
