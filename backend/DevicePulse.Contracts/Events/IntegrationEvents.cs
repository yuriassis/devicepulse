namespace DevicePulse.Contracts.Events;

public abstract record IntegrationEvent(Guid EventId, Guid OrganizationId, string CorrelationId, DateTime OccurredAtUtc, int Version = 1);
public sealed record TelemetryReadingCreated(Guid EventId, Guid OrganizationId, string CorrelationId, DateTime OccurredAtUtc, Guid ReadingId, Guid DeviceId, double Value, string Source) : IntegrationEvent(EventId, OrganizationId, CorrelationId, OccurredAtUtc);
public sealed record AlertTriggered(Guid EventId, Guid OrganizationId, string CorrelationId, DateTime OccurredAtUtc, Guid AlertId, Guid DeviceId) : IntegrationEvent(EventId, OrganizationId, CorrelationId, OccurredAtUtc);
public sealed record AlertUpdated(Guid EventId, Guid OrganizationId, string CorrelationId, DateTime OccurredAtUtc, Guid AlertId) : IntegrationEvent(EventId, OrganizationId, CorrelationId, OccurredAtUtc);
public sealed record AlertAcknowledged(Guid EventId, Guid OrganizationId, string CorrelationId, DateTime OccurredAtUtc, Guid AlertId, string UserId) : IntegrationEvent(EventId, OrganizationId, CorrelationId, OccurredAtUtc);
public sealed record AlertResolved(Guid EventId, Guid OrganizationId, string CorrelationId, DateTime OccurredAtUtc, Guid AlertId) : IntegrationEvent(EventId, OrganizationId, CorrelationId, OccurredAtUtc);
public sealed record DeviceStatusChanged(Guid EventId, Guid OrganizationId, string CorrelationId, DateTime OccurredAtUtc, Guid DeviceId, string PreviousStatus, string Status) : IntegrationEvent(EventId, OrganizationId, CorrelationId, OccurredAtUtc);
