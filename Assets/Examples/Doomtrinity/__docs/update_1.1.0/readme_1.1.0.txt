doomtrinity's FPS Prototype - version 1.1.0

lorenzo DOT pk3 AT gmail DOT com

//================================================================================

1. About this update
2. Thanks/Credits

//================================================================================
// 1. About this update
//================================================================================

New cool things have been added to this asset. In order of importance:

- Powerful Messaging System
Removed the C# 'event' stuff and replaced it with a powerful messaging system 
based on custom delegates and "message" classes. This is great for keeping the code decoupled. 

- Custom Update system for MonoBehaviour
Script components that need to use the 'Update' method ( classes derived from MonoBehaviour ), 
can now rely on a custom "master update" class instead, which allows to manage the update loop 
of the updateable objects in a more flexible fashion. It also allows to gain performance 
in relation to classic 'Update' method call, especially if you have hundreds of updateable 
objects in the scene.

- Custom Input Manager
This allows you to bind virtual keys to physical keys, and you can do that at runtime too! 
The settings menu has been implemented in "game" scenes too, so you can quickly change 
the keys configuration while in pause menu.

- Enhanced First Person Controller
Player can climb ladders and crouch. This has been implemented in the 'FirstPersonController' 
script of Unity Standard Assets - well, this script is heavily modified now.
Test it in the scene "_scenes/test_crouch_climb.unity".
 
	About crouch: you can customize the crouch height via Inspector. The size of the controller's 
	capsule will be calculated automatically when crouching. If there's an obstacle above the player 
	while he's crouching, he will stay in that state until the above space will be obstacle-free. 
	Player can also crouch while jumping, allowing him to pass through elevated holes.
 
	About climb: there's no need to explain the basic behaviour, so I'll move to few notes. 
	The player can jump toward the ladder to begin climbing. While on ladder, he can detach from it 
	at any time by jumping+moving or turning left/right ( the default "limit" angle is 90 degrees ). 
	To climb-down, the player must move backward. The setup is very easy, you just need a 
	"ladder trigger" gameobject - there are few rules to place it correctly.

- Enhanced Settings Menu
The main changes are in 'input' and 'video' menu. As mentioned above, the settings menu has been 
implemented in pause menu too. The video settings of the default Unity lancher have been added to
 the video menu - you can set the resolution from a list of all available resolutions 
 for your monitor, and the quality preset.

- Improvements in the code and assets in general
Lots of enhancements in the code to make it more robust.

Take a look to the technical documentation update for more information.

Reminder: even if this not a framework, you will find a lot of interesting techniques which 
you can reuse for your project - packing them in a complete game-like 
project will help you to understand how to use them. This is why I'll try to make things 
modular and decoupled as much as possible for each future update.
Obviously, if you already purchased this asset, you'll get this update with no additional cost!

//================================================================================
// 2. Thanks/Credits
//================================================================================

Thanks again to all guys I mentioned in the base readme 
( special thanks to Chris Dickinson for his book 'Unity 5 Game Optimization' ).
I want to thank new people for this update:
----------------------------------------------------------------------------------

Robert Nystrom - for his book 'Game Programming Patterns';
----------------------------------------------------------------------------------

Jon Skeet - for his book 'C# in Depth, 3rd Edition';
----------------------------------------------------------------------------------

Joseph Albahari and Ben Albahari - for the book 'C# 6.0 in a Nutshell: The Definitive Reference';
----------------------------------------------------------------------------------

Igor Aherne - for his amazing video tutorial:
https://www.youtube.com/watch?v=TwZH-aoSzJk
----------------------------------------------------------------------------------

3rd Party Contributers:
Non-music sounds  in this asset are slightly modified/adapted versions of sounds 
from http://www.freesound.org/
Following sounds are licensed under the 
Creative Commons Attribution 3.0 Unported (CC BY 3.0) license,
and CC0 1.0 Universal (CC0 1.0) license.
http://creativecommons.org/licenses/by/3.0/
http://creativecommons.org/publicdomain/zero/1.0/

By 'DasDeer' ( https://www.freesound.org/people/DasDeer/)
ladder_metalX.ogg ( https://www.freesound.org/people/DasDeer/sounds/161809/ ) - CC BY 3.0

//================================================================================









