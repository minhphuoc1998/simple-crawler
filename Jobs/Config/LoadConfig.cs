using WebApi.Models;

namespace WebApi.Jobs.Config;

public class LoaderConfig {
    public LoadType Type { get; set; }
    public LoaderAction[]? Actions { get; set; }
}

public enum LoaderActionType
{
    WAIT,
    CLICK,
    SCROLL,
    MOUSE_MOVE
}

public abstract class LoaderAction {
    public LoaderActionType Type { get; set; } 
}

public class WaitAction : LoaderAction {
    public int Time { get; set; }
    public int Timeout { get; set; }
    public string[] WaitForSelectors { get; set; } = [];
    public WaitAction() {
        Type = LoaderActionType.WAIT;
    }
}

public class ClickAction : LoaderAction {
    public string? Selector { get; set; }
    public int Timeout { get; set; }
    public int ClickCount { get; set; } = 1;
    public int MaxTry { get; set; } = 10;
    public int WaitTime { get; set; } = 1000;
    public ClickAction() {
        Type = LoaderActionType.CLICK;
    }
}

public class ScrollAction : LoaderAction {
    public int ScrollTimes { get; set; }
    public int ScrollDistance { get; set; }
    public int WaitTime { get; set; }
    public ScrollAction() {
        Type = LoaderActionType.SCROLL;
    }
}

public class MouseMoveAction : LoaderAction {
    public int X { get; set; } = 1000;
    public int Y { get; set; } = 1000;
    public string? ElementSelector { get; set; }
    public int Timeout = 1000;
}
