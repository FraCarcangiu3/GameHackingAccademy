# EntityManager - Sistema di Gestione Entità per Giochi

## 📋 Indice
1. [Cos'è EntityManager](#cosè-entitymanager)
2. [Come Funziona](#come-funziona)
3. [Struttura del Progetto](#struttura-del-progetto)
4. [Installazione e Configurazione](#installazione-e-configurazione)
5. [Guida all'Utilizzo](#guida-allutilizzo)
6. [Implementazione per Altri Giochi](#implementazione-per-altri-giochi)
7. [Esempi Pratici](#esempi-pratici)
8. [Troubleshooting](#troubleshooting)

## 🎯 Cos'è EntityManager

**EntityManager** è un framework C# progettato per gestire le entità (giocatori, oggetti, nemici) in qualsiasi gioco. Il progetto utilizza tecniche di **memory hacking** per leggere direttamente dalla memoria del gioco e fornire informazioni in tempo reale sulle entità.

### Caratteristiche Principali:
- ✅ **Generico**: Funziona con qualsiasi gioco
- ✅ **Modulare**: Facile da estendere per nuovi giochi
- ✅ **Real-time**: Aggiornamento continuo dei dati
- ✅ **Flessibile**: Sistema di offset configurabile
- ✅ **Sicuro**: Gestione degli errori integrata

## 🔧 Come Funziona

### 1. **Memory Reading**
Il sistema utilizza la libreria `Swed32` per:
- Connettere al processo del gioco
- Leggere la memoria del gioco in tempo reale
- Accedere a indirizzi specifici dove sono memorizzati i dati delle entità

### 2. **Sistema di Offset**
Gli **offset** sono indirizzi di memoria che indicano dove si trovano specifici dati:
- `localPlayer`: Posizione del giocatore locale in memoria
- `entityList`: Lista di tutte le entità del gioco
- `health`: Posizione della salute di un'entità

### 3. **Architettura a Livelli**
```
Program.cs (Entry Point)
    ↓
ACEntityManager (Implementazione specifica per Assault Cube)
    ↓
EntityManager (Classe base astratta)
    ↓
Entity (Struttura dati per le entità)
```

## 📁 Struttura del Progetto

```
EntityManager/
├── EntityManager/
│   ├── Program.cs                    # Punto di ingresso dell'applicazione
│   ├── Entity Structures/
│   │   └── Entity.cs                # Struttura dati per le entità
│   ├── Entity Handling/
│   │   ├── EntityManager.cs         # Classe base astratta
│   │   └── ACEntityManager.cs       # Implementazione per Assault Cube
│   ├── Game Offsets/
│   │   └── Offsets.cs               # Offset di memoria per Assault Cube
│   └── EntityManager.csproj         # File di progetto
└── EntityManager.slnx               # Solution file
```

### Spiegazione dei File:

#### **Entity.cs** - Struttura Dati
```csharp
public class Entity
{
    public IntPtr baseAddress { get; set; }    // Indirizzo base dell'entità in memoria
    public int health { get; set; }            // Salute dell'entità
    public string name { get; set; }           // Nome del giocatore
    public int team { get; set; }              // Squadra (0 = nemici, 1 = alleati)
    public Vector3 originPosition3d { get; set; } // Posizione 3D nel mondo
    // ... altri campi
}
```

#### **EntityManager.cs** - Classe Base
```csharp
public abstract class EntityManager
{
    protected Entity localPlayer;              // Il nostro giocatore
    protected List<Entity> entities;           // Lista di tutte le entità
    
    // Metodi astratti da implementare per ogni gioco
    public abstract void UpdateEntity(Entity entity);
    public abstract void UpdateLocalPlayer();
    public abstract void UpdateEntities();
    
    // Metodi comuni a tutti i giochi
    public List<Entity> GetEntitites() { return entities; }
    public void SortEntitiesByMagnitude() { /* ... */ }
}
```

#### **ACEntityManager.cs** - Implementazione Assault Cube
```csharp
public class ACEntityManager : EntityManager
{
    private Swed swed;                         // Istanza per leggere la memoria
    private IntPtr mainModule;                 // Indirizzo base del gioco
    
    // Implementa i metodi astratti per Assault Cube
    public override void UpdateEntity(Entity entity) { /* ... */ }
    public override void UpdateLocalPlayer() { /* ... */ }
    public override void UpdateEntities() { /* ... */ }
}
```

## 🚀 Installazione e Configurazione

### Prerequisiti:
- **Visual Studio 2022** o **Visual Studio Code**
- **.NET 8.0 SDK**
- **Assault Cube** installato (per l'esempio)

### Passo 1: Clona il Repository
```bash
git clone [URL_DEL_REPOSITORY]
cd EntityManager
```

### Passo 2: Apri il Progetto
- Apri `EntityManager.slnx` in Visual Studio
- Oppure usa VS Code con l'estensione C#

### Passo 3: Installa le Dipendenze
Le dipendenze sono già configurate nel file `.csproj`:
```xml
<PackageReference Include="swed32" Version="1.1.0" />
```

### Passo 4: Compila il Progetto
```bash
dotnet build
```

## 📖 Guida all'Utilizzo

### Utilizzo Base

```csharp
// 1. Crea un'istanza di Swed per connettersi al gioco
Swed swed = new Swed("ac_client"); // Nome del processo del gioco

// 2. Ottieni l'indirizzo base del modulo principale
IntPtr moduleBase = swed.GetModuleBase(".exe");

// 3. Crea il manager delle entità
ACEntityManager entityManager = new ACEntityManager(swed, moduleBase);

// 4. Aggiorna le entità
entityManager.UpdateEntities();
entityManager.UpdateLocalPlayer();

// 5. Ottieni e usa i dati
foreach(Entity entity in entityManager.GetEntitites())
{
    Console.WriteLine($"Salute: {entity.health}");
    Console.WriteLine($"Posizione: {entity.originPosition3d}");
}
```

### Funzionalità Avanzate

#### Ordinamento delle Entità
```csharp
// Ordina per velocità
entityManager.SortEntitiesByMagnitude();

// Ordina per distanza dal crosshair
entityManager.SortEntitiesByFov();
```

#### Calcolo delle Distanze
```csharp
Entity player1 = entityManager.GetLocalPlayer();
Entity player2 = entityManager.GetEntitites()[0];

float distance = entityManager.CalculateEntityDistances(player1, player2);
Console.WriteLine($"Distanza: {distance}");
```

## 🎮 Implementazione per Altri Giochi

### Passo 1: Trova gli Offset
Usa strumenti come:
- **Cheat Engine** per trovare gli indirizzi di memoria
- **x64dbg** per il debugging
- **Process Hacker** per analizzare la memoria

### Passo 2: Crea una Nuova Classe Manager
```csharp
public class [NomeGioco]EntityManager : EntityManager
{
    private Swed swed;
    private IntPtr mainModule;
    
    public [NomeGioco]EntityManager(Swed swedInstance, IntPtr mainModule)
    {
        this.swed = swedInstance;
        this.mainModule = mainModule;
    }
    
    public override void UpdateEntity(Entity entity)
    {
        // Leggi i dati specifici del gioco
        entity.health = swed.ReadInt(entity.baseAddress, Offsets.health);
        entity.name = swed.ReadString(entity.baseAddress, Offsets.name);
        // ... altri campi
    }
    
    public override void UpdateLocalPlayer()
    {
        // Aggiorna il giocatore locale
        localPlayer.baseAddress = swed.ReadPointer(mainModule, Offsets.localPlayer);
        UpdateEntity(localPlayer);
    }
    
    public override void UpdateEntities()
    {
        entities.Clear();
        
        // Loop attraverso le entità del gioco
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

### Passo 3: Definisci gli Offset
```csharp
public static class Offsets
{
    // Offset specifici per il nuovo gioco
    public static int localPlayer = 0x[INDIRIZZO];
    public static int entityList = 0x[INDIRIZZO];
    public static int health = 0x[INDIRIZZO];
    public static int name = 0x[INDIRIZZO];
    // ... altri offset
}
```

## 💡 Esempi Pratici

### Esempio 1: Visualizzazione Salute
```csharp
ACEntityManager manager = new ACEntityManager(swed, moduleBase);
manager.UpdateEntities();

foreach(Entity entity in manager.GetEntitites())
{
    if (entity.health > 0) // Solo entità vive
    {
        Console.WriteLine($"Giocatore: {entity.name} - Salute: {entity.health}");
    }
}
```

### Esempio 2: Sistema di Radar
```csharp
Entity localPlayer = manager.GetLocalPlayer();
manager.UpdateEntities();

foreach(Entity entity in manager.GetEntitites())
{
    if (entity.health > 0 && entity.team != localPlayer.team)
    {
        float distance = manager.CalculateEntityDistances(localPlayer, entity);
        Console.WriteLine($"Nemico a {distance:F2} metri di distanza");
    }
}
```

### Esempio 3: Sistema di Aim Assist
```csharp
// Ordina le entità per distanza dal crosshair
manager.SortEntitiesByFov();

Entity closestEnemy = manager.GetEntitites()
    .Where(e => e.health > 0 && e.team != manager.GetLocalPlayer().team)
    .FirstOrDefault();

if (closestEnemy != null)
{
    // Calcola l'angolo per mirare al nemico
    Vector3 enemyPos = closestEnemy.originPosition3d;
    Vector3 playerPos = manager.GetLocalPlayer().originPosition3d;
    
    // Logica per calcolare l'angolo di mira
    Console.WriteLine($"Mira al nemico: {closestEnemy.name}");
}
```

## 🔧 Troubleshooting

### Problemi Comuni:

#### 1. **Errore: "Processo non trovato"**
```
Soluzione: Assicurati che il gioco sia in esecuzione e che il nome del processo sia corretto.
```

#### 2. **Errore: "Accesso negato alla memoria"**
```
Soluzione: Esegui l'applicazione come amministratore.
```

#### 3. **Dati non aggiornati**
```
Soluzione: Chiama UpdateEntities() e UpdateLocalPlayer() prima di leggere i dati.
```

#### 4. **Offset non funzionanti**
```
Soluzione: Gli offset cambiano con gli aggiornamenti del gioco. Usa Cheat Engine per trovare i nuovi offset.
```

### Debug Tips:

1. **Verifica la Connessione**:
```csharp
if (swed.IsValid())
    Console.WriteLine("Connesso al gioco!");
else
    Console.WriteLine("Errore di connessione!");
```

2. **Controlla gli Indirizzi**:
```csharp
Console.WriteLine($"Indirizzo base: {moduleBase.ToString("X")}");
Console.WriteLine($"Entità trovate: {entities.Count}");
```

3. **Testa gli Offset**:
```csharp
int testHealth = swed.ReadInt(entity.baseAddress, Offsets.health);
Console.WriteLine($"Salute letta: {testHealth}");
```

## 📚 Risorse Utili

- **Swed32 Documentation**: [Link alla documentazione]
- **Memory Hacking Guide**: [Link alla guida]
- **Assault Cube Offsets**: [Link agli offset aggiornati]
- **Cheat Engine Tutorial**: [Link al tutorial]

## ⚠️ Disclaimer

Questo progetto è destinato esclusivamente a scopi educativi e di ricerca. L'uso di tecniche di memory hacking può violare i termini di servizio dei giochi e può comportare il ban dell'account. Utilizza questo codice responsabilmente e solo su giochi di tua proprietà o in ambienti di test.

## 🤝 Contributi

Se vuoi contribuire al progetto:
1. Fai un fork del repository
2. Crea un branch per la tua feature
3. Implementa le modifiche
4. Crea una pull request

## 📄 Licenza

Questo progetto è rilasciato sotto licenza MIT. Vedi il file LICENSE per maggiori dettagli.

---

**Buon coding! 🚀**

Se hai domande o problemi, non esitare a creare una issue nel repository.
