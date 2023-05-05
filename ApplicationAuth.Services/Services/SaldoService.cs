using ApplicationAuth.Common.Constants;
using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.Models.ResponseModels.Saldo;
using ApplicationAuth.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ApplicationAuth.Services.Services
{
    public class SaldoService : ISaldoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SaldoService(IUnitOfWork unitOfWork, 
                            IMapper mapper) 
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;   
        }
        public async Task<SaldoResponseModel> Get(string telegramId)
        {
            //var user = _unitOfWork.Repository<ApplicationUser>().Get(x => x.TelegramId == telegramId)
              //                                                  .Include(w => w.Saldo)
                //                                                .FirstOrDefault();
            using (IWebDriver driver = new ChromeDriver()) 
            {
                driver.Url = Saldo.Base;
                driver.FindElement(By.XPath("//input[@id='mainform:cardnumber']")).SendKeys(/*user.Saldo.AccountNumber*/ "9690033280");
                driver.FindElement(By.XPath("//input[@id='mainform:password']")).SendKeys("8772");
                driver.FindElement(By.XPath("//a[@href='#'][contains(.,'Next')]")).Click();
                await Task.Delay(1000);
                var balance = driver.FindElement(By.XPath("//td[contains(.,'€')]")).GetAttribute("textContent");
                balance = Regex.Replace(balance, @"[ \r\n\t]", "").TrimStart().Replace("€", "").Replace(" ", "");
                balance = Regex.Replace(balance, @"\s+", String.Empty);
                //user.Saldo.Balance = double.Parse(balance.Substring(1, balance.Length));
                return new SaldoResponseModel() { AccountNumber = "9690033280", Balance = double.Parse(balance, CultureInfo.InvariantCulture), Status=true};
            }
        }
    }
}
