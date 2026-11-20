namespace SunamoRobotsTxt;

/// <summary>
/// Represents the robots txt builder type.
/// </summary>
public class RobotsTxtBuilder
{
    private const string sitemapPrefix = "Sitemap: ";
    private const string disallowPrefix = "Disallow: ";
    private const string allowPrefix = "Allow: ";
    private const string userAgentPrefix = "User-agent: ";

    /// <summary>
    /// Gets or sets the allows.
    /// </summary>
    public Dictionary<string, List<string>> Allows { get; set; } = new();

    /// <summary>
    /// Gets or sets the disallows.
    /// </summary>
    public Dictionary<string, List<string>> Disallows { get; set; } = new();

    /// <summary>
    /// Gets or sets the not recognized lines.
    /// </summary>
    public List<string> NotRecognizedLines { get; set; } = new();

    /// <summary>
    /// Gets or sets the sitemaps.
    /// </summary>
    public List<string> Sitemaps { get; set; } = new();

    /// <summary>
    /// Robots txt builder.
    /// </summary>
    /// <returns>The robots txt builder result.</returns>
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

    /// <summary>
    /// Sitemap.
    /// </summary>
    /// <param name="prefix.Length">The prefix length.</param>
    /// <returns>The sitemap result.</returns>
    public void Sitemap(string path)
    {
        if (!Sitemaps.Contains(path)) Sitemaps.Add(path);
    }

    /// <summary>
    /// Disallow.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <param name="prefix">The prefix.</param>
    /// <param name="text">The text.</param>
    /// <returns>The disallow result.</returns>
    public void Disallow(string userAgent, string path)
    {
        AddOrCreateIfDontExists(Disallows, userAgent, path);
    }

    /// <summary>
    /// Allow.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The allow result.</returns>
    public void Allow(string userAgent, string path)
    {
        AddOrCreateIfDontExists(Allows, userAgent, path);
    }

    /// <summary>
    /// Add or create if dont exists.
    /// </summary>
    /// <param name="userAgent">The user agent.</param>
    /// <param name="path">The path.</param>
    /// <returns>The add or create if dont exists result.</returns>
    public static void AddOrCreateIfDontExists(IDictionary<string, List<string>> dictionary, string key, string value)
    {
        AddOrCreateIfDontExists<string, string>(dictionary, key, value);
    }

    /// <summary>
    /// Add or create if dont exists t key t value.
    /// </summary>
    /// <param name="userAgent">The user agent.</param>
    /// <param name="path">The path.</param>
    /// <returns>The add or create if dont exists t key t value result.</returns>
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

    /// <summary>
    /// Initializes a new instance of the save class.
    /// </summary>
    /// <param name="dictionary.ContainsKey(key)">The dictionary contains key key.</param>
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

    /// <summary>
    /// Get values or empty t key t value.
    /// </summary>
    /// <param name="stringBuilder">The string builder.</param>
    /// <param name="agentName">The agent name.</param>
    /// <param name="dictionary">The dictionary.</param>
    /// <param name="prefix">The prefix.</param>
    /// <returns>The get values or empty t key t value result.</returns>
    public static List<TValue> GetValuesOrEmpty<TKey, TValue>(IDictionary<TKey, List<TValue>> dictionary, TKey key)
    {
        if (dictionary.ContainsKey(key)) return dictionary[key];
        return new List<TValue>();
    }
}
