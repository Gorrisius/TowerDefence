# Урок: картки та розміщення двох веж

Дата: 21.09.2026. Проєкт: Godot 4 / C#. Пояснення — українською, код і коментарі — англійською.

У цій папці два навчальні документи: цей урок і `Final_Solution.md` із повним C#-кодом двох файлів. У `cards/` лежать дві нові ілюстрації карток. Реалізацію ще не підключено до твоєї робочої сцени: ти пройдеш урок і напишеш її сам.

## 1. Що саме робимо зараз

Після уроку гравець бачить дві картки, вибирає одну, наводить курсор на платформу, бачить зелений або червоний попередній перегляд і ставить вежу лівою кнопкою. Успішне розміщення витрачає енергію та одну картку. Невдалий клік або скасування нічого не витрачають.

Навчальні параметри: 5 енергії, кулемет коштує 3, Tesla — 4. У руці по одній картці кожного типу. Обидві зіграти за цього запасу не можна — це демонструє обмеження енергії з твого задуму. Для перевірки двох установлених веж можна тимчасово задати 7 енергії. Це не остаточний баланс.

Поточний результат — **розміщення веж**, їхні сцени та інтерфейс карток. Стрільби, здоров'я ворогів і шкоди ще немає. Tesla поки не отримує вигаданої ланцюгової атаки чи уповільнення. Випадкова колода, раунди, discard/retain, комбінації й метапрогресія залишаються наступними етапами. Зараз «зіграно» означає недоступну картку, а не повноцінну стопку скидання.

### Важливе рішення щодо маршрутів

На твоїй мапі є чотири BuildZones: `(8,2)`, `(14,2)`, `(8,11)`, `(14,11)`. Це фіксовані платформи для веж. **Усі платформи зарезервовані для будівництва від початку рівня**, тому вороги обходять їх навіть до появи вежі. Так поточні маршрути не застарівають при розміщенні.

Це навчальне рішення для веж. Барикади працюватимуть інакше: за твоїм GDD повне перекриття дозволене, звичайні вороги тоді атакують найближчу барикаду, а Breaker руйнує барикади на найкоротшому шляху. Ми не вводимо заборону повного перекриття для майбутньої системи барикад.

## 2. Які об'єкти за що відповідають

| Об'єкт | Відповідальність |
| --- | --- |
| `machine_gun_card.png`, `tesla_card.png` | Ілюстрації для руки гравця; не спрайти веж на мапі |
| `TextureButton` | Натискання на картку |
| `Label` на картці | Назва, ціна та стан, які можна змінювати незалежно від PNG |
| `PackedScene` вежі | Шаблон; кожна поставлена вежа — новий екземпляр |
| `TowerPlacement` | Вибір, перевірки, попередній перегляд, витрата енергії |
| `Towers` | Контейнер поставлених веж |
| `BuildZones` | Дозволені клітинки для будівництва |
| `HashSet<Vector2I>` | Уже зайняті платформи; швидка перевірка без обходу всіх веж |
| `GridNavigation` | Шлях ворогів із заздалегідь зарезервованими платформами |
| `CanvasLayer` | Інтерфейс, який не рухається разом із Camera2D |

Довідка: [CanvasLayer](https://docs.godotengine.org/en/stable/classes/class_canvaslayer.html), [TextureButton](https://docs.godotengine.org/en/stable/classes/class_texturebutton.html).

## 3. Створи дві сцени веж

Усі дії нижче виконуються в Godot. ЛКМ — ліва, ПКМ — права кнопка миші. Спершу зупини гру через F8.

1. Угорі натисни **Scene → New Scene**.
2. У порожній панелі Scene натисни **2D Scene**. Корінь буде Node2D.
3. Вибери корінь, F2, назви `MachineGunTower`, Enter.
4. Вибери корінь → «+» над деревом Scene → знайди `Sprite2D` → Create.
5. Вибери Sprite2D. Із FileSystem перетягни `res://towers/machine_gun.png` у його **Texture** в Inspector.
6. На Sprite2D відкрий **Transform → Scale**: x=`0.076555`, y=`0.076555`. Це приблизно 96 пікселів для всього полотна 1254 px; видима основа менша через прозоре тло. Position залиш `(0,0)`.
7. Вибери кореневий MachineGunTower → **Ordering → Z Index**: `5`. Scale кореня залиш `(1,1)`.
8. Ctrl+S → відкрий `res://towers` → ім'я `machine_gun.tscn` → Save.
9. Знову **Scene → New Scene → 2D Scene**. Назви корінь `TeslaTower`.
10. Додай Sprite2D тією самою кнопкою «+», Texture=`res://towers/tesla.png`, Scale=`(0.076555,0.076555)`.
11. На корені TeslaTower постав Z Index=`5`. Ctrl+S → `res://towers/tesla.tscn`.

На цьому етапі на вежах немає скрипта. Сцени зберігають вигляд, а розміщенням керує окремий контролер. Не додавай CollisionShape2D під Node2D: фізичної поведінки це не створить. Непрохідність платформ задається в навігаційній сітці.

Перевір: у двох сцен є дочірній Sprite2D, у кожної своя текстура; текстура картки сюди не потрапила.

## 4. Додай контролер до твоєї мапи

1. У FileSystem відкрий `levels/level_01.tscn` подвійним кліком.
2. Вибери Level01 → «+» → Node2D → Create → F2 → `Towers`.
3. Знову вибери Level01 → «+» → Node2D → Create → назви `TowerPlacement`.
4. На TowerPlacement залиш Transform: Position `(0,0)`, Scale `(1,1)`, Rotation `0`.
5. Вибери TowerPlacement → «+» → Sprite2D → назви `Preview`.
6. На Preview постав **Ordering → Z Index = 10**, **Visibility → Visible = Off**. Texture залиш порожньою: вона змінюватиметься при виборі картки.
7. Вибери TowerPlacement → ПКМ → **Attach Script**.
8. Language=`C#`, Inherits=`Node2D`, Path=`res://levels/TowerPlacement.cs` → Create.
9. Ctrl+S. На **Level01 має залишитися GridNavigation.cs**. Не замінюй його скриптом розміщення.

## 5. Створи панель руки

У цьому уроці будуємо UI в редакторі, тому ти бачитимеш кожен вузол. Нічого не потрібно створювати в `_Ready()` замість цих кроків.

1. Вибери TowerPlacement → «+» → `CanvasLayer` → Create → назви `HUD`.
2. Вибери HUD → «+» → `Panel` → Create → назви `HandPanel`.
3. Вибери HandPanel. В Inspector відкрий **Layout**. Встанови режим **Anchors**, якщо редактор показує вибір Layout Mode.
4. В **Anchors Preset** вибери **Top Right**. У розширених параметрах Layout перевір anchors: Left=`1`, Right=`1`, Top=`0`, Bottom=`0`.
5. У **Layout → Offsets** задай Left=`-372`, Top=`16`, Right=`-16`, Bottom=`348`. Якщо Offsets приховані, розгорни розширені параметри anchors. Має вийти панель **356 × 332**, відступ 16 px від верхнього та правого краю viewport.
6. На HandPanel відкрий **Mouse → Filter**, вибери **Stop**. Панель має поглинати кліки, щоб вони не будували вежу на мапі під нею.
7. Вибери HandPanel → «+» → Label → назви `EnergyLabel`.
8. На EnergyLabel задай **Layout → Transform → Position** `(12,8)`, **Size** `(332,24)`, Text=`Енергія: 5 / 5`.
9. На Label встанови **Mouse → Filter = Ignore**.
10. Знову вибери HandPanel → «+» → Label → назви `StatusLabel`.
11. Position=`(12,34)`, Size=`(332,40)`, Text=`Обери картку`, **Autowrap Mode = Word Smart**, Mouse Filter=`Ignore`.

Для дочірніх Label залиш anchors зверху зліва й режим Position. Якщо поля Size не зменшуються, перевір мінімальний розмір тексту та шрифту. `Label` не має перехоплювати кліки поверх кнопки.

Довідка з координат і Mouse Filter: [Control](https://docs.godotengine.org/en/stable/classes/class_control.html).

## 6. Збери картку кулемета

1. Вибери HandPanel → «+» → `TextureButton` → Create → назви `MachineGun`.
2. Із FileSystem відкрий `2026-09-21/cards`. Перетягни `machine_gun_card.png` в **Textures → Normal** кнопки.
3. Увімкни **Ignore Texture Size**. У **Stretch Mode** вибери **Keep Aspect Centered**.
4. Layout → Transform: Position=`(12,80)`, Size=`(156,234)`. Це пропорція 2:3, як у PNG.
5. Mouse Filter=`Stop`. **Focus → Mode = None**, щоб клавіші керування не залишали випадковий фокус на картці.
6. Вибери MachineGun → «+» → Label → назви `Cost`.
7. Cost: Position=`(7,7)`, Size=`(24,24)`, Text=`3`, Horizontal Alignment=`Center`, Vertical Alignment=`Center`, Mouse Filter=`Ignore`.
8. На Cost відкрий **Theme Overrides → Font Sizes**. Увімкни override **Font Size**, введи `16`.
9. Вибери MachineGun → «+» → Label → назви `Title`.
10. Title: Position=`(38,8)`, Size=`(112,18)`, Text=`КУЛЕМЕТ`, Font Size override=`12`, Mouse Filter=`Ignore`.
11. Вибери MachineGun → «+» → Label → назви `Description`.
12. Description: Position=`(12,184)`, Size=`(132,42)`, Text=`Поставити кулеметну вежу`, Autowrap Mode=`Word Smart`, Font Size=`12`, Mouse Filter=`Ignore`.

Назви вузлів потрібно вводити точно. Написи Text можна змінювати: це текст для гравця. Ціна й опис стану оновлюються кодом, назва Title залишається заданою в редакторі.

## 7. Зроби другу картку

1. У Scene вибери **MachineGun** разом із його дочірніми вузлами через вибір батьківського вузла.
2. Натисни **Ctrl+D**. Дублікат має з'явитися поруч усередині HandPanel.
3. F2 → назви дублікат `Tesla`.
4. На Tesla задай Position=`(188,80)`, Size залиш `(156,234)`.
5. У Textures → Normal перетягни `2026-09-21/cards/tesla_card.png`.
6. Розгорни Tesla у Scene. На її Cost постав Text=`4`; на Title — `TESLA`; на Description — `Поставити Tesla-вежу`.
7. Переконайся, що Mouse Filter у трьох Label — Ignore, у кнопки — Stop.
8. Ctrl+S.

Новий фрагмент дерева:

```text
Level01                       [існуючий GridNavigation.cs]
├── Ground / Walls / BuildZones / Details / Spawn / Base / ...
├── Towers                    [Node2D]
└── TowerPlacement            [Node2D + TowerPlacement.cs]
    ├── Preview               [Sprite2D]
    └── HUD                   [CanvasLayer]
        └── HandPanel         [Panel]
            ├── EnergyLabel   [Label]
            ├── StatusLabel   [Label]
            ├── MachineGun    [TextureButton]
            │   ├── Cost      [Label]
            │   ├── Title     [Label]
            │   └── Description [Label]
            └── Tesla         [TextureButton, такі самі три Label]
```

## 8. Напиши логіку самостійно: дані та ініціалізація

Відкрий `levels/TowerPlacement.cs`. Клас має називатися **TowerPlacement**, успадковувати **Node2D** та містити `partial`. Код нижче описаний по методах; повний розв'язок — у другому документі.

1. Підключи `Godot` і `System.Collections.Generic`.
2. Усередині класу створи `enum TowerKind { MachineGun, Tesla }`. Так тип вежі не залежить від напису на кнопці.
3. Оголоси експортовані властивості: `PackedScene MachineGunScene`, `PackedScene TeslaScene`, `Texture2D MachineGunPreview`, `Texture2D TeslaPreview`, `int StartingEnergy = 5`, `int MachineGunCost = 3`, `int TeslaCost = 4`. Для властивостей використовуй `{ get; set; }`.
4. Збережи поточну енергію окремо: `public int Energy { get; private set; }`. StartingEnergy — налаштування, Energy — змінний стан гри.
5. Приватні поля: посилання на три TileMapLayer, Node2D Towers, два Marker2D, Sprite2D Preview, Panel, два Label, дві TextureButton.
6. Додай `HashSet<Vector2I> _occupied = new()` для зайнятих платформ; `bool[] _used = new bool[2]` для двох карток; `TowerKind? _selected` для вибору. Знак `?` дозволяє значення null — жодної вибраної картки. `bool _ready` захищає методи до завершення налаштування.
7. У `_Ready()` отримай вузли через GetNode один раз. Від TowerPlacement шлях до підлоги — `../Ground`, до контейнера — `../Towers`, до панелі — `HUD/HandPanel`, до кнопки від панелі — `MachineGun`.
8. Перевір чотири ресурси на null та коректність енергії/цін. При помилці покажи її, вимкни кнопки та обробку вводу; не дозволяй напівналаштованій системі працювати.
9. Присвой Energy = StartingEnergy. Підключи `Pressed` кожної кнопки до `SelectCard` з її типом. Приклад: `button.Pressed += () => SelectCard(TowerKind.MachineGun);`.
10. Запиши ціни у Cost.Text, вистав `_ready=true`, виклич RefreshHand.

Не підключай ці самі сигнали через Signals у редакторі: інакше клік може обробитися двічі.

## 9. Вибір картки та скасування

Напиши `public bool SelectCard(TowerKind kind)`:

- Поверни false, якщо система не готова, тип не підтримується, картка вже використана або не вистачає енергії.
- Якщо цю картку вже вибрано — виклич CancelSelection і поверни true.
- Інакше запиши тип у `_selected`, признач Preview.Texture відповідно до типу.
- Обчисли масштаб попереднього перегляду: `96 / найбільша сторона текстури`. Це спрайт вежі, а не її картка.
- Онови підказку й кольори кнопок. **На цьому кроці енергію не віднімай.**

Напиши `CancelSelection()`: `_selected=null`, Preview.Visible=false, онови підказку й UI. Не повертай енергію — при виборі її ще не витрачали.

У `_Input(InputEvent input)` оброби лише скасування: натиснутий Esc без Echo або натиснута права кнопка миші. Виклич CancelSelection і `GetViewport().SetInputAsHandled()`. Так скасування працює також над UI.

## 10. Перевірка місця і попередній перегляд

Окремий метод `CanPlaceAt(Vector2I cell, out string reason)` має тільки читати стан і повертати результат, без списання енергії чи створення вежі.

Перевір послідовно:

1. Є вибрана картка, вона доступна, енергії достатньо.
2. На цій клітинці є тайл BuildZones.
3. Є Ground, немає Walls.
4. Це не клітинка Spawn або Base.
5. Клітинка відсутня у `_occupied`.

`GetCellSourceId(cell) == -1` означає порожню клітинку. `_occupied.Contains(cell)` означає, що тут уже поставили вежу. `out string reason` дає змогу пояснити невдалий клік гравцю.

У `_Process`:

```text
якщо немає вибору або курсор над HandPanel: приховати Preview
інакше:
	cell = Ground.LocalToMap(Ground.GetLocalMousePosition())
	center = Ground.ToGlobal(Ground.MapToLocal(cell))
	поставити Preview.GlobalPosition = center
	дозволено? зелений напівпрозорий колір : червоний
	показати Preview
```

Не діли екранну позицію миші просто на 64: Camera2D і масштаб вікна змінюють співвідношення екранних та світових координат. Перевірка `_panel.GetGlobalRect().HasPoint(GetViewport().GetMousePosition())` виконується в координатах UI; у цьому уроці HUD має стандартну трансформацію.

Довідка: [TileMapLayer](https://docs.godotengine.org/en/stable/classes/class_tilemaplayer.html).

## 11. Встановлення — одна завершена операція

У `_UnhandledInput` оброби лише натиснуту ЛКМ при вибраній картці й курсорі поза панеллю. Виклич `TryPlaceAt(MouseCell())`. UI обробляється раніше за `_UnhandledInput`, тому вибір картки не стане одночасно кліком по мапі. Для розміщення не використовуй `_Input` чи перевірку затиснутої кнопки щокадру.

`TryPlaceAt` виконує:

```text
CanPlaceAt не пройдено → показати reason, повернути false
отримати PackedScene за вибраним типом
Instantiate → перевірити, що корінь Node2D
додати в Towers через AddChild
поставити GlobalPosition у центр клітинки
записати cell у _occupied
відняти ціну від Energy
позначити картку використаною
очистити вибір і приховати Preview
оновити UI, повернути true
```

Видалення невірного екземпляра через Free допустиме до додавання в дерево. Успішно встановлені вежі в цьому уроці не продаються й не видаляються. Завдяки очищенню вибору другий швидкий клік не поставить ще одну вежу.

`RefreshHand` оновлює EnergyLabel і дві кнопки. `RefreshCard` вимикає використану/недоступну картку, затемнює її та змінює Description. Так картка залишається на місці з написом «Зіграно», а її відсутність у доступній руці позначена явно.

Довідка: [порядок обробки вводу](https://docs.godotengine.org/en/stable/tutorials/inputs/inputevent.html).

## 12. Додай платформи до навігації

Відкрий **наявний** `levels/GridNavigation.cs`. Не додавай новий однойменний клас в іншій папці.

1. Поряд із `_ground` та `_walls` оголоси `private TileMapLayer _buildZones;`.
2. У `_Ready` після отримання Walls додай `_buildZones = GetNode<TileMapLayer>("BuildZones");`.
3. У внутрішньому циклі після перевірки wall визнач `bool platform = _buildZones.GetCellSourceId(cell) != -1;`.
4. Онови блокування: `_grid.SetPointSolid(cell, noFloor || wall || platform);`.
5. Ти дозволив діагональний рух, закоментувавши Never. Для обходу кутів встанови `_grid.DiagonalMode = AStarGrid2D.DiagonalModeEnum.OnlyIfNoObstacles;` **до Update()**. Діагоналі залишаться, але зрізання заблокованих кутів зникне.
6. Виклик StartSpawning залиш наприкінці `_Ready`, після заповнення сітки.

Повна версія цього файла також є у `Final_Solution.md`. Це статичне резервування: не змінюй BuildZones під час запущеної гри без перебудови маршрутів. Усі три шари мають однаковий TileSet, Position `(0,0)`, Scale `(1,1)`. На Spawn/Base не має бути платформ.

## 13. Признач ресурси та запусти

1. Збережи C#-файли. У Godot натисни **Build** угорі праворуч, дочекайся успішного збирання.
2. На вкладці level_01 вибери TowerPlacement.
3. В Inspector перетягни `towers/machine_gun.tscn` у **Machine Gun Scene**.
4. `towers/tesla.tscn` → **Tesla Scene**.
5. `towers/machine_gun.png` → **Machine Gun Preview**.
6. `towers/tesla.png` → **Tesla Preview**.
7. Starting Energy=`5`, Machine Gun Cost=`3`, Tesla Cost=`4`.
8. Перевір Textures/Normal у кнопок: PNG з `2026-09-21/cards`, а не текстури прев'ю.
9. Ctrl+S. Відкрий вкладку **level_01**, натисни F6.
10. Обери картку кулемета, наведи на одну з чотирьох платформ, клацни ЛКМ. Має залишитися 2 енергії, кулемет — «Зіграно», Tesla — «Бракує енергії».

## 14. Перевірки, які потрібно пройти самому

- Клік по картці не створює вежу і не витрачає енергію.
- Курсор на платформі зелений, на звичайній підлозі червоний. За межами мапи поставити не можна.
- Клік у заборонене місце залишає вибрану картку й енергію.
- ПКМ/Esc скасовують вибір, навіть якщо курсор над карткою.
- Після успіху з'являється один дочірній вузол у Towers. Подвійний клік не створює другу копію.
- Перезапусти з Starting Energy=`7`: постав кулемет, вибери Tesla і спробуй ту саму клітинку. Має бути «Платформа вже зайнята», енергія лишається 4. Постав Tesla на іншу платформу: дві вежі, енергія 0.
- Перезапусти з Starting Energy=`2`: обидві картки недоступні.
- Вороги обходять зарезервовані платформи й доходять до Base.
- Переміщення/масштабування Camera2D не зсуває HUD і не ламає прив'язку прев'ю до клітинки.
- Після завершення перевірок поверни Starting Energy=`5`.

Якщо Node not found — звір дерево й регістр назв. Якщо експортованих полів не видно — Build та повторний вибір вузла. Якщо всі платформи червоні — перевір Ground/Walls/BuildZones на однакових координатах. Якщо кнопки гігантські — увімкни Ignore Texture Size. Якщо клік на написі не вибирає картку — Mouse Filter у Label має бути Ignore. Якщо вороги перестали з'являтися — прочитай Output: перевір, чи резервування платформ не перекрило єдиний прохід.

## 15. Наступні уроки та зв'язок із дипломом

Цей модуль демонструє поділ сцени-шаблону й екземпляра, скінченні стани вибору, перевірку умов перед зміною стану, координатні перетворення та UI-ввід. Поясни своїми словами, чому CanPlaceAt не витрачає енергію і чому клітинки платформ резервуються до старту ворогів.

Далі: характеристики й атаки веж → здоров'я ворогів → раунди та випадкове добирання → discard/retain → комбінації → барикади та два типи навігації. Для більшої колоди дані картки варто винести в Resource; нинішні два типи й ціни спеціально прості, щоб спершу опанувати розміщення.

## 16. Графіка: джерело та промпти

Обидві картинки створено вбудованим imagegen, 1024 × 1536 PNG, окремо для кожної вежі. Це оформлення карток із порожніми полями, де Godot відображає текст. Ілюстрація картки може мати перспективу; спрайт вежі на мапі залишається наявним зображенням згори. Оригінальні PNG в `towers/` не змінено.

Точні промпти генерації наведені нижче для відтворюваності.

### Machine gun

```text
Use case: stylized-concept. Create ONE production game card front illustration for a Godot 2D post-apocalyptic metro tower-defense deckbuilding game. Portrait 2:3 aspect ratio. Flat straight-on full-bleed rectangular card, no tabletop, no mockup, no external margins. Worn dark charcoal steel frame, restrained rivets, desaturated olive armor, rusty copper and small amber light accents. Centerpiece: recognizable improvised single-barrel machine-gun turret on a circular bolted steel base, ammunition box, long barrel, no operator, hand-painted chunky readable silhouette, gloomy metro concrete backdrop. Upper 12% quiet blank title strip and lower 22% quiet dark blank rules panel, blank round energy badge top-left. No text, no letters, no numbers, no watermark. Polished cohesive collectible game card, atmospheric but clearly readable at 160px wide. Keep the weapon fully inside central illustration area.
```

### Tesla

```text
Use case: stylized-concept. Create ONE production game card front illustration for a Godot 2D post-apocalyptic metro tower-defense deckbuilding game. Portrait 2:3 aspect ratio. Flat straight-on full-bleed rectangular card, no tabletop, no mockup, no external margins. Worn dark charcoal steel frame, restrained rivets, desaturated olive armor, rusty copper with small cyan electrical light accents. Centerpiece: improvised Tesla defense tower on a circular bolted steel base, thick copper coils surrounding a ceramic insulator and spherical top electrode, scavenged olive control box, small contained cyan electrical arcs around electrode. Hand-painted chunky readable silhouette, gloomy metro concrete backdrop. Upper 12% quiet blank title strip and lower 22% quiet dark blank rules panel, blank round energy badge top-left. No text, no letters, no numbers, no watermark. Polished cohesive collectible game card, atmospheric but clearly readable at 160px wide. Keep tower fully inside central illustration area.
```


## Перевірка готового розв'язку

Код із Final_Solution.md перевірено окремо від робочого проєкту: Godot.NET.Sdk 4.7.0, збирання без помилок і попереджень компілятора. У headless-запусках на копії твоєї мапи пройшли перевірки вибору й скасування, відмови поза платформами, одноразового списання, використаної картки, зайнятого місця та недостатньої енергії. За 7 енергії встановилися дві вежі; за 5 — одна. В обох випадках п'ять ворогів дійшли до бази, маршрут не містив платформ.

Ручне компонування UI, вигляд у вікні й кліки мишею перевір за розділом 14: headless-перевірки викликали методи й сигнал кнопки, а не відтворювали всі натискання в редакторі. Тестова обв'язка завантажувала PNG напряму й отримувала попередження про такий спосіб завантаження при експорті; готовий розв'язок використовує призначені через Inspector ресурси, а не цей тестовий завантажувач.
