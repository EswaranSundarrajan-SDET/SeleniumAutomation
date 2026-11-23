using AutomationFramework.UI;
using AutomationTests.Configuration;
using OpenQA.Selenium;

namespace AutomationTests.UI.Pages;

public sealed class AdvancedControlsPage(IWebDriver driver)
{
    private static readonly By MultipleSelect = By.Id("multiple-dropdown");
    private static readonly By Autocomplete = By.Id("autocomplete");
    private static readonly By DynamicInput = By.CssSelector("#input-example input");
    private static readonly By FirstItem = By.CssSelector("#items li:first-child");
    private readonly BrowserActions browser = new(driver);
    private readonly ElementActions elements = new(driver);
    private readonly InputActions input = new(driver);

    public void Open(string path) => browser.Navigate(new Uri(TestSettings.BaseUri, path));

    public void SelectRed() => elements.SelectRadio(By.Id("red"));
    public bool RedSelected => elements.IsSelected(By.Id("red"));
    public bool BlueSelected => elements.IsSelected(By.Id("blue"));
    public bool GreenSelected => elements.IsSelected(By.Id("green"));

    public void UpdateMultipleSelection()
    {
        elements.SelectByIndex(MultipleSelect, 0);
        elements.SelectByValue(MultipleSelect, "3");
        elements.DeselectByText(MultipleSelect, "Option 1");
    }
    public IReadOnlyList<string> SelectedOptions => elements.SelectedOptions(MultipleSelect);

    public void ChooseBanana()
    {
        elements.Click(By.Id("fruit-combo"));
        elements.Click(By.Id("fruit-banana"));
    }
    public string CustomSelection => elements.Text(By.Id("result"));

    public void ChooseSuggestion(string query, string value)
    {
        elements.Type(Autocomplete, query);
        elements.ClickByText(By.CssSelector("#suggestions [role='option']"), value);
    }
    public string? AutocompleteValue => elements.Property(Autocomplete, "value");

    public void PickDate(string date)
    {
        elements.Click(By.Id("open-calendar"));
        elements.Click(By.CssSelector($"#calendar-days button[data-date='{date}']"));
    }
    public string? AppointmentDate => elements.Property(By.Id("widget-date"), "value");

    public bool DynamicInputEnabled => elements.IsEnabled(DynamicInput);
    public void EnterDynamicInput(string value)
    {
        elements.Click(By.CssSelector("#input-example button"));
        elements.Type(DynamicInput, value);
    }
    public string? DynamicInputValue => elements.Property(DynamicInput, "value");

    public void RebuildList()
    {
        var oldItem = elements.Find(FirstItem);
        elements.Click(By.Id("rerender"));
        elements.WaitUntilStale(oldItem);
    }
    public string FirstItemText => elements.Text(FirstItem);

    public void SwipeRight() => input.DragByOffset(By.Id("swipe-area"), 100, 0);
    public string SwipeResult => elements.Text(By.Id("swipe-result"));

    public void LongPress() => input.LongPress(By.Id("long-press"), TimeSpan.FromSeconds(1));
    public string LongPressResult => elements.Text(By.Id("press-result"));
}
