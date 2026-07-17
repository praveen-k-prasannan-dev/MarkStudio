using System.Windows.Input;

namespace MarkdownEditor.App;

/// <summary>Routed commands for the ribbon, with their keyboard gestures.</summary>
public static class EditorCommands
{
    private static RoutedUICommand Create(string name, Key key = Key.None, ModifierKeys modifiers = ModifierKeys.None)
    {
        var gestures = new InputGestureCollection();
        if (key != Key.None)
            gestures.Add(new KeyGesture(key, modifiers));
        return new RoutedUICommand(name, name, typeof(EditorCommands), gestures);
    }

    // Font
    public static readonly RoutedUICommand Bold = Create("Bold", Key.B, ModifierKeys.Control);
    public static readonly RoutedUICommand Italic = Create("Italic", Key.I, ModifierKeys.Control);
    public static readonly RoutedUICommand Strikethrough = Create("Strikethrough", Key.X, ModifierKeys.Control | ModifierKeys.Shift);
    public static readonly RoutedUICommand InlineCode = Create("InlineCode", Key.C, ModifierKeys.Control | ModifierKeys.Shift);
    public static readonly RoutedUICommand Highlight = Create("Highlight", Key.H, ModifierKeys.Control | ModifierKeys.Shift);

    // Paragraph
    public static readonly RoutedUICommand Heading1 = Create("Heading1", Key.D1, ModifierKeys.Control);
    public static readonly RoutedUICommand Heading2 = Create("Heading2", Key.D2, ModifierKeys.Control);
    public static readonly RoutedUICommand Heading3 = Create("Heading3", Key.D3, ModifierKeys.Control);
    public static readonly RoutedUICommand Heading4 = Create("Heading4", Key.D4, ModifierKeys.Control);
    public static readonly RoutedUICommand Heading5 = Create("Heading5", Key.D5, ModifierKeys.Control);
    public static readonly RoutedUICommand Heading6 = Create("Heading6", Key.D6, ModifierKeys.Control);
    public static readonly RoutedUICommand ClearHeading = Create("ClearHeading", Key.D0, ModifierKeys.Control);
    public static readonly RoutedUICommand BulletList = Create("BulletList", Key.D8, ModifierKeys.Control | ModifierKeys.Shift);
    public static readonly RoutedUICommand NumberedList = Create("NumberedList", Key.D7, ModifierKeys.Control | ModifierKeys.Shift);
    public static readonly RoutedUICommand TaskList = Create("TaskList", Key.D9, ModifierKeys.Control | ModifierKeys.Shift);
    public static readonly RoutedUICommand Blockquote = Create("Blockquote", Key.Q, ModifierKeys.Control | ModifierKeys.Shift);

    // Insert
    public static readonly RoutedUICommand CodeBlock = Create("CodeBlock", Key.K, ModifierKeys.Control | ModifierKeys.Shift);
    public static readonly RoutedUICommand HorizontalRule = Create("HorizontalRule");
    public static readonly RoutedUICommand InsertLink = Create("InsertLink", Key.K, ModifierKeys.Control);
    public static readonly RoutedUICommand InsertImage = Create("InsertImage", Key.I, ModifierKeys.Control | ModifierKeys.Shift);
    public static readonly RoutedUICommand InsertTable = Create("InsertTable");

    // Editing
    public static readonly RoutedUICommand Find = Create("Find", Key.F, ModifierKeys.Control);
    public static readonly RoutedUICommand Replace = Create("Replace", Key.H, ModifierKeys.Control);
    public static readonly RoutedUICommand FindNext = Create("FindNext", Key.F3);
    public static readonly RoutedUICommand FindPrevious = Create("FindPrevious", Key.F3, ModifierKeys.Shift);
}
