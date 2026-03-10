# TornClaw

## Trailer:
[![Watch the trailer](https://img.youtube.com/vi/46xHJ6VbbVk/0.jpg)](https://www.youtube.com/watch?v=46xHJ6VbbVk)


## Premise:
Fly around as a witch on your speedy broom in the mystical mountains and collect the latent magical auras that linger in this dark, forbidden swamp. Each orb of aura you pick up is stored in a long snake tail behind you - be careful not to touch it! As you get richer, hire assistant birds that help extract more aura from these mires and upgrade your collection range to be able to farm more aura than ever before.

## Controls:

WASD 		Move
Space		Fly Up
Shift		Fly Down

Tab 		Shop
Mouse 	 	Free Look
(Hold)


# System Architecture:

- Managers Singletons
	- Game Manager (Handles orb/prey spawning and collection, stores player)

	- Effects Manager (Particle system effects when orbs burst)
	- Audio Manager (Uses Audiomixer to control global SFX / Music (TBA))
	- UI Manager (Updates text, handles UI clicks)

	- Save Manager (Saves orbs collected and purchases in JSON, can choose to load at start)
	- Shop Manager (Handle purchases, upgrades)
	


	- Snake Manager (Keeps a snake tail of all collected orbs that move after player's path)
		PathBuffer: Circular FIFO buffer that caches player's path + distances 
	  	  with O(1) write/read to determine each orb position based on individual radius



- PlayerController (Mimics aircraft dynamics (acceleration, dynamic handling, pitch/yaw, drag/glide) with RigidBody)
	Uses AnimationCurves to allow for tuning the gameplay feel
	Two modes: Flying, Tumbling (spinning out of control)

	- Player Model - Actual 3D model, separates visuals (angle, bobbing) from flying logic


	- Collector (Wrapper for orb collection trigger)
		- CollectorRanged (Upgradeable; pulls orbs to you as you get closer)

			Orb Puller (optimized to move many orbs with few calculations)

	- Camera Mover (Smoothly counter-moves camera according to player velocity and turning)
		- Pivot and Camera for separate localized movement

	- Audio Listener, Wind SFX (dynamic wind sound according to speed / altitude)
		- SFX Sound Player


	

- Orb (Collectable that follows player in a snake; acts as currency)
	Color Settings (Scriptable Object): Keeps a list of colors (+ emission) that gets randomly mixed for each orb

	States: 
		Collectable 
		-> InSnakeHidden (searching for workable spot in snake) 
		-> InSnake (Visible - when hit takes damager and bounces player away)

	Optimization: Only updates when necessary, minimal triggers (based on layers), turns off collider when grabbed

	
- Prey Cage (Prey will try to mostly fly inside this area)
	- Prey (Purchaseable; bird that flies around randomly and spawns orbs)




## UI Layouts:
- Start Menu
- Pause Menu

- Settings (Saves between sessions)

- Shop Menu (Purchaseable upgrades + birds)

- HUD (Updates collected orb count)

- Tutorial (Shows controls when near witch tower)
