using ApplicationAuth.Common.Constants;
using ApplicationAuth.Common.Exceptions;
using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.Domain.Entities.Saldo;
using ApplicationAuth.Models.Enums;
using ApplicationAuth.Models.RequestModels;
using ApplicationAuth.Models.RequestModels.Saldo;
using ApplicationAuth.Models.ResponseModels;
using ApplicationAuth.Models.ResponseModels.Saldo;
using ApplicationAuth.Services.Interfaces;
using ApplicationAuth.Services.Services.Telegram;
using AutoMapper;
using Braintree;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Database;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ApplicationAuth.Services.Services
{
    public class SaldoService : ISaldoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SaldoService> _logger;

        public SaldoService(IUnitOfWork unitOfWork, 
                            IMapper mapper,
                            ILogger<SaldoService> logger) 
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<string> DeleteSaldo(ApplicationUser user)
        {
            _unitOfWork.Repository<SaldoProfile>().Delete(user.Saldo);
            _unitOfWork.SaveChanges();
            return $"{user.TelegramId}_Saldo було видалено!";
        }


        public async Task<SaldoResponseModel> Get(ApplicationUser user)
        {
            using (IWebDriver driver = new ChromeDriver()) 
            {
                driver.Url = Saldo.Base;
                driver.FindElement(By.XPath("//input[@id='mainform:cardnumber']")).SendKeys(user.Saldo.AccountNumber);
                driver.FindElement(By.XPath("//input[@id='mainform:password']")).SendKeys(user.Saldo.SecureCode);
                driver.FindElement(By.XPath("//a[@href='#'][contains(.,'Next')]")).Click();
                var balance = driver.FindElement(By.XPath("//td[contains(.,'€')]")).GetAttribute("textContent");
                balance = Regex.Replace(balance, @"[ \r\n\t]", "").TrimStart().Replace("€", "").Replace(" ", "");
                balance = Regex.Replace(balance, @"\s+", String.Empty);
                user.Saldo.Balance = double.Parse(balance, CultureInfo.InvariantCulture);
                _unitOfWork.SaveChanges();
                return new SaldoResponseModel() { AccountNumber = user.Saldo.AccountNumber, Balance = double.Parse(balance, CultureInfo.InvariantCulture), Status=true};
            }
        }

        public async Task<List<string>> GetHistoryPeriods(ApplicationUser user)
        {
            List<string> periods = new List<string>();

            using IWebDriver driver = new ChromeDriver();

            driver.Url = Saldo.Base;
            driver.FindElement(By.XPath("//input[@id='mainform:cardnumber']")).SendKeys(user.Saldo.AccountNumber);
            driver.FindElement(By.XPath("//input[@id='mainform:password']")).SendKeys(user.Saldo.SecureCode);
            driver.FindElement(By.XPath("//a[@href='#'][contains(.,'Next')]")).Click();
            driver.FindElement(By.XPath("//a[@href='#'][contains(.,'Next')]")).Click();

            IWebElement periodSelect = driver.FindElement(By.XPath("//select[@id='mainform:period']"));
            foreach (var item in periodSelect.FindElements(By.TagName("option")))
            {
                periods.Add(item.GetAttribute("textContent"));
            }
            return periods;
        }

        /*public async Task<TransactionResponseModel> GetTransaction(ApplicationUser user, string transaction)
        {
            using IWebDriver driver = new ChromeDriver();

            driver.Url = Saldo.Base;
            driver.FindElement(By.XPath("//input[@id='mainform:cardnumber']")).SendKeys(user.Saldo.AccountNumber);
            driver.FindElement(By.XPath("//input[@id='mainform:password']")).SendKeys(user.Saldo.SecureCode);
            driver.FindElement(By.XPath("//a[@href='#'][contains(.,'Next')]")).Click();
            driver.FindElement(By.XPath("//a[@href='#'][contains(.,'Next')]")).Click();


        }*/

        //TODO: Search with inlinebutton(pagination model already contains search property)
        public async Task<PaginationResponseModel<TransactionTableRowResponseModel>> GetTransactionsHistory(SaldoPaginationRequestModel<SaldoTableColumn> model, ApplicationUser user)
        {
            List<TransactionTableRowResponseModel> transactions = new List<TransactionTableRowResponseModel>();
            int totalCount = 0;

            using IWebDriver driver = new ChromeDriver();

            //var search = !string.IsNullOrEmpty(model.Search) && model.Search.Length > 1;
            driver.Url = Saldo.Base;
            driver.FindElement(By.XPath("//input[@id='mainform:cardnumber']")).SendKeys(user.Saldo.AccountNumber);
            driver.FindElement(By.XPath("//input[@id='mainform:password']")).SendKeys(user.Saldo.SecureCode);
            driver.FindElement(By.XPath("//a[@href='#'][contains(.,'Next')]")).Click();
            driver.FindElement(By.XPath("//a[@href='#'][contains(.,'Next')]")).Click();

            // Парсинг таблицы
            //int totalCount = await _getPagesCount(driver);

            IWebElement periodSelect = driver.FindElement(By.XPath("//select[@id='mainform:period']"));
            var period = periodSelect.FindElements(By.TagName("option")).First(x => x.GetAttribute("textContent").Replace(" ", "").Replace(",", "").Contains(model.Period));

            periodSelect.Click();
            period.Click();

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(x => x.FindElement(By.ClassName("pager")) != null);

            IWebElement pager = driver.FindElement(By.ClassName("pager"));

            try
            {
                var li = pager.FindElements(By.TagName("li")).First(x => x.GetAttribute("class") == "");
                IWebElement pagesList = li.FindElement(By.TagName("ul"));
                totalCount += int.Parse(pagesList.FindElements(By.TagName("li")).Last().GetAttribute("textContent"));
            }
            catch (Exception)
            {
                totalCount = 1;
            }

            var page = pager.FindElements(By.TagName("li")).FirstOrDefault(x => x.GetAttribute("class") == "active_page").GetAttribute("textContent");
            while (page != model.CurrentPage.ToString()) 
            {
                pager.FindElements(By.TagName("li")).FirstOrDefault(x => x.GetAttribute("class") == "next").Click();
                wait.Until(x => x.FindElement(By.ClassName("pager")) != null);
                pager = driver.FindElement(By.ClassName("pager"));
                page = pager.FindElements(By.TagName("li")).FirstOrDefault(x => x.GetAttribute("class") == "active_page").GetAttribute("textContent");
            }

            IWebElement tableElement = driver.FindElement(By.XPath("//table[@id='mainform:zoekresultaten']"));
            IEnumerable<IWebElement> rows = tableElement.FindElements(By.TagName("tr")).Skip(1);

            rows = rows.Take(model.Limit);

            foreach (var row in rows)
            {

                string date = row.FindElement(By.ClassName("colTxDate")).GetAttribute("textContent");
                string company = row.FindElement(By.ClassName("colMerchantName")).GetAttribute("textContent");
                string transactionType = row.FindElement(By.ClassName("colTxTypeCode")).GetAttribute("textContent");
                string debitOrCredit = row.FindElement(By.ClassName("colPaymentCode")).GetAttribute("textContent");
                string amount = row.FindElement(By.ClassName("colAmount")).GetAttribute("textContent");

                transactions.Add(new TransactionTableRowResponseModel()
                {
                    Date = date,
                    Company = company,
                    TransactionType = transactionType,
                    DebitOrCredit = debitOrCredit,
                    Amount = amount,
                });
            }

            return new(transactions, totalCount);
        }
        private async Task<int> _getPagesCount(IWebDriver driver)
        {
            int pagesCount = 1;
            IWebElement periodSelect = driver.FindElement(By.XPath("//select[@id='mainform:period']"));
            var periods = periodSelect.FindElements(By.TagName("option")).Skip(1);
            foreach (var item in periods)
            {
                //driver.Navigate().Refresh();
                periodSelect.Click();
                item.Click();

                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                wait.Until(x => x.FindElement(By.ClassName("pager")) != null);

                periodSelect = driver.FindElement(By.XPath("//select[@id='mainform:period']"));
                periods = periodSelect.FindElements(By.TagName("option")).Where(x => int.Parse(x.GetAttribute("value")) > int.Parse(item.GetAttribute("value")));

                IWebElement pager = driver.FindElement(By.ClassName("pager"));
                try
                {
                    foreach (var li in pager.FindElements(By.TagName("li")))
                    {
                        if (li.GetAttribute("class") == null) 
                        { 
                            IWebElement pagesList = li.FindElement(By.TagName("ul"));
                            pagesCount += int.Parse(pagesList.FindElements(By.TagName("li")).Last().GetAttribute("textContent"));
                        }
                        else 
                        {
                            pagesCount++;
                        }
                    }
                    
                }
                catch (StaleElementReferenceException ex)
                {
                    pagesCount++;
                }
            }

            return pagesCount;
        }
    }
}
