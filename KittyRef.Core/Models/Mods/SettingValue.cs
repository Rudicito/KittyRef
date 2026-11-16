using System;

namespace KittyRef.Core.Models.Mods
{
    public abstract record SettingValue
    {
        private protected SettingValue() { }
    }

    public sealed record BoolValue(bool Value) : SettingValue;
    public sealed record NumberValue(double? Value) : SettingValue;
    public sealed record StringValue(string? Value) : SettingValue;
    public sealed record EnumValue(Enum? Value) : SettingValue;
}
