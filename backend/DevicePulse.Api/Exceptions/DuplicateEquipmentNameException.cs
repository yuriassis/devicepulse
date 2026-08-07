namespace DevicePulse.Api.Exceptions;

public sealed class DuplicateEquipmentNameException(string name)
    : Exception($"An equipment named '{name}' already exists.");
