using ApplicationAuth.Services.Interfaces.Driver;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Services.Services.Driver
{
    public class WebDriverManager : IWebDriverManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly List<(IWebDriver driver, bool isBusy)> _driverList;
        private readonly object _lock = new object();

        public WebDriverManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _driverList = new List<(IWebDriver driver, bool isBusy)>();
        }

        public IWebDriver GetDriver()
        {
            lock (_lock)
            {
                var availableDriver = _driverList.FirstOrDefault(d => d.isBusy == false);

                if (availableDriver.driver != null)
                {
                    _driverList.Remove(availableDriver);
                    availableDriver.isBusy = true;
                    _driverList.Add(availableDriver);
                    return availableDriver.driver;
                }

                var newDriver = _serviceProvider.GetRequiredService<IWebDriver>();
                _driverList.Add((newDriver, true));
                return newDriver;
            }
        }

        public void ReleaseDriver(IWebDriver driver)
        {
            lock (_lock)
            {
                var driverInfo = _driverList.FirstOrDefault(d => d.driver == driver);

                if (driverInfo != default)
                {
                    _driverList.Remove(driverInfo);
                    driverInfo.isBusy = false;
                    _driverList.Add(driverInfo);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (_lock)
                {
                    foreach (var item in _driverList)
                    {
                        item.driver.Quit();
                        item.driver.Dispose();
                    }

                    _driverList.Clear();
                }
            }
        }
    }
}
