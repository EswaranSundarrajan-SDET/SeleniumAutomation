using AutomationFramework.UI;
using AutomationTests.Configuration;
using OpenQA.Selenium;

namespace AutomationTests.UI.Pages;

public sealed class AdvancedControlsPage(IWebDriver driver) : PageObject(driver, TestSettings.BaseUri)
{
    private static readonly By MultipleSelect = By.Id("multiple-dropdown");
    private static readonly By Autocomplete = By.Id("autocomplete");
    private static readonly By DynamicInput = By.CssSelector("#input-example input");
    private static readonly By FirstItem = By.CssSelector("#items li:first-child");
    private readonly InputActions input = new(driver);

    public void Open(string path) => NavigateTo(path);

    public void SelectRed() => Elements.SelectRadio(By.Id("red"));
    public bool RedSelected => Elements.IsSelected(By.Id("red"));
    public bool BlueSelected => Elements.IsSelected(By.Id("blue"));
    public bool GreenSelected => Elements.IsSelected(By.Id("green"));

    public void UpdateMultipleSelection()
    {
        Elements.SelectByIndex(MultipleSelect, 0);
        Elements.SelectByValue(MultipleSelect, "3");
        Elements.DeselectByText(MultipleSelect, "Option 1");
    }
    public IReadOnlyList<string> SelectedOptions => Elements.SelectedOptions(MultipleSelect);

    public void ChooseBanana()
    {
        Elements.Click(By.Id("fruit-combo"));
        Elements.Click(By.Id("fruit-banana"));
    }
    public string CustomSelection => Elements.Text(By.Id("result"));

    public void ChooseSuggestion(string query, string value)
    {
        Elements.Type(Autocomplete, query);
        Elements.ClickByText(By.CssSelector("#suggestions [role='option']"), value);
    }
    public string? AutocompleteValue => Elements.Property(Autocomplete, "value");

    public void PickDate(string date)
    {
        Elements.Click(By.Id("open-calendar"));
        Elements.Click(By.CssSelector($"#calendar-days button[data-date='{date}']"));
    }
    public string? AppointmentDate => Elements.Property(By.Id("widget-date"), "value");

    public bool DynamicInputEnabled => Elements.IsEnabled(DynamicInput);
    public void EnterDynamicInput(string value)
    {
        Elements.Click(By.CssSelector("#input-example button"));
        Elements.Type(DynamicInput, value);
    }
    public string? DynamicInputValue => Elements.Property(DynamicInput, "value");

    public void RebuildList()
    {
        var oldItem = Elements.Find(FirstItem);
        Elements.Click(By.Id("rerender"));
        Elements.WaitUntilStale(oldItem);
    }
    public string FirstItemText => Elements.Text(FirstItem);

    public void SwipeRight() => input.DragByOffset(By.Id("swipe-area"), 100, 0);
    public string SwipeResult => Elements.Text(By.Id("swipe-result"));

    public void LongPress() => input.LongPress(By.Id("long-press"), TimeSpan.FromSeconds(1));
    public string LongPressResult => Elements.Text(By.Id("press-result"));
}
