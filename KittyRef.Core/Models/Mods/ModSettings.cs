using System;

namespace KittyRef.Core.Models.Mods;

public abstract record ModSetting(string Name, string Label, string Description);

public sealed record BoolSetting(string Name, string Label, string Description, bool Default)
    : ModSetting(Name, Label, Description);

public sealed record NumberSetting(
    string Name,
    string Label,
    string Description,
    double? Default,
    double? Min = null,
    double? Max = null,
    double? Step = null,
    bool IsInteger = false,
    bool IsNullable = false) : ModSetting(Name, Label, Description);

public sealed record StringSetting(string Name, string Label, string Description, string? Default = null)
    : ModSetting(Name, Label, Description);

public interface IEnumSetting
{
    Type EnumType { get; }
    object? DefaultValue { get; }
    bool IsNullable { get; }
}

public sealed record EnumSetting<T>(string Name, string Label, string Description, T Default, bool IsNullable = false)
    : ModSetting(Name, Label, Description), IEnumSetting
    where T : struct, Enum
{
    public Type EnumType => typeof(T);
    object? IEnumSetting.DefaultValue => Default;
}
