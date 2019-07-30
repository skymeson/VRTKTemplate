doomtrinity's FPS Prototype - version 1.2.0

lorenzo DOT pk3 AT gmail DOT com

//================================================================================

1. About this update
2. Bugs fixed
2. Thanks/Credits

//================================================================================
// 1. About this update
//================================================================================

New cool things have been added to this asset. In order of importance:

- Developer Console
	This is the main addition for this update. 
	The console is based on the 'DevConsole' asset by Antonio Cobo (see 'Credits').
	Anyway, I changed a substantial part of the code to make it work as I wanted.
	In particular, I put effort on the auto-complete (done by 'TAB' key). 
	I really love the 'Doom-style' console,	so I wanted to recreate it (or make something 
	that resembles it, at least) for my asset.	
	You will find more information in the technical readme.

- Adapted the code to support the new debugging commands
	Just to mention some of them...
	god <0-1> - player cannot be damaged
	noclip <0-1> - fly mode
	giveall - give all weapons with max ammo and restore health to max
	spawn <entityname> - to spawn an entity in front of you
	bind <key> <command> - to bind a console command to a button
	...type 'listcmds' and 'listcvars' to show all commands and cvars.	
- Changed the enemy script to support enemies without animator
- Simple (well, very simple) test scene I made to test console commands (enemy spawning)
- Improvements in the code and assets in general

Take a look to the technical documentation update for more information.

Reminder: even if this not a framework, you will find a lot of interesting techniques which 
you can reuse for your project - packing them in a complete game-like 
project will help you to understand how to use them. This is why I'll try to make things 
modular and decoupled as much as possible for each future update.
Anyway, I remind you that my current goal is to make a FPS prototype that resembles 
old-school FPS from the 90's, which have been developed for PC. 
So, this asset has actually been tested on PC only. Even though it may work on other devices
with some changes in the code, I'm not actually taking them into account.

//================================================================================
// 2. Bugs Fixed
//================================================================================

- Reloading the equipped weapon causes the reload of all other weapons in inventory
- Ammo pick-up may update the GUI with the ammo of other weapons.
- Other minor bugs

//================================================================================
// 2. Thanks/Credits
//================================================================================

Thanks again to all the people I mentioned in the previous readme files.
I want to thank new people for this update:
----------------------------------------------------------------------------------

Antonio Cobo - for his cool 'DevConsole' asset and for his permission to include it in my asset.
	You can find more resources about his work here:
	https://www.assetstore.unity3d.com/en/#!/content/16833
	https://forum.unity3d.com/threads/c...onitor-and-communicate-with-your-game.437909/
----------------------------------------------------------------------------------


//================================================================================









