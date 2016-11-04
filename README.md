# CafiineServer
An extended Cafiine server mostly rewritten from scratch, handling the default Wii U Cafiine clients, based on the original server by MrRean.

It offers the following new functionality:
- Support for encrypted, hash-signed and timebombed [game data pack files](https://github.com/Syroot/CafiineServer/wiki/Game-Packs).
- Dump mode to dump any queried file automatically, into a [separate directory dedicated for dumps](https://github.com/Syroot/CafiineServer/wiki/Dump-Folder).
- [Command line parameters](https://github.com/Syroot/CafiineServer/wiki/Starting-The-Server) to set port and network interface to listen on.
- Optimized console output and [file logging](https://github.com/Syroot/CafiineServer/wiki/Logs-Folder).
- Several bigger and smaller code and performance optimizations.
- Running on Linux and macOS by targeting .NET 4.5 / Mono and .NET Standard 1.1.

The features are described in detail on the [wiki](https://github.com/Syroot/CafiineServer/wiki).
