using System.Net.Sockets;
namespace server;
public class ClientConnection : IClientConnection
{
    private TcpClient _tcpClient;

    public ClientConnection(TcpClient tcpClient)
    {
        _tcpClient = tcpClient;
    }

    public string Name { get; set; }
    public ClientState ClientState { get; set; } = ClientState.INIT;
    public List<string> CommandHistory { get; set; } = new();

    public bool IsConnected()
    {
        try
        {
            return !(_tcpClient.Client.Poll(1, SelectMode.SelectRead) && _tcpClient.Client.Available == 0);
        }
        catch
        {
            return false;
        }
    }

    public bool IsNewConnection() {
        return CommandHistory.Count == 0;
    }

    public Stream GetStream() => _tcpClient.GetStream();

    public void Close() => _tcpClient.Close();
    public bool DataAvailable() => _tcpClient.GetStream().DataAvailable;
}
