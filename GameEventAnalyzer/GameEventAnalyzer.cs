using System.Linq;

namespace GameEventsAnalyzer;

public static partial class GameEventAnalyzer
{
    // ==================== П2: Лента событий и порядок ====================

    public static List<GameEvent> GetKillsUnordered(IEnumerable<GameEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);
        // TODO 1: Формирование ленты без сохранения порядка
        return events.AsParallel().Where(e => e.Type == "Kill").ToList();
    }

    public static List<GameEvent> GetKillsOrdered(IEnumerable<GameEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);
        // TODO 2: Формирование ленты с сохранением исходного порядка
        return events.AsParallel().AsOrdered().Where(e => e.Type == "Kill").ToList();
    }

    public static List<GameEvent> GetKillsSortedByTime(IEnumerable<GameEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        // TODO 4, 5, 6: AsOrdered здесь УБРАН, так как OrderBy явно задает новый порядок.
        // ThenBy по Id добавлен для гарантии детерминированного результата.
        return events
            .AsParallel()
            .Where(e => e.Type == "Kill")
            .OrderBy(e => e.Timestamp)
            .ThenBy(e => e.Id)
            .ToList();
    }

    public static List<PlayerStatistics> AnalyzeWithLinq(IEnumerable<GameEvent> events)
    {
        // TODO 9: Проверка на null
        ArgumentNullException.ThrowIfNull(events);

        return events
            // TODO 1: Отбор событий Kill и группировка по PlayerId
            .Where(e => e.Type == "Kill")
            .GroupBy(e => e.PlayerId)
            .Select(group => new PlayerStatistics(
                PlayerId: group.Key,
                KillCount: group.Count(),
                TotalPoints: group.Sum(e => (long)e.Points)))
            // TODO 3: Сортировка по TotalPoints от большего к меньшему
            .OrderByDescending(x => x.TotalPoints)
            // TODO 4: ThenBy по PlayerId для стабильного порядка при равных очках
            .ThenBy(x => x.PlayerId)
            .ToList();
    }

    public static List<PlayerStatistics> AnalyzeWithPlinq(IEnumerable<GameEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        return events
            // TODO 2: Использование AsParallel() для распараллеливания
            .AsParallel()
            .Where(e => e.Type == "Kill")
            .GroupBy(e => e.PlayerId)
            .Select(group => new PlayerStatistics(
                PlayerId: group.Key,
                KillCount: group.Count(),
                TotalPoints: group.Sum(e => (long)e.Points)))
            .OrderByDescending(x => x.TotalPoints)
            .ThenBy(x => x.PlayerId)
            .ToList();
    }

    // ==================== Дополнительное задание: Статистика по типам ====================

    public static List<EventTypeStatistics> AnalyzeEventTypesLinq(IEnumerable<GameEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);
        return events
            .GroupBy(e => e.Type)
            .Select(g => new EventTypeStatistics(g.Key, g.Count(), g.Sum(e => (long)e.Points)))
            .OrderByDescending(x => x.Count)
            .ToList();
    }

    public static List<EventTypeStatistics> AnalyzeEventTypesPlinq(IEnumerable<GameEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);
        return events
            .AsParallel()
            .GroupBy(e => e.Type)
            .Select(g => new EventTypeStatistics(g.Key, g.Count(), g.Sum(e => (long)e.Points)))
            .OrderByDescending(x => x.Count)
            .ToList();
    }
}

// Модель для дополнительного задания
public sealed record EventTypeStatistics
    (
    string EventType,
    int Count,
    long TotalPoints
    );