using DeskAssistant.Core.Extensions;
using GrpcService;
using Microsoft.Playwright;
using NLog;
using Polly;
using System.Text.RegularExpressions;
using Windows.Storage.Streams;

namespace DeskAssistant.WebClient
{
    public class WebBrowser
    {
        private readonly ILogger _logger;
        private EnumExtensions _enumExtensions = new();
        private TaskService.TaskServiceClient _grpcClient;
        private List<OrderItem> _orderItems;
        private BrowserLocators _locators = new();


        public WebBrowser(ILogger logger, TaskService.TaskServiceClient grpcClient)
        {            
            _logger = logger;
            _grpcClient = grpcClient;
        }


        public async Task OrderLunchAsync()
        {
            try
            {
                using var playwright = await Playwright.CreateAsync();

                if (!Path.Exists(playwright.Chromium.ExecutablePath))
                {
                    Microsoft.Playwright.Program.Main(new[] { "install" });
                }

                await using var browser = await playwright.Chromium.LaunchAsync(
                    new()
                    {
                        Headless = false,
                        //SlowMo = 1000
                    });

                var page = await browser.NewPageAsync();

                await SetupPageAsync(page);

                await LoginAsync(page);
                await NavigateToWeekMenuAsync(page);
                await WaitForSendButtonAsync(page);
                await ParseMenuAsync(page);

                await WaitForOrderHistoryTabAndSendTaskAsync(page);

                await browser.CloseAsync();
            }
            catch (Exception ex)
            {
                _logger.Error($"Browser not Opened - [{ex.InnerException.Message}]");
            }
        }



        private async Task SetupPageAsync(IPage page)
        {
            await page.SetViewportSizeAsync(1900, 925);
            await page.GotoAsync(_locators.CafeBalukUrl);

            _logger.Info("Browser is Open");
        }

        private async Task LoginAsync(IPage page)
        {
            await page.ClickAsync(_locators.AuthorisationButton);
            _logger.Trace("Authorisation button is clicked");

            await page.FillAsync(_locators.PhoneSelectorField, "297051440");
            _logger.Trace("Phone field is filled");

            await page.FillAsync(_locators.PasswordSelectorField, "1440");
            _logger.Trace("Password field is filled");

            await page.ClickAsync(_locators.LoginButton);
            _logger.Trace("Login button is clicked");

            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            _logger.Info("User is authorized");
        }

        private async Task NavigateToWeekMenuAsync(IPage page)
        {
            try
            {
                _logger.Trace("Перехожу на страницу недельного меню...");

                await page.GotoAsync(_locators.WeekMenuUrl);

                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                if (!page.Url.Contains("week"))
                {
                    _logger.Error("Не перешли на страницу недельного меню");
                }

                _logger.Info("Navigate to week menu success");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in navigation - [{ex}]);");
            }
        }

        private async Task NavigateToMenuByButtonAsync(IPage page)
        {
            try
            {
                await page.HoverAsync(_locators.MenuButton);

                await page.WaitForSelectorAsync(_locators.DropdownMenu,
                    new()
                    {
                        Timeout = 3000,
                        State = WaitForSelectorState.Visible
                    });

                await page.ClickAsync(_locators.WeekMenuButton);

                await page.WaitForLoadStateAsync(LoadState.Load);


                _logger.Info("Navigate to week menu success");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in navigation - [{ex}]);");
            }            
        }

        private async Task WaitForSendButtonAsync(IPage page)
        {
            var browserPolly = Policy.Handle<Exception>()
                .WaitAndRetryAsync(300, retryAttempt => TimeSpan.FromSeconds(1));

            await browserPolly.ExecuteAsync(async () =>
            {
                if (page.IsClosed)
                {
                    _logger.Error("Page is close");
                    return;
                }

                var element = await page.QuerySelectorAsync(_locators.SubmitOrderButton);

                if (element == null)
                {
                    _logger.Error("Order button not found, retry...");
                    throw new Exception();
                }
                _logger.Info("Order button is found");
            });
        }

        private async Task<List<OrderItem>> ParseMenuAsync(IPage page)
        {
            _orderItems = new List<OrderItem>();

            var dateOrder = await page.QuerySelectorAllAsync(_locators.DateHeader);

            foreach (var dayHeader in dateOrder)
            {
                var dateText = await dayHeader.TextContentAsync();

                var dateMatch = Regex.Match(dateText, @"\d{2}\.\d{2}\.\d{4}");

                if (!dateMatch.Success) continue;

                var orderDate = dateMatch.Value;
                _logger.Trace($"Дата: {orderDate}");

                var nextSibling = await dayHeader.EvaluateHandleAsync(_locators.ElementSibling);
                if (nextSibling != null)
                {
                    var table = await nextSibling.AsElement().QuerySelectorAsync(_locators.OrderTable);
                    if (table != null)
                    {
                        var products = await table.QuerySelectorAllAsync(_locators.ProductLink);

                        _logger.Trace($"   Найдено товаров: {products.Count}");

                        foreach (var product in products)
                        {
                            var productName = (await product.TextContentAsync())?.Trim();
                            if (!string.IsNullOrEmpty(productName))
                            {
                                _logger.Info($"   - {productName}");

                                _orderItems.Add(new OrderItem
                                {
                                    Date = orderDate,
                                    ProductName = productName
                                });
                            }
                        }
                    }
                }
            }
            return _orderItems;
        }

        private async Task WaitForOrderHistoryTabAndSendTaskAsync(IPage page)
        {
            var browserPolly = Policy.Handle<Exception>()
                .WaitAndRetryAsync(60, retryAttempt => TimeSpan.FromSeconds(1));

            await browserPolly.ExecuteAsync(async () =>
            {
                if (page.IsClosed)
                {
                    _logger.Error("Page is close");
                    return;
                }

                var element = await page.QuerySelectorAsync(_locators.OrderHistory);

                if (element == null)
                {
                    _logger.Error("OrderHistory Tab not found, retry...");
                    throw new Exception();
                }

                _logger.Info("OrderHistory Tab is found");

                foreach (var orderItem in _orderItems)
                {
                    // Отправляем на сервер
                    await CreateTaskForLunch(orderItem.ProductName, orderItem.Date);
                }
            });
        }


        private async Task CreateTaskForLunch(string productName, string dueDate)
        {
            var request = new TaskItem
            {
                Name = "Заказ обеда",
                Description = productName,
                CreatedDate = DateTime.Now.ToString(),
                DueDate = dueDate,
                Priority = _enumExtensions.PrioritiesLevelToString(PrioritiesLevelEnum.Средний),
                Category = "Обеды",
                IsCompleted = false.ToString(),
                Status = _enumExtensions.StatusToString(TaskStatusEnum.Pending),
                Tags = "#общие",
                RecurrencePattern = "None"
            };

            await _grpcClient.CreateTaskAsync(request);
        }
    }
}
