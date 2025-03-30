# SOCKETS

## Installationb

### Linux
On Ubuntu (or other systems with `snap`):
```
sudo snap install dotnet-sdk --classic --channel 8.0/stable
```

### Windows
Just install Visual Studio or Visual Studio Code + dotnet stuff and windows handle it for you.

## Running the tests

### Bash end to end test
For Windows use Git Bash
```bash
cd [path]/Sockets/tests/end-to-end/bash
bash two_clients_join.sh
```

### .NET Tests (WIP)
```powershell
cd [path]/Server.Tests
dotnet test
```
