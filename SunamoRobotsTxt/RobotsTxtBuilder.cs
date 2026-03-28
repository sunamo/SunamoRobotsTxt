namespace SunamoRobotsTxt;

/// <summary>
/// Builds and parses robots.txt files.
/// </summary>
public class RobotsTxtBuilder
{
    private const string sitemapPrefix = "Sitemap: ";
    private const string disallowPrefix = "Disallow: ";
    private const string allowPrefix = "Allow: ";
    private const string userAgentPrefix = "User-agent: ";

    /// <summary>
    /// Gets or sets the allowed paths per user agent.
    /// </summary>
    public Dictionary<string, List<string>> Allows { get; set; } = new();

    /// <summary>
    /// Gets or sets the disallowed paths per user agent.
    /// </summary>
    public Dictionary<string, List<string>> Disallows { get; set; } = new();

    /// <summary>
    /// Gets or sets lines that could not be parsed.
    /// </summary>
    public List<string> NotRecognizedLines { get; set; } = new();

    /// <summary>
    /// Gets or sets the sitemap URLs.
    /// </summary>
    public List<string> Sitemaps { get; set; } = new();

    /// <summary>
    /// Parses the given lines of a robots.txt file.
    /// </summary>
    /// <param name="enumerable">Lines from a robots.txt file to parse.</param>
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
    /// Adds a sitemap URL.
    /// </summary>
    /// <param name="path">URL of the sitemap.</param>
    public void Sitemap(string path)
    {
        if (!Sitemaps.Contains(path)) Sitemaps.Add(path);
    }

    /// <summary>
    /// Adds a disallow rule for a user agent.
    /// </summary>
    /// <param name="userAgent">The user agent identifier.</param>
    /// <param name="path">The disallowed path.</param>
    public void Disallow(string userAgent, string path)
    {
        AddOrCreateIfDontExists(Disallows, userAgent, path);
    }

    /// <summary>
    /// Adds an allow rule for a user agent.
    /// </summary>
    /// <param name="userAgent">The user agent identifier.</param>
    /// <param name="path">The allowed path.</param>
    public void Allow(string userAgent, string path)
    {
        AddOrCreateIfDontExists(Allows, userAgent, path);
    }

    /// <summary>
    /// Adds a value to the list for the given key, creating the list if it does not exist.
    /// </summary>
    /// <param name="dictionary">The dictionary to add to.</param>
    /// <param name="key">The key to add the value under.</param>
    /// <param name="value">The value to add.</param>
    public static void AddOrCreateIfDontExists(IDictionary<string, List<string>> dictionary, string key, string value)
    {
        AddOrCreateIfDontExists<string, string>(dictionary, key, value);
    }

    /// <summary>
    /// Adds a value to the list for the given key, creating the list if it does not exist.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary key.</typeparam>
    /// <typeparam name="TValue">The type of the list elements.</typeparam>
    /// <param name="dictionary">The dictionary to add to.</param>
    /// <param name="key">The key to add the value under.</param>
    /// <param name="value">The value to add.</param>
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
    /// Saves the robots.txt content to a file.
    /// </summary>
    /// <param name="path">The file path to save to.</param>
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
    /// Returns the list of values for the given key, or an empty list if the key does not exist.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary key.</typeparam>
    /// <typeparam name="TValue">The type of the list elements.</typeparam>
    /// <param name="dictionary">The dictionary to look up.</param>
    /// <param name="key">The key to look up.</param>
    /// <returns>The list of values, or an empty list.</returns>
    public static List<TValue> GetValuesOrEmpty<TKey, TValue>(IDictionary<TKey, List<TValue>> dictionary, TKey key)
    {
        if (dictionary.ContainsKey(key)) return dictionary[key];
        return new List<TValue>();
    }
}
