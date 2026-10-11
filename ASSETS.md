# Metro defense starter artwork

Generated with the built-in image generation tool; exact prompts are in ASSET_PROMPTS.md.
All five PNGs are 1254 × 1254 pixels. Enemy and tower images have verified alpha transparency.

| File | Purpose |
| --- | --- |
| tiles/metro_atlas.png | 4 × 4 visual atlas of 16 environment tiles |
| enemies/runner.png | Light tunnel mutant, intended to prefer open routes |
| enemies/breaker.png | Armored mutant, intended to attack barricades |
| towers/machine_gun.png | Single-target gun tower |
| towers/tesla.png | Electrical tower |

Atlas order, left to right:

1. Plain concrete, cracked concrete, stained concrete, rubble concrete.
2. Horizontal drain, vertical drain, steel plate, damp concrete.
3. Horizontal wall, vertical wall, wall corner, solid wall block.
4. Horizontal rails, vertical rails, hatch, hazard stripes.

These are starter source images, with no animations, collision, behavior, or Godot TileSet resource yet.
The generator produced 1254-pixel images rather than the requested 1024 pixels; the atlas is not an exact integer-cell grid. Slice and normalize its regions before configuring a regular Godot tile atlas. Tile edge continuity also needs checking when assembling the map.
The art uses an elevated overhead appearance; character and electrical tower perspective is more angled than strict orthographic top-down.
For the prototype, aim for roughly 64-pixel ground cells, 48–64-pixel runners, 64–80-pixel breakers, and 96-pixel towers, adjusting for transparent margins. The machine-gun image includes its base, so independent barrel rotation would require separating the artwork later.
