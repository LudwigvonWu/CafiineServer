# CafiineServer
An extended Cafiine server mostly rewritten from scratch, handling the default Wii U Cafiine clients, based on the original server by Chadderz.

At the moment, it offers the following new functionality:
- Support for encrypted, hash-signed and time-bombed game data packs files (s. section below).
- Command line parameters to set port an network interface to listen on.
- Optimized console output.
- Several bigger and smaller code and performance optimizations.

The following features are planned:
- Dump mode to dump any queried file automatically, into a separate directory dedicated for dumps.
- Optimized file logging (at the moment, file logging is completely removed).

## Game Data Packs
Besides supporting the classic method of a raw file structure directly stored in title ID directories under the root directory (like the original Cafiine server does), this server additionally supports so called game data packs. These are a containers of a custom file format (*.csgp), imaginable like a ZIP file, storing files and folders inside of them. Other than being a file, they are handled like the classic title ID folders, put into the Cafiine root / data folder.

They have the following advantages:
- They encrypt files and file names, thus making it harder for curious sneaky eyes to look into the mods. This does *not* mean that the mods are undecryptable; in fact, encryption is very simple and easily reversable. It should be seen as a "script kiddie" filter.
- The whole container stores a hash checked at load time to prevent nooby tampers.
- They can be time-bombed to not work outside of a set time period. When a player tries to use a game pack outside its intended time period, no data will be sent back to Cafiine. This mostly crashes the Wii U, so make sure to tell players at which time the package can be used. Again, this is *not* safe, hence the source is public and the code handling the time can simply be removed.
- They can be shared and managed as just one file without the need to pack / unpack them. 

## PackCreator Tool
Game packs are created with the PackCreator tool. It accepts a parameter specifying the name of the file in which the pack will be stored (which will be forced to have the .csgp extension to be recognized by the server later on). Another parameter specifies the directory which contents the pack will store. This is a typical title ID directory as used in the original Cafiine server. Additionally, time-bomb dates can be set. A custom encryption key is automatically created.

Some examples of calling the PackCreator tool:
- `PackCreator /TARGET=D:\Cafiine\data\mk8.csgp /SOURCE=D:\Mods\00050000-1010ED00`

  This stores all the contents of `00050000-1010ED00` in an `mk8.csgp` file (including the folder itself to know the title ID).
- `PackCreator /TARGET=D:\Cafiine\data\toadondrugs.csgp /SOURCE=D:\Mods\toady ROOTNAME=00050000-1010ED00`

  This stores all the contents of the `toady` folder in a `toadondrugs.csgp` file, with the root folder's name becoming `00050000-1010ED00` (useful if you want to change a title ID on the fly).
- `PackCreator /TARGET=mycoolmod SOURCE=D:\cafiine_old\cafiine_root\00050000-1010EC00 /MINDATE=12:00-11.06.2016`

  The created pack `mycoolmod.csgp` cannot be used before 11.06.2016, 12:00 UTC.
- `PackCreator -TARGET=blub SOURCE=D:\mk8mods\yoshishit\00050000-1010ED00 /MAXDATE=23:59-31.12.2016`

  The created pack `blub.csgp` makes itself invalid when being used after New Year's Eve 2016. Note how you can use parameters with `-` or no prefix.

The packs are then put into the root / data directory of the Cafiine server, besides any possible classic raw title ID directories.

If there are collisions between packs or with the raw file system having the same files multiple times, the following path is taken to resolve the collision:
- The raw file system always has priority over packs (so you can still play around there before creating a pack).
- The packs are prioritized in alphabetical order (e.g. "aaatakethismod.csgp" has higher priority than "crapmod.csgp").
 
## Dumping Files
This server does not store dumped files beside their "-request" file like the original server does, it stores them in the specified dump folder (being "dump" by default), recreating the folder structure there.

If the server is started with the `/DUMPALL` parameter, every queried file will be dumped, basically recreating the game contents in the `dump\titleID` folder. Files on the Wii U are not replaced in this mode.

## Optimized Console Output / Logging
I like fancy text and made the console output a bit more colorful and changed a lot of the messages. When using game packs, replaced files are of course not shown to keep the modded file names secret (it still shows which files are queried).

Logging tries to store complete logs for a client, rather than a connection (not yet implemented). The log folder can be set when starting the server.
