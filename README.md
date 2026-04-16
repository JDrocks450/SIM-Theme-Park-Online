<p align="center"> 
  <img src="https://github.com/user-attachments/assets/675ef66b-8590-44e2-b4f0-94627bbbf35e" height="250"> 
</center>

# SIM Theme Park Online v1.0 Server Emulator

![image](https://user-images.githubusercontent.com/16988651/183787176-372a4f0e-1925-432e-b8d3-a335eab18c33.png)

A project aimed to making a compatible server for the original game client, primarily for documentation/preservation purposes.

## Additional Resources
**TCRF Article** - Documentation on these defunct online functions can be found here: [See more on The Cutting Room Floor](https://tcrf.net/Sim_Theme_Park_(Windows,_Mac_OS_Classic)/SIM_Theme_Park_Online)

**SIM Theme Park Instruction Booklet** - Become familiar with the original game's online functions by viewing the instruction booklet. [See more on The Internet Archive](https://archive.org/details/sim-theme-park-manual)

## Project Status
Functional (with missing features)! This project features a nice UI for hosting a local server, so running it shouldn't be too problematic for most users. 
Setting up the game to use TPW-SE is fairly straight-forward as well. 

*Note that due to game-related issues, the game has only been seen to connect to locally hosted servers, not remote ones.*

## Setup
After cloning the repository, most code is written entirely from scratch. You should not need any extra prerequesites other than a
valid copy of Visual Studio compatible with .NET 5.0 / 6.0, and the WPF package installed.

To ensure the game connects to your server, refer to the Online.sam file, located in your installation directory */data* directory.

It should look like this: https://github.com/JDrocks450/SIM-Theme-Park-Online/wiki/Example-Online.sam-Configuration-for-TPW-SE-development

## Roadmap
For roadmap information, see the following document.
https://1drv.ms/w/s!AuOXXqHhGp4krQumTp-ot6Bt0GZR?e=kANSRc

## TPW-SE / Bullfrog Protocol
I have drafted a brief resource to aid in shedding light on the packet structure that Theme Park World / SIM Theme Park used to create the 
online world seen in-game. While it is incomplete, it does contain some important details.

*Check out the Wiki for more information.*
