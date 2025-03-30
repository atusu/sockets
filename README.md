# SOCKETS

## Installation

### Windows
Install Visual Studio or Visual Studio Code + dotnet runtime 8.0 and Windows will handle it for you.

### Linux
Tested on Raspberry Pi OS and Ubuntu 24.04
```
cd $HOME/kits
wget https://dot.net/v1/dotnet-install.sh
sudo bash ./dotnet-install.sh --version 8.0.100 --install-dir `pwd`/dotnet
echo 'export DOTNET_ROOT="$HOME/libs/dotnet"' >> ~/.bashrc
echo 'export PATH="$PATH:$HOME/libs/dotnet"' >> ~/.bashrc
```

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
