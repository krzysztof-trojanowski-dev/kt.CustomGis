using kt.CustomGis.AppMain.Mvvm;

namespace kt.CustomGis.App.Tests.Mvvm;

[TestFixture]
public class AsyncRelayCommandTests
{
    [Test]
    public void CanExecute_WithoutPredicate_ReturnsTrue()
    {
        var command = new AsyncRelayCommand(() => Task.CompletedTask);

        Assert.That(command.CanExecute(null), Is.True);
    }

    [Test]
    public void CanExecute_WithPredicate_ReturnsPredicateValue()
    {
        var command = new AsyncRelayCommand(() => Task.CompletedTask, () => false);

        Assert.That(command.CanExecute(null), Is.False);
    }

    [Test]
    public async Task Execute_InvokesDelegate()
    {
        var tcs = new TaskCompletionSource();
        var command = new AsyncRelayCommand(() =>
        {
            tcs.SetResult();
            return Task.CompletedTask;
        });

        command.Execute(null);

        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(1));
        Assert.That(tcs.Task.IsCompleted, Is.True);
    }

    [Test]
    public void RaiseCanExecuteChanged_RaisesEvent()
    {
        var command = new AsyncRelayCommand(() => Task.CompletedTask);
        var raised = false;
        command.CanExecuteChanged += (_, _) => raised = true;

        command.RaiseCanExecuteChanged();

        Assert.That(raised, Is.True);
    }
}
