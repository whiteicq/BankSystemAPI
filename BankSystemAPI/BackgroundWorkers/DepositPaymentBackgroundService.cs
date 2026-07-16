using BusinessLogicLayer.Services;

namespace BankSystemAPI.BackgroundWorkers
{
    public class DepositPaymentBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<DepositPaymentBackgroundService> _logger;

        public DepositPaymentBackgroundService(IServiceProvider services, ILogger<DepositPaymentBackgroundService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Фоновая задача начисления процентов по вкладам успешно запущена");

            // цикл будет выполняться всё время, пока работает приложение
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;

                // настраиваем запуск ровно в 00:01 следующего дня (каждую полночь)
                var nextRun = DateTime.Today.AddDays(1).AddMinutes(1);
                var delay = nextRun - now;

                _logger.LogInformation($"Следующее автоматическое начисление произойдет через: {delay}");

                // воркер засыпает в отдельном потоке до наступления полуночи
                await Task.Delay(delay, stoppingToken);

                _logger.LogInformation("Полночь наступила. Запуск процедуры ежедневных начислений по вкладам...");

                try
                {
                    // т.к. DepositService является Scoped, создаем Scope вручную в Singleton-воркере
                    using (var scope = _services.CreateScope())
                    {
                        // достаем интерфейс через DI
                        var depositService = scope.ServiceProvider.GetRequiredService<DepositService>();

                        // вызываем метод массового списания
                        depositService.ExecuteDepositMonthlyPayments();
                    }

                    _logger.LogInformation("Ежедневные начисления по вкладам успешно завершены");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Произошла критическая ошибка во время фонового начисления по вкладам!");
                }
            }
        }
    }
}
