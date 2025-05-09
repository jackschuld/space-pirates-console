# SpacePirates.Console Project Structure

## Top-Level Folders

```
SpacePirates.Console/
│
├── Core/
├── Game/
├── Services/
├── UI/
├── Program.cs
```

---

## Folder Breakdown

### Core/
- **Purpose:** Contains all core game logic, domain models, and interfaces. No UI code.
- **Subfolders:**
  - **Models/**: Game state, movement, and other domain models.
    - **State/**: GameState, ConsoleConfig, etc.
    - **Movement/**: ShipTrail, MovementSystem, etc.
  - **Interfaces/**: Contracts for game state, engine, input, and rendering.

### Game/
- **Purpose:** Game orchestration and engine logic.
- **Files:**
  - **Engine/**: Contains `GameEngine.cs`, which manages the main game loop, state updates, and high-level orchestration.

### Services/
- **Purpose:** Infrastructure and external integrations.
- **Files:**
  - **ApiClient.cs**: Handles API communication.

### UI/
- **Purpose:** All user interface code, including rendering, input, controls, and views.
- **Subfolders:**
  - **Views/**: All UI screens and panels.
    - **GameView.cs**: The main container for the game UI.
    - **MapView.cs, GalaxyMapView.cs, SolarSystemMapView.cs**: Map-related views.
    - **StatusView.cs, ShipStatusView.cs, SolarSystemStatusView.cs, PlanetStatusView.cs**: Status panels.
    - **CommandLineView.cs**: Command input bar.
    - **InstructionsView.cs**: Instructions/help panel.
    - **MenuView.cs, StartMenuView.cs**: Menus.
    - **PanelView.cs, BaseView.cs**: Base classes for views and panels.
  - **Controls/**: Input handlers for each view (e.g., `GalaxyControls`, `SolarSystemControls`, `GameControls`, `MenuControls`, `BaseControls`).
  - **Styles/**: Style and color providers for UI elements (e.g., `PanelStyles`, `StatusPanelStyle`, `GalaxyColors`, etc.).
  - **ConsoleRenderer/**: Console rendering logic (e.g., `ConsoleRenderer`, `ConsoleBufferWriter`, `ConsoleBuffer`).
  - **InputHandling/**: Command system and input handlers (e.g., `CommandParser`, `CommandContext`, `MoveCommand`, etc.).
  - **Interfaces/**: UI-specific interfaces (e.g., `IView`, `IHasInstructions`).
  - **Utils/**: UI helpers (e.g., `StatusComponentHelpers`).

---

## File/Folder Responsibilities

- **Core/Models/**: All non-UI data structures and logic for the game world, player, movement, etc.
- **Core/Interfaces/**: Contracts for dependency inversion and testability.
- **Game/Engine/**: The main game loop and orchestration logic.
- **Services/**: API and external service integration.
- **UI/Views/**: All user-facing screens, panels, and their base classes.
- **UI/Controls/**: All input logic, mapped per view for OOP/SOLID compliance.
- **UI/Styles/**: All color, style, and theme logic for the UI.
- **UI/ConsoleRenderer/**: Low-level console rendering and buffer management.
- **UI/InputHandling/**: Command parsing and input system.
- **UI/Interfaces/**: UI-specific contracts.
- **UI/Utils/**: Small, reusable UI helpers.
