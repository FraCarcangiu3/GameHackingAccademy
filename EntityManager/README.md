# EntityManager - Game Entity Management System

## 📋 Table of Contents
1. [What is EntityManager](#what-is-entitymanager)
2. [How It Works](#how-it-works)
3. [Project Structure](#project-structure)
4. [Installation and Setup](#installation-and-setup)
5. [Usage Guide](#usage-guide)
6. [Implementation for Other Games](#implementation-for-other-games)
7. [Practical Examples](#practical-examples)
8. [Troubleshooting](#troubleshooting)

## 🎯 What is EntityManager

**EntityManager** is a C# framework designed to manage entities (players, objects, enemies) in any game. The project uses **memory hacking** techniques to read directly from the game's memory and provide real-time information about entities.

### Key Features:
- ✅ **Generic**: Works with any game
- ✅ **Modular**: Easy to extend for new games
- ✅ **Real-time**: Continuous data updates
- ✅ **Flexible**: Configurable offset system
- ✅ **Safe**: Built-in error handling

## 🔧 How It Works

### 1. **Memory Reading**
The system uses the `Swed32` library to:
- Connect to the game process
- Read game memory in real-time
- Access specific addresses where entity data is stored

### 2. **Offset System**
**Offsets** are memory addresses that indicate where specific data is located:
- `localPlayer`: Local player's position in memory
- `entityList`: List of all game entities
- `health`: Entity's health position

### 3. **Layered Architecture**
```
Program.cs (Entry Point)
    ↓
ACEntityManager (Assault Cube specific implementation)
    ↓
EntityManager (Abstract base class)
    ↓
Entity (Data structure for entities)
```

## 📁 Project Structure

```
EntityManager/
├── EntityManager/
│   ├── Program.cs                    # Application entry point
│   ├── Entity Structures/
│   │   └── Entity.cs                # Data structure for entities
│   ├── Entity Handling/
│   │   ├── EntityManager.cs         # Abstract base class
│   │   └── ACEntityManager.cs       # Assault Cube implementation
│   ├── Game Offsets/
│   │   └── Offsets.cs               # Memory offsets for Assault Cube
│   └── EntityManager.csproj         # Project file
└── EntityManager.slnx               # Solution file
```

### File Explanations:

#### **Entity.cs** - Data Structure
```csharp
public class Entity
{
    public IntPtr baseAddress { get; set; }    // Entity's base address in memory
    public int health { get; set; }            // Entity's health
    public string name { get; set; }           // Player name
    public int team { get; set; }              // Team (0 = enemies, 1 = allies)
    public Vector3 originPosition3d { get; set; } // 3D position in world
    // ... other fields
}
```

#### **EntityManager.cs** - Base Class
```csharp
public abstract class EntityManager
{
    protected Entity localPlayer;              // Our player
    protected List<Entity> entities;           // List of all entities
    
    // Abstract methods to implement for each game
    public abstract void UpdateEntity(Entity entity);
    public abstract void UpdateLocalPlayer();
    public abstract void UpdateEntities();
    
    // Common methods for all games
    public List<Entity> GetEntitites() { return entities; }
    public void SortEntitiesByMagnitude() { /* ... */ }
}
```

#### **ACEntityManager.cs** - Assault Cube Implementation
```csharp
public class ACEntityManager : EntityManager
{
    private Swed swed;                         // Instance for reading memory
    private IntPtr mainModule;                 // Game's base address
    
    // Implements abstract methods for Assault Cube
    public override void UpdateEntity(Entity entity) { /* ... */ }
    public override void UpdateLocalPlayer() { /* ... */ }
    public override void UpdateEntities() { /* ... */ }
}
```

## 🚀 Installation and Setup

### Prerequisites:
- **Visual Studio 2022** or **Visual Studio Code**
- **.NET 8.0 SDK**
- **Assault Cube** installed (for the example)

### Step 1: Clone the Repository
```bash
git clone [REPOSITORY_URL]
cd EntityManager
```

### Step 2: Open the Project
- Open `EntityManager.slnx` in Visual Studio
- Or use VS Code with C# extension

### Step 3: Install Dependencies
Dependencies are already configured in the `.csproj` file:
```xml
<PackageReference Include="swed32" Version="1.1.0" />
```

### Step 4: Build the Project
```bash
dotnet build
```

## 📖 Usage Guide

### Basic Usage

```csharp
// 1. Create a Swed instance to connect to the game
Swed swed = new Swed("ac_client"); // Game process name

// 2. Get the base address of the main module
IntPtr moduleBase = swed.GetModuleBase(".exe");

// 3. Create the entity manager
ACEntityManager entityManager = new ACEntityManager(swed, moduleBase);

// 4. Update entities
entityManager.UpdateEntities();
entityManager.UpdateLocalPlayer();

// 5. Get and use the data
foreach(Entity entity in entityManager.GetEntitites())
{
    Console.WriteLine($"Health: {entity.health}");
    Console.WriteLine($"Position: {entity.originPosition3d}");
}
```

### Advanced Features

#### Entity Sorting
```csharp
// Sort by speed
entityManager.SortEntitiesByMagnitude();

// Sort by distance from crosshair
entityManager.SortEntitiesByFov();
```

#### Distance Calculation
```csharp
Entity player1 = entityManager.GetLocalPlayer();
Entity player2 = entityManager.GetEntitites()[0];

float distance = entityManager.CalculateEntityDistances(player1, player2);
Console.WriteLine($"Distance: {distance}");
```

## 🎮 Implementation for Other Games

### Step 1: Find Offsets
Use tools like:
- **Cheat Engine** to find memory addresses
- **x64dbg** for debugging
- **Process Hacker** to analyze memory

### Step 2: Create a New Manager Class
```csharp
public class [GameName]EntityManager : EntityManager
{
    private Swed swed;
    private IntPtr mainModule;
    
    public [GameName]EntityManager(Swed swedInstance, IntPtr mainModule)
    {
        this.swed = swedInstance;
        this.mainModule = mainModule;
    }
    
    public override void UpdateEntity(Entity entity)
    {
        // Read game-specific data
        entity.health = swed.ReadInt(entity.baseAddress, Offsets.health);
        entity.name = swed.ReadString(entity.baseAddress, Offsets.name);
        // ... other fields
    }
    
    public override void UpdateLocalPlayer()
    {
        // Update local player
        localPlayer.baseAddress = swed.ReadPointer(mainModule, Offsets.localPlayer);
        UpdateEntity(localPlayer);
    }
    
    public override void UpdateEntities()
    {
        entities.Clear();
        
        // Loop through game entities
        for (int i = 0; i < maxEntities; i++)
        {
            IntPtr entityAddress = swed.ReadPointer(mainModule, Offsets.entityList, i * entitySize);
            
            if (entityAddress == IntPtr.Zero)
                continue;
                
            Entity entity = new Entity();
            entity.baseAddress = entityAddress;
            UpdateEntity(entity);
            entities.Add(entity);
        }
    }
}
```

### Step 3: Define Offsets
```csharp
public static class Offsets
{
    // Offsets specific to the new game
    public static int localPlayer = 0x[ADDRESS];
    public static int entityList = 0x[ADDRESS];
    public static int health = 0x[ADDRESS];
    public static int name = 0x[ADDRESS];
    // ... other offsets
}
```

## 💡 Practical Examples

### Example 1: Health Display
```csharp
ACEntityManager manager = new ACEntityManager(swed, moduleBase);
manager.UpdateEntities();

foreach(Entity entity in manager.GetEntitites())
{
    if (entity.health > 0) // Only living entities
    {
        Console.WriteLine($"Player: {entity.name} - Health: {entity.health}");
    }
}
```

### Example 2: Radar System
```csharp
Entity localPlayer = manager.GetLocalPlayer();
manager.UpdateEntities();

foreach(Entity entity in manager.GetEntitites())
{
    if (entity.health > 0 && entity.team != localPlayer.team)
    {
        float distance = manager.CalculateEntityDistances(localPlayer, entity);
        Console.WriteLine($"Enemy at {distance:F2} meters distance");
    }
}
```

### Example 3: Aim Assist System
```csharp
// Sort entities by distance from crosshair
manager.SortEntitiesByFov();

Entity closestEnemy = manager.GetEntitites()
    .Where(e => e.health > 0 && e.team != manager.GetLocalPlayer().team)
    .FirstOrDefault();

if (closestEnemy != null)
{
    // Calculate angle to aim at enemy
    Vector3 enemyPos = closestEnemy.originPosition3d;
    Vector3 playerPos = manager.GetLocalPlayer().originPosition3d;
    
    // Logic to calculate aim angle
    Console.WriteLine($"Aim at enemy: {closestEnemy.name}");
}
```

## 🔧 Troubleshooting

### Common Issues:

#### 1. **Error: "Process not found"**
```
Solution: Make sure the game is running and the process name is correct.
```

#### 2. **Error: "Access denied to memory"**
```
Solution: Run the application as administrator.
```

#### 3. **Data not updating**
```
Solution: Call UpdateEntities() and UpdateLocalPlayer() before reading data.
```

#### 4. **Offsets not working**
```
Solution: Offsets change with game updates. Use Cheat Engine to find new offsets.
```

### Debug Tips:

1. **Verify Connection**:
```csharp
if (swed.IsValid())
    Console.WriteLine("Connected to game!");
else
    Console.WriteLine("Connection error!");
```

2. **Check Addresses**:
```csharp
Console.WriteLine($"Base address: {moduleBase.ToString("X")}");
Console.WriteLine($"Entities found: {entities.Count}");
```

3. **Test Offsets**:
```csharp
int testHealth = swed.ReadInt(entity.baseAddress, Offsets.health);
Console.WriteLine($"Health read: {testHealth}");
```

## 📚 Useful Resources

- **Swed32 Documentation**: [Documentation link]
- **Memory Hacking Guide**: [Guide link]
- **Assault Cube Offsets**: [Updated offsets link]
- **Cheat Engine Tutorial**: [Tutorial link]

## ⚠️ Disclaimer

This project is intended exclusively for educational and research purposes. The use of memory hacking techniques may violate game terms of service and may result in account bans. Use this code responsibly and only on games you own or in test environments.

## 🤝 Contributing

If you want to contribute to the project:
1. Fork the repository
2. Create a branch for your feature
3. Implement the changes
4. Create a pull request

## 📄 License

This project is released under MIT license. See the LICENSE file for more details.

---

**Happy coding! 🚀**

If you have questions or issues, don't hesitate to create an issue in the repository.
