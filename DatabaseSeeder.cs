using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace BDP_MVVM
{
    // Класс для заполнения базы данных тестовыми данными
    // Создаёт 50 записей в каждой таблице с реалистичными данными
    public static class DatabaseSeeder
    {
        #region Public Methods
        // Заполнить базу данных тестовыми данными
        // Создаёт 50 тегов, 50 платформ, 50 контестов, 4 пользователей, 50 задач и все связи между ними
        // Проверяет, не заполнена ли БД уже, чтобы избежать дублирования данных
        // <param name="connectionString">Строка подключения к базе данных SQLite</param>
        public static void SeedDatabase(string connectionString)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    // Проверяем заполнена ли уже БД
                    string checkQuery = "SELECT COUNT(*) FROM Tags";
                    using (var cmd = new SQLiteCommand(checkQuery, connection))
                    {
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        if (count > 0)
                        {
                            System.Diagnostics.Debug.WriteLine("БД уже заполнена!");
                            return;
                        }
                    }
                    System.Diagnostics.Debug.WriteLine("Начинаем заполнение БД...");
                    // 1. Теги
                    SeedTags(connection);
                    System.Diagnostics.Debug.WriteLine("✓ Добавлено 50 тегов");
                    // 2. Платформы
                    SeedPlatforms(connection);
                    System.Diagnostics.Debug.WriteLine("✓ Добавлено 50 платформ");
                    // 3. Контесты
                    SeedContests(connection);
                    System.Diagnostics.Debug.WriteLine("✓ Добавлено 50 контестов");
                    // 4. Пользователи
                    SeedUsers(connection);
                    System.Diagnostics.Debug.WriteLine("✓ Добавлено 4 пользователя");
                    // 5. Задачи
                    SeedTasks(connection);
                    System.Diagnostics.Debug.WriteLine("✓ Добавлено 50 задач");
                    // 6. Связи
                    SeedTaskTags(connection);
                    SeedTaskPlatforms(connection);
                    SeedTaskContests(connection);
                    System.Diagnostics.Debug.WriteLine("✓ Добавлены все связи");
                    System.Diagnostics.Debug.WriteLine("✅ База данных успешно заполнена!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка заполнения БД: {ex.Message}");
                throw;
            }
        }
        #endregion
        #region Private Methods - Core Data
        // Добавить 50 тегов с названиями алгоритмов и структур данных
        private static void SeedTags(SQLiteConnection connection)
        {
            string[] tags = {
                "Динамическое программирование", "Жадные алгоритмы", "Бинарный поиск", "Графы", "Деревья",
                "Сортировки", "Хеш-таблицы", "Двоичный поиск", "Рекурсия", "Битовые операции",
                "Математика", "Геометрия", "Строки", "Поиск в глубину", "Поиск в ширину",
                "Кратчайшие пути", "Минимальное остовное дерево", "Топологическая сортировка",
                "Сильно связные компоненты", "Двудольные графы", "Потоки в сетях", "Паросочетания",
                "Префиксные суммы", "Разреженная таблица", "Дерево отрезков", "Дерево Фенвика",
                "Декартово дерево", "Система непересекающихся множеств", "Алгоритм Мо", "Тернарный поиск",
                "Числа Фибоначчи", "НОД и НОК", "Модульная арифметика", "Быстрое возведение в степень",
                "Решето Эратосфена", "Теория чисел", "Комбинаторика", "Вероятность", "Игры",
                "Конструктивные задачи", "Интерактивные задачи", "Имплементация", "Симуляция",
                "Перебор", "Два указателя", "Скользящее окно", "Стек", "Очередь",
                "Приоритетная очередь", "Куча"
            };
            foreach (var tag in tags)
            {
                string query = "INSERT INTO Tags (Название) VALUES (@name)";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@name", tag);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        // Добавить 50 платформ для автопроверки задач
        // Включает популярные платформы: Codeforces, AtCoder, LeetCode и другие
        private static void SeedPlatforms(SQLiteConnection connection)
        {
            var platforms = new (string name, int auto)[] {
                ("Codeforces", 1), ("AtCoder", 1), ("Yandex Contest", 1), ("LeetCode", 1), ("HackerRank", 1),
                ("TopCoder", 1), ("CodeChef", 1), ("SPOJ", 0), ("Timus Online Judge", 0), ("UVa Online Judge", 0),
                ("E-Olymp", 1), ("Informatics", 1), ("acmp.ru", 1), ("Stepik", 1), ("Coursera", 0),
                ("edX", 0), ("Kaggle", 0), ("Project Euler", 0), ("Rosalind", 0), ("HackerEarth", 1),
                ("Codewars", 1), ("Exercism", 0), ("CodinGame", 1), ("CheckiO", 1), ("DMOJ", 1),
                ("Kattis", 1), ("USACO", 1), ("CSES", 0), ("CP-Algorithms", 0), ("GeeksforGeeks", 0),
                ("InterviewBit", 1), ("AlgoExpert", 1), ("CodeSignal", 1), ("Binarysearch", 1), ("LightOJ", 0),
                ("ICPC Live Archive", 0), ("POJ", 0), ("HDU Online Judge", 0), ("CSAcademy", 1), ("oj.uz", 0),
                ("Baekjoon", 1), ("Google Kickstart", 0), ("Facebook Hacker Cup", 0), ("Topcoder SRM", 1),
                ("Russian Code Cup", 0), ("VK Cup", 0), ("Яндекс.Алгоритм", 0), ("Mail.ru Cup", 0),
                ("Технокубок", 0), ("Открытая олимпиада школьников", 0)
            };
            foreach (var platform in platforms)
            {
                string name = platform.name;
                int auto = platform.auto;
                string query = "INSERT INTO Platform (Название, Автопроверка_готовности) VALUES (@name, @auto)";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@auto", auto);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        // Добавить 50 контестов с названиями реальных соревнований
        // Включает Codeforces Rounds, AtCoder Contests, Google Code Jam и другие
        private static void SeedContests(SQLiteConnection connection)
        {
            var contests = new (string name, int year)[] {
                ("Codeforces Round #800", 2023), ("AtCoder Beginner Contest 300", 2023), ("Google Code Jam 2023", 2023),
                ("Facebook Hacker Cup 2023", 2023), ("Яндекс.Алгоритм 2023", 2023), ("VK Cup 2023", 2023),
                ("Russian Code Cup 2023", 2023), ("Технокубок 2023", 2023), ("ICPC World Finals 2023", 2023),
                ("IOI 2023", 2023), ("Educational Codeforces Round 150", 2023), ("Codeforces Global Round 25", 2023),
                ("AtCoder Grand Contest 60", 2023), ("LeetCode Weekly Contest 350", 2023), ("HackerRank Week of Code 40", 2022),
                ("TopCoder Open 2023", 2023), ("CodeChef SnackDown 2023", 2023), ("Google Kickstart Round A 2023", 2023),
                ("USACO December 2023", 2023), ("Baltic Olympiad 2023", 2023), ("Central European Olympiad 2023", 2023),
                ("Asia-Pacific Informatics Olympiad 2023", 2023), ("Balkan Olympiad 2023", 2023),
                ("Nordic Collegiate Programming Contest 2023", 2023), ("South East European Contest 2023", 2023),
                ("Bubble Cup 15", 2023), ("Helvetic Coding Contest 2023", 2023), ("Moscow Team Olympiad 2023", 2023),
                ("All-Russian Olympiad 2023", 2023), ("Moscow Olympiad 2023", 2023), ("Saint Petersburg Olympiad 2023", 2023),
                ("Innopolis Open 2023", 2023), ("HighLoad Cup 2023", 2023), ("CodeIT 2023", 2023),
                ("RuCode 2023", 2023), ("Tinkoff Challenge 2023", 2023), ("Huawei Honor Cup 2023", 2023),
                ("Ozon Tech Challenge 2023", 2023), ("Avito Cool Challenge 2023", 2023), ("Raiffeisen Bank Contest 2023", 2023),
                ("Sberbank Code Challenge 2023", 2023), ("Mail.ru Cup 2023", 2023), ("VK Internship Contest 2023", 2023),
                ("Wildberries Tech Cup 2023", 2023), ("Kaspersky Cybersecurity Cup 2023", 2023),
                ("Acronis Developer Cup 2023", 2023), ("JetBrains Contest 2023", 2023), ("Samsung IT Cup 2023", 2023),
                ("MTS Contest 2023", 2023), ("Megafon Code Battle 2023", 2023)
            };
            foreach (var contest in contests)
            {
                string name = contest.name;
                int year = contest.year;
                string query = "INSERT INTO Contest (Название, Год_создания) VALUES (@name, @year)";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@year", year);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        // Добавить 4 тестовых пользователя с разными ролями
        // Пароль для всех: "admin" (хеш: 8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918)
        private static void SeedUsers(SQLiteConnection connection)
        {
            var users = new (string login, string email, string desc, int role)[] {
                ("editor1", "editor1@test.com", "Редактор задач", 2),
                ("user_ivan", "ivan@test.com", "Студент МГУ", 2),
                ("user_maria", "maria@test.com", "Преподаватель", 1),
                ("guest1", "guest@test.com", "Гостевой аккаунт", 3)
            };
            string hash = "8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918";
            foreach (var user in users)
            {
                string login = user.login;
                string email = user.email;
                string desc = user.desc;
                int role = user.role;
                string query = "INSERT INTO Users (Логин, Пароль_hash, Email, Описание, Role_ID) VALUES (@login, @hash, @email, @desc, @role)";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@login", login);
                    cmd.Parameters.AddWithValue("@hash", hash);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@desc", desc);
                    cmd.Parameters.AddWithValue("@role", role);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        // Добавить 50 задач с разными уровнями сложности (1-10)
        // Включает классические алгоритмические задачи со ссылками на Polygon
        private static void SeedTasks(SQLiteConnection connection)
        {
            var tasks = new (string name, int author, string brief, string idea, string note, int diff, string link)[] {
                ("Сумма двух чисел", 1, "Дано два числа A и B", "Использовать сложение", "Базовая задача", 1, "https://polygon.codeforces.com/p123456"),
                ("Максимум в массиве", 2, "Найти максимум", "Пройти по массиву", null, 2, "https://polygon.codeforces.com/p123457"),
                ("Бинарный поиск", 3, "Найти элемент", "Бинарный поиск", "O(log N)", 3, "https://polygon.codeforces.com/p123458"),
                ("DFS графа", 4, "Обойти граф", "DFS", null, 4, "https://polygon.codeforces.com/p123459"),
                ("Кратчайший путь", 5, "Найти путь", "Дейкстра", "Взвешенный", 5, "https://polygon.codeforces.com/p123460"),
                ("Рюкзак", 1, "Задача о рюкзаке", "DP", null, 6, "https://polygon.codeforces.com/p123461"),
                ("LCS", 2, "Общая подпоследовательность", "DP", null, 5, "https://polygon.codeforces.com/p123462"),
                ("Сортировка слиянием", 3, "Отсортировать", "Merge sort", null, 4, "https://polygon.codeforces.com/p123463"),
                ("MST", 4, "Остовное дерево", "Краскал", null, 6, "https://polygon.codeforces.com/p123464"),
                ("Проверка простоты", 5, "Простое число", "sqrt(N)", "N до 10^9", 4, "https://polygon.codeforces.com/p123465"),
                ("Быстрое возведение", 1, "A^B mod M", "Бинарное", null, 5, "https://polygon.codeforces.com/p123466"),
                ("Segment Tree", 2, "Запросы на отрезке", "Дерево отрезков", "RMQ", 7, "https://polygon.codeforces.com/p123467"),
                ("Топосорт", 3, "Топологический порядок", "DFS", null, 5, "https://polygon.codeforces.com/p123468"),
                ("SCC", 4, "Сильные компоненты", "Косарайю", null, 7, "https://polygon.codeforces.com/p123469"),
                ("DSU", 5, "Union Find", "DSU", null, 6, "https://polygon.codeforces.com/p123470"),
                ("Fenwick Tree", 1, "Сумма на отрезке", "BIT", "O(log N)", 6, "https://polygon.codeforces.com/p123471"),
                ("Префиксные суммы", 2, "Сумма", "Префиксы", "Базовая", 3, "https://polygon.codeforces.com/p123472"),
                ("Two Pointers", 3, "Два указателя", "Техника", null, 3, "https://polygon.codeforces.com/p123473"),
                ("Sliding Window", 4, "Окно", "Deque", null, 5, "https://polygon.codeforces.com/p123474"),
                ("Жадник расписания", 5, "Отрезки", "Сортировка", "Классика", 4, "https://polygon.codeforces.com/p123475"),
                ("BFS", 1, "Поиск в ширину", "BFS", null, 4, "https://polygon.codeforces.com/p123476"),
                ("Z-функция", 2, "Подстрока", "Z-алгоритм", null, 6, "https://polygon.codeforces.com/p123477"),
                ("Хеш строки", 3, "Сравнение", "Хеш", "Коллизии", 5, "https://polygon.codeforces.com/p123478"),
                ("Treap", 4, "Декартово дерево", "Treap", "Сложная", 8, "https://polygon.codeforces.com/p123479"),
                ("Sparse Table", 5, "RMQ", "Таблица", null, 6, "https://polygon.codeforces.com/p123480"),
                ("Mo Algorithm", 1, "Запросы офлайн", "Mo", null, 7, "https://polygon.codeforces.com/p123481"),
                ("НОД НОК", 2, "НОД и НОК", "Евклид", "Базовая", 2, "https://polygon.codeforces.com/p123482"),
                ("Решето", 3, "Простые числа", "Решето", "N до 10^7", 4, "https://polygon.codeforces.com/p123483"),
                ("Модульная арифметика", 4, "Mod", "Ферма", null, 5, "https://polygon.codeforces.com/p123484"),
                ("Фибоначчи", 5, "N-ое число", "Матрица", "N до 10^18", 6, "https://polygon.codeforces.com/p123485"),
                ("Паросочетание", 1, "Matching", "Кун", null, 7, "https://polygon.codeforces.com/p123486"),
                ("Потоки", 2, "Максимальный поток", "Диниц", "Взвешенный", 8, "https://polygon.codeforces.com/p123487"),
                ("Вершинная крышка", 3, "Минимальная крышка", "Кёниг", null, 7, "https://polygon.codeforces.com/p123488"),
                ("Эйлеров цикл", 4, "Эйлер", "Флёри", null, 6, "https://polygon.codeforces.com/p123489"),
                ("Гамильтон", 5, "Гамильтонов путь", "DP", "Малое N", 9, "https://polygon.codeforces.com/p123490"),
                ("Раскраска графа", 1, "Раскрасить", "Жадник", null, 7, "https://polygon.codeforces.com/p123491"),
                ("Изоморфизм деревьев", 2, "Изоморфны", "Хеш", null, 8, "https://polygon.codeforces.com/p123492"),
                ("Центр дерева", 3, "Центр", "Удаление листьев", null, 5, "https://polygon.codeforces.com/p123493"),
                ("LCA", 4, "Общий предок", "Binary lifting", "O(log N)", 6, "https://polygon.codeforces.com/p123494"),
                ("HLD", 5, "Heavy-Light", "HLD", "Сложная", 9, "https://polygon.codeforces.com/p123495"),
                ("Центроид", 1, "Центроидная декомпозиция", "Рекурсия", null, 9, "https://polygon.codeforces.com/p123496"),
                ("TSP", 2, "Коммивояжёр", "DP по подмножествам", "NP", 10, "https://polygon.codeforces.com/p123497"),
                ("Convex Hull", 3, "Выпуклая оболочка", "Грэхем", null, 6, "https://polygon.codeforces.com/p123498"),
                ("Пересечение", 4, "Отрезки", "Векторное", "Геометрия", 5, "https://polygon.codeforces.com/p123499"),
                ("Площадь", 5, "Многоугольник", "Гаусс", null, 4, "https://polygon.codeforces.com/p123500"),
                ("Триангуляция", 1, "Разбить", "DP", null, 7, "https://polygon.codeforces.com/p123501"),
                ("Closest Pair", 2, "Ближайшая пара", "Разделяй", "O(N log N)", 7, "https://polygon.codeforces.com/p123502"),
                ("Иосиф", 3, "Задача Иосифа", "Рекурр", null, 4, "https://polygon.codeforces.com/p123503"),
                ("Инверсии", 4, "Подсчёт инверсий", "Merge", null, 5, "https://polygon.codeforces.com/p123504"),
                ("LIS", 5, "Подпоследовательность", "DP", "LIS", 6, "https://polygon.codeforces.com/p123505")
            };
            foreach (var task in tasks)
            {
                string name = task.name;
                int author = task.author;
                string brief = task.brief;
                string idea = task.idea;
                string note = task.note;
                int diff = task.diff;
                string link = task.link;
                string query = @"INSERT INTO Task (Название, Автор_ID, Краткое_условие, Идея_решения, Примечание, Сложность, Ссылка_polygon) 
                    VALUES (@name, @author, @brief, @idea, @note, @diff, @link)";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@author", author);
                    cmd.Parameters.AddWithValue("@brief", brief ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@idea", idea ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@note", note ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@diff", diff);
                    cmd.Parameters.AddWithValue("@link", link ?? (object)DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        #endregion
        #region Private Methods - Relationships
        // Добавить связи задач с тегами (many-to-many)
        // Каждая задача получает от 2 до 4 подходящих тегов
        private static void SeedTaskTags(SQLiteConnection connection)
        {
            var taskTagMapping = new Dictionary<int, int[]>
            {
                {1, new[] {42, 44, 11}}, {2, new[] {42, 45, 6}}, {3, new[] {3, 8, 1}}, {4, new[] {4, 14, 9}},
                {5, new[] {4, 16, 1}}, {6, new[] {1, 11, 37}}, {7, new[] {1, 13}}, {8, new[] {6, 9, 42}},
                {9, new[] {4, 17, 2}}, {10, new[] {11, 36, 35}}, {11, new[] {11, 33, 34}}, {12, new[] {25, 1, 5}},
                {13, new[] {4, 18, 14}}, {14, new[] {4, 19, 14}}, {15, new[] {4, 28}}, {16, new[] {26, 1}},
                {17, new[] {23, 1, 11}}, {18, new[] {45, 2, 6}}, {19, new[] {46, 48, 47}}, {20, new[] {2, 6}},
                {21, new[] {4, 15, 48}}, {22, new[] {13, 7, 42}}, {23, new[] {13, 7}}, {24, new[] {5, 27, 9}},
                {25, new[] {24, 1, 5}}, {26, new[] {29, 1}}, {27, new[] {11, 32}}, {28, new[] {11, 35, 36}},
                {29, new[] {11, 33}}, {30, new[] {11, 31, 34, 1}}, {31, new[] {4, 22, 20}}, {32, new[] {4, 21, 20}},
                {33, new[] {4, 20, 22}}, {34, new[] {4, 11, 9}}, {35, new[] {4, 1, 44, 10}}, {36, new[] {4, 2, 44}},
                {37, new[] {5, 7, 9}}, {38, new[] {5, 4, 15}}, {39, new[] {5, 24, 9}}, {40, new[] {5, 25, 1, 14}},
                {41, new[] {5, 9, 4}}, {42, new[] {4, 1, 44, 10}}, {43, new[] {12, 11, 2}}, {44, new[] {12, 11}},
                {45, new[] {12, 11}}, {46, new[] {12, 1, 11}}, {47, new[] {12, 11, 6, 9}}, {48, new[] {11, 9, 1}},
                {49, new[] {6, 1, 9}}, {50, new[] {1, 3, 45}}
            };
            foreach (var kvp in taskTagMapping)
            {
                int taskId = kvp.Key;
                int[] tagIds = kvp.Value;
                foreach (var tagId in tagIds)
                {
                    string query = "INSERT INTO Task_Tag (Task_ID, Tag_ID) VALUES (@taskId, @tagId)";
                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@taskId", taskId);
                        cmd.Parameters.AddWithValue("@tagId", tagId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
        // Добавить связи задач с платформами (many-to-many)
        // Каждая задача привязывается к Codeforces (готова) и случайной платформе 2-5 (частично готова)
        private static void SeedTaskPlatforms(SQLiteConnection connection)
        {
            var random = new Random(42);
            for (int taskId = 1; taskId <= 50; taskId++)
            {
                int platform1 = 1;
                int platform2 = random.Next(2, 6);
                string query = "INSERT INTO Task_Platform (Task_ID, Platform_ID, Готовность) VALUES (@taskId, @platformId, @ready)";
                // Добавляем первую платформу (Codeforces - всегда готова)
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@taskId", taskId);
                    cmd.Parameters.AddWithValue("@platformId", platform1);
                    cmd.Parameters.AddWithValue("@ready", 1);
                    cmd.ExecuteNonQuery();
                }
                // Добавляем вторую платформу (готовность чередуется)
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@taskId", taskId);
                    cmd.Parameters.AddWithValue("@platformId", platform2);
                    cmd.Parameters.AddWithValue("@ready", taskId % 2);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        // Добавить связи задач с контестами (many-to-many)
        // Каждая задача привязывается к контесту циклически (1-20)
        private static void SeedTaskContests(SQLiteConnection connection)
        {
            for (int taskId = 1; taskId <= 50; taskId++)
            {
                int contestId = (taskId % 20) + 1;
                string query = "INSERT INTO Task_Contest (Task_ID, Contest_ID) VALUES (@taskId, @contestId)";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@taskId", taskId);
                    cmd.Parameters.AddWithValue("@contestId", contestId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        #endregion
    }
}