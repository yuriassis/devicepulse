namespace DevicePulse.Api.Contracts;

public sealed record DashboardSummaryResponse(
    int EquipmentCount,
    int ReadingCount,
    int InitialReadingCount,
    int ManualReadingCount,
    int AutopilotReadingCount);
