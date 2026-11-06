using System.Diagnostics;

namespace GameEventsAnalyzer;

internal class Program
{
    static void Main()
    {
        Console.WriteLine("=== Game Events Analyzer (П1 и П2) ===\n");

        RunControlTests();
        RunEdgeCaseTests();
        RunOrderTests();
        RunBenchmarks();

        Console.WriteLine("\nРабота завершена.");
    }

    static void RunControlTests()
    {
        var testEvents = new List<GameEvent>
        {
            new(1, 1, "Kill", 100, DateTime.Now),
            new(2, 1, "Kill", 150, DateTime.Now),
            new(3, 2, "Kill", 200, DateTime.Now),
            new(4, 1, "Death", 0, DateTime.Now),
            new(5, 2, "Loot", 20, DateTime.Now)
        };

        var linqResult = GameEventAnalyzer.AnalyzeWithLinq(testEvents);
        var plinqResult = GameEventAnalyzer.AnalyzeWithPlinq(testEvents);

        // TODO 5 (П1)
        bool same = linqResult.SequenceEqual(plinqResult);
        Console.WriteLine($"[TODO 5 П1] Результаты LINQ и PLINQ совпадают: {same}");
    }

    static void RunEdgeCaseTests()
    {
        Console.WriteLine("\n--- Граничные условия ---");

        // TODO 6 (П1)
        TestScenario("[TODO 6 П1] Пустая коллекция", new List<GameEvent>());

        // TODO 7 (П1)
        TestScenario("[TODO 7 П1] Нет событий Kill", new List<GameEvent> { new(1, 1, "Death", 0, DateTime.Now) });

        // TODO 8 (П1)
        TestScenario("[TODO 8 П1] Все убийства одного игрока", new List<GameEvent> { new(1, 1, "Kill", 100, DateTime.Now), new(2, 1, "Kill", 50, DateTime.Now) });

        // TODO 9 (П1)
        try { GameEventAnalyzer.AnalyzeWithLinq(null!); }
        catch (ArgumentNullException) { Console.WriteLine("[TODO 9 П1] null: пойман ArgumentNullException (как и должно быть)."); }
    }

    static void TestScenario(string testName, List<GameEvent> events)
    {
        var result = GameEventAnalyzer.AnalyzeWithPlinq(events);
        Console.WriteLine($"{testName,-45} -> элементов: {result.Count}");
    }

    static void RunOrderTests()
    {
        Console.WriteLine("\n--- Проверка порядка ---");
        var events = GameEventGenerator.Generate(10_000, seed: 42);

        var ordered = GameEventAnalyzer.GetKillsOrdered(events);

        // TODO 3 (П2)
        bool isOrdered = ordered.Select(e => e.Id).SequenceEqual(ordered.Select(e => e.Id).OrderBy(id => id));
        Console.WriteLine($"[TODO 3 П2] AsOrdered сохранил исходный порядок ID: {isOrdered}");
    }

    static void RunBenchmarks()
    {
        Console.WriteLine("\n--- Бенчмарки ---");
        int[] sizes = { 10_000, 100_000, 1_000_000 };

        foreach (int size in sizes)
        {
            Console.WriteLine($"\nРазмер: {size:N0}");
            var events = GameEventGenerator.Generate(size, seed: 12345);

            // TODO 9 (П2)
            Measure("[TODO 9 П2] LINQ (Игроки)", () => GameEventAnalyzer.AnalyzeWithLinq(events));
            Measure("[TODO 9 П2] PLINQ (Игроки)", () => GameEventAnalyzer.AnalyzeWithPlinq(events));

            Measure("Доп. LINQ (Типы)", () => GameEventAnalyzer.AnalyzeEventTypesLinq(events));
            Measure("Доп. PLINQ (Типы)", () => GameEventAnalyzer.AnalyzeEventTypesPlinq(events));
        }
    }

    static void Measure(string testName, Action action)
    {
        action(); // Прогрев
        var times = new List<double>();
        for (int i = 0; i < 5; i++)
        {
            var sw = Stopwatch.StartNew();
            action();
            sw.Stop();
            times.Add(sw.Elapsed.TotalMilliseconds);
        }

        var sorted = times.OrderBy(x => x).ToArray();
        int mid = sorted.Length / 2;
        double median = sorted.Length % 2 == 1 ? sorted[mid] : (sorted[mid - 1] + sorted[mid]) / 2.0;

        Console.WriteLine($"  {testName,-25} : {median:F3} мс");
    }
}

/* =========================================================================
 * ОТВЕТЫ НА TODO (заметки от руки)
 * =========================================================================
 * 
 * ===================== ПРАКТИКА 1 =====================
 * 
 * [TODO 1 П1] Реализация AnalyzeWithLinq:
 *   Where(Kill) -> GroupBy(PlayerId) -> Select(статистика).
 * 
 * [TODO 2 П1] Реализация AnalyzeWithPlinq:
 *   То же самое, но с AsParallel() в самом начале цепочки.
 * 
 * [TODO 3 П1] Сортировка по TotalPoints:
 *   .OrderByDescending(x => x.TotalPoints) — от большего к меньшему.
 * 
 * [TODO 4 П1] Стабильный порядок при равных очках:
 *   .ThenBy(x => x.PlayerId) — вторичный ключ.
 * 
 * [TODO 5 П1] Сравнение LINQ и PLINQ:
 *   SequenceEqual работает корректно только благодаря ThenBy.
 *   Без него PLINQ мог бы выдать тот же набор, но в другом порядке.
 * 
 * [TODO 6 П1] Пустая коллекция:
 *   Возвращает пустой список. Ничего не падает.
 * 
 * [TODO 7 П1] Нет событий Kill:
 *   Where отфильтрует всё, GroupBy сработает на пустой коллекции — результат пустой.
 * 
 * [TODO 8 П1] Все убийства одного игрока:
 *   Одна запись в результате со всеми очками этого игрока.
 * 
 * [TODO 9 П1] Передача null:
 *   ArgumentNullException.ThrowIfNull бросает ArgumentNullException.
 *   Это правильная защита от NullReferenceException глубже в коде.
 * 
 * [TODO 10 П1] Почему PLINQ не всегда быстрее?
 *   Оверхед на потоки и слияние результатов часто БОЛЬШЕ, 
 *   чем время самой операции. На мелких данных LINQ всегда выигрывает.
 * 
 * [Побочные эффекты] Почему нельзя писать в List<T> через ForAll?
 *   List<T> не потокобезопасен. Параллельный Add устроит гонку: 
 *   часть элементов потеряется, или программа упадёт.
 *   Правильно — использовать .Select(...).ToList(), он внутри потокобезопасен.
 * 
 * ===================== ПРАКТИКА 2 =====================
 * 
 * [TODO 1 П2] Лента без AsOrdered:
 *   AsParallel().Where(...).ToList() — порядок не гарантирован.
 * 
 * [TODO 2 П2] Лента с AsOrdered:
 *   AsParallel().AsOrdered().Where(...).ToList() — исходный порядок сохранён.
 * 
 * [TODO 3 П2] Проверка сохранения порядка:
 *   Сравниваем последовательность Id с отсортированной версией. 
 *   Если равны — порядок сохранён.
 * 
 * [TODO 4 П2] Сортировка по Timestamp:
 *   .OrderBy(e => e.Timestamp)
 * 
 * [TODO 5 П2] ThenBy по Id:
 *   .ThenBy(e => e.Id) — вторичный ключ для стабильности.
 * 
 * [TODO 6 П2] Убираем AsOrdered из запроса с OrderBy:
 *   Порядок НЕ изменился, потому что OrderBy сам задаёт новый порядок.
 *   AsOrdered тут просто не нужен.
 * 
 * [TODO 7 П2] Генератор с одинаковым временем:
 *   Заменили start.AddSeconds(i) на start.AddSeconds(i / 10).
 *   Каждые 10 событий теперь имеют одинаковый Timestamp.
 * 
 * [TODO 8 П2] Зачем ThenBy по Id?
 *   Если Timestamp совпадает, сортировка хаотична. 
 *   ThenBy(Id) — вторичный ключ, чтобы результат был предсказуем.
 * 
 * [TODO 9 П2] Сравнение времени:
 *   Замеряем медиану из 5 прогонов. На малых данных LINQ быстрее, 
 *   на больших PLINQ начинает выигрывать (но не всегда).
 * 
 * [TODO 10 П2] Почему AsOrdered снижает производительность?
 *   Он заставляет PLINQ буферизировать данные, запоминать исходные индексы 
 *   и потом заново собирать их в правильном порядке. 
 *   Это лишняя трата времени и памяти.
 * 
 * ===================== ДОПОЛНИТЕЛЬНОЕ ЗАДАНИЕ =====================
 * 
 * Почему группировка по типу события (4 типа) не ускоряется в PLINQ?
 *   Ключей группировки всего 4. Создавать и сливать 4 локальные хеш-таблицы 
 *   в каждом потоке дольше, чем просто сделать это один раз. 
 *   Выгода от параллелизма съедается оверхедом на слияние.
 * 
 * ========================================================================= */