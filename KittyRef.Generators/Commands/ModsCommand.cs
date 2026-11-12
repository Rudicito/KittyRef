// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using McMaster.Extensions.CommandLineUtils;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Extensions;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using Scriban;

namespace KittyRef.Generators.Commands
{
    [Command(Name = "mods", Description = "Generate C# classes of osu! mods")]
    public class ModsCommand
    {
        [Option("-o|--output <DIR>", "Output directory", CommandOptionType.SingleValue)]
        public string OutputDir { get; set; } = "Generated/Mods";

        private const string path_file = "Templates/mod.sbn";
        private readonly Template template = Template.Parse(File.ReadAllText(path_file), path_file);

        [UsedImplicitly]
        private int OnExecute()
        {
            var allRulesets = Enumerable.Range(0, ILegacyRuleset.MAX_LEGACY_RULESET_ID + 1)
                .Select(LegacyHelper.GetRulesetFromLegacyId);

            foreach (var ruleset in allRulesets)
                getDefinitionsForRuleset(ruleset);

            return 0;
        }

        private void getDefinitionsForRuleset(Ruleset ruleset)
        {
            var allMods = ruleset.CreateAllMods().ToArray();
            string rulesetName = ruleset.GetType().Name.Replace("Ruleset", string.Empty);
            string dir = Path.Combine(OutputDir, rulesetName);

            if (Directory.Exists(dir))
                Directory.Delete(dir, recursive: true);
            Directory.CreateDirectory(dir);

            var mods = allMods.Select(mod => new
            {
                Ruleset = rulesetName,
                ClassName = mod.GetType().Name,
                mod.Acronym,
                mod.Name,
                Description = mod.Description.ToString(),
                Type = mod.Type.ToString().ToPascalCase(),
                Settings = getSettingsDefinitions(mod),
                IncompatibleMods = getAllImplementations(mod.IncompatibleMods),
                RequiresConfiguration = mod.RequiresConfiguration.ToString().ToLowerInvariant(),
                UserPlayable = mod.UserPlayable.ToString().ToLowerInvariant(),
                ValidForMultiplayer = mod.ValidForMultiplayer.ToString().ToLowerInvariant(),
                ValidForFreestyleAsRequiredMod = mod.ValidForFreestyleAsRequiredMod.ToString().ToLowerInvariant(),
                ValidForMultiplayerAsFreeMod = mod.ValidForMultiplayerAsFreeMod.ToString().ToLowerInvariant(),
                AlwaysValidForSubmission = mod.AlwaysValidForSubmission.ToString().ToLowerInvariant(),
                Ranked = mod.Ranked.ToString().ToLowerInvariant()
            });

            foreach (var mod in mods) createClassFile(mod);
            return;

            void createClassFile(dynamic mod)
            {
                string result = template.Render(new {m = mod});

                string path = Path.Combine(dir, $"{mod.ClassName}.cs");
                File.WriteAllText(path, result.ReplaceLineEndings("\n"));
            }

            IEnumerable<string> getAllImplementations(Type[] incompatibleTypes)
            {
                foreach (var mod in allMods)
                {
                    if (incompatibleTypes.Any(t => t.IsInstanceOfType(mod)))
                        yield return mod.GetType().Name ;
                }
            }

            IEnumerable<dynamic> getSettingsDefinitions(Mod mod)
            {
                var sourceProperties = mod.GetSettingsSourceProperties();

                foreach (var (settingsSource, propertyInfo) in sourceProperties)
                {
                    object? bindable = propertyInfo.GetValue(mod);

                    Debug.Assert(bindable != null);

                    string name = propertyInfo.Name.ToSnakeCase();
                    string label = settingsSource.Label.ToString();
                    string description = settingsSource.Description.ToString();

                    string type;
                    dynamic? enumSetting = null;

                    switch (bindable)
                    {
                        case DifficultyBindable bd:
                            type = getModSettingString<float>(name, label, description, 5, bd.ExtendedMinValue ?? bd.MinValue,
                                bd.ExtendedMaxValue ?? bd.MaxValue, getPrecisionDifficultyBindable(bd), false, false);
                            break;

                        case BindableNumber<double> bd:
                            type = getModSettingString<double>(name, label, description, bd.Default, bd.MinValue,
                                bd.MaxValue, bd.Precision, false, false);
                            break;

                        case BindableNumber<float> bd:
                            type = getModSettingString<float>(name, label, description, bd.Default, bd.MinValue,
                                bd.MaxValue, bd.Precision, false, false);
                            break;

                        case BindableNumber<int> bd:
                            type = getModSettingString<int>(name, label, description, bd.Default, bd.MinValue,
                                bd.MaxValue, bd.Precision, true, false);
                            break;

                        case BindableNumber<long> bd:
                            type = getModSettingString<long>(name, label, description, bd.Default, bd.MinValue,
                                bd.MaxValue, bd.Precision, true, false);
                            break;

                        case IBindable<double> bd:
                            type = getModSettingString<double>(name, label, description, bd.Default, null,
                                null, null, false, false);
                            break;

                        case IBindable<double?> bd:
                            type = getModSettingString(name, label, description, bd.Default, null,
                                null, null, false, true);
                            break;

                        case IBindable<float> bd:
                            type = getModSettingString<float>(name, label, description, bd.Default, null,
                                null, null, false, false);
                            break;

                        case IBindable<float?> bd:
                            type = getModSettingString(name, label, description, bd.Default, null,
                                null, null, false, true);
                            break;

                        case IBindable<int> bd:
                            type = getModSettingString<int>(name, label, description, bd.Default, null,
                                null, null, true, false);
                            break;

                        case IBindable<int?> bd:
                            type = getModSettingString(name, label, description, bd.Default, null,
                                null, null, true, true);
                            break;

                        case IBindable<long> bd:
                            type = getModSettingString<long>(name, label, description, bd.Default, null,
                                null, null, true, false);
                            break;

                        case IBindable<long?> bd:
                            type = getModSettingString(name, label, description, bd.Default, null,
                                null, null, true, true);
                            break;

                        case IBindable<bool> bd:
                            type = $"BoolSetting(\"{name}\", \"{label}\", \"{description}\", {bd.Default.ToString().ToLowerInvariant()})";
                            break;

                        case IBindable<string> bd:
                            type = $"StringSetting(\"{name}\", \"{label}\", \"{description}\", \"{bd.Default}\")";
                            break;

                        default:
                            var bdP = bindable.GetType().GetInterface("IBindable`1");
                            if (bdP == null)
                                throw new Exception("Bindable was expected");

                            var bdType = bdP.GenericTypeArguments.FirstOrDefault();

                            if (!(bdType?.IsEnum ?? false))
                                throw new Exception($"Bindable type not implemented {bdType}");

                            string enumName = bdType.Name;
                            var enumValueList = bdType.GetEnumNames().ToList();
                            Debug.Assert(enumValueList.Count > 0);
                            object? enumDefault = bdP.GetProperty("Default")!.GetValue(bindable);

                            type = $"EnumSetting<{enumName}>(\"{name}\", \"{label}\", \"{description}\", {enumName}.{enumDefault})";

                            enumSetting = new
                            {
                                Name = enumName,
                                Values = enumValueList
                            };
                            break;
                    }

                    yield return new
                    {
                        Name = propertyInfo.Name.ToSnakeCase(),
                        TypeMethod = type,
                        EnumSetting = enumSetting,
                        Label = settingsSource.Label.ToString(),
                        Description = settingsSource.Description.ToString(),
                    };
                }
            }
        }

        private static string getModSettingString<T>(
            string name,
            string label,
            string description,
            T? @default,
            T? minValue,
            T? maxValue,
            T? precision,
            bool isInteger,
            bool canBeNull
        ) where T : struct
        {
            return $"NumberSetting(\"{name}\", \"{label}\", \"{description}\", {fmt(@default)}, {fmt(minValue)}, {fmt(maxValue)}, {fmt(precision)}, {isInteger.ToString().ToLowerInvariant()}, {canBeNull.ToString().ToLowerInvariant()})";
        }

        private static string fmt<T>(T? value) where T : struct
            => value != null
                ? Convert.ToString(value.Value, CultureInfo.InvariantCulture)!
                : "null";

        private static readonly FieldInfo current_number_field =
            typeof(DifficultyBindable).GetField("CurrentNumber",
                BindingFlags.Instance | BindingFlags.NonPublic)!;

        private static float getPrecisionDifficultyBindable(DifficultyBindable db)
        {
            var current = (BindableFloat)current_number_field.GetValue(db)!;
            return current.Precision;
        }
    }
}
