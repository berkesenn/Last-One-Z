# Zombie Survival Game

A first-person survival horror game built with Unity 6.0, where players must survive against waves of zombies while managing their ammunition and health.

## 🎮 Game Overview

Fight for survival in a post-apocalyptic world overrun by zombies. Your objective is to survive as long as possible while eliminating the undead threats. The game features realistic AI pathfinding, headshot mechanics, and an intense survival experience.

## 🕹️ Controls

| Action | Key/Button |
|--------|-----------|
| Move | W, A, S, D |
| Look Around | Mouse |
| Shoot | Left Mouse Button |
| Reload | R |
| Sprint | Left Shift |
| Jump | Space |
| Flashlight | F |
| Pause Menu | ESC |

## ✨ Features

### Combat System
- **Headshot Mechanics**: Deal 50 damage with headshots, 25 damage with body shots
- **Weapon System**: Manual reload required when magazine is empty
- **Realistic Recoil**: Camera shake and weapon kickback on each shot
- **Visual Effects**: Muzzle flash and impact effects

### Enemy AI
- **NavMesh Pathfinding**: Zombies intelligently navigate around obstacles to reach the player
- **Dynamic Behavior**: Zombies walk when far away, run when they get closer
- **Attack System**: Close-range melee attacks with cooldown
- **Hit Reactions**: Zombies stagger when shot, with temporary movement pause

### Game Mechanics
- **Health System**: Monitor your health and avoid zombie attacks
- **Timer System**: Track your survival time with best time records
- **Wave Spawning**: Continuous zombie spawns to maintain pressure
- **Pause System**: Access pause menu with options to resume, return to menu, or quit

### UI/UX
- **Dynamic Crosshair**: Visual feedback for aiming
- **Ammo Counter**: Track remaining ammunition
- **Health Bar**: Visual health indicator
- **Death Screen**: Game over screen with fade effects
- **Main Menu**: Clean interface for starting the game

### Audio
- **Atmospheric Background Music**: Horror-themed soundtrack
- **Weapon Sounds**: Realistic gunshot audio
- **Enemy Sounds**: Zombie screams, attacks, and hit reactions
- **Footstep System**: Different sounds for walking and running

## 🎯 Game Objective

Survive as long as possible by eliminating zombies and avoiding their attacks. Your survival time is tracked, and the game records your best time. Ammunition is limited, so make every shot count—aim for headshots to conserve ammo and eliminate threats quickly.

## 🛠️ Technical Features

- **Unity Version**: 6.0 with Universal Render Pipeline (URP)
- **AI Navigation**: Unity NavMesh for intelligent enemy movement
- **Object-Oriented Design**: Clean, modular code architecture
- **Singleton Pattern**: Centralized managers (AudioManager, GameManager)
- **Event-Driven System**: Efficient communication between components

##  Getting Started

### Prerequisites
- Unity 6.0 or later
- AI Navigation package (install via Package Manager)

### Installation
1. Clone this repository
2. Open the project in Unity 6.0+
3. Install the AI Navigation package from Package Manager
4. Open the Main Menu scene from `Assets/Scenes/`
5. Press Play to start the game

## 📁 Project Structure

```
Assets/
├── Scenes/          # Game scenes (Main Menu, Main Game)
├── Scripts/         # All C# scripts
│   ├── GameManager.cs
│   ├── PlayerController.cs
│   ├── PlayerHealth.cs
│   ├── Enemy.cs
│   ├── EnemyHeadshot.cs
│   ├── WeaponSystem.cs
│   ├── AudioManager.cs
│   └── ...
├── Prefabs/         # Reusable game objects
└── Settings/        # URP and project settings
```

## 🐛 Known Issues

None at the moment. Please report any bugs in the Issues section.

## 📝 License

© 2025 Berke Sen. All Rights Reserved.

This project and its source code are protected by copyright. Unauthorized copying, distribution, or use is strictly prohibited without written permission from the author.

## 👤 Author

**Berke Sen**

Developed with Unity 6.0 and C#.

---

**Tip**: Aim for the head to maximize damage and conserve ammunition!
