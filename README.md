# Conway's Game of Life – Teaser Simulation  
**By Brennan Waters**

This is a modern recreation of **Conway's Game of Life**, a zero-player simulation based on cellular automata principles. This version was built as a visual teaser project with added analytics and automatic stagnation detection.

---

## 🔷 Rules of the Game

Conway's Game of Life follows four simple rules applied to each cell on a grid:

1. A live cell with fewer than 2 neighbors **dies** (underpopulation).
2. A live cell with 2 or 3 neighbors **lives** (survival).
3. A live cell with more than 3 neighbors **dies** (overpopulation).
4. A dead cell with exactly 3 neighbors **becomes alive** (reproduction).

---

## 🎮 Controls

- `SPACEBAR` – Pause or resume the simulation
- `ESC` – Quit the application
- The simulation will **automatically end** when stagnation is detected (i.e. no new births or deaths, or an oscillating loop is reached)

---

## 📈 Features

- Visual display of live cells in real-time
- Per-generation stats (Alive, Births, Deaths)
- Auto-detection of stagnation (oscillation or static)
- Full-screen overlay on stagnation with generation number
- Historical tracking of generation data
- Live dashboard of highest/lowest birth and death rates

---

## 🧠 Built With

- Visual Studio 2022
- C#
- .NET Windows Forms

---

Enjoy experimenting with life itself.  
*– Brennan Waters*
