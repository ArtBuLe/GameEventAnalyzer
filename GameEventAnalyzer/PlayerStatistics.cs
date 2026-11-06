namespace GameEventsAnalyzer;
// Модель статистики игрока.
public sealed record PlayerStatistics(
    int PlayerId,
    int KillCount,
    long TotalPoints);