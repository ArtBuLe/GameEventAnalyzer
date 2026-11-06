namespace GameEventsAnalyzer;
// Модель одного игрового события.
public sealed record GameEvent(
    int Id,
    int PlayerId,
    string Type,
    int Points,
    DateTime Timestamp);