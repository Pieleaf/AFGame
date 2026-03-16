## Aura Farmer

Large-scale collection game prototype: Focused on satisfying movement feel and performance optimization with scalable object systems. Throughout, I'm exploring ways to handle thousands of moving objects efficiently while maintaining responsive flight controls.

## Trailer

[![Watch the trailer](https://img.youtube.com/vi/46xHJ6VbbVk/0.jpg)](https://www.youtube.com/watch?v=46xHJ6VbbVk)


# Game Overview

Fly through a mystical forest swamp as a witch collecting magical aura orbs.

Collected orbs form a growing snake-like trail behind the player.
As the trail grows, navigating without colliding with it becomes increasingly difficult.

Players can access the shop to:

* Hire flying birds that generate additional aura
* Upgrade collection radius


# Technical Highlights

### Custom flight controller

Aircraft-inspired flight system implemented without relying on Unity physics simulation.

Features:

* AnimationCurve-driven acceleration and drag
* Quaternion-based rotation
* Glide and tumble states
* Smooth camera that complements the flight movement

This approach provided precise gameplay control and feel tuning while keeping CPU cost low.


### Scalable trailing snake system

Collected orbs form a long chain that follows the player’s previous movement.

Implementation:

* Circular path buffer - first in, first out
* Cached distance values between each point
* Constant-time lookup for quick segment positioning

This allows hundreds of orbs to follow the player smoothly with minimal overhead.


### Large-scale object management

The prototype supports 2000+ active orbs.

Optimizations include:

* State-based update logic
* Selective disabling of physics and collision checks
* Layer-based filtering for triggers
* Event-driven visual updates


### Efficient mass-movement system

The Orb Grabber system moves many orbs toward the player simultaneously while minimizing per-object calculations.

This allows the collector size to be upgraded without expensive per-frame physics interactions.


### Procedural AI movement

Flying creatures that generate aura use:

* Perlin-noise-based motion
* Containment logic to stay within a defined area

This produces natural movement patterns with minimal logic.


### Adaptive camera system

The camera moves against player motion using position smoothing to keep framing stable even during fast turns and dives.


### Save system

Player upgrades and progress are stored using:

* JSON serialization
* PlayerPrefs for persistence



## Performance Profiling

The prototype was tested with 2000+ active orbs to evaluate scalability. During development I used the Unity profiler to find bottlenecks in the code.

Optimizations included:

- Avoiding unnecessary Update calls and disabling physics, collision checks for orbs based on state

- Used object pooling for audio sources to avoid unnecessarily recreating the components every time

- Using a circular path buffer to make snake position calculations O(1)




# System Architecture (Simplified)

Core systems are separated into **gameplay, management, and presentation layers**.

```
PlayerController
      │
      ▼
 SnakeManager ──► PathBuffer
      │
      ▼
     Orbs
```


### Core Gameplay

**PlayerController**

Handles flight dynamics, player states, and interaction with the world.

**SnakeManager**

Maintains the collected orb chain using a circular path buffer.

**Orb**

State-driven collectible object with optimized update behavior.

**Prey**

Flying entities that generate new aura orbs.


### Managers

* GameManager — spawning and core game loop
* ShopManager — upgrades and purchases
* SaveManager — persistence
* UIManager — interface updates
* AudioManager — sound playback
* EffectsManager — particle effects


# Controls

```
WASD   Move
Space  Fly Up
Shift  Fly Down

Mouse  Free look
Tab    Open shop
```


# Tools & Technologies

* Unity, C#
* AnimationCurves for gameplay tuning
* ScriptableObjects for configurable orb visuals
* JSON serialization for save data
* ParticleSystem effects for orb collection
* New Unity Input System with untested controller support
