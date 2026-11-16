using System;
using System.Collections.Generic;
using System.Text.Json;

namespace KittyRef.Core.Models.Mods;

public abstract class ModSetting
{
    private protected ModSetting(string name, string label, string description)
    {
        Name = name;
        Label = label;
        Description = description;
    }

    public string Name { get; }
    public string Label { get; }
    public string Description { get; }

    public abstract object? ObjectValue { get; }
    public abstract bool IsDefault { get; }

    internal abstract void copyValueFrom(ModSetting other);

    internal abstract bool trySetFromJson(JsonElement raw);
}

public abstract class ModSetting<T> : ModSetting
{
    private T value;

    private protected ModSetting(string name, string label, string description, T @default)
        : base(name, label, description)
    {
        Default = @default;
        value = @default;
    }

    public T Default { get; }

    public T Value
    {
        get => value;
        set => this.value = Normalize(value);
    }

    protected virtual T Normalize(T rawValue) => value;

    public override object? ObjectValue => value;
    public override bool IsDefault => EqualityComparer<T>.Default.Equals(value, Default);

    internal override void copyValueFrom(ModSetting other)
        => Value = ((ModSetting<T>)other).Value;
}

public sealed class BoolSetting : ModSetting<bool>
{
    internal BoolSetting(string name, string label, string description, bool @default)
        : base(name, label, description, @default)
    {
    }

    internal override bool trySetFromJson(JsonElement raw)
    {
        if (raw.ValueKind != JsonValueKind.True || raw.ValueKind != JsonValueKind.False) return false;
        Value = raw.GetBoolean();
        return true;
    }
}

public sealed class NumberSetting : ModSetting<double?>
{
    internal NumberSetting(
        string name,
        string label,
        string description,
        double? @default,
        double? min = null,
        double? max = null,
        double? step = null,
        bool isInteger = false,
        bool isNullable = false)
        : base(name, label, description, @default)
    {
        Min = min;
        Max = max;
        Step = step;
        IsInteger = isInteger;
        IsNullable = isNullable;
    }

    public double? Min { get; }
    public double? Max { get; }
    public double? Step { get; }
    public bool IsInteger { get; }
    public bool IsNullable { get; }

    protected override double? Normalize(double? value)
    {
        if (value == null)
            return IsNullable ? null : Default;

        double v = value.Value;

        if (IsInteger) v = Math.Round(v);
        if (Min != null) v = Math.Max(v, Min.Value);
        if (Max != null) v = Math.Min(v, Max.Value);
        return v;
    }

    internal override bool trySetFromJson(JsonElement raw)
    {
        switch (raw.ValueKind)
        {
            case JsonValueKind.Number:
                Value = raw.GetDouble();
                return true;

            case JsonValueKind.Null:
                Value = null;
                return true;

            default:
                return false;
        }
    }
}

public sealed class StringSetting : ModSetting<string?>
{
    internal StringSetting(string name, string label, string description, string? @default)
        : base(name, label, description, @default)
    {
    }

    internal override bool trySetFromJson(JsonElement raw)
    {
        if (raw.ValueKind != JsonValueKind.String) return false;
        Value = raw.GetString();
        return true;
    }
}

public sealed class ChoiceSetting : ModSetting<string>
{
    internal ChoiceSetting(string name, string label, string description, string @default,
        IReadOnlyList<string> choices)
        : base(name, label, description, @default)
    {
        Choices = choices;
    }

    public IReadOnlyList<string> Choices { get; }

    protected override string Normalize(string value)
    {
        foreach (string choice in Choices)
            if (string.Equals(choice, value, StringComparison.OrdinalIgnoreCase))
                return choice;
        return Value;
    }

    internal override bool trySetFromJson(JsonElement raw)
    {
        if (raw.ValueKind != JsonValueKind.String) return false;
        Value = raw.GetString()!;
        return true;
    }
}
