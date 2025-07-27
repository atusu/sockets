using server;
using System.Text;

namespace Server.Tests;

public class MockClientConnection : IClientConnection
{
    private MemoryStream _readStream;
    private MemoryStream _writeStream;

    public string Name { get; set; }
    public ClientState ClientState { get; set; } = ClientState.INIT;
    public List<string> CommandHistory { get; set; } = new();


    public void SetInput(string input)
    {
        _readStream = new MemoryStream(Encoding.ASCII.GetBytes(input + "\n"));
        _writeStream = new MemoryStream();
    }

    public bool IsConnected() => true;

    public Stream GetStream() => new CombinedStream(_readStream, _writeStream);

    public void Close() { }

    public string GetResponse()
    {
        return Encoding.ASCII.GetString(_writeStream.ToArray()).Trim();
    }

    // Helper to combine read/write into one stream
    private class CombinedStream : Stream
    {
        private readonly Stream _read;
        private readonly Stream _write;

        public CombinedStream(Stream read, Stream write)
        {
            _read = read;
            _write = write;
        }

        public override bool CanRead => _read.CanRead;
        public override bool CanSeek => false;
        public override bool CanWrite => _write.CanWrite;
        public override long Length => _read.Length;
        public override long Position { get => _read.Position; set => _read.Position = value; }

        public override void Flush() => _write.Flush();
        public override int Read(byte[] buffer, int offset, int count) => _read.Read(buffer, offset, count);
        public override void Write(byte[] buffer, int offset, int count) => _write.Write(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => _read.Seek(offset, origin);
        public override void SetLength(long value) => _write.SetLength(value);
    }
}