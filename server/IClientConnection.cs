namespace server;

public interface IClientConnection
{
    string Name { get; set; }
    ClientState ClientState { get; set; }
    List<string> CommandHistory { get; set; }
    List<File> SharedFiles { get; set; }

    bool IsConnected();
    Stream GetStream();
    void Close();
    bool DataAvailable();
    List<File> GetSharedFiles();
}