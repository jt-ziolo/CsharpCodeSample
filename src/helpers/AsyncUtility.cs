namespace MyGameName;
using System;
using System.Threading;
using System.Threading.Tasks;

public static class AsyncUtility {
  #region methods

  public static async Task RetryWithTimeoutAsync(Func<Task> operation, TimeSpan globalTimeout, int maxAttempts, TimeSpan backoffPerAttempt) {
    TimeSpan trackedTime = new();
    for (int i = 0; i < maxAttempts; i++) {
      try {
        await operation();
      }
      catch (Exception innerExcept) when (i < (maxAttempts - 1)) {
        trackedTime += backoffPerAttempt;
        if (trackedTime.TotalMilliseconds > globalTimeout.TotalMilliseconds) {
          throw new TimeoutException($"Operation did not complete within {i} attempt(s) before timeout. Trace: {innerExcept.Message}");
        }
        await Task.Delay((int)backoffPerAttempt.TotalMilliseconds * (i + 1));
      }
    }
    throw new TimeoutException($"Operation failed after maximum number of attempts ({maxAttempts}).");
  }

  public static async Task RunAfterWaitUnlessCancelledAsync(Action onCompleted, TimeSpan waitTime, CancellationTokenSource tokenSource) {
    var token = tokenSource.Token;
    var result = Task.Delay((int)waitTime.TotalMilliseconds, token);
    await result;
    if (result.IsCanceled) {
      return;
    }
    onCompleted();
  }

  public static async Task WaitUnscaledAsync(TimeSpan duration) {
    await Task.Delay((int)duration.TotalMilliseconds);
  }

  #endregion
}
