# CafiineServer
An extended Cafiine server mostly rewritten from scratch, handling the default Wii U Cafiine clients, based on the original server by MrRean.

It offers the following new functionality:
- Support for encrypted, hash-signed and timebombed [game data pack files](https://github.com/Syroot/CafiineServer/wiki/Game-Packs).
- Dump mode to dump any queried file automatically, into a separate directory dedicated for dumps.
- Command line parameters to set port and network interface to listen on.
- Optimized console output and file logging.
- Several bigger and smaller code and performance optimizations.

## Note for testers
This server looks into the `data` directory instead of `cafiine_root` by default! In case you want to keep all your stuff in `cafiine_root`, pass the `/DATA=cafiine_root` parameter to the server. It will tell you the data directory it looks in at the start.

## Dumping Files
This server does not store dumped files besides their `-request` file in the data folder like the original server does, it stores them in the dump folder under their original path.

The following methods can be used to dump files:
- The classic way: Create an empty file in the data directory, ending on `-request`. If a file without this postfix is queried, it will be stored in the dump directory under the same path. If you have problems dumping a file this way (e.g. the console randomly disconnects), use the `-request_slow` postfix, which will dump the file slower, but more safely.
- The new hardcore way: Start the server with the `/DUMPALL` parameter. This will dump absolutely every file queried by the Cafiine client. Some files are known to throw up the client, you should exclude this file. Files are not replaced at all when running the server in this mode.
- To exclude a file from the dump, just create an empty file in the dump folder with the same path and name of the file you want to block (if the server finds a file already in the dump directory, it skips dumping it again).

## Optimized Console Output / Logging
I like fancy text and made the console output a bit more colorful and changed a lot of the messages. When using game packs, replaced files are of course not shown to keep the modded file names secret (it still shows which files are queried).

File logging creates a new folder in the log directory each time a server is started, and client logs are created in there for each client, not each connection. This makes it easier to find problems a specific client reported in case you connect multiple Wii U's to the server. The `/NOLOGS` parameter disables any file logging (but console output will still be visible). The log directory can also be set with the `/LOGS` parameter.
