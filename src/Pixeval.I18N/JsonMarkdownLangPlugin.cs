using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace Pixeval.I18N;

public class JsonMarkdownLangPlugin : ILangPlugin
{
    public Dictionary<string, string> DefaultCultureResources { get; } = [];

    public Dictionary<string, string>? CurrentCultureResources { get; private set; }

    public CultureInfo CurrentCulture { get; private set; } = null!;

    public CultureInfo DefaultCulture { get; private set; } = null!;

    public void Load(CultureInfo c, DirectoryInfo resourceDirectory, bool isDefaultCulture)
    {
        Dictionary<string, string> dictionary;
        if (isDefaultCulture)
        {
            DefaultCulture = c;
            dictionary = DefaultCultureResources;
        }
        else
        {
            CurrentCulture = c;
            dictionary = CurrentCultureResources = new();
        }

        FileInfo[] files;
        try
        {
            files = resourceDirectory.GetFiles();
        }
        catch
        {
            return;
        }

        foreach (var file in files)
        {
            try
            {
                var nameWithoutExtension = Path.GetFileNameWithoutExtension(file.Name);
                switch (file.Extension.ToLowerInvariant())
                {
                    case ".json":
                    {
                        using var doc = JsonDocument.Parse(File.ReadAllText(file.FullName));
                        var root = doc.RootElement;

                        // 递归收集所有键值对
                        CollectJsonProperties(root, nameWithoutExtension, dictionary);
                        break;
                    }
                    case ".md":
                    {
                        dictionary["Markdown." + nameWithoutExtension] = File.ReadAllText(file.FullName);
                        break;
                    }
                }
            }
            catch
            {
                // A broken optional language file must not prevent the remaining resources from loading.
            }
        }
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
