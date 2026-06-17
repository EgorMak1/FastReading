using System.Text.Json;

namespace MauiApp1.Trainings;

public static class WordErasingContent
{
    private const string AssetName = "word-erasing-texts.json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static IReadOnlyList<WordErasingTextDefinition>? _cachedTexts;

    public static async Task<IReadOnlyList<WordErasingTextDefinition>> GetTextsAsync()
    {
        if (_cachedTexts != null)
        {
            return _cachedTexts;
        }

        await using Stream stream = await FileSystem.OpenAppPackageFileAsync(AssetName);
        var texts = await JsonSerializer.DeserializeAsync<List<WordErasingTextDefinition>>(stream, SerializerOptions) ?? [];

        if (texts.Count == 0)
        {
            throw new InvalidOperationException("Не удалось загрузить тексты упражнения \"Стирание слов\".");
        }

        _cachedTexts = texts;
        return _cachedTexts;
    }
}

public sealed record WordErasingTextDefinition
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public IReadOnlyList<WordErasingQuestionDefinition> Questions { get; init; } = [];
}

public sealed record WordErasingQuestionDefinition
{
    public string Prompt { get; init; } = string.Empty;
    public IReadOnlyList<string> Options { get; init; } = [];
    public int CorrectOptionIndex { get; init; }
}
