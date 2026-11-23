using AutomationFramework.Utilities;
using OpenQA.Selenium;

namespace AutomationFramework.Pages;

public class DynamicLoadingPage(IWebDriver driver)
{
    public void Open() => driver.Navigate().GoToUrl(new Uri(ConfigHelper.BaseUri, "dynamic_loading/2"));

    public void Start() => WaitHelper.Visible(driver, By.CssSelector("#start button")).Click();

    public string Message => WaitHelper.Visible(driver, By.CssSelector("#finish h4")).Text;
}
