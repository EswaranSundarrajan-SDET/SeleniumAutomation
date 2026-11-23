using System.Globalization;
using AutomationFramework.Utilities;
using OpenQA.Selenium;

namespace AutomationFramework.Pages;

public class SettlementPage(IWebDriver driver)
{
    public bool FormVisible => WaitHelper.Visible(driver, "settlement").Displayed;
    public string Confirmation => WaitHelper.Visible(driver, "confirmation").Text;

    public void Submit(string reference, decimal amount)
    {
        WaitHelper.Visible(driver, "reference").SendKeys(reference);
        WaitHelper.Visible(driver, "amount").SendKeys(amount.ToString(CultureInfo.InvariantCulture));
        WaitHelper.Visible(driver, "submit").Click();
    }
}
