namespace GameEventsAnalyzer;

internal class Program
{
    static void Main()
    {
        Console.WriteLine("=== Game Events Analyzer (П1) ===\n");

        // 1. Базовая проверка на контрольных данных
        RunControlTests();

        // 2. Проверка граничных условий (TODO 6, 7, 8, 9)
        RunEdgeCaseTests();
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

        // TODO 5: Сравнение результатов через SequenceEqual
        // Работает корректно только благодаря добавлению .ThenBy(x => x.PlayerId) в оба метода.
        bool same = linqResult.SequenceEqual(plinqResult);

        Console.WriteLine($"Результаты LINQ и PLINQ совпадают: {same}");
        Console.WriteLine($"Топ игрок: ID={linqResult[0].PlayerId}, Убийств={linqResult[0].KillCount}, Очков={linqResult[0].TotalPoints}");
    }

    static void RunEdgeCaseTests()
    {
        Console.WriteLine("\n--- Проверка граничных условий ---");

        // TODO 6: Пустая коллекция
        TestScenario("Пустая коллекция", new List<GameEvent>());

        // TODO 7: Коллекция без событий Kill
        TestScenario("Нет событий Kill", new List<GameEvent>
        {
            new(1, 1, "Death", 0, DateTime.Now),
            new(2, 2, "Loot", 20, DateTime.Now)
        });

        // TODO 8: Все убийства одного игрока
        TestScenario("Все убийства одного игрока", new List<GameEvent>
        {
            new(1, 1, "Kill", 100, DateTime.Now),
            new(2, 1, "Kill", 50, DateTime.Now)
        });

        // TODO 9: Проверка передачи null
        try
        {
            GameEventAnalyzer.AnalyzeWithLinq(null!);
        }
        catch (ArgumentNullException)
        {
            Console.WriteLine("Тест 'null': Успешно пойман ArgumentNullException (ожидаемое поведение).");
        }

        /*
         * TODO 10: Почему PLINQ не обязательно быстрее LINQ?
         * 1. Накладные расходы на разделение данных (partitioning) между потоками.
         * 2. Накладные расходы на слияние (merging) результатов и синхронизацию.
         * 3. Если операция внутри Where/Select тривиальна (простое сравнение строк), 
         *    стоимость создания и переключения потоков превышает время самого вычисления.
         * 4. Для малых коллекций (например, < 10 000 элементов) последовательный LINQ 
         *    почти всегда выигрывает за счет отсутствия этих издержек и лучшего использования кэша CPU.
         */
    }

    static void TestScenario(string testName, List<GameEvent> events)
    {
        var result = GameEventAnalyzer.AnalyzeWithPlinq(events);
        Console.WriteLine($"{testName,-30} -> элементов в результате: {result.Count}");
    }
}