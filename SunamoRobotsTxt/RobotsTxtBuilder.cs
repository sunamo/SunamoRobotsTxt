namespace SunamoRobotsTxt;

public class RobotsTxtBuilder
{
    private const string sitemapPrefix = "Sitemap: ";
    private const string disallowPrefix = "Disallow: ";
    private const string allowPrefix = "Allow: ";
    private const string userAgentPrefix = "User-agent: ";

    public Dictionary<string, List<string>> Allows { get; set; } = new();

    public Dictionary<string, List<string>> Disallows { get; set; } = new();

    public List<string> NotRecognizedLines { get; set; } = new();

    public List<string> Sitemaps { get; set; } = new();

    public RobotsTxtBuilder(IEnumerable<string> enumerable)
    {
        var currentUserAgent = string.Empty;
        foreach (var item in enumerable)
        {
            if (item.Trim() == string.Empty) continue;
            if (item.StartsWith(sitemapPrefix))
                AddWithoutPrefix(Sitemaps, sitemapPrefix, item);
            else if (item.StartsWith(userAgentPrefix))
                currentUserAgent = item.Substring(userAgentPrefix.Length);
            else if (item.StartsWith(allowPrefix))
                AddWithoutPrefix(Allows, allowPrefix, currentUserAgent, item);
            else if (item.StartsWith(disallowPrefix))
                AddWithoutPrefix(Disallows, disallowPrefix, currentUserAgent, item);
            else
                NotRecognizedLines.Add(item);
        }
    }

    private static void AddWithoutPrefix(Dictionary<string, List<string>> dictionary, string prefix, string agentName,
        string text)
    {
        text = text.Substring(prefix.Length);
        AddOrCreateIfDontExists(dictionary, agentName, text);
    }

    private static void AddWithoutPrefix(List<string> list, string prefix, string text)
    {
        text = text.Substring(prefix.Length);
        if (!list.Contains(text)) list.Add(text);
    }

    public void Sitemap(string path)
    {
        if (!Sitemaps.Contains(path)) Sitemaps.Add(path);
    }

    public void Disallow(string userAgent, string path)
    {
        AddOrCreateIfDontExists(Disallows, userAgent, path);
    }

    public void Allow(string userAgent, string path)
    {
        AddOrCreateIfDontExists(Allows, userAgent, path);
    }

    public static void AddOrCreateIfDontExists(IDictionary<string, List<string>> dictionary, string key, string value)
    {
        AddOrCreateIfDontExists<string, string>(dictionary, key, value);
    }

    public static void AddOrCreateIfDontExists<TKey, TValue>(IDictionary<TKey, List<TValue>> dictionary, TKey key, TValue value)
    {
        if (dictionary.ContainsKey(key))
        {
            if (!dictionary[key].Contains(value)) dictionary[key].Add(value);
        }
        else
        {
            var list = new List<TValue>();
            list.Add(value);
            dictionary.Add(key, list);
        }
    }

    public void Save(string path)
    {
        var stringBuilder = new StringBuilder();

        var userAgents = new List<string>();
        userAgents.AddRange(Allows.Keys);
        userAgents.AddRange(Disallows.Keys);
        userAgents = userAgents.Distinct().ToList();

        foreach (var item in userAgents)
        {
            stringBuilder.AppendLine(userAgentPrefix + item);
            stringBuilder.AppendLine();
            WriteAllowDisallow(stringBuilder, item, Allows, allowPrefix);
            WriteAllowDisallow(stringBuilder, item, Disallows, disallowPrefix);
        }

        foreach (var item in Sitemaps) stringBuilder.AppendLine(sitemapPrefix + item);

        File.WriteAllText(path, stringBuilder.ToString());
    }

    private static void WriteAllowDisallow(StringBuilder stringBuilder, string agentName, IDictionary<string, List<string>> dictionary,
        string prefix)
    {
        var values = GetValuesOrEmpty(dictionary, agentName);

        foreach (var item in values) stringBuilder.AppendLine(prefix + item);

        if (values.Count != 0) stringBuilder.AppendLine();
    }

    public static List<TValue> GetValuesOrEmpty<TKey, TValue>(IDictionary<TKey, List<TValue>> dictionary, TKey key)
    {
        if (dictionary.ContainsKey(key)) return dictionary[key];
        return new List<TValue>();
    }
}
