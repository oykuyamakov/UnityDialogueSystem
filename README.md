# Unity Dialogue Graph Editor

A fully custom dialogue graph editor built using Unity's UIElements and GraphView framework.

This tool enables developers and narrative designers to create branching dialogue structures with conditional logic in a visual, node-based interface. It supports runtime serialization, recovery saves, and integration with gameplay logic through triggers and conditions.

---

## Features

- Visual node-based editor for:
  - NPC lines
  - Player choices
  - Start nodes
- Inline editing of dialogue content and branching logic
- Dialogue conditions and trigger actions per line
- Recovery system for unsaved work
- Save/load system using Unity Resources
- Clean and dependency-free (no external libraries or packages)

---

### Dialogue Graph Overview
![Dialogue Graph Overview](media/gif1.gif)

## How to Use

### 1. Open the Editor

Go to: Graph > Dialogue Graph


This opens the dialogue graph editor window.

---

### 2. Select an NPC

Use the `NpcName` enum dropdown in the toolbar to select or bind the dialogue to an NPC.

---

### 3. Define File Name

Add a short extension (e.g., `Intro`, `Scene1`) using the adjacent input field to name the dialogue file.

The final file will be saved under: Resources/Dialogues/NpcName/NpcName_Extension.asset


---

### 4. Create Nodes

Right-click anywhere in the graph view to add nodes:

- **Start Node** – Marks the entry point of the dialogue.
- **NPC Line Node** – For lines spoken by NPCs.
- **Player Line Node** – For player response options.

Connect nodes to define the branching structure of the conversation.

Each line node can contain:

- Multiple dialogue entries
- Conditional logic (e.g., item possession, location checks)
- Trigger actions (e.g., set a flag, unlock a new node)

### Inline Condition & Trigger Editing
![Inline Condition & Trigger Editing](media/gif2.gif)
---

### 5. Save and Load

- Click **Save** to serialize and store the dialogue graph.
- Click **Load** to open and edit a previously saved file.
- A temporary recovery file (`Dialogues/Recovery.asset`) is auto-saved on exit.

- 

### Save & Load System
![Save & Load System](media/gif3.gif)

---
