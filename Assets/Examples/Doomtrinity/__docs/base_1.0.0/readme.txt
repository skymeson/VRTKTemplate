doomtrinity's FPS Prototype - version 1.00

lorenzo DOT pk3 AT gmail DOT com

//================================================================================

1. About
2. Installation
3. Keys
4. FAQ
5. What's Next?
6. Thanks/Credits

//================================================================================
// 1. About
//================================================================================

This is a super-simple example of (single-player) First Person Shooter game.
It collects some nice features together in a well documented FPS game project.
Specifically, this project aims to recreate the fast-paced style of milestones like 'Quake'.

This is neither a game, nor a framework, at least not in current version.

It's my very first project in Unity, and my goal is to enhance it for my future projects.

Despite the pretty spartan look, there are some interesting things in it.
This is what you'll get ( more info in the technical documentation, 
which you can find in 'Assets/__docs/technical.txt' ):

- 2 simple maps: 
	"Firing Range" and "Vendetta".	
	
- 3 weapons with relative ammo:	
	pistol, machinegun and shotgun.
		
- "Pick up" objects:
	health pack, ammo and weapons.
	
- Player Inventory:
	weapons and ammo.
	
- GUI:
	main menu ( with settings ), pause, end level and game over menu, HUD.
	The HUD shows ammo, enemy counter, map title, and objective.
	
- AI:
	Some dumb enemies that will run towards you!
	
- Save and Load from/to xml:
	game ( enemies transform etc. ) and settings stuff from/to 2 external xml files.
	
- Prefab Pooling System:
	for projectiles, shells, projectile impact particles.
	
//================================================================================
// 2. Installation and configuration
//================================================================================

Everything should work out of the box.
Just import the package in a new project, and start the scene "_scenes/Menu".
Scenes also works if launched independently, but you actually should launch the game 
from the menu for minor reasons ( settings, for example ).
More info on project and scene setup in the technical documentation.

//================================================================================
// 3. Keys
//================================================================================

The game uses typical keys of first person shooters:

	W - A - S - D : --- move;
	SPACE : ----------- jump;
	SHIFT : ----------- run;
	1 : --------------- pistol;
	2 : --------------- machinegun;
	3 : --------------- shotgun;
	LMB : ------------- fire;
	R : --------------- reload;
	F7 : -------------- quick save;
	M : --------------- objective;
	ESC: -------------- pause;
	
Key numbers for weapons are the ones on top of alphanumeric keyboard.


//================================================================================
// 4. FAQ - Few Awkward Questions...
//================================================================================

Q.	What's the strength of this package? Why should I buy it?
A.	Firstly, it's very simple, but it covers some important aspects in Unity.
	No pumped up graphics, just the core for an old-school FPS :)
	Also, my goal is to enhance it over time, without altering its style.

Q.	Why this is not free?
A.	I believe in support of free software, I worked - and I keep working on free and 
	open source game modifications, where I put several hundreds hours of work, and I really love it.
	But I'm actually trying to survive with game development. This means that I need to gain
	money with it somehow, and this time I choose to sell this asset. 
	
Q.	When I will see the next release?
A.	I'd really like to see some feedback so I'll have one more reason to do faster releases!
	Anyway, any important fix will be released as soon as possible.
	

//================================================================================
// 5. What's Next?
//================================================================================

These are the features I'm gonna put in next releases of this project.

- Enhanced save and load;
- Enhanced player controller and input;
- Melee weapon system;
- Long range attack by enemy;
- In-game console ( to show debug messages at the moment, maybe few simple commands );
- Raise and lower anims for weapons.

There are plenty of things I want to implement, but this is what I'm planning to do 
after this release.
Also, a new enemy, map or weapon will be in every major update of the project :)
I could release minor updates to fix any possible critical bug ( hope not so! ),
these updates won't include new contents.


//================================================================================
// 6. Thanks/Credits
//================================================================================

Alan Thorn - for his book, "Mastering Unity Scripting".
----------------------------------------------------------------------------------

Chris Dickinson - for his book, "Unity 5 Game Optimization";
special thanks for the 'Prefab Pooling System' code.
----------------------------------------------------------------------------------

Sebastian Lague - for his great TDS tutorial -  https://www.youtube.com/user/Cercopithecan/featured
----------------------------------------------------------------------------------

Daniele Gazzetta - for music - special thanks for making soundtracks for me!
----------------------------------------------------------------------------------

Unity Technologies - Standard Assets;
I used a slightly modified version of the following assets from Unity Standard Assets: 

- scripts/Character/Player/FirstPersonController.cs;
- scripts/Character/Player/MouseLook.cs;
- scripts/Util/CurveControlledBob.cs;
- scripts/Util/FOVKick.cs;
- scripts/Util/LerpControlledBob.cs;
- shaders/Skybox-Procedural;
----------------------------------------------------------------------------------

Unity Community - for the priceless support and documentation.
----------------------------------------------------------------------------------

3rd Party Contributers:
Non-music sounds  in this asset are slightly modified/adapted versions of sounds from http://www.freesound.org/
Following sounds are licensed under the Creative Commons Attribution 3.0 Unported (CC BY 3.0) license,
and CC0 1.0 Universal (CC0 1.0) license.
http://creativecommons.org/licenses/by/3.0/
http://creativecommons.org/publicdomain/zero/1.0/

By 'unfa' ( https://www.freesound.org/people/unfa/ )
pistol_fire.ogg ( https://www.freesound.org/people/unfa/sounds/187119/ ) - CC BY 3.0

By 'ermfilm' ( https://www.freesound.org/people/ermfilm/ )
shotgun_fire.ogg ( https://www.freesound.org/people/ermfilm/sounds/162431/ ) - CC BY 3.0
shotgun_reload.ogg ( https://www.freesound.org/people/ermfilm/sounds/162533/ ) - CC BY 3.0
machinegun_fire.ogg ( https://www.freesound.org/people/ermfilm/sounds/162435/ ) - CC BY 3.0
machinegun_reload.ogg ( https://www.freesound.org/people/ermfilm/sounds/162430/ ) - CC BY 3.0

By 'hintringer' ( http://www.freesound.org/people/hintringer/ )
player_death.ogg ( http://www.freesound.org/people/hintringer/sounds/250033/ ) - CC0 1.0

By 'aglinder' ( https://www.freesound.org/people/aglinder/ )
enemy_attack.ogg, enemy_death.ogg, impact1.ogg
( https://www.freesound.org/people/aglinder/sounds/265580/ ) - CC0 1.0

//================================================================================

