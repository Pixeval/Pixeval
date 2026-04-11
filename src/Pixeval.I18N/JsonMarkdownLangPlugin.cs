using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Pixeval.I18N;

public class JsonMarkdownLangPlugin : ILangPlugin
{
    public Dictionary<string, string> DefaultCultureResources { get; } = [];

    public Dictionary<string, string>? CurrentCultureResources { get; private set; }

    public IReadOnlyList<CultureInfo> AvailableCultures { get; private set; } = null!;

    public string ResourceFolder { get; set; } = AppDomain.CurrentDomain.BaseDirectory;

    public CultureInfo CurrentCulture { get; private set; } = null!;

    public CultureInfo DefaultCulture { get; private set; } = null!;

    [MemberNotNull(nameof(DefaultCulture), nameof(AvailableCultures))]
    public void Load(CultureInfo currentCulture, CultureInfo defaultCulture)
    {
        CurrentCulture = currentCulture;
        DefaultCulture = defaultCulture;
        DefaultCultureResources.Clear();
        CurrentCultureResources = null;

        var directory = ResolveResourceDirectory();
        if (directory is null)
        {
            AvailableCultures = [];
            return;
        }

        var cultures = new List<CultureInfo>();
        AvailableCultures = cultures;

        foreach (var languageFolder in directory.EnumerateDirectories())
        {
            CultureInfo culture;
            try
            {
                culture = new CultureInfo(languageFolder.Name);
            }
            catch (CultureNotFoundException)
            {
                continue;
            }

            cultures.Add(culture);

            Dictionary<string, string> dictionary;

            if (DefaultCulture.Equals(culture))
                dictionary = DefaultCultureResources;
            else if (CurrentCulture.Equals(culture))
                dictionary = CurrentCultureResources ??= [];
            else
                continue;

            foreach (var file in languageFolder.GetFiles())
            {
                var nameWithoutExtension = Path.GetFileNameWithoutExtension(file.Name);
                var extension = file.Extension.ToLowerInvariant();
                switch (extension)
                {
                    case ".json":
                    {
                        try
                        {
                            using var doc = JsonDocument.Parse(File.ReadAllText(file.FullName));
                            var root = doc.RootElement;

                            // 递归收集所有键值对
                            CollectJsonProperties(root, nameWithoutExtension, dictionary);
                        }
                        catch
                        {
                            // ignored
                        }

                        break;
                    }
                    case ".md":
                    {
                        dictionary[nameWithoutExtension + ".Markdown"] = File.ReadAllText(file.FullName);
                        break;
                    }
                }
            }
        }

        if (DefaultCultureResources.Count is 0 && CurrentCultureResources is { Count: > 0 })
        {
            foreach (var (key, value) in CurrentCultureResources)
                DefaultCultureResources[key] = value;
        }
    }

    private DirectoryInfo? ResolveResourceDirectory()
    {
        var candidatePaths = new[]
            {
                Path.Combine(ResourceFolder, "i18n"),
                Path.Combine(AppContext.BaseDirectory, "i18n"),
                Path.Combine(Environment.CurrentDirectory, "i18n"),
                Path.GetFullPath(Path.Combine(ResourceFolder, "..", "i18n")),
                Path.GetFullPath(Path.Combine(ResourceFolder, "..", "Resources", "i18n"))
            }
            .Distinct(StringComparer.OrdinalIgnoreCase);

        return candidatePaths
            .Where(Directory.Exists)
            .Select(candidatePath => new DirectoryInfo(candidatePath))
            .FirstOrDefault();
    }

    /// <summary>
    /// 递归遍历JSON元素，收集所有键值对（生成类似XML的层级键）
    /// </summary>
    /// <param name="element"></param>
    /// <param name="currentPath"></param>
    /// <param name="result"></param>
    private static void CollectJsonProperties(JsonElement element, string currentPath, Dictionary<string, string> result)
    {
        switch (element.ValueKind)
        {
            // 遍历对象的所有属性
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    var newPath = string.IsNullOrEmpty(currentPath)
                        ? property.Name
                        : $"{currentPath}.{property.Name}";

                    CollectJsonProperties(property.Value, newPath, result);
                }
                break;

            // 处理数组（按索引拼接键名）
            case JsonValueKind.Array:
                var index = 0;
                foreach (var item in element.EnumerateArray())
                {
                    var newPath = $"{currentPath}[{index}]";
                    CollectJsonProperties(item, newPath, result);
                    index++;
                }
                break;

            // 处理基本类型值
            case JsonValueKind.String:
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
                result[currentPath] = element.ToString();
                break;
        }
    }
}
