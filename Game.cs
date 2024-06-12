using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

// Клас танку гравця
public class Tank // Taнк гравця
{
    // Властивості для позиції та розміру танка
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; } = 50; // Ширина танка за замовчуванням
    public int Height { get; set; } = 50; // Висота танка за замовчуванням
    public int Angle { get; set; } = 90; // Кут повороту танка за замовчуванням
    public Image TankImage { get; set; } // Зображення танка

    // Константи для обмежень руху танка
    protected const int MinX = 0;
    protected const int MaxX = 800;
    protected const int MinY = 0;
    protected const int MaxY = 450;

    // Конструктор, що завантажує зображення танка
    public Tank(string imagePath)
    {
        TankImage = Image.FromFile(imagePath);
    }

    // Метод для відображення танка на екрані
    public void Draw(Graphics g)
    {
        var state = g.Save(); // Збереження стану графічного об'єкта
        g.TranslateTransform(X + Width / 2, Y + Height / 2); // Перенос координат в центр танка
        g.RotateTransform(Angle); // Поворот танка на заданий кут
        g.DrawImage(TankImage, -Width / 2, -Height / 2, Width, Height); // Малювання танка
        g.Restore(state); // Відновлення стану графічного об'єкта
    }

    // Метод для руху танка вверх
    public void MoveUp(List<Wall> walls)
    {
        if (Y - 5 >= MinY && !CheckCollision(X, Y - 5, walls))
        {
            Y -= 5;
            Angle = 90; // Поворот танка на 90 градусів (вверх)
        }
    }

    // Метод для руху танка вниз
    public void MoveDown(List<Wall> walls)
    {
        if (Y + 5 + Height <= MaxY && !CheckCollision(X, Y + 5, walls))
        {
            Y += 5;
            Angle = 270; // Поворот танка на 270 градусів (вниз)
        }
    }

    // Метод для руху танка вправо
    public void MoveRight(List<Wall> walls)
    {
        if (X + 5 + Width <= MaxX && !CheckCollision(X + 5, Y, walls))
        {
            X += 5;
            Angle = 180; // Поворот танка на 180 градусів (вправо)
        }
    }

    // Метод для руху танка вліво
    public void MoveLeft(List<Wall> walls)
    {
        if (X - 5 >= MinX && !CheckCollision(X - 5, Y, walls))
        {
            X -= 5;
            Angle = 0; // Поворот танка на 0 градусів (вліво)
        }
    }

    // Метод для перевірки зіткнення з стінами
    public bool CheckCollision(int newX, int newY, List<Wall> walls)
    {
        Rectangle tankRect = new Rectangle(newX, newY, Width, Height); // Прямокутник, що представляє танк

        foreach (var wall in walls)
        {
            if (wall.IsDestroyed) continue; // Пропустити знищені стіни

            Rectangle wallRect = new Rectangle(wall.X * 50, wall.Y * 50, 50, 50); // Прямокутник, що представляє стіну

            if (tankRect.IntersectsWith(wallRect))
            {
                return true; // Якщо є зіткнення, повернути true
            }
        }

        return false; // Якщо зіткнень немає, повернути false
    }

    // Метод для отримання позиції, де буде створений снаряд
    public Point GetProjectileSpawnPosition()
    {
        int offset = 30; // Зміщення для створення снаряда поза танком
        int spawnX = X + Width / 2;
        int spawnY = Y + Height / 2;

        // Встановлення позиції в залежності від кута повороту танка
        switch (Angle)
        {
            case 0: // Поворот вправо
                spawnX = X + Width - offset - 40;
                break;
            case 90: // Поворот вверх
                spawnY = Y - offset;
                break;
            case 180: // Поворот вліво
                spawnX = X + offset + 40;
                break;
            case 270: // Поворот вниз
                spawnY = Y + Height + offset;
                break;
        }

        return new Point(spawnX, spawnY); // Повернення координат для створення снаряда
    }
}

// Клас ворожого танку
public class EnemyTank : Tank
{
    private int vx; // Швидкість в напрямку X
    private int vy; // Швидкість в напрямку Y
    private const int MoveSpeed = 1; // Швидкість руху (можна змінити за потреби)
    private const int UpdateInterval = 30; // Інтервал оновлення в мілісекундах
    private Map map; // Карта гри
    private List<Projectile> projectiles; // Список снарядів
    private Image projectileImage; // Зображення снаряда
    private Random random; // Генератор випадкових чисел
    private Battle_City.Game gameForm; // Посилання на екземпляр форми гри

    public EnemyTank(string imagePath, int x, int y, Map map, List<Projectile> projectiles, Image projectileImage, Battle_City.Game form)
        : base(imagePath)
    {
        X = x;
        Y = y;
        this.map = map;
        this.projectiles = projectiles;
        this.projectileImage = projectileImage;
        random = new Random();
        gameForm = form; // Зберігання посилання на екземпляр форми

        // Ініціалізація швидкості
        vx = 0;
        vy = 0;

        // Таймер для стрільби
        System.Windows.Forms.Timer shootTimer = new System.Windows.Forms.Timer();
        shootTimer.Interval = 5000; // Інтервал стрільби (можна змінити за потреби)
        shootTimer.Tick += ShootTimer_Tick;
        shootTimer.Start();

        // Таймер для руху
        System.Windows.Forms.Timer moveTimer = new System.Windows.Forms.Timer();
        moveTimer.Interval = UpdateInterval;
        moveTimer.Tick += MoveTimer_Tick;
        moveTimer.Start();
    }

    // Метод, який виконується при кожному тіку таймера руху
    private void MoveTimer_Tick(object sender, EventArgs e)
    {
        if (!gameForm.gamePaused) // Перевірка на паузу гри
        {
            // Оновлення позиції танка на основі швидкості
            X += vx;
            Y += vy;

            // Перевірка на зіткнення зі стінами
            if (CheckWallCollision())
            {
                // Зміна напрямку руху для уникнення виходу за межі
                vx = -vx;
                vy = -vy;
                UpdateAngleFromVelocity();
            }

            // Випадковий вибір нового напрямку руху
            if (random.Next(1000) < 5)
            {
                ChooseRandomDirection();
            }
        }
    }

    // Метод для перевірки зіткнення зі стінами
    private bool CheckWallCollision()
    {
        bool collisionOccurred = false;

        // Перевірка на зіткнення з межами карти
        if (X < 0 || X + Width > map.Width || Y < 0 || Y + Height > map.Height)
        {
            collisionOccurred = true;
        }

        // Перевірка на зіткнення зі стінами
        Rectangle tankRect = new Rectangle(X, Y, Width, Height);
        foreach (var wall in map.Walls)
        {
            if (wall.IsDestroyed) continue; // Пропустити знищені стіни

            Rectangle wallRect = new Rectangle(wall.X * 50, wall.Y * 50, 50, 50);
            if (tankRect.IntersectsWith(wallRect))
            {
                collisionOccurred = true;
                break;
            }
        }

        return collisionOccurred;
    }

    // Метод для оновлення кута повороту танка на основі поточної швидкості
    private void UpdateAngleFromVelocity()
    {
        if (vx == 0 && vy < 0)
        {
            Angle = 90; // Вверх
        }
        else if (vx == 0 && vy > 0)
        {
            Angle = 270; // Вниз
        }
        else if (vx < 0 && vy == 0)
        {
            Angle = 0; // Вліво
        }
        else if (vx > 0 && vy == 0)
        {
            Angle = 180; // Вправо
        }
    }

    // Метод для вибору випадкового напрямку руху
    private void ChooseRandomDirection()
    {
        // Вибір нового випадкового напрямку і оновлення швидкості
        int direction = random.Next(4);
        switch (direction)
        {
            case 0: // Вверх
                vx = 0;
                vy = -MoveSpeed;
                Angle = 90;
                break;
            case 1: // Вниз
                vx = 0;
                vy = MoveSpeed;
                Angle = 270;
                break;
            case 2: // Вліво
                vx = -MoveSpeed;
                vy = 0;
                Angle = 0;
                break;
            case 3: // Вправо
                vx = MoveSpeed;
                vy = 0;
                Angle = 180;
                break;
        }
    }

    // Метод, який виконується при кожному тіку таймера стрільби
    private void ShootTimer_Tick(object sender, EventArgs e)
    {
        if (!gameForm.gamePaused) // Перевірка на паузу гри
        {
            Point spawnPosition = GetProjectileSpawnPosition(); // Отримання позиції для створення снаряда
            int adjustedAngle = Angle; // Початковий кут снаряда встановлений на кут танка

            // Якщо танк спрямований вліво або вправо, коригуємо кут снаряда
            if (Angle == 0 || Angle == 180)
            {
                if (Angle == 0)
                {
                    adjustedAngle = 180; // Снаряд повинен рухатися вліво
                }
                else
                {
                    adjustedAngle = 0; // Снаряд повинен рухатися вправо
                }
            }

            var projectile = new Projectile(spawnPosition.X, spawnPosition.Y, adjustedAngle, projectileImage);
            projectiles.Add(projectile); // Додавання снаряда в список снарядів
        }
    }
}

// Клас снаряда
public class Projectile
{
    // Властивості снаряда
    public int X { get; set; } // Позиція X
    public int Y { get; set; } // Позиція Y
    public int Width { get; set; } // Ширина
    public int Height { get; set; } // Висота
    public int Speed { get; set; } = 10; // Швидкість снаряда
    public Image ProjectileImage { get; set; } // Зображення снаряда
    public int Angle { get; set; } // Кут руху снаряда

    // Конструктор снаряда
    public Projectile(int x, int y, int angle, Image image)
    {
        X = x;
        Y = y;
        Angle = angle;
        ProjectileImage = ScaleImage(image, 20, 20); // Масштабування зображення до розміру 20x20
        Width = ProjectileImage.Width; // Встановлення ширини
        Height = ProjectileImage.Height; // Встановлення висоти
    }

    // Метод для масштабування зображення
    private Image ScaleImage(Image image, int width, int height)
    {
        Bitmap scaledImage = new Bitmap(width, height); // Створення нового зображення заданого розміру
        using (Graphics g = Graphics.FromImage(scaledImage))
        {
            g.DrawImage(image, 0, 0, width, height); // Малювання масштабованого зображення
        }
        return scaledImage; // Повернення масштабованого зображення
    }

    // Метод для руху снаряда
    public void Move()
    {
        double radians = Math.PI * Angle / 180; // Конвертація кута в радіани
        X += (int)(Speed * Math.Cos(radians)); // Оновлення позиції X на основі швидкості та кута
        Y -= (int)(Speed * Math.Sin(radians)); // Оновлення позиції Y (координата Y інвертована в більшості графічних систем)
    }

    // Метод для відображення снаряда на екрані
    public void Draw(Graphics g)
    {
        g.DrawImage(ProjectileImage, X, Y); // Малювання зображення снаряда
    }

    // Метод для перевірки зіткнення зі стінами
    public bool CheckWallCollision(List<Wall> walls, List<Projectile> projectiles)
    {
        Rectangle projectileRect = new Rectangle(X, Y, Width, Height); // Прямокутник, що представляє снаряд

        foreach (var wall in walls)
        {
            if (wall.IsDestroyed) continue; // Пропустити знищені стіни

            Rectangle wallRect = new Rectangle(wall.X * 50, wall.Y * 50, 50, 50); // Прямокутник, що представляє стіну
            if (projectileRect.IntersectsWith(wallRect))
            {
                if (wall.Destructible)
                {
                    projectiles.Remove(this); // Видалення снаряда зі списку
                    wall.IsDestroyed = true; // Позначення стіни як знищеної
                    return true;
                }
                else
                {
                    projectiles.Remove(this); // Видалення снаряда зі списку
                }
            }
        }
        return false; // Повернення false, якщо зіткнень немає
    }

    // Метод для перевірки зіткнення з танком
    public bool CheckTankCollision(Tank tank)
    {
        Rectangle projectileRect = new Rectangle(X, Y, Width, Height); // Прямокутник, що представляє снаряд
        Rectangle tankRect = new Rectangle(tank.X, tank.Y, tank.Width, tank.Height); // Прямокутник, що представляє танк
        return projectileRect.IntersectsWith(tankRect); // Перевірка на зіткнення
    }
}

// Перерахування типів місцевості
public enum TerrainType
{
    Grass, 
    Water  
}

// Клас стінок
public class Wall
{
    // Властивості стіни
    public int X { get; set; } // Позиція X стіни
    public int Y { get; set; } // Позиція Y стіни
    public bool Destructible { get; set; } // Чи можна знищити стіну
    public bool IsDestroyed { get; set; } // Чи знищена стіна

    // Конструктор для ініціалізації властивостей стіни
    public Wall(int x, int y, bool destructible)
    {
        X = x; // Встановлення позиції X
        Y = y; // Встановлення позиції Y
        Destructible = destructible; // Встановлення, чи стіна руйнується
        IsDestroyed = false; // Ініціалізація стіни як незруйнованої
    }
}

// Клас карти
public class Map
{
    // Властивості карти
    public int Width { get; } // Ширина карти
    public int Height { get; } // Висота карти
    public TerrainType[,] Tiles { get; } // Двовимірний масив, що представляє типи місцевості
    public List<Wall> Walls { get; } // Список стін на карті

    // Конструктор карти
    public Map(int width, int height)
    {
        Width = width; // Встановлення ширини
        Height = height; // Встановлення висоти
        Tiles = new TerrainType[width / 50, height / 50]; // Ініціалізація масиву типів місцевості
        Walls = new List<Wall>(); // Ініціалізація списку стін

        Random rand = new Random(); // Об'єкт для генерації випадкових чисел

        // Заповнення масиву типів місцевості значеннями "Grass" (трава)
        for (int x = 0; x < Tiles.GetLength(0); x++)
        {
            for (int y = 0; y < Tiles.GetLength(1); y++)
            {
                Tiles[x, y] = TerrainType.Grass; // Установка типу місцевості на "Grass"
            }
        }

        // Додавання випадкових стін на карту
        for (int i = 0; i < Tiles.GetLength(0) * Tiles.GetLength(1) / 13; i++)
        {
            int x, y;
            do
            {
                x = rand.Next(Tiles.GetLength(0)); // Випадкове значення X
                y = rand.Next(Tiles.GetLength(1)); // Випадкове значення Y
            } while (Walls.Any(w => w.X == x && w.Y == y)); // Перевірка, щоб уникнути накладення стін

            bool destructible = rand.Next(2) == 0; // Випадкове визначення, чи стіна руйнується
            Walls.Add(new Wall(x, y, destructible)); // Додавання нової стіни до списку
        }
    }
}

namespace Battle_City
{
    // Частковий клас, який описує основну гру
    public partial class Game : Form
    {
        private Tank tank; // Гравецький танк
        private EnemyTank enemyTank; // Ворожий танк
        private List<Projectile> projectiles = new List<Projectile>(); // Список снарядів
        private Image projectileImage; // Зображення снаряда
        private bool canShoot = true; // Чи може танк стріляти
        private System.Windows.Forms.Timer cooldownTimer = new System.Windows.Forms.Timer(); // Таймер для перезарядки
        private Map map; // Карта гри
        private Image mapImage; // Зображення карти
        private bool gameEnded = false; // Прапорець для перевірки, чи гра закінчена
        private System.Windows.Forms.Timer gameTimer; // Таймер для оновлення гри
        private DateTime startTime; // Час початку гри
        string resultMessage; // Повідомлення про результат гри
        public bool gamePaused = false; // Прапорець для перевірки, чи гра призупинена

        // Конструктор гри
        public Game()
        {
            InitializeComponent(); // Ініціалізація компонентів форми
            this.StartPosition = FormStartPosition.CenterScreen; // Встановлення позиції в центрі екрану
            startTime = DateTime.Now; // Встановлення часу початку гри

            // Ініціалізація гравецького танка
            string tankImagePath = @"E:\University\Term 2\Oop\Battle City\Battle City\Tank.png";
            tank = new Tank(tankImagePath)
            {
                X = 0, // Початкова позиція X
                Y = 0  // Початкова позиція Y
            };

            // Завантаження зображення снаряда
            string projectileImagePath = @"E:\University\Term 2\Oop\Battle City\Battle City\05.png";
            projectileImage = Image.FromFile(projectileImagePath);

            // Ініціалізація карти
            map = new Map(800, 450);

            // Ініціалізація ворожого танка
            string enemyTankImagePath = @"E:\University\Term 2\Oop\Battle CIty\Battle City\Tank2.png";
            enemyTank = new EnemyTank(enemyTankImagePath, 400, 100, map, projectiles, projectileImage, this)
            {
                X = map.Width - 50,   // Нижній правий кут
                Y = map.Height - 50   // Нижній правий кут
            };

            // Попереднє відображення карти на зображенні
            mapImage = new Bitmap(800, 450);
            using (Graphics g = Graphics.FromImage(mapImage))
            {
                RenderMap(g); // Відображення карти
            }

            pictureBox1.Paint += pictureBox1_Paint; // Додавання обробника події Paint для pictureBox1
            this.KeyDown += form1_KeyDown; // Додавання обробника події KeyDown для форми
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 30; // Інтервал таймера
            timer.Tick += Timer_Tick; // Додавання обробника події Tick для таймера
            timer.Start(); // Запуск таймера

            cooldownTimer.Interval = 5000; // Інтервал для таймера перезарядки
            cooldownTimer.Tick += CooldownTimer_Tick; // Додавання обробника події Tick для таймера перезарядки
            cooldownTimer.Start(); // Запуск таймера перезарядки

            // Включення подвоєної буферизації для покращення продуктивності малювання
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
        }

        // Метод, що виконується при кожному тіку таймера перезарядки
        private void CooldownTimer_Tick(object sender, EventArgs e)
        {
            canShoot = true; // Скидання прапорця canShoot, коли таймер перезарядки спрацьовує
        }

        // Метод, що виконується при кожному тіку основного таймера гри
        private void Timer_Tick(object sender, EventArgs e)
        {
            bool wallDestroyed = false; // Прапорець для відстеження, чи була знищена стіна
            List<Projectile> projectilesToRemove = new List<Projectile>(); // Список снарядів для видалення

            // Проходження по всіх снарядах у зворотному порядку
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                var projectile = projectiles[i];
                projectile.Move(); // Переміщення снаряда

                // Перевірка на зіткнення з ворожим танком
                if (projectile.CheckTankCollision(enemyTank))
                {
                    if (!gameEnded)
                    {
                        gameEnded = true; // Встановлення прапорця завершення гри
                        resultMessage = "Victory!"; // Повідомлення про перемогу
                        OnGameEnd(); // Виклик методу завершення гри
                    }
                }

                // Перевірка на зіткнення з танком гравця
                if (projectile.CheckTankCollision(tank))
                {
                    if (!gameEnded)
                    {
                        gameEnded = true; // Встановлення прапорця завершення гри
                        resultMessage = "Defeat!"; // Повідомлення про поразку
                        OnGameEnd(); // Виклик методу завершення гри
                    }
                }

                // Перевірка на зіткнення з стінами
                if (projectile.CheckWallCollision(map.Walls, projectiles))
                {
                    wallDestroyed = true; // Встановлення прапорця, що стіна знищена
                    projectilesToRemove.Add(projectile); // Додавання снаряда до списку для видалення
                }
            }

            // Видалення снарядів, що зіткнулися зі стінами
            foreach (var projectile in projectilesToRemove)
            {
                projectiles.Remove(projectile); // Видалення снаряда зі списку
            }

            // Перемальовування карти, якщо стіна була знищена
            if (wallDestroyed)
            {
                RedrawMap();
            }

            pictureBox1.Invalidate(); // Виклик перерисовки екрану гри
        }

        // Метод для перемальовування карти
        private void RedrawMap()
        {
            using (Graphics g = Graphics.FromImage(mapImage))
            {
                RenderMap(g); // Виклик методу для малювання карти
            }
            pictureBox1.Invalidate(); // Виклик перерисовки pictureBox1
        }

        // Метод для обробки події Paint на pictureBox1
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(mapImage, 0, 0); // Малювання зображення карти
            tank.Draw(e.Graphics); // Малювання танка гравця
            enemyTank.Draw(e.Graphics); // Малювання ворожого танка
            foreach (var projectile in projectiles)
            {
                projectile.Draw(e.Graphics); // Малювання кожного снаряда
            }
        }

        // Метод для обробки події KeyDown
        private void form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
            {
                tank.MoveUp(map.Walls); // Рух танка вгору
            }
            else if (e.KeyCode == Keys.Down)
            {
                tank.MoveDown(map.Walls); // Рух танка вниз
            }
            else if (e.KeyCode == Keys.Left)
            {
                tank.MoveLeft(map.Walls); // Рух танка вліво
            }
            else if (e.KeyCode == Keys.Right)
            {
                tank.MoveRight(map.Walls); // Рух танка вправо
            }
            else if (e.KeyCode == Keys.Space && canShoot)
            {
                Point spawnPosition = tank.GetProjectileSpawnPosition(); // Отримання позиції для спавну снаряда
                int adjustedAngle = tank.Angle; // Початковий кут снаряда

                // Якщо танк дивиться вліво або вправо, корегуємо кут снаряда
                if (tank.Angle == 0 || tank.Angle == 180)
                {
                    if (tank.Angle == 0)
                    {
                        adjustedAngle = 180; // Снаряд повинен рухатись вліво
                    }
                    else
                    {
                        adjustedAngle = 0; // Снаряд повинен рухатись вправо
                    }
                }

                var projectile = new Projectile(spawnPosition.X, spawnPosition.Y, adjustedAngle, projectileImage); // Створення нового снаряда
                projectiles.Add(projectile); // Додавання снаряда до списку

                canShoot = false; // Встановлення прапорця, що стріляти не можна
                cooldownTimer.Start(); // Запуск таймера перезарядки
            }

            pictureBox1.Invalidate(); // Виклик перерисовки pictureBox1
        }

        // Метод для малювання карти
        private void RenderMap(Graphics g)
        {
            // Завантаження зображень типів місцевості 
            Image grassImage = Image.FromFile(@"E:\University\Term 2\Oop\Battle City\Battle City\Grass 001.png");

            // Завантаження зображень стін (знищувана та незнищувана)
            Image destructibleWallImage = Image.FromFile(@"E:\University\Term 2\Oop\Battle City\Battle City\885.jpg");
            Image indestructibleWallImage = Image.FromFile(@"E:\University\Term 2\Oop\Battle City\Battle City\878.jpg");

            int cellSize = 50; // Розмір клітинки, можна змінювати

            for (int x = 0; x < map.Tiles.GetLength(0); x++)
            {
                for (int y = 0; y < map.Tiles.GetLength(1); y++)
                {
                    Rectangle cellRect = new Rectangle(x * cellSize, y * cellSize, cellSize, cellSize); // Прямокутні клітинки
                    switch (map.Tiles[x, y])
                    {
                        case TerrainType.Grass:
                            g.DrawImage(grassImage, cellRect); // Малювання трави
                            break;
                    }
                }
            }

            // Малювання стін
            foreach (var wall in map.Walls)
            {
                if (!wall.IsDestroyed) // Якщо стіна не знищена
                {
                    Rectangle wallRect = new Rectangle(wall.X * cellSize, wall.Y * cellSize, cellSize, cellSize); // Прямокутник стіни
                    if (wall.Destructible)
                        g.DrawImage(destructibleWallImage, wallRect); // Малювання знищуваної стіни
                    else
                        g.DrawImage(indestructibleWallImage, wallRect); // Малювання незнищуваної стіни
                }
            }
        }

        // Метод, який викликається при завершенні гри
        private void OnGameEnd()
        {
            TimeSpan currentTime = DateTime.Now - startTime; // Поточний час гри
            TimeSpan bestTime = BestTimeManager.ReadBestTime(); // Найкращий час гри

            // Форматування часів у форматі "хв:сек"
            string formattedCurrentTime = currentTime.ToString(@"mm\:ss");
            string formattedBestTime = bestTime.ToString(@"mm\:ss");

            // Визначення результату гри
            if (currentTime < bestTime)
            {
                BestTimeManager.WriteBestTime(currentTime); // Запис нового найкращого часу гри
                MessageBox.Show($"{resultMessage}\nNew best time: {formattedCurrentTime}"); // Повідомлення про новий найкращий час
            }
            else
            {
                MessageBox.Show($"{resultMessage}\nYour time: {formattedCurrentTime}\nBest time: {formattedBestTime}"); // Повідомлення про результат гри та час
            }

            // Закриття вікна гри та відображення головного меню
            this.Close();
            MainMenuForm mainMenu = new MainMenuForm();
            mainMenu.Show();
        }

        // Перевизначений метод для обробки клавіш
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape) // Перевірка, чи натиснута клавіша "Escape"
            {
                PauseGame(); // Виклик методу для паузи гри
                return true; // Повернення true для позначення обробки команди
            }
            return base.ProcessCmdKey(ref msg, keyData); // Передача обробки до базового класу, якщо команда не співпадає з клавішею "Escape"
        }

        // Метод для призупинення гри
        private void PauseGame()
        {
            gamePaused = true; // Встановлення флагу паузи гри

            // Показ вікна паузи
            PauseForm pauseForm = new PauseForm();
            pauseForm.StartPosition = FormStartPosition.CenterParent; // Позиціювання вікна паузи по центру
            pauseForm.ShowDialog(this); // Показ вікна паузи та очікування від користувача дії

            // Відновлення гри, якщо користувач обрав продовження
            if (!pauseForm.ResumeGame) // Якщо користувач не хоче продовжувати гру
            {
                this.Close(); // Закриття поточного вікна гри
                MainMenuForm mainMenu = new MainMenuForm(); // Створення головного меню
                mainMenu.Show(); // Відображення головного меню
            }
            else // Якщо користувач обрав продовження гри
            {
                gamePaused = false; // Знову встановлення флагу паузи гри у значення false
            }
        }
    }
}
