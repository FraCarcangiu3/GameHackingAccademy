# 🎯 Guida Completa EntityManager - Sistema di Memory Hacking

## 📋 Indice
1. [Panoramica del Sistema](#panoramica-del-sistema)
2. [Come Funziona il Memory Hacking](#come-funziona-il-memory-hacking)
3. [Struttura dei Progetti](#struttura-dei-progetti)
4. [Guida all'Utilizzo](#guida-allutilizzo)
5. [Offset e Come Trovarli](#offset-e-come-trovarli)
6. [Troubleshooting](#troubleshooting)
7. [Esempi Pratici](#esempi-pratici)

---

## 🎯 Panoramica del Sistema

### **Cosa Sono gli EntityManager?**
Gli EntityManager sono sistemi per **leggere la memoria dei giochi** e ottenere informazioni in tempo reale su:
- 👤 **Giocatori** (salute, posizione, squadra)
- 🎯 **Entità** (nemici, alleati, oggetti)
- 📊 **Dati del gioco** (stato, statistiche)

### **Perché Usarli?**
- 🎮 **Cheat/Hack** - Per creare funzionalità avanzate
- 📈 **Analisi** - Per studiare il comportamento del gioco
- 🔬 **Ricerca** - Per capire come funzionano i giochi
- 🛠️ **Sviluppo** - Per creare tool e utility

---

## 🔧 Come Funziona il Memory Hacking

### **1. Connessione al Processo**
```csharp
Swed swed = new Swed("nome_processo"); // Connette al gioco
IntPtr moduleBase = swed.GetModuleBase(".exe"); // Ottiene l'indirizzo base
```

### **2. Lettura della Memoria**
```csharp
int health = swed.ReadInt(indirizzo, offset); // Legge un valore intero
IntPtr pointer = swed.ReadPointer(indirizzo, offset); // Legge un puntatore
```

### **3. Offset di Memoria**
Gli **offset** sono "indirizzi" che indicano dove si trovano i dati:
- `0x17E0A8` = Indirizzo del giocatore locale
- `0x18AC04` = Lista delle entità
- `0xEC` = Offset della salute (relativo all'entità)

---

## 📁 Struttura dei Progetti

### **EntityManager (Base)**
```
EntityManager/
├── Program.cs                    # Entry point semplice
├── Entity Structures/
│   └── Entity.cs                # Struttura dati base
├── Entity Handling/
│   ├── EntityManager.cs         # Classe base
│   └── ACEntityManager.cs       # Implementazione Assault Cube
├── Game Offsets/
│   └── Offsets.cs               # Offset hardcoded
└── README.md                    # Documentazione
```

### **AdvancedEntityManager (Avanzato)**
```
AdvancedEntityManager/
├── Program.cs                    # Entry point avanzato
├── Entity Structures/
│   └── Entity.cs                # Struttura dati avanzata
├── Entity Handling/
│   ├── AdvancedEntityManager.cs # Classe base avanzata
│   └── ACAdvancedEntityManager.cs # Implementazione AC avanzata
├── Offset Scanner/
│   ├── DynamicOffsetScanner.cs  # Scanner con offset noti
│   └── UniversalOffsetScanner.cs # Scanner universale
└── README.md                    # Documentazione avanzata
```

---

## 🚀 Guida all'Utilizzo

### **Metodo 1: Sistema Base (Raccomandato per Principianti)**

#### **Passo 1: Avvia il Gioco**
1. Apri Assault Cube
2. Entra in una partita (singleplayer o multiplayer)
3. Mantieni il gioco in esecuzione

#### **Passo 2: Esegui EntityManager**
```bash
cd EntityManager/EntityManager
dotnet run
```

#### **Passo 3: Risultato Atteso**
```
Base: 13E11618 Health: 100
Base: 19836A38 Health: 100
Base: 13DF09A0 Health: 100
```

### **Metodo 2: Sistema Avanzato (Per Utenti Esperti)**

#### **Passo 1: Avvia il Gioco**
1. Apri Assault Cube
2. Entra in una partita
3. Mantieni il gioco in esecuzione

#### **Passo 2: Esegui AdvancedEntityManager**
```bash
cd AdvancedEntityManager/AdvancedEntityManager
dotnet run
```

#### **Passo 3: Risultato Atteso**
```
🚀 AdvancedEntityManager - Sistema Avanzato con Offset Dinamici
============================================================

✅ Connesso al gioco Assault Cube!
📍 Indirizzo base del modulo: 400000

🔍 Avvio scansione dinamica degli offset...
✅ LocalPlayer trovato: 0x17E0A8
✅ EntityList trovato: 0x18AC04
✅ Health trovato: 0xEC
✅ Team trovato: 0xF0
✅ Position trovato: 0x34
✅ Shooting trovato: 0xF0

📊 Riepilogo Entità:
   Totale: 9
   Vive: 3
   Che stanno sparando: 0
   Squadra 0: 9
   Squadra 1: 0
```

---

## 🔍 Offset e Come Trovarli

### **Cosa Sono gli Offset?**
Gli offset sono "coordinate" nella memoria del gioco che indicano dove si trovano i dati:
- **Indirizzi Assoluti**: `0x17E0A8` (posizione fissa in memoria)
- **Offset Relativi**: `0xEC` (distanza da un indirizzo base)

### **Come Trovarli con Cheat Engine**

#### **Passo 1: Trova la Salute**
1. Apri Cheat Engine
2. Seleziona il processo del gioco
3. Cerca il valore della tua salute (es. 100)
4. Cambia la salute nel gioco
5. Cerca il nuovo valore
6. Ripeti fino a trovare l'indirizzo

#### **Passo 2: Trova il Giocatore Locale**
1. Clicca destro sull'indirizzo della salute
2. "Pointer scan" o "Find what accesses this address"
3. Trova l'indirizzo base del giocatore

#### **Passo 3: Trova la Lista delle Entità**
1. Cerca pattern di puntatori
2. Trova array di indirizzi simili
3. Verifica che puntino a entità valide

### **Offset Comuni per Assault Cube**
```csharp
// Indirizzi assoluti (relativi al modulo base)
public static int localPlayer = 0x0017E0A8;  // Giocatore locale
public static int entityList = 0x0018AC04;   // Lista entità

// Offset relativi (relativi all'entità)
public static int health = 0xEC;             // Salute
public static int team = 0xF0;               // Squadra
public static int position = 0x34;           // Posizione X
public static int shooting = 0xF0;            // Sta sparando
```

---

## 🛠️ Troubleshooting

### **Problema: "Processo non trovato"**
```
❌ Errore: Impossibile connettersi al gioco Assault Cube!
```
**Soluzione:**
1. Verifica che il gioco sia in esecuzione
2. Controlla il nome del processo (`ac_client`)
3. Esegui come amministratore

### **Problema: "Accesso negato alla memoria"**
```
❌ Errore: Access denied to memory
```
**Soluzione:**
1. Esegui il programma come amministratore
2. Verifica che il gioco sia in esecuzione
3. Controlla che l'architettura sia corretta (x86)

### **Problema: "Dati non plausibili"**
```
⚠️ ATTENZIONE: Salute del giocatore locale non plausibile!
```
**Soluzione:**
1. Verifica che il gioco sia in una partita attiva
2. Controlla che gli offset siano corretti
3. Riavvia il gioco e riprova

### **Problema: "Offset non funzionanti"**
```
❌ LocalPlayer non valido
```
**Soluzione:**
1. Gli offset cambiano con gli aggiornamenti del gioco
2. Usa Cheat Engine per trovare nuovi offset
3. Aggiorna il file `Offsets.cs`

---

## 💡 Esempi Pratici

### **Esempio 1: Visualizzazione Salute**
```csharp
// Leggi la salute del giocatore locale
IntPtr localPlayer = swed.ReadPointer(moduleBase, Offsets.localPlayer);
int health = swed.ReadInt(localPlayer, Offsets.health);
Console.WriteLine($"Salute: {health}");
```

### **Esempio 2: Lista Entità**
```csharp
// Leggi tutte le entità
for (int i = 0; i < 10; i++)
{
    IntPtr entity = swed.ReadPointer(moduleBase, Offsets.entityList, i * 0x4);
    if (entity != IntPtr.Zero)
    {
        int health = swed.ReadInt(entity, Offsets.health);
        Console.WriteLine($"Entità {i}: Salute = {health}");
    }
}
```

### **Esempio 3: Sistema di Radar**
```csharp
// Trova nemici vicini
foreach (var entity in entities)
{
    if (entity.team != localPlayer.team && entity.health > 0)
    {
        float distance = CalculateDistance(localPlayer, entity);
        Console.WriteLine($"Nemico a {distance:F2} metri");
    }
}
```

### **Esempio 4: HitsoundHack**
```csharp
// Riproduci suono quando qualcuno spara
foreach (var entity in entities)
{
    if (entity.shooting == 1)
    {
        PlayHitsound();
        Console.WriteLine("🔊 Qualcuno sta sparando!");
    }
}
```

---

## 🎯 Scelta del Sistema

### **Usa EntityManager Base Se:**
- ✅ Sei un principiante
- ✅ Vuoi semplicità
- ✅ Hai già gli offset corretti
- ✅ Vuoi massima velocità

### **Usa AdvancedEntityManager Se:**
- ✅ Vuoi funzionalità avanzate
- ✅ Vuoi scansione automatica
- ✅ Vuoi analisi comportamentale
- ✅ Vuoi rilevamento anomalie

### **Usa UniversalOffsetScanner Se:**
- ✅ Stai lavorando con un nuovo gioco
- ✅ Non conosci gli offset
- ✅ Vuoi massima compatibilità
- ✅ Vuoi un sistema completamente automatico

---

## 📚 Risorse Utili

### **Tool Essenziali:**
- **Cheat Engine** - Per trovare offset
- **Process Hacker** - Per analizzare processi
- **x64dbg** - Per debugging avanzato
- **Visual Studio** - Per sviluppo

### **Librerie:**
- **Swed32** - Per memory hacking in C#
- **System.Numerics** - Per calcoli vettoriali
- **NAudio** - Per riproduzione audio

### **Documentazione:**
- **README.md** - Guida base
- **README_it.md** - Guida in italiano
- **Codice sorgente** - Commenti dettagliati

---

## 🎯 Conclusione

Questo sistema ti permette di:
1. **Leggere la memoria** di qualsiasi gioco
2. **Trovare automaticamente** gli offset
3. **Analizzare le entità** in tempo reale
4. **Creare funzionalità avanzate** come radar, hitsound, ecc.

**Ricorda:** Usa sempre responsabilmente e solo su giochi di tua proprietà! 🚀

---

*Guida creata per il progetto GameHackingAccademy - Sistema EntityManager*
