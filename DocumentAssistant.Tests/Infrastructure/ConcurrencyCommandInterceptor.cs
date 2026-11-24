
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DocumentAssistant.Tests.Infrastructure;

public class ConcurrencyCommandInterceptor : DbCommandInterceptor
{
    private readonly TaskCompletionSource<bool> _firstQuery =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly TaskCompletionSource<bool> _releaseQueries =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private int _queryCount;

    public override async ValueTask<InterceptionResult<DbDataReader>>
        ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
    {
        if (command.CommandText.Contains(
                "Documents",
                StringComparison.OrdinalIgnoreCase))
        {
            var currentCount = Interlocked.Increment(ref _queryCount);

            if (currentCount == 1)
            {
                _firstQuery.TrySetResult(true);
            }

            if (currentCount <= 2)
            {
                if (currentCount == 2)
                {
                    _releaseQueries.TrySetResult(true);
                }

                await _releaseQueries.Task.WaitAsync(
                    TimeSpan.FromSeconds(15),
                    cancellationToken);
            }
        }

        return result;
    }

    public Task WaitForFirstQueryAsync() => _firstQuery.Task;
}
