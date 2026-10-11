# Картки: заповнюємо CardPlacement.cs крок за кроком

Цей урок продовжує твою роботу станом на 27 вересня 2026 року. Дані `TowerCardData` та `cards/machine_gun.tres` уже є. Сцена інтерфейсу почата в `levels/level_cards.tscn`, а скрипт прикріплений із **`levels/CardPlacement.cs`**. Працюй у ньому; створювати другий CardPlacement у папці cards не потрібно.

Мета: дві картки на екрані, вибір однієї, позначка під мишею, встановлення турелі на платформу і списання 3 енергії з початкових 6. Натискання на картку лише вибирає її. Енергія витрачається після успішного встановлення.

[Повний файл для звіряння](Final_Solution.md) лежить поруч. Він записаний у Markdown, тому не створює дубліката класу у проєкті.

## Як працювати з фрагментами

Крок 1 замінює початковий каркас файлу. Подальші методи додавай **всередині класу**, перед його останньою `}`. Якщо метод з такою назвою вже є, замінюй його, а не додавай другий. Зокрема, порожній `_Process` потрібно замінити реалізацією з кроку 8.

Методи посилаються один на одного, тому до завершення кроків 1–10 редактор може підкреслювати ще не написані методи. Це очікувано. Перший повний Build і запуск роби після кроку 10. Перевірки в попередніх кроках — це пояснення очікуваного результату після складання всього файлу.

## Перед кодом: звір дерево сцени

```text
Level01
├── Ground (TileMapLayer)
├── Walls (TileMapLayer)
├── BuildZones (TileMapLayer)
├── Spawn (Marker2D)
├── Base (Marker2D)
├── Enemies
├── Towers (Node2D)
├── EnemySpawner
└── CardPlacement (Node2D, скрипт levels/CardPlacement.cs)
    ├── Preview (Sprite2D)
    └── HUD (CanvasLayer)
        └── HandPanel (Panel)
            ├── EnergyLabel (Label)
            ├── StatusLabel (Label)
            └── Hand (HBoxContainer)
```

У твоєму збереженому `level_cards.tscn` на момент написання уроку **ще немає Hand**. Створи HBoxContainer під HandPanel, назви точно `Hand`. Його Position — `(12, 78)`, Size — `(290, 210)`, Theme Overrides → Constants → Separation — `10`. Кнопки всередині створюватиме код: вручну їх додавати не потрібно.

Для HandPanel використовуй **Anchors Preset = Custom**, якщо вводиш параметри вручну: Anchor Points Left/Right = 1, Top/Bottom = 0; Offsets Left = −330, Right = −16, Top = 16, Bottom = 312. Це панель 314 × 296 з відступами 16 зверху та справа. Для позиції й розміру дітей EnergyLabel, StatusLabel і Hand достатньо Layout Mode = Position.

| Вузол | Налаштування | Навіщо |
|---|---|---|
| CardPlacement | Position (0,0), Rotation 0, Scale (1,1) | Зрозумілі координати контролера |
| Preview | Visible Off, Z Index 10 | До вибору нічого не показуємо; позначка над мапою |
| HandPanel | Mouse → Filter = Stop | Натискання на панель не ставить турель позаду неї |
| EnergyLabel | Position (12,8), Size (290,24), Mouse Filter Ignore | Напис не перехоплює кліки |
| StatusLabel | Position (12,34), Size (290,40), Mouse Filter Ignore, Autowrap Word Smart | Повідомлення переносяться й не блокують ввід |
| Hand | Mouse Filter Pass | Події можуть дійти до панелі-батька |

CanvasLayer залишає панель у координатах екрана, коли Camera2D рухає вигляд мапи. Preview, навпаки, має залишатися у світі гри, тому він не всередині HUD.

## 1. Каркас і поля: що контролер має пам’ятати

Залиш лише ці using: `Godot` для вузлів та векторів, `System.Collections.Generic` для List і HashSet. Прибери `using System.Numerics;` — він містить інший Vector2.

```csharp
using Godot;
using System.Collections.Generic;

public partial class CardPlacement : Node2D
{
    [Export] public Godot.Collections.Array<TowerCardData> StartingHand { get; set; } = new();
    [Export] public int StartingEnergy { get; set; } = 6;
    public int Energy { get; private set; }
    public int PlacedCount => _occupied.Count;
    private int _selected = -1;
    private bool _ready;
    private bool[] _used;
    private readonly List<TextureButton> _buttons = new();
    private readonly HashSet<Vector2I> _occupied = new();
    private TileMapLayer _ground, _walls, _zones;
    private Node2D _towers;
    private Sprite2D _preview;
    private Panel _panel;
    private Label _energy, _status;
    private Vector2I _spawnCell, _baseCell;

    // Тут поступово додаємо методи з наступних кроків.
}
```

Поля — пам’ять контролера між кадрами. Ось призначення **кожного**:

| Поле | Що зберігає і чому потрібне |
|---|---|
| StartingHand | Масив ресурсів карток, які задаєш в Inspector |
| StartingEnergy | Початковий запас; у прикладі 6 |
| Energy | Поточний запас; змінюється після встановлення |
| PlacedCount | Кількість зайнятих клітинок; включає зареєстровані готові вежі |
| _selected | Номер вибраної копії: 0 або 1; −1 означає відсутність вибору |
| _ready | Чи завершилося налаштування без помилок |
| _used | Для кожного слота окремо: чи вже зіграно його картку |
| _buttons | Кнопки, щоб оновлювати їхні кольори, підписи й доступність |
| _occupied | Набір координат зайнятих клітинок |
| _ground | Шар підлоги: за ним переводимо позиції в клітинки |
| _walls | Шар стін: на них ставити не можна |
| _zones | Шар BuildZones: дозволені платформи |
| _towers | Батьківський вузол для встановлених турелей |
| _preview | Sprite2D позначки під мишею |
| _panel | Панель руки; також перевіряємо, чи миша над нею |
| _energy | Label із числом енергії; це вузол, а не саме число Energy |
| _status | Label з інструкцією чи причиною відмови |
| _spawnCell, _baseCell | Клітинки входу й бази, де будувати не можна |

`new()` створює порожню колекцію. `HashSet<Vector2I>` зберігає кожну клітинку лише один раз; `Contains(cell)` перевіряє зайнятість. `readonly` забороняє замінювати саму колекцію, але не заважає додавати в неї елементи.

`public int Energy { get; private set; }` дозволяє читати енергію ззовні, а змінювати — лише всередині контролера. `[Export]` для поточної Energy не потрібний: початкове значення задаємо через StartingEnergy.

Дві картки можуть посилатися на один `machine_gun.tres`, але `_used[0]` і `_used[1]` — незалежні. Якби ознаку використання записали в спільний ресурс, використання першої картки могло б вимкнути й другу.

## 2. Дві допоміжні перевірки

Додай усередину класу:

```csharp
private Vector2I WorldCell(Vector2 position) =>
    _ground.LocalToMap(_ground.ToLocal(position));

private bool OverHud() =>
    _panel.GetGlobalRect().HasPoint(_panel.GetGlobalMousePosition());
```

`WorldCell` приймає світову позицію й повертає індекс клітинки. Спочатку `ToLocal` переводить її в координати Ground. Потім `LocalToMap` знаходить клітинку. Наприклад, при клітинках 64 × 64 і Ground без зміщення позиція `(150, 90)` належить клітинці `(2, 1)`. Vector2 — дробові координати; Vector2I — цілі номери клітинок.

`OverHud` відповідає на запитання «миша зараз над прямокутником панелі?». Це потрібно, щоб клік по руці не поставив вежу на мапі під нею.

`=>` тут скорочує метод до одного виразу. Це звичайний метод із поверненням результату, а не окрема подія Godot.

## 3. Початкове налаштування в _Ready

Заміни свій незавершений `_Ready()` цим методом. Нижче розберемо його по частинах.

```csharp
public override void _Ready()
{
    _ground = GetNode<TileMapLayer>("../Ground");
    _walls = GetNode<TileMapLayer>("../Walls");
    _zones = GetNode<TileMapLayer>("../BuildZones");
    _towers = GetNode<Node2D>("../Towers");
    _preview = GetNode<Sprite2D>("Preview");
    _panel = GetNode<Panel>("HUD/HandPanel");
    _energy = _panel.GetNode<Label>("EnergyLabel");
    _status = _panel.GetNode<Label>("StatusLabel");
    _preview.Hide();
    var hand = _panel.GetNode<HBoxContainer>("Hand");
    _spawnCell = WorldCell(GetNode<Marker2D>("../Spawn").GlobalPosition);
    _baseCell = WorldCell(GetNode<Marker2D>("../Base").GlobalPosition);
    if (StartingEnergy < 0 || StartingHand.Count == 0)
    { _status.Text = "Перевір Starting Hand і Starting Energy."; return; }
    foreach (var card in StartingHand)
        if (card == null || card.Cost < 0 || card.TowerScene == null || card.Artwork == null || card.PreviewTexture == null)
        { _status.Text = "Заповни всі поля ресурсів карток."; return; }

    // Register manually placed towers as occupied cells too.
    foreach (Node node in _towers.GetChildren())
        if (node is Node2D tower) _occupied.Add(WorldCell(tower.GlobalPosition));
    Energy = StartingEnergy;
    _used = new bool[StartingHand.Count];
    for (int i = 0; i < StartingHand.Count; i++)
    {
        int slot = i; // Capture the copy's slot, not the shared resource.
        var button = new TextureButton
        {
            CustomMinimumSize = new Vector2(140, 210),
            TextureNormal = StartingHand[i].Artwork,
            IgnoreTextureSize = true,
            StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered,
            MouseFilter = Control.MouseFilterEnum.Stop,
            FocusMode = Control.FocusModeEnum.None
        };
        var label = new Label
        {
            Name = "Caption", Position = new Vector2(6, 157), Size = new Vector2(128, 48),
            HorizontalAlignment = HorizontalAlignment.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        label.AddThemeFontSizeOverride("font_size", 14);
        button.AddChild(label);
        hand.AddChild(button);
        button.Pressed += () => SelectCard(slot);
        _buttons.Add(button);
    }
    _ready = true;
    _status.Text = "Обери картку. ПКМ / Esc — скасування.";
    RefreshHand();
}
```

**Отримання вузлів.** Скрипт прикріплений до CardPlacement. Шлях `../Ground` означає «піднятися до Level01, потім знайти Ground». Шлях `Preview` веде до власної дитини. `_panel.GetNode("EnergyLabel")` уже починається від панелі. Якщо отримуєш Node not found — звір шлях і дерево, а не назву змінної.

**Перевірка конфігурації.** Порожня рука чи незаповнена текстура дають повідомлення в StatusLabel і `return`. Це завершує `_Ready`, залишаючи `_ready = false`. Інші методи бачать цей прапорець і не починають працювати з неповними даними. `||` означає «або»; при `card == null` наступні звернення до card вже не виконуються.

**Наявні вежі.** Перебираємо дітей Towers і записуємо їхні клітинки в `_occupied`, щоб не поставити другу вежу поверх готової. Для першої перевірки зручніше залишити Towers порожнім у копії рівня.

**Стан руки.** Energy отримує StartingEnergy. `new bool[StartingHand.Count]` створює по одному false для кожного слота — усі копії спочатку невикористані.

**Кнопки.** Для кожного ресурсу створюємо TextureButton. Artwork стає його картинкою. Розмір 140 × 210 задає місце в руці; IgnoreTextureSize не дозволяє великому PNG розтягнути панель, а KeepAspectCentered зберігає пропорції. Caption — Label поверх кнопки. Його MouseFilter Ignore дозволяє натиснути кнопку навіть по напису.

**Чому `int slot = i`.** `Pressed += () => SelectCard(slot)` підписує дію на майбутнє натискання. Стрілка означає «коли натиснуть, виконай SelectCard». Окрема локальна slot зберігає номер саме цієї кнопки. Спільна змінна циклу i до моменту натискання вже змінилася б.

Наприкінці `_ready = true` дозволяє роботу контролера, а RefreshHand заповнює написи. Метод RefreshHand напишемо наступним.

## 4. Оновлення написів і стану кнопок

Додай метод:

```csharp
private void RefreshHand()
{
    _energy.Text = $"Енергія: {Energy}/{StartingEnergy}";
    for (int i = 0; i < _buttons.Count; i++)
    {
        var card = StartingHand[i];
        _buttons[i].Disabled = _used[i] || Energy < card.Cost;
        _buttons[i].Modulate = _used[i] ? new Color(0.4f, 0.4f, 0.4f)
            : i == _selected ? new Color(0.6f, 1, 0.7f) : Colors.White;
        string state = _used[i] ? "Зіграно" : Energy < card.Cost ? "Бракує енергії" : $"Енергія: {card.Cost}";
        _buttons[i].GetNode<Label>("Caption").Text = $"{card.Title}\n{state}";
    }
}
```

Цей метод нічого не купує і не створює: лише показує поточний стан.

`Disabled` стає true, якщо картка вже зіграна або енергії недостатньо. Modulate затемнює використану, підсвічує вибрану, а решту залишає білими. Запис `умова ? A : B` означає «при істинній умові взяти A, інакше B».

`$"Енергія: {Energy}/{StartingEnergy}"` підставляє числа в текст. `\n` у Caption переносить стан на другий рядок.

При старті очікуємо «Енергія: 6/6». Після першої покупки — «3/6», перша картка затемнена, друга доступна. Номер слота починається з 0.

## 5. Вибір картки і скасування

```csharp
public bool SelectCard(int slot)
{
    if (!_ready || slot < 0 || slot >= StartingHand.Count || _used[slot] || Energy < StartingHand[slot].Cost) return false;
    if (_selected == slot) { CancelSelection(); return true; }
    _selected = slot;
    _preview.Texture = StartingHand[slot].PreviewTexture;
    _preview.Scale = Vector2.One * (90f / Mathf.Max(_preview.Texture.GetWidth(), _preview.Texture.GetHeight()));
    _status.Text = "ЛКМ на вільній платформі — встановити.";
    RefreshHand();
    return true;
}
```

```csharp
public void CancelSelection()
{
    _selected = -1;
    _preview.Hide();
    if (!_ready) return;
    _status.Text = "Вибір скасовано. Енергія збережена.";
    RefreshHand();
}
```

Спочатку SelectCard перевіряє, чи контролер готовий, чи існує слот, чи копія ще доступна і чи вистачає енергії. `return false` означає, що вибір відхилено. Натискання вже вибраної картки скасовує вибір.

При успіху запам’ятовуємо `_selected` і задаємо PreviewTexture. Формула масштабу обмежує більшу сторону **всієї текстури** 90 пікселями: для зображення шириною 1028 коефіцієнт близько 0.0875. Прозорі поля теж входять у цей розмір, тому сама основа турелі може виглядати значно меншою. Це не помилка позиціювання. Спочатку перевір механіку; розмір позначки можна підібрати зміною 90f.

CancelSelection скидає номер у −1 й ховає Preview. В обох методах немає `Energy -= ...`: гравець ще нічого не встановив.

**Перевір після завершення файлу:** вибір і повторне натискання не змінюють енергію.

## 6. Окремо перевіряємо, чи можна будувати

```csharp
public bool CanPlaceAt(Vector2I cell, out string reason)
{
    reason = "";
    if (!_ready || _selected < 0) reason = "Спочатку обери картку.";
    else if (_used[_selected]) reason = "Цю копію картки вже зіграно.";
    else if (Energy < StartingHand[_selected].Cost) reason = "Бракує енергії.";
    else if (_zones.GetCellSourceId(cell) == -1) reason = "Потрібна платформа BuildZones.";
    else if (_ground.GetCellSourceId(cell) == -1 || _walls.GetCellSourceId(cell) != -1) reason = "Немає підлоги або є стіна.";
    else if (cell == _spawnCell || cell == _baseCell) reason = "Тут вхід або база.";
    else if (_occupied.Contains(cell)) reason = "Платформа зайнята.";
    return reason.Length == 0;
}
```

`CanPlaceAt` лише перевіряє, не змінюючи світ. Тому ним можна користуватися кожен кадр для кольору Preview і ще раз під час кліку.

`out string reason` повертає додатковий результат: текст причини. Основний bool каже «так/ні», а reason пояснює «чому ні». Порожній рядок означає відсутність відмови.

Порядок перевірок: вибір → використання копії → енергія → платформа → підлога і стіна → вхід/база → зайнятість. `GetCellSourceId(cell) == -1` означає, що на цьому шарі в клітинці немає тайла.

Ground, Walls та BuildZones повинні мати однакові розміри клітинок і трансформації: ми перевіряємо один номер cell на всіх трьох шарах.

Приклад: вибрана копія 0, енергія 6, платформа вільна — true. Та сама клітинка після встановлення є в `_occupied` — false з причиною «Платформа зайнята».

## 7. Встановлюємо вежу і лише тоді витрачаємо енергію

```csharp
public bool TryPlaceAt(Vector2I cell)
{
    if (!CanPlaceAt(cell, out string reason))
    { if (_ready) _status.Text = reason; return false; }
    TowerCardData card = StartingHand[_selected];
    Node instance = card.TowerScene.Instantiate();
    if (instance is not Node2D tower)
    { instance.Free(); _status.Text = "Сцена вежі повинна мати корінь Node2D."; return false; }
    // Set position before AddChild: the tower's _Ready sees its final location.
    tower.Position = _towers.ToLocal(_ground.ToGlobal(_ground.MapToLocal(cell)));
    _towers.AddChild(tower);
    _occupied.Add(cell);
    Energy -= card.Cost;
    _used[_selected] = true;
    _selected = -1;
    _preview.Hide();
    _status.Text = "Турель встановлено.";
    RefreshHand();
    return true;
}
```

Спочатку повторно викликаємо CanPlaceAt: зелений колір позначки не замінює перевірку в момент кліку. Якщо є відмова, показуємо її текст і виходимо без витрат.

`Instantiate` створює екземпляр TowerScene. Перевірка `instance is not Node2D tower` відхиляє сцену з неправильним коренем. Це перевірка типу кореня, а не всіх внутрішніх налаштувань турелі: BulletScene і текстури лампи повинні бути налаштовані в самій сцені турелі заздалегідь.

Розбір позиції:

```text
cell → Ground.MapToLocal → центр клітинки відносно Ground
     → Ground.ToGlobal → позиція у світі
     → Towers.ToLocal → позиція відносно нового батька
```

Position задаємо до AddChild, щоб `_Ready()` нової вежі вже бачив правильне розташування. Після додавання резервуємо клітинку, списуємо ціну й позначаємо лише вибраний слот використаним. Потім скидаємо вибір: другий швидкий клік не поставить ще одну вежу.

У цій версії зайнятість зберігається до кінця запуску. Продаж або руйнування вежі пізніше потребуватимуть окремого видалення клітинки з набору.

## 8. Позначка під мишею щокадру

Заміни порожній `_Process`:

```csharp
public override void _Process(double delta)
{
    if (!_ready || _selected < 0 || OverHud()) { _preview.Hide(); return; }
    Vector2I cell = _ground.LocalToMap(_ground.GetLocalMousePosition());
    _preview.GlobalPosition = _ground.ToGlobal(_ground.MapToLocal(cell));
    _preview.Modulate = CanPlaceAt(cell, out _)
        ? new Color(0.3f, 1, 0.5f, 0.65f) : new Color(1, 0.25f, 0.25f, 0.65f);
    _preview.Show();
}
```

Якщо налаштування не завершене, вибору немає або курсор над HUD — ховаємо позначку. Інакше переводимо мишу в клітинку Ground і ставимо Preview точно в її центр.

`out _` означає «причина відмови зараз не потрібна». Для кольору достатньо bool. Текст причини покажемо при невдалому встановленні. Останній компонент Color — прозорість 0.65: позначка напівпрозора, щоб бачити мапу під нею.

**Перевір:** позначка рухається по клітинках, на платформі зелена, на звичайній підлозі червона. Над панеллю зникає. Якщо її перекриває мапа, перевір Z Index = 10 на Preview і Visible у його батьків.

## 9. ЛКМ ставить, ПКМ і Esc скасовують

```csharp
public override void _Input(InputEvent input)
{
    if (!_ready || _selected < 0) return;
    bool cancel = input is InputEventKey key && key.Pressed && !key.Echo && key.Keycode == Key.Escape
        || input is InputEventMouseButton mouse && mouse.Pressed && mouse.ButtonIndex == MouseButton.Right;
    if (cancel) { CancelSelection(); GetViewport().SetInputAsHandled(); }
}
```

```csharp
public override void _UnhandledInput(InputEvent input)
{
    if (!_ready || _selected < 0 || OverHud()) return;
    if (input is InputEventMouseButton mouse && mouse.Pressed && mouse.ButtonIndex == MouseButton.Left)
    {
        TryPlaceAt(_ground.LocalToMap(_ground.GetLocalMousePosition()));
        GetViewport().SetInputAsHandled();
    }
}
```

`_Input` дозволяє обробити скасування навіть над інтерфейсом. Перевіряємо саме натискання Pressed; `!key.Echo` відсікає повтори утримуваної клавіші.

`_UnhandledInput` отримує події, які не забрав інтерфейс. Тому натискання TextureButton спершу вибирає картку й не повинно одночасно ставити вежу. OverHud додатково захищає всю площу панелі.

`GetViewport().SetInputAsHandled()` позначає подію обробленою, щоб інший обробник не реагував на те саме натискання повторно.

## 10. Build і заповнення Inspector

Тепер у класі мають бути поля та всі методи: WorldCell, OverHud, _Ready, RefreshHand, SelectCard, CancelSelection, CanPlaceAt, TryPlaceAt, _Process, _Input, _UnhandledInput. Порядок оголошення методів у класі не впливає на роботу. Дужки й точні назви впливають.

1. Збережи `levels/CardPlacement.cs`, натисни Build. Порівняй з [повним файлом](Final_Solution.md), якщо є помилки.
2. Відкрий `levels/level_cards.tscn`, вибери вузол CardPlacement.
3. В Inspector встанови Starting Energy = 6.
4. Розгорни Starting Hand. Якщо поле порожнє й редактор пропонує створити Array, створи його. Задай Size = 2.
5. В обидва елементи, 0 і 1, перетягни **ресурс `cards/machine_gun.tres`**, не PNG і не сцену турелі.
6. Відкрий цей ресурс у FileSystem. Перевір Cost = 3, Artwork, Preview Texture та Tower Scene. Актуальна сцена: `res://towers/rotating_tower/rotating_turret.tscn`. Твоє Artwork уже посилається на `res://towers/cards/machine_gun_card.png`.
7. Збережи сцену й запускай **F6**. F5 може запускати старий головний рівень без карток.

StartingHand — поле вузла CardPlacement. Artwork і TowerScene — поля ресурсу machine_gun.tres. Hand — вузол-контейнер у дереві. Це три різні речі: перейменування вузла не заповнює поле ресурсу.

## 11. Платформи повинні бути непрохідними для ворогів

Поточний GridNavigation враховує підлогу й стіни, але ще не BuildZones. Для цього етапу резервуємо платформи з початку рівня, навіть до встановлення турелей. Тоді вже розрахований шлях ворогів не потрібно перебудовувати після покупки.

У наявному `levels/GridNavigation.cs` додай поле:

```csharp
private TileMapLayer _buildZones;
```

У `_Ready`, поруч з отриманням Ground і Walls:

```csharp
_buildZones = GetNode<TileMapLayer>("BuildZones");
```

Перед `_grid.Update()` встанови:

```csharp
_grid.DiagonalMode = AStarGrid2D.DiagonalModeEnum.OnlyIfNoObstacles;
```

У циклі, де обчислюєш noFloor і wall, заміни виклик SetPointSolid:

```csharp
bool platform = _buildZones.GetCellSourceId(cell) != -1;
_grid.SetPointSolid(cell, noFloor || wall || platform);
```

`OnlyIfNoObstacles` дозволяє діагоналі лише без перетинання заблокованих кутів. Відсутність підлоги, стіна або платформа роблять клітинку непрохідною. Повна версія GridNavigation також є в [розв’язку](Final_Solution.md). Змінюй існуючий клас: другий із такою назвою створювати не можна.

Якщо після цього вороги не з’являються, перевір Output: платформи не повинні перекривати всі проходи або стояти на Spawn/Base. Не прибирай перевірку лише для приховування помилки — залиш прохід на мапі.

## 12. Перевір механіку в такому порядку

| Дія | Що має статися |
|---|---|
| Запустити level_cards через F6 | Дві картки, енергія 6/6 |
| Натиснути першу картку | Вона підсвічена, енергія досі 6 |
| Навести на звичайну підлогу й натиснути | Червоне прев’ю, повідомлення про платформу, енергія 6 |
| Навести на вільну платформу й натиснути | Вежа з’явилась, енергія 3, перша картка «Зіграно» |
| Вибрати другу картку й натиснути на зайняту платформу | Відмова, енергія 3 |
| Поставити другу вежу на іншу платформу | Енергія 0, дві використані копії |
| Перезапустити, вибрати картку, натиснути Esc або ПКМ | Прев’ю зникає, енергія 6 |
| Перезапустити зі Starting Energy = 2 | Картки недоступні; після тесту повернути 6 |

Це фіксована рука з двох копій. Використані картки залишаються затемненими, енергія сама не відновлюється. Колода й добирання будуть окремими механіками.

## Якщо щось не працює

| Симптом | Що перевірити і виправити |
|---|---|
| WorldCell does not exist | Додай метод із кроку 2 всередину класу |
| Vector2 is ambiguous | Прибери using System.Numerics |
| Метод уже визначений | Заміни старий метод, не залишай дві реалізації |
| Node not found: Hand | Додай HBoxContainer Hand саме під HandPanel |
| Панель порожня | Starting Hand має два заповнені слоти; прочитай StatusLabel і Output |
| Немає Starting Hand в Inspector | Build має пройти; вибери вузол CardPlacement зі скриптом, а не Hand |
| Клік на картку не працює | Перевір Energy, Cost, Disabled; Caption повинен мати MouseFilter Ignore |
| Клік на панелі ставить вежу | HandPanel MouseFilter Stop; код встановлення в _UnhandledInput |
| Прев’ю маленьке | 90f масштабує всю текстуру разом із прозорими полями |
| Усюди червоне прев’ю | Перевір BuildZones, Ground, Walls, їхні трансформації та зайнятість |
| Вежа створюється, але не стріляє | Перевір її BulletScene, дальність до маршруту, Output і ресурси лампи |
| Інтерфейс не видно | Запускай level_cards через F6; перевір Visible і Custom anchors панелі |

Змінюй одну річ за раз і повторюй відповідну перевірку. Цей урок не змінює твій робочий скрипт автоматично: ти дописуєш його за кроками й бачиш результат кожної частини після складання.
