using BusinessLogicLayer.Services;

namespace BankSystemAPI.BackgroundWorkers
{
    public class CreditPaymentBackgroundWorker : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<CreditPaymentBackgroundWorker> _logger;

        public CreditPaymentBackgroundWorker(IServiceProvider services, ILogger<CreditPaymentBackgroundWorker> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Фоновая задача списания кредитов успешно запущена");

            // цикл будет выполняться всё время, пока работает приложение
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;

                // настраиваем запуск ровно в 00:01 следующего дня (каждую полночь)
                var nextRun = DateTime.Today.AddDays(1).AddMinutes(1);
                var delay = nextRun - now;

                _logger.LogInformation($"Следующее автоматическое списание произойдет через: {delay}");

                // воркер засыпает в отдельном потоке до наступления полуночи
                await Task.Delay(delay, stoppingToken);

                _logger.LogInformation("Полночь наступила. Запуск процедуры ежедневных списаний по кредитам...");

                try
                {
                    // т.к. CreditService является Scoped, создаем Scope вручную в Singleton-воркере
                    using (var scope = _services.CreateScope())
                    {
                        // достаем интерфейс через DI
                        var creditService = scope.ServiceProvider.GetRequiredService<CreditService>();

                        // вызываем метод массового списания
                        creditService.ExecuteLoanMonthlyPayments();
                    }

                    _logger.LogInformation("Ежедневные списания по кредитам успешно завершены");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Произошла критическая ошибка во время фонового списания кредитов");
                }
            }
        }
    }
}
