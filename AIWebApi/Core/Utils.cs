namespace AIWebApi.Core;

public static class Utils
{
    public static string RemovePolishCharacters(string text)
    {
        Dictionary<char, char> polishToEnglish = new()
        {
            {'Ą', 'A'}, {'Ć', 'C'}, {'Ę', 'E'}, {'Ł', 'L'}, {'Ń', 'N'},
            {'Ó', 'O'}, {'Ś', 'S'}, {'Ź', 'Z'}, {'Ż', 'Z'}
        };

        return string.Concat(text.Select(c => polishToEnglish.TryGetValue(c, out char value) ? value : c));
    }
}
