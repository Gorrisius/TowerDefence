# Graph Report - game-4  (2026-09-26)

## Corpus Check
- 23 files · ~513,538 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 127 nodes · 138 edges · 12 communities (11 shown, 1 thin omitted)
- Extraction: 97% EXTRACTED · 3% INFERRED · 0% AMBIGUOUS · INFERRED: 4 edges (avg confidence: 0.85)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- Original starter artwork prompts
- Graphify knowledge graph pipeline
- Урок: картки та розміщення двох веж
- Урок: перша кулеметна вежа
- EnemySpawner
- game-4
- 🎮 System Context & AI Assistant Directives
- Enemy
- Готовий розв'язок: картки та розміщення веж
- Godot Logo
- Готовий розв’язок: перша бойова вежа

## God Nodes (most connected - your core abstractions)
1. `Урок: картки та розміщення двох веж` - 18 edges
2. `EnemySpawner` - 14 edges
3. `Урок: перша кулеметна вежа` - 9 edges
4. `Graphify knowledge graph pipeline` - 9 edges
5. `Enemy` - 8 edges
6. `Original starter artwork prompts` - 8 edges
7. `GridNavigation` - 7 edges
8. `Metro defense starter artwork` - 7 edges
9. `Metro Tile Atlas (4×4)` - 5 edges
10. `Готовий розв'язок: картки та розміщення веж` - 4 edges

## Surprising Connections (you probably didn't know these)
- `Metro atlas: 16 environment tiles` --references--> `Metro Tile Atlas (4×4)`  [EXTRACTED]
  ASSETS.md → tiles/metro_atlas.png
- `Breaker: barricade attacker` --references--> `Breaker Sprite`  [EXTRACTED]
  ASSETS.md → enemies/breaker.png
- `Runner: open-route enemy` --references--> `Runner Sprite`  [EXTRACTED]
  ASSETS.md → enemies/runner.png
- `Single-target machine-gun tower` --references--> `Machine Gun Tower Sprite`  [EXTRACTED]
  ASSETS.md → towers/machine_gun.png
- `Electrical Tesla tower` --references--> `Tesla Tower Sprite`  [EXTRACTED]
  ASSETS.md → towers/tesla.png

## Import Cycles
- None detected.

## Communities (12 total, 1 thin omitted)

### Community 0 - "Original starter artwork prompts"
Cohesion: 0.09
Nodes (27): Original starter artwork prompts, Godot 2D PC tower defense, Post-apocalyptic underground metro style, Atlas slicing and normalization required, Breaker: barricade attacker, Single-target machine-gun tower, Metro atlas: 16 environment tiles, Runner: open-route enemy (+19 more)

### Community 1 - "Graphify knowledge graph pipeline"
Cohesion: 0.21
Nodes (13): URL ingestion and file watcher, Graph exports and benchmark, Extraction confidence and provenance, Repository cloning and graph merging, Post-commit graph refresh, Vocabulary-constrained query expansion, Graph query path and explain, Whisper media transcription (+5 more)

### Community 2 - "Урок: картки та розміщення двох веж"
Cohesion: 0.09
Nodes (21): 10. Перевірка місця і попередній перегляд, 11. Встановлення — одна завершена операція, 12. Додай платформи до навігації, 13. Признач ресурси та запусти, 14. Перевірки, які потрібно пройти самому, 15. Наступні уроки та зв'язок із дипломом, 16. Графіка: джерело та промпти, 1. Що саме робимо зараз (+13 more)

### Community 3 - "Урок: перша кулеметна вежа"
Cohesion: 0.20
Nodes (9): 1. Результат і межі уроку, 2. Як пов’язані вузли й код, 3. Додай здоров’я самостійно, 4. Створи сцену вежі, 5. Напиши алгоритм вежі, 6. Підключи й запусти, 7. Перевір поведінку, 8. Питання для самоперевірки (+1 more)

### Community 4 - "EnemySpawner"
Cohesion: 0.11
Nodes (17): AStarGrid2D, List, Node2D, Vector2, EnemySpawner, EnemyScene, SpawnCount, SpawnInterval (+9 more)

### Community 5 - "game-4"
Cohesion: 0.50
Nodes (4): net8.0, net9.0, game-4, Godot.NET.Sdk/4.7.0

### Community 6 - "🎮 System Context & AI Assistant Directives"
Cohesion: 0.25
Nodes (7): 👤 AI Role, Core Mechanics, File 1: `Instructions_and_Logic.md`, File 2: `Final_Solution.cs` (or relevant extension), 📜 Game Design Document (GDD Summary), 🛠️ Strict Pedagogical Protocol (The "Two-File" Rule), 🎮 System Context & AI Assistant Directives

### Community 8 - "Enemy"
Cohesion: 0.39
Nodes (3): List, Vector2, Enemy

### Community 9 - "Готовий розв'язок: картки та розміщення веж"
Cohesion: 0.40
Nodes (4): 1. res://levels/TowerPlacement.cs, 2. res://levels/GridNavigation.cs, Готовий розв'язок: картки та розміщення веж, Налаштування для першого запуску

### Community 11 - "Готовий розв’язок: перша бойова вежа"
Cohesion: 0.50
Nodes (3): res://enemies/Enemy.cs, res://towers/MachineGunTower.cs, Готовий розв’язок: перша бойова вежа

## Knowledge Gaps
- **57 isolated node(s):** `net8.0`, `net9.0`, `Godot.NET.Sdk/4.7.0`, `EnemyScene`, `SpawnCount` (+52 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **1 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `EnemySpawner` connect `EnemySpawner` to `Enemy`?**
  _High betweenness centrality (0.031) - this node is a cross-community bridge._
- **What connects `net8.0`, `net9.0`, `Godot.NET.Sdk/4.7.0` to the rest of the system?**
  _57 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Original starter artwork prompts` be split into smaller, more focused modules?**
  _Cohesion score 0.09116809116809117 - nodes in this community are weakly interconnected._
- **Should `Урок: картки та розміщення двох веж` be split into smaller, more focused modules?**
  _Cohesion score 0.09090909090909091 - nodes in this community are weakly interconnected._
- **Should `EnemySpawner` be split into smaller, more focused modules?**
  _Cohesion score 0.1067193675889328 - nodes in this community are weakly interconnected._