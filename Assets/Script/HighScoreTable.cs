using System.Collections.Generic;
using UnityEngine;

// Arcade style top 10: three-letter initials + score.
// There is no server - like a real arcade cabinet, every computer / browser keeps
// its own table. It is saved in PlayerPrefs as one line, e.g. "CAT 1200|ABC 950".
public static class HighScoreTable
{
    public const int Size = 10;
    private const string TableKey = "HighScoreTable";
    private const string LastInitialsKey = "LastInitials";
    private const string LastRankKey = "LastRank";

    public struct Entry
    {
        public string Initials;
        public int Score;
    }

    // Initials typed last time - the entry screen starts with them
    public static string LastInitials => PlayerPrefs.GetString(LastInitialsKey, "CAT");

    // Place of the last added entry (1-10), 0 if the last run did not get in.
    // The Score screen uses it to highlight that row.
    public static int LastRank => PlayerPrefs.GetInt(LastRankKey, 0);

    public static List<Entry> Load()
    {
        List<Entry> entries = new List<Entry>();
        string saved = PlayerPrefs.GetString(TableKey, "");

        foreach (string part in saved.Split('|'))
        {
            string[] fields = part.Split(' ');
            if (fields.Length == 2 && int.TryParse(fields[1], out int score))
            {
                entries.Add(new Entry { Initials = fields[0], Score = score });
            }
        }
        return entries;
    }

    // Is this score good enough for the table?
    public static bool Qualifies(int score)
    {
        if (score <= 0) return false;

        List<Entry> entries = Load();
        return entries.Count < Size || score > entries[entries.Count - 1].Score;
    }

    // Adds the score and returns its place (1 = best)
    public static int Add(string initials, int score)
    {
        initials = Clean(initials);
        List<Entry> entries = Load();

        // Insert after every entry with the same or higher score (older one stays above)
        int index = 0;
        while (index < entries.Count && entries[index].Score >= score) index++;
        entries.Insert(index, new Entry { Initials = initials, Score = score });
        if (entries.Count > Size) entries.RemoveRange(Size, entries.Count - Size);

        List<string> parts = new List<string>();
        foreach (Entry entry in entries) parts.Add(entry.Initials + " " + entry.Score);

        int rank = index < Size ? index + 1 : 0;
        PlayerPrefs.SetString(TableKey, string.Join("|", parts));
        PlayerPrefs.SetString(LastInitialsKey, initials);
        PlayerPrefs.SetInt(LastRankKey, rank);
        PlayerPrefs.Save();
        return rank;
    }

    // Called for runs that did not get into the table, so no old row stays highlighted
    public static void ClearLastRank()
    {
        PlayerPrefs.SetInt(LastRankKey, 0);
    }

    // Exactly 3 letters A-Z (the separators of the saved line can never get in)
    private static string Clean(string initials)
    {
        char[] letters = { 'A', 'A', 'A' };
        for (int i = 0; i < 3 && initials != null && i < initials.Length; i++)
        {
            char c = char.ToUpperInvariant(initials[i]);
            if (c >= 'A' && c <= 'Z') letters[i] = c;
        }
        return new string(letters);
    }
}
