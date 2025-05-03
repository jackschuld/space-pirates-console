# SpacePirates Console Game

A terminal-based space game built with C# and .NET 8.0, featuring physics-based movement, ship management systems, and real-time combat mechanics.

## Features

- Physics-based ship movement with inertia and momentum
- Advanced ship systems:
  - Hull integrity management
  - Shield system with recharge mechanics
  - Engine performance and fuel consumption
  - Cargo management
  - Weapon systems
- Real-time status display with:
  - Hull and shield percentages
  - Fuel levels
  - Position coordinates
  - Credit balance
- Efficient console rendering with buffer management
- Star field background
- Collision detection with boundary physics

## Technology Stack

### Console Application (.NET 8.0)
- **Language**: C# 12
- **Framework**: .NET 8.0
- **Project Type**: Console Application
- **Architecture**: Multi-project solution with API backend

### Packages
#### Console Application
- `Microsoft.EntityFrameworkCore` (8.0.13) - Core ORM functionality
- `Microsoft.EntityFrameworkCore.Relational` (8.0.13) - Relational database support

#### API Backend
- `Microsoft.AspNetCore.OpenApi` (8.0.8) - OpenAPI support
- `Microsoft.EntityFrameworkCore` (8.0.13) - Core ORM functionality
- `Microsoft.EntityFrameworkCore.Tools` (8.0.13) - EF Core CLI tools
- `Pomelo.EntityFrameworkCore.MySql` (8.0.0) - MySQL database provider
- `Swashbuckle.AspNetCore` (8.0.13) - Swagger/OpenAPI tooling

## Requirements

- .NET 8.0 SDK
- MySQL Server (for game state persistence)
- Windows Terminal (recommended) or Command Prompt
- Git (for version control)

## Getting Started

1. Clone the repository:
```bash
git clone <repository-url>
cd SpacePirates.Console
```

2. Install the .NET 8.0 SDK if not already installed:
- Download from [.NET Download Page](https://dotnet.microsoft.com/download/dotnet/8.0)

3. Set up the database:
```bash
cd ../SpacePirates.API
dotnet ef database update
```

4. Build the solution:
```bash
dotnet build
```

5. Run the game:
```bash
cd ../SpacePirates.Console
dotnet run
```

## Controls

- **Arrow Keys**: Control ship movement
  - Up: Thrust forward
  - Down: Thrust backward
  - Left/Right: Lateral thrusters
- **F**: Fire weapons
- **S**: Toggle shields
- **C**: View cargo status
- **Q**: Quit game

## Game Mechanics

### Movement
- Physics-based movement with inertia
- Velocity-based ship control
- Boundary collision with bounce effects
- Fuel consumption based on thrust

### Ship Systems
- **Hull**: Base integrity of 100 per level
- **Shields**: 
  - 50 capacity per level
  - Auto-recharge at 0.5 per level per second
  - Can be toggled on/off
- **Engine**: 
  - 20 speed units per level
  - Affects maneuverability
- **Fuel System**:
  - 200 fuel units per level
  - 10% better efficiency per level
- **Cargo System**: 100 cargo units per level
- **Weapon System**:
  - 25 damage per level
  - Accuracy improves with level (caps at 95%)
  - Critical hit chance increases with level

## Development

### Branch Structure
- `main`: Production-ready code
- `staging`: Integration and testing
- `feature/*`: Feature development branches

### Contributing
1. Create a feature branch:
```bash
git checkout staging
git checkout -b feature/your-feature-name
```

2. Make your changes and commit:
```bash
git add .
git commit -m "Description of changes"
```

3. Push changes and create a pull request to staging

## License

[Your chosen license]