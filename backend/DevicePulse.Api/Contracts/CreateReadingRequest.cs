using DevicePulse.Api.Models;

namespace DevicePulse.Api.Contracts;

public sealed record CreateReadingRequest(double Value, ReadingSource Source);
