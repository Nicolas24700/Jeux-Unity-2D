public interface IMinigameUI
{
    // Interface provides a common interface for all minigames, so they can be controlled in the same way within triggers.
    void Show();
    void Hide();
    void Toggle();
    bool IsVisible { get; }
}