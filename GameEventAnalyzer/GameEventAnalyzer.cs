using System.Linq;

namespace GameEventsAnalyzer;

public static class GameEventAnalyzer
{
    // ==================== П1: Анализ статистики игроков ====================

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
}