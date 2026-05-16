using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyCoinBoxBank.Models;
using MyCoinBoxBank.Services;
using Avalonia.Threading;

namespace MyCoinBoxBank.ViewModels;

public partial class InsertCoinViewModel : ObservableObject
{
    private readonly IDatabaseService _db;
    private readonly IESP32Service _esp32;

    private CancellationTokenSource? _pollCts;
    private CancellationTokenSource? _timerCts;
    private const int AutoDoneSeconds = 10;
    private const int PollIntervalMs  = 800; // poll ESP32 every 800ms

    [ObservableProperty] private decimal _sessionTotal = 0m;
    [ObservableProperty] private string  _sessionTotalDisplay = "₱0.00";
    [ObservableProperty] private string  _statusMessage = "Press 'Insert Coin' to start";
    [ObservableProperty] private int     _timerSeconds = 0;
    [ObservableProperty] private bool    _isSessionActive = false;
    [ObservableProperty] private bool    _isWaitingForCoin = false;
    [ObservableProperty] private bool    _isBusy = false;

    [ObservableProperty] private int _count1Peso  = 0;
    [ObservableProperty] private int _count5Peso  = 0;
    [ObservableProperty] private int _count10Peso = 0;
    [ObservableProperty] private int _count20Peso = 0;

    public InsertCoinViewModel(IDatabaseService db, IESP32Service esp32)
    {
        _db    = db;
        _esp32 = esp32;
    }

    // Called when user presses "Insert Coin" button
    [RelayCommand]
    public async Task OpenCoinSlot()
    {
        IsWaitingForCoin = true;
        StatusMessage    = "Activating coin slot...";

        var started = await _esp32.StartCoinAcceptorAsync();

        if (started)
        {
            StatusMessage = "Please insert a coin now...";
            StartPolling();
        }
        else
        {
            // ESP32 offline — still open popup but log as offline
            StatusMessage    = "ESP32 offline — session running locally";
            IsSessionActive  = true;
            IsWaitingForCoin = false;
        }
    }

    [RelayCommand]
    public async Task Done()
    {
        if (SessionTotal <= 0)
        {
            StatusMessage = "No coins inserted yet.";
            return;
        }

        IsBusy        = true;
        StatusMessage = "Saving...";

        StopPolling();
        CancelTimer();
        await _esp32.StopCoinAcceptorAsync();

        await _db.AddTransactionAsync(new Transaction
        {
            Type   = TransactionType.Insert,
            Amount = SessionTotal,
            Notes  = $"Coin insert session: ₱{SessionTotal:0.00}"
        });

        StatusMessage = $"₱{SessionTotal:0.00} added to your balance!";
        ResetSession();
        IsBusy = false;
    }

    [RelayCommand]
    public async Task Cancel()
    {
        StopPolling();
        CancelTimer();
        await _esp32.StopCoinAcceptorAsync();
        ResetSession();
        StatusMessage = "Press 'Insert Coin' to start";
    }

    // Polls ESP32 to detect if a coin was physically inserted
    private void StartPolling()
    {
        StopPolling();
        _pollCts = new CancellationTokenSource();
        var token = _pollCts.Token;

        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(PollIntervalMs, token).ContinueWith(_ => { });
                if (token.IsCancellationRequested) break;

                var coinValue = await _esp32.GetLastCoinAsync();

                if (coinValue > 0)
                {
                    Dispatcher.UIThread.Post(() => OnCoinDetected(coinValue));
                }
            }
        }, token);
    }

    private void OnCoinDetected(decimal value)
    {
        // Update coin counts
        if      (value == 1.00m)  Count1Peso++;
        else if (value == 5.00m)  Count5Peso++;
        else if (value == 10.00m) Count10Peso++;
        else if (value == 20.00m) Count20Peso++;

        SessionTotal        += value;
        SessionTotalDisplay  = $"₱{SessionTotal:0.00}";
        StatusMessage        = $"₱{value:0.00} detected! Insert more or press Done";
        IsSessionActive      = true;
        IsWaitingForCoin     = false;

        ResetIdleTimer();
    }

    private void StopPolling()
    {
        _pollCts?.Cancel();
        _pollCts?.Dispose();
        _pollCts = null;
    }

    // Auto-Done timer — resets every time a coin is detected
    private void ResetIdleTimer()
    {
        CancelTimer();
        _timerCts = new CancellationTokenSource();
        var token = _timerCts.Token;

        TimerSeconds = AutoDoneSeconds;

        Task.Run(async () =>
        {
            while (TimerSeconds > 0 && !token.IsCancellationRequested)
            {
                await Task.Delay(1000, token).ContinueWith(_ => { });
                if (token.IsCancellationRequested) break;

                Dispatcher.UIThread.Post(() =>
                {
                    TimerSeconds--;
                    if (TimerSeconds is <= 3 and > 0)
                        StatusMessage = $"Auto-saving in {TimerSeconds}s...";
                });
            }

            if (!token.IsCancellationRequested)
                Dispatcher.UIThread.Post(async () => await Done());

        }, token);
    }

    private void CancelTimer()
    {
        _timerCts?.Cancel();
        _timerCts?.Dispose();
        _timerCts = null;
        TimerSeconds = 0;
    }

    private void ResetSession()
    {
        SessionTotal        = 0m;
        SessionTotalDisplay = "₱0.00";
        Count1Peso          = 0;
        Count5Peso          = 0;
        Count10Peso         = 0;
        Count20Peso         = 0;
        IsSessionActive     = false;
        IsWaitingForCoin    = false;
        TimerSeconds        = 0;
    }
}