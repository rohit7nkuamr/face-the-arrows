# 🏹 Face the Arrows

![Unity](https://img.shields.io/badge/Unity-2021.3%2B-black?style=flat-square&logo=unity)
![Platform](https://img.shields.io/badge/Platform-Android%20%7C%20iOS-blue?style=flat-square)
![Genre](https://img.shields.io/badge/Genre-Endless%20Runner-green?style=flat-square)
![License](https://img.shields.io/badge/License-MIT-yellow?style=flat-square)

**Face the Arrows** is an adrenaline-pumping 3D endless runner where you must outrun savage wild animals while dodging deadly arrows in a treacherous jungle path. How far can you survive?

## 🎮 Game Overview

You are an adventurer who has stolen a sacred idol from an ancient temple. Now, mystical wild animals chase you relentlessly while hidden traps unleash volleys of arrows. Your only hope? Run faster, dodge smarter, and survive longer!

### ✨ Key Features

- **🏃 3-Lane Running System** - Swipe left/right to switch between lanes
- **🏹 Arrow Dodging Mechanic** - React to warning indicators and dodge incoming arrows
- **❤️ Health System** - Survive up to 3 arrow hits before game over
- **🐾 Wild Animal Chase** - Relentless pack of shadow panthers and spirit wolves
- **📈 Progressive Difficulty** - Game gets harder the longer you survive
- **🏆 High Score System** - Compete for the best distance and score
- **💎 Power-Ups** - Collect shields, health boosts, and score multipliers
- **📱 Mobile Optimized** - Intuitive swipe controls for mobile devices

## 🎯 Gameplay

### Core Mechanics

1. **Run** - Character runs forward automatically at increasing speed
2. **Dodge** - Swipe left/right to switch lanes and avoid obstacles
3. **Jump** - Swipe up to jump over low obstacles
4. **Slide** - Swipe down to slide under high obstacles
5. **Survive** - Avoid arrows (3-hit system) and obstacles (instant death)

### Controls

| Action | Mobile | PC (Testing) |
|--------|--------|--------------|
| Move Left | Swipe Left | Left Arrow |
| Move Right | Swipe Right | Right Arrow |
| Jump | Swipe Up | Up Arrow |
| Slide | Swipe Down | Down Arrow |
| Pause | Tap Pause Button | ESC |

## 🛠️ Technical Details

### Built With

- **Unity 2021.3+** - Game engine
- **C#** - Programming language
- **Universal Render Pipeline** - Graphics pipeline
- **Input System** - New Unity Input System for mobile controls

### Architecture

The game follows a modular architecture with singleton managers:

- **GameManager** - Handles game state, scoring, and scene flow
- **PlayerController** - Manages player movement and health
- **ArrowSpawner** - Controls arrow generation with warning system
- **LevelGenerator** - Creates endless level segments
- **ObstacleSpawner** - Spawns environmental hazards
- **UIManager** - Manages all UI elements and screens

## 📦 Installation

### For Players (Android)

1. Download the latest APK from [Releases](https://github.com/yourusername/face-the-arrows/releases)
2. Enable "Install from Unknown Sources" on your Android device
3. Install and play!

### For Developers

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/face-the-arrows.git
   cd face-the-arrows
   ```

2. **Open in Unity**
   - Unity version: 2021.3 or higher
   - Open Unity Hub and add the project
   - Let Unity import all assets

3. **Configure Build Settings**
   - File → Build Settings
   - Switch to Android/iOS platform
   - Configure player settings

4. **Run in Editor**
   - Open `Assets/Scenes/SampleScene.unity`
   - Press Play to test

## 🎨 Assets Required

### 3D Models (Blender)
- **Player Character** - Adventurer/Explorer
- **Wild Animals** - Shadow panthers, spirit wolves
- **Environment** - Jungle path segments, trees, ruins
- **Obstacles** - Logs, rocks, vines
- **Arrow** - Simple arrow model
- **Power-ups** - Shield, health, magnet icons

### Audio
- Background music (jungle drums)
- Sound effects:
  - Footsteps
  - Arrow whoosh/impact
  - Animal growls
  - Power-up collection
  - UI sounds

## 🚀 Roadmap

- [x] Core gameplay mechanics
- [x] Arrow spawning system
- [x] Endless level generation
- [x] Mobile controls
- [x] Power-up system
- [ ] Character customization
- [ ] Daily challenges
- [ ] Leaderboards
- [ ] Achievement system
- [ ] Multiple environments

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👥 Credits

- **Game Design & Programming** - Rohit Kumar
- **Inspired by** - Temple Run, Subway Surfers
- **Unity Asset Store** - [List any assets used]

## 📧 Contact

- **Developer** - Rohit Kumar
- **Email** - rohit7ngod@gmail.com
- **Project Link** - https://github.com/rohit7nkuamr/face-the-arrows
## 🎮 Screenshots

[Add screenshots here once the game is ready]

---

<p align="center">Made with ❤️ and Unity</p>
