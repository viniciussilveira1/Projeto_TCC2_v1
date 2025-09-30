using System.Collections.Generic;

public static class SessionProgress
{
    private static readonly HashSet<string> resolved   = new HashSet<string>();
    private static readonly HashSet<string> eliminated = new HashSet<string>();

    public static bool IsResolved(string id) =>
        !string.IsNullOrEmpty(id) && (resolved.Contains(id) || eliminated.Contains(id));

    public static bool IsEliminated(string id) =>
        !string.IsNullOrEmpty(id) && eliminated.Contains(id);

    public static void MarkResolved(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        resolved.Add(id);
    }

    public static void MarkEliminated(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        eliminated.Add(id);
        resolved.Add(id); // eliminado implica resolvido
    }

    public static void ClearAll()
    {
        resolved.Clear();
        eliminated.Clear();
    }
}
