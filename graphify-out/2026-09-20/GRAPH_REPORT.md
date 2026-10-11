# Graph Report - game-4  (2026-09-19)

## Corpus Check
- 18 files · ~357,381 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 65 nodes · 71 edges · 9 communities (7 shown, 2 thin omitted)
- Extraction: 97% EXTRACTED · 3% INFERRED · 0% AMBIGUOUS · INFERRED: 2 edges (avg confidence: 0.9)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- Metro Tile Atlas (4×4)
- Graphify knowledge graph pipeline
- GridNavigation
- Tesla Tower Sprite
- EnemySpawner
- game-4
- Original starter artwork prompts
- Godot Logo

## God Nodes (most connected - your core abstractions)
1. `Graphify knowledge graph pipeline` - 9 edges
2. `Original starter artwork prompts` - 8 edges
3. `Metro defense starter artwork` - 7 edges
4. `GridNavigation` - 6 edges
5. `EnemySpawner` - 5 edges
6. `Metro Tile Atlas (4×4)` - 5 edges
7. `Runner` - 4 edges
8. `Metro atlas: 16 environment tiles` - 4 edges
9. `Tesla Tower Sprite` - 4 edges
10. `game-4` - 3 edges

## Surprising Connections (you probably didn't know these)
- `Metro atlas: 16 environment tiles` --references--> `Metro Tile Atlas (4×4)`  [EXTRACTED]
  ASSETS.md → tiles/metro_atlas.png
- `Electrical Tesla tower` --references--> `Tesla Tower Sprite`  [EXTRACTED]
  ASSETS.md → towers/tesla.png
- `Single-target machine-gun tower` --references--> `Machine Gun Tower Sprite`  [EXTRACTED]
  ASSETS.md → towers/machine_gun.png
- `Breaker: barricade attacker` --references--> `Breaker Sprite`  [EXTRACTED]
  ASSETS.md → enemies/breaker.png
- `Runner: open-route enemy` --references--> `Runner Sprite`  [EXTRACTED]
  ASSETS.md → enemies/runner.png

## Import Cycles
- None detected.

## Communities (9 total, 2 thin omitted)

### Community 0 - "Metro Tile Atlas (4×4)"
Cohesion: 0.40
Nodes (5): Weathered Concrete Floor Variants, Grates, Metal Plate, Hatch and Hazard Stripes, Metro Tile Atlas (4×4), Horizontal and Vertical Rail Segments, Concrete Wall Segments and Corner

### Community 1 - "Graphify knowledge graph pipeline"
Cohesion: 0.21
Nodes (13): URL ingestion and file watcher, Graph exports and benchmark, Extraction confidence and provenance, Repository cloning and graph merging, Post-commit graph refresh, Vocabulary-constrained query expansion, Graph query path and explain, Whisper media transcription (+5 more)

### Community 2 - "GridNavigation"
Cohesion: 0.17
Nodes (7): AStarGrid2D, Runner, GridNavigation, List, Node2D, TileMapLayer, Vector2

### Community 3 - "Tesla Tower Sprite"
Cohesion: 0.22
Nodes (9): Single-target machine-gun tower, Electrical Tesla tower, Long Barrel and Ammunition Belt, Weathered Circular Turret Mount, Machine Gun Tower Sprite, Weathered Circular Electrical Tower Mount, Control Box with Cyan Indicator, Copper Coils and Spherical Terminal (+1 more)

### Community 5 - "game-4"
Cohesion: 0.50
Nodes (4): net8.0, net9.0, game-4, Godot.NET.Sdk/4.7.0

### Community 6 - "Original starter artwork prompts"
Cohesion: 0.19
Nodes (13): Original starter artwork prompts, Godot 2D PC tower defense, Post-apocalyptic underground metro style, Atlas slicing and normalization required, Breaker: barricade attacker, Metro atlas: 16 environment tiles, Runner: open-route enemy, Metro defense starter artwork (+5 more)

## Knowledge Gaps
- **19 isolated node(s):** `net8.0`, `net9.0`, `Godot.NET.Sdk/4.7.0`, `Weathered Concrete Floor Variants`, `Grates, Metal Plate, Hatch and Hazard Stripes` (+14 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **2 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Original starter artwork prompts` connect `Original starter artwork prompts` to `Tesla Tower Sprite`?**
  _High betweenness centrality (0.067) - this node is a cross-community bridge._
- **Why does `Metro atlas: 16 environment tiles` connect `Original starter artwork prompts` to `Metro Tile Atlas (4×4)`?**
  _High betweenness centrality (0.062) - this node is a cross-community bridge._
- **Why does `Metro defense starter artwork` connect `Original starter artwork prompts` to `Tesla Tower Sprite`?**
  _High betweenness centrality (0.056) - this node is a cross-community bridge._
- **What connects `net8.0`, `net9.0`, `Godot.NET.Sdk/4.7.0` to the rest of the system?**
  _19 weakly-connected nodes found - possible documentation gaps or missing edges._