# Graph Report - game-4  (2026-10-02)

## Corpus Check
- 40 files · ~632,914 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 297 nodes · 337 edges · 25 communities (22 shown, 3 thin omitted)
- Extraction: 99% EXTRACTED · 1% INFERRED · 0% AMBIGUOUS · INFERRED: 5 edges (avg confidence: 0.84)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- Original starter artwork prompts
- Graphify knowledge graph pipeline
- Урок: картки та розміщення двох веж
- Урок: перша кулеметна вежа
- EnemySpawner
- game-4
- 🎮 System Context & AI Assistant Directives
- CardPlacement
- Готовий розв'язок: картки та розміщення веж
- Godot Logo
- Готовий розв’язок: перша бойова вежа
- RotatingTurret
- Урок: картки для встановлення турелей
- Стінка: модель, картка, HP і маршрути мобів
- TowerCardData
- Asset generation
- Готовий розв’язок
- Картки: заповнюємо CardPlacement.cs крок за кроком
- Готовий розв’язок: картки, енергія й розміщення
- Зелений варіант лампи
- Урок: як турель повертається і стріляє
- Повний розв’язок: стінка
- render_wall.py
- TurretBullet

## God Nodes (most connected - your core abstractions)
1. `CardPlacement` - 26 edges
2. `RotatingTurret` - 18 edges
3. `Урок: картки та розміщення двох веж` - 18 edges
4. `Enemy` - 16 edges
5. `Картки: заповнюємо CardPlacement.cs крок за кроком` - 16 edges
6. `EnemySpawner` - 14 edges
7. `Урок: картки для встановлення турелей` - 13 edges
8. `Урок: як турель повертається і стріляє` - 13 edges
9. `Стінка: модель, картка, HP і маршрути мобів` - 10 edges
10. `TowerCardData` - 9 edges

## Surprising Connections (you probably didn't know these)
- `Breaker: barricade attacker` --references--> `Breaker Sprite`  [EXTRACTED]
  ASSETS.md → enemies/breaker.png
- `Single-target machine-gun tower` --references--> `Machine Gun Tower Sprite`  [EXTRACTED]
  ASSETS.md → towers/machine_gun.png
- `Metro atlas: 16 environment tiles` --references--> `Metro Tile Atlas (4×4)`  [EXTRACTED]
  ASSETS.md → tiles/metro_atlas.png
- `Runner: open-route enemy` --references--> `Runner Sprite`  [EXTRACTED]
  ASSETS.md → enemies/runner.png
- `Electrical Tesla tower` --references--> `Tesla Tower Sprite`  [EXTRACTED]
  ASSETS.md → towers/tesla.png

## Import Cycles
- None detected.

## Communities (25 total, 3 thin omitted)

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
Nodes (16): AStarGrid2D, List, Marker2D, Node2D, PackedScene, Vector2, EnemySpawner, EnemyScene (+8 more)

### Community 5 - "game-4"
Cohesion: 0.50
Nodes (4): net8.0, net9.0, game-4, Godot.NET.Sdk/4.7.0

### Community 6 - "🎮 System Context & AI Assistant Directives"
Cohesion: 0.25
Nodes (7): 👤 AI Role, Core Mechanics, File 1: `Instructions_and_Logic.md`, File 2: `Final_Solution.cs` (or relevant extension), 📜 Game Design Document (GDD Summary), 🛠️ Strict Pedagogical Protocol (The "Two-File" Rule), 🎮 System Context & AI Assistant Directives

### Community 8 - "CardPlacement"
Cohesion: 0.12
Nodes (18): Array, HashSet, HBoxContainer, InputEvent, Label, CardPlacement, Energy, PlacedCount (+10 more)

### Community 9 - "Готовий розв'язок: картки та розміщення веж"
Cohesion: 0.40
Nodes (4): 1. res://levels/TowerPlacement.cs, 2. res://levels/GridNavigation.cs, Готовий розв'язок: картки та розміщення веж, Налаштування для першого запуску

### Community 11 - "Готовий розв’язок: перша бойова вежа"
Cohesion: 0.50
Nodes (3): res://enemies/Enemy.cs, res://towers/MachineGunTower.cs, Готовий розв’язок: перша бойова вежа

### Community 12 - "RotatingTurret"
Cohesion: 0.10
Nodes (19): List, Vector2, Enemy, CanBeTargeted, Health, MaxHealth, Node2D, Marker2D (+11 more)

### Community 13 - "Урок: картки для встановлення турелей"
Cohesion: 0.14
Nodes (13): 10. Заповни руку та перевір, 11. Далі, 1. Що робимо зараз, 2. Розподіл відповідальності, 3. Спочатку створи дані картки, 4. Підготуй рівень, 5. Напиши стан і початкове налаштування, 6. Вибір не витрачає енергію (+5 more)

### Community 14 - "Стінка: модель, картка, HP і маршрути мобів"
Cohesion: 0.18
Nodes (10): 1. Що отримаємо, 2. Асети та сцена, 3. Дані картки, 4. Wall: стан і руйнування, 5. GridNavigation: дві сітки, 6. Enemy: обхід, атака, оновлення, 7. EnemySpawner і CardPlacement, 8. Налаштуй руку та перевір (+2 more)

### Community 15 - "TowerCardData"
Cohesion: 0.20
Nodes (9): PackedScene, Texture2D, TowerCardData, Artwork, Cost, PreviewTexture, Title, TowerScene (+1 more)

### Community 16 - "Asset generation"
Cohesion: 0.29
Nodes (6): Asset generation, base, bullet, flash, gun, lamp

### Community 17 - "Готовий розв’язок"
Cohesion: 0.17
Nodes (10): res://towers/rotating_tower/bullet.tscn, res://towers/rotating_tower/rotating_turret.tscn, res://towers/rotating_tower/RotatingTurret.cs, res://towers/rotating_tower/TurretBullet.cs, Готовий розв’язок, Звідки b, c і discriminant, Навіщо додаткові перевірки, Приклад із числами (+2 more)

### Community 18 - "Картки: заповнюємо CardPlacement.cs крок за кроком"
Cohesion: 0.09
Nodes (20): 10. Build і заповнення Inspector, 11. Платформи повинні бути непрохідними для ворогів, 12. Перевір механіку в такому порядку, 1. Каркас і поля: що контролер має пам’ятати, 2. Дві допоміжні перевірки, 3. Початкове налаштування в _Ready, 4. Оновлення написів і стану кнопок, 5. Вибір картки і скасування (+12 more)

### Community 19 - "Готовий розв’язок: картки, енергія й розміщення"
Cohesion: 0.40
Nodes (4): res://cards/CardPlacement.cs, res://cards/TowerCardData.cs, res://levels/GridNavigation.cs, Готовий розв’язок: картки, енергія й розміщення

### Community 21 - "Урок: як турель повертається і стріляє"
Cohesion: 0.15
Nodes (13): 10. Як куля завдає шкоди, 11. Чому порядок рядків важливий, 12. Перевір у грі по одній ознаці, 1. Що саме має обертатися, 2. Як скрипт знаходить частини сцени, 3. Які дані зберігаємо між кадрами, 4. Як вибрати ціль, 5. Поворот — це три прості обчислення (+5 more)

### Community 22 - "Повний розв’язок: стінка"
Cohesion: 0.25
Nodes (7): `cards/TowerCardData.cs`, `enemies/Enemy.cs`, `levels/CardPlacement.cs`, `levels/EnemySpawner.cs`, `levels/GridNavigation.cs`, `walls/Wall.cs`, Повний розв’язок: стінка

### Community 24 - "TurretBullet"
Cohesion: 0.22
Nodes (6): Sprite2D, Vector2, TurretBullet, HitRadius, Lifetime, Speed

## Knowledge Gaps
- **153 isolated node(s):** `Title`, `Cost`, `Artwork`, `PreviewTexture`, `TowerScene` (+148 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **3 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `CardPlacement` connect `CardPlacement` to `RotatingTurret`?**
  _High betweenness centrality (0.037) - this node is a cross-community bridge._
- **Why does `Enemy` connect `RotatingTurret` to `TurretBullet`?**
  _High betweenness centrality (0.031) - this node is a cross-community bridge._
- **What connects `Title`, `Cost`, `Artwork` to the rest of the system?**
  _153 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Original starter artwork prompts` be split into smaller, more focused modules?**
  _Cohesion score 0.09116809116809117 - nodes in this community are weakly interconnected._
- **Should `Урок: картки та розміщення двох веж` be split into smaller, more focused modules?**
  _Cohesion score 0.09090909090909091 - nodes in this community are weakly interconnected._
- **Should `EnemySpawner` be split into smaller, more focused modules?**
  _Cohesion score 0.11255411255411256 - nodes in this community are weakly interconnected._
- **Should `CardPlacement` be split into smaller, more focused modules?**
  _Cohesion score 0.12183908045977011 - nodes in this community are weakly interconnected._