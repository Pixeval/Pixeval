using System.Collections.Generic;

namespace Pixeval.Utilities;

internal enum TabCloseScope
{
    Others,
    Left,
    Right
}

internal static class TabClosePlanner
{
    public static IReadOnlyList<T> GetTargets<T>(
        IReadOnlyList<T> tabs,
        T contextTab,
        TabCloseScope scope)
    {
        var contextIndex = -1;
        for (var index = 0; index < tabs.Count; index++)
            if (EqualityComparer<T>.Default.Equals(tabs[index], contextTab))
            {
                contextIndex = index;
                break;
            }

        if (contextIndex is -1)
            return [];

        var targets = new List<T>();
        for (var index = 0; index < tabs.Count; index++)
        {
            var shouldClose = scope switch
            {
                TabCloseScope.Others => true,
                TabCloseScope.Left => index < contextIndex,
                TabCloseScope.Right => index > contextIndex,
                _ => false
            };

            if (shouldClose && index != contextIndex)
                targets.Add(tabs[index]);
        }

        return targets;
    }
}
