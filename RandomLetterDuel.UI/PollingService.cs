namespace RandomLetterDuel.UI
{
    public class PollingService
    {
        public async Task StartPolling(Func<Task> action, int intervalMs, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await action();
                await Task.Delay(intervalMs, token);
            }
        }

    }
}
