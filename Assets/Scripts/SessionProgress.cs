using System.Collections.Generic;

public static class SessionProgress
{
    private static readonly HashSet<string> _resolved = new();
    private static readonly HashSet<string> _eliminated = new();

    public static bool IsResolved(string id)    => !string.IsNullOrEmpty(id) && _resolved.Contains(id);
    public static bool IsEliminated(string id)  => !string.IsNullOrEmpty(id) && _eliminated.Contains(id);

    public static void MarkResolved(string id)
    {
        if (!string.IsNullOrEmpty(id)) _resolved.Add(id);
    }

    public static void MarkEliminated(string id)
    {
        if (!string.IsNullOrEmpty(id)) _eliminated.Add(id);
    }

    public static void ResetAll()
    {
        _resolved.Clear();
        _eliminated.Clear();
    }

    // Garante reset ao entrar no Play (mesmo com "Reload Domain" desabilitado)
    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ResetOnLoad() => ResetAll();
}
