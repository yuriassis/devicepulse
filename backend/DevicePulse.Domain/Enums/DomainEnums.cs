namespace DevicePulse.Domain.Enums;

public enum DeviceStatus { Unknown, Online, Warning, Critical, Offline, Disabled }
public enum TelemetrySource { Initial, Manual, Autopilot, External }
public enum AlertOperator { GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual, Equal, OutsideRange, InsideRange }
public enum AlertSeverity { Info, Warning, Critical }
public enum AlertStatus { Active, Acknowledged, Resolved }
public enum SimulationMode { Random, LinearIncrease, LinearDecrease, SineWave, StableWithNoise, FailureSimulation }
public enum SimulationStatus { Running, Paused, Stopped }
