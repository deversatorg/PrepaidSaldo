using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Services.Interfaces.Driver
{
    public interface IWebDriverManager : IDisposable
    {
        IWebDriver GetDriver();
        void ReleaseDriver(IWebDriver driver);
    }
}
