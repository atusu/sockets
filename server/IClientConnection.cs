namespace server;

public interface IClientConnection
{
    string Name { get; set; }
    ClientState ClientState { get; set; }
    List<string> CommandHistory { get; set; }

    bool IsConnected();
    Stream GetStream();
    void Close();
}