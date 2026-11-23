using AutomationFramework.UI;
using AutomationTests.Configuration;
using OpenQA.Selenium;

namespace AutomationTests.UI.Pages;

/// <summary>Models the demo widgets that need selection, asynchronous waiting, or pointer sequences.</summary>
public sealed class AdvancedControlsPage(IWebDriver driver) : PageObject(driver, TestSettings.BaseUri)
{
    private static readonly By MultipleSelect = By.Id("multiple-dropdown");
    private static readonly By Autocomplete = By.Id("autocomplete");
    private static readonly By DynamicInput = By.CssSelector("#input-example input");
    private static readonly By FirstItem = By.CssSelector("#items li:first-child");
    private readonly InputActions input = new(driver);

    /// <summary>Opens the requested advanced widget example.</summary>
    public void Open(string path) => NavigateTo(path);

    /// <summary>Selects the red radio button.</summary>
    public void SelectRed() => Elements.SelectRadio(By.Id("red"));
    /// <summary>Gets the current state of the red radio button.</summary>
    public bool RedSelected => Elements.IsSelected(By.Id("red"));
    /// <summary>Gets the current state of the blue radio button.</summary>
    public bool BlueSelected => Elements.IsSelected(By.Id("blue"));
    /// <summary>Gets the current state of the green radio button.</summary>
    public bool GreenSelected => Elements.IsSelected(By.Id("green"));

    /// <summary>Selects the first and third native options, then removes the first selection.</summary>
    public void UpdateMultipleSelection()
    {
        Elements.SelectByIndex(MultipleSelect, 0);
        Elements.SelectByValue(MultipleSelect, "3");
        Elements.DeselectByText(MultipleSelect, "Option 1");
    }
    /// <summary>Gets the labels remaining selected in the native multi-select.</summary>
    public IReadOnlyList<string> SelectedOptions => Elements.SelectedOptions(MultipleSelect);

    /// <summary>Opens the custom combo box and clicks its Banana option.</summary>
    public void ChooseBanana()
    {
        Elements.Click(By.Id("fruit-combo"));
        Elements.Click(By.Id("fruit-banana"));
    }
    /// <summary>Reads the custom combo box's displayed selection result.</summary>
    public string CustomSelection => Elements.Text(By.Id("result"));

    /// <summary>Enters the query and waits for the matching rendered suggestion before clicking.</summary>
    public void ChooseSuggestion(string query, string value)
    {
        Elements.Type(Autocomplete, query);
        Elements.ClickByText(By.CssSelector("#suggestions [role='option']"), value);
    }
    /// <summary>Reads the autocomplete input's current value.</summary>
    public string? AutocompleteValue => Elements.Property(Autocomplete, "value");

    /// <summary>Opens the calendar and selects the button for the supplied ISO date.</summary>
    public void PickDate(string date)
    {
        Elements.Click(By.Id("open-calendar"));
        Elements.Click(By.CssSelector($"#calendar-days button[data-date='{date}']"));
    }
    /// <summary>Reads the date copied into the appointment input.</summary>
    public string? AppointmentDate => Elements.Property(By.Id("widget-date"), "value");

    /// <summary>Gets the current enabled state of the dynamic input.</summary>
    public bool DynamicInputEnabled => Elements.IsEnabled(DynamicInput);
    /// <summary>Enables the dynamic field and waits for it to accept the supplied text.</summary>
    public void EnterDynamicInput(string value)
    {
        Elements.Click(By.CssSelector("#input-example button"));
        Elements.Type(DynamicInput, value);
    }
    /// <summary>Reads the dynamically enabled input's current value.</summary>
    public string? DynamicInputValue => Elements.Property(DynamicInput, "value");

    /// <summary>Triggers a rerender and waits for the original first item to become stale.</summary>
    public void RebuildList()
    {
        var oldItem = Elements.Find(FirstItem);
        Elements.Click(By.Id("rerender"));
        Elements.WaitUntilStale(oldItem);
    }
    /// <summary>Reads the first item after locating it in the current DOM.</summary>
    public string FirstItemText => Elements.Text(FirstItem);

    /// <summary>Drags the demo pointer area 100 pixels to the right.</summary>
    public void SwipeRight() => input.DragByOffset(By.Id("swipe-area"), 100, 0);
    /// <summary>Reads the pointer widget's reported swipe result.</summary>
    public string SwipeResult => Elements.Text(By.Id("swipe-result"));

    /// <summary>Holds the pointer button over the demo target for one second.</summary>
    public void LongPress() => input.LongPress(By.Id("long-press"), TimeSpan.FromSeconds(1));
    /// <summary>Reads the pointer widget's reported long-press result.</summary>
    public string LongPressResult => Elements.Text(By.Id("press-result"));
}
