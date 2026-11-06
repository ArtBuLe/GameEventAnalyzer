namespace GameEventsAnalyzer;

public static class GameEventGenerator
{
    private static readonly string[] EventTypes = 
    { 
    "Kill", 
    "Death", 
    "Loot", 
    "LevelComplete" 
    };

    public static List<GameEvent> Generate(int count, int seed = 12345)
    {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));

        var random = new Random(seed);
        var events = new List<GameEvent>(count);
        DateTime start = new(2026, 1, 1, 12, 0, 0);

        for (int i = 0; i < count; i++)
        {
            string type = EventTypes[random.Next(EventTypes.Length)];
            int points = type switch
            {
                "Kill" => random.Next(50, 201),
                "Death" => 0,
                "Loot" => random.Next(5, 51),
                "LevelComplete" => random.Next(100, 501),
                _ => 0
            };

            // TODO 7: Изменено для создания событий с одинаковым Timestamp 
            // (например, каждые 10 событий будут иметь одинаковую секунду).
            // Для обычных тестов можно вернуть start.AddSeconds(i).
            events.Add(new GameEvent(i + 1, random.Next(1, 101), type, points, start.AddSeconds(i / 10)));
        }
        return events;
    }
}