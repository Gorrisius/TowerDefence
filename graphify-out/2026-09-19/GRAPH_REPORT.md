# Graph Report - game-4  (2026-09-09)

## Corpus Check
- 19 files · ~248,358 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 42 nodes · 49 edges · 11 communities (9 shown, 2 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 2 edges (avg confidence: 0.9)
- Token cost: unavailable (host worker usage not exposed; see accounting note)

## Community Hubs (Navigation)
- Metro Map Tiles
- Graph Navigation and Updates
- Graphify Build Pipeline
- Electrical Tower Artwork
- Machine Gun Artwork
- Semantic Extraction Provenance
- Game Art Direction
- Barricade Breaker Enemy
- Tunnel Runner Enemy
- Starter Asset Inventory
- Godot Project Icon

## God Nodes (most connected - your core abstractions)
1. `Graphify knowledge graph pipeline` - 9 edges
2. `Original starter artwork prompts` - 8 edges
3. `Metro defense starter artwork` - 7 edges
4. `Metro Tile Atlas (4×4)` - 5 edges
5. `Metro atlas: 16 environment tiles` - 4 edges
6. `Tesla Tower Sprite` - 4 edges
7. `Semantic extraction and cache` - 3 edges
8. `Graph query path and explain` - 3 edges
9. `Incremental extraction and manifest` - 3 edges
10. `Runner: open-route enemy` - 3 edges

## Surprising Connections (you probably didn't know these)
- `Metro atlas: 16 environment tiles` --references--> `Metro Tile Atlas (4×4)`  [EXTRACTED]
  ASSETS.md → tiles/metro_atlas.png
- `Runner: open-route enemy` --references--> `Runner Sprite`  [EXTRACTED]
  ASSETS.md → enemies/runner.png
- `Breaker: barricade attacker` --references--> `Breaker Sprite`  [EXTRACTED]
  ASSETS.md → enemies/breaker.png
- `Single-target machine-gun tower` --references--> `Machine Gun Tower Sprite`  [EXTRACTED]
  ASSETS.md → towers/machine_gun.png
- `Electrical Tesla tower` --references--> `Tesla Tower Sprite`  [EXTRACTED]
  ASSETS.md → towers/tesla.png

## Communities (11 total, 2 thin omitted)

### Community 0 - "Metro Map Tiles"
Cohesion: 0.29
Nodes (7): Atlas slicing and normalization required, Metro atlas: 16 environment tiles, Weathered Concrete Floor Variants, Grates, Metal Plate, Hatch and Hazard Stripes, Metro Tile Atlas (4×4), Horizontal and Vertical Rail Segments, Concrete Wall Segments and Corner

### Community 1 - "Graph Navigation and Updates"
Cohesion: 0.40
Nodes (5): URL ingestion and file watcher, Vocabulary-constrained query expansion, Graph query path and explain, Incremental extraction and manifest, Project graph-first navigation policy

### Community 2 - "Graphify Build Pipeline"
Cohesion: 0.50
Nodes (5): Graph exports and benchmark, Repository cloning and graph merging, Post-commit graph refresh, Deterministic AST extraction, Graphify knowledge graph pipeline

### Community 3 - "Electrical Tower Artwork"
Cohesion: 0.40
Nodes (5): Electrical Tesla tower, Weathered Circular Electrical Tower Mount, Control Box with Cyan Indicator, Copper Coils and Spherical Terminal, Tesla Tower Sprite

### Community 4 - "Machine Gun Artwork"
Cohesion: 0.50
Nodes (4): Single-target machine-gun tower, Long Barrel and Ammunition Belt, Weathered Circular Turret Mount, Machine Gun Tower Sprite

### Community 5 - "Semantic Extraction Provenance"
Cohesion: 0.67
Nodes (3): Extraction confidence and provenance, Whisper media transcription, Semantic extraction and cache

### Community 6 - "Game Art Direction"
Cohesion: 0.67
Nodes (3): Original starter artwork prompts, Godot 2D PC tower defense, Post-apocalyptic underground metro style

### Community 7 - "Barricade Breaker Enemy"
Cohesion: 0.67
Nodes (3): Breaker: barricade attacker, Rusted Scrap Armor, Breaker Sprite

### Community 8 - "Tunnel Runner Enemy"
Cohesion: 0.67
Nodes (3): Runner: open-route enemy, Clawed Humanoid Creature, Runner Sprite

## Knowledge Gaps
- **16 isolated node(s):** `Graph exports and benchmark`, `Repository cloning and graph merging`, `Static artwork without gameplay resources`, `Post-apocalyptic underground metro style`, `Godot 2D PC tower defense` (+11 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **2 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Original starter artwork prompts` connect `Game Art Direction` to `Metro Map Tiles`, `Electrical Tower Artwork`, `Machine Gun Artwork`, `Barricade Breaker Enemy`, `Tunnel Runner Enemy`, `Starter Asset Inventory`?**
  _High betweenness centrality (0.166) - this node is a cross-community bridge._
- **Why does `Metro atlas: 16 environment tiles` connect `Metro Map Tiles` to `Starter Asset Inventory`, `Game Art Direction`?**
  _High betweenness centrality (0.152) - this node is a cross-community bridge._
- **Why does `Metro defense starter artwork` connect `Starter Asset Inventory` to `Metro Map Tiles`, `Electrical Tower Artwork`, `Machine Gun Artwork`, `Game Art Direction`, `Barricade Breaker Enemy`, `Tunnel Runner Enemy`?**
  _High betweenness centrality (0.137) - this node is a cross-community bridge._
- **What connects `Graph exports and benchmark`, `Repository cloning and graph merging`, `Static artwork without gameplay resources` to the rest of the system?**
  _16 weakly-connected nodes found - possible documentation gaps or missing edges._
## Token accounting note

Semantic extraction used the host session and delegated workers. Their tool results did not expose token usage; recorded zeros are placeholders, not a claim of zero token consumption. No external model API was called.

## Validation

Graph health: OK. No missing or dangling endpoints, self-loops, duplicate or collapsed edges.

Benchmark error: No matching nodes found for sample questions. Build the graph first.
The graph exists and exported successfully; benchmark sample questions did not match, so no token-reduction measurement is available.

Scope: detector left project.godot and node_2d.tscn unclassified; structural extraction produced no code nodes. This graph describes assets, documentation, and bundled graphify tooling.
