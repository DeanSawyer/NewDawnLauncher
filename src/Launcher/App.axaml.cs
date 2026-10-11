using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using CommunityToolkit.Mvvm.Input;

using Launcher.Models;
using Launcher.ViewModels;

using NLog;

using Velopack;
using Velopack.Sources;

namespace Launcher;

public partial class App : Application
{
    private readonly Logger _logger;

    private Main _main = null!;
    private Window _window = null!;

    private const string GitHubRepoUrl = "https://github.com/DeanSawyer/NewDawnLauncher";
    private static readonly UpdateManager _updateManager = new(new GithubSource(GitHubRepoUrl, null, false));

    public static SemanticVersion CurrentVersion => _updateManager.CurrentVersion ?? new SemanticVersion(0, 0, 0);

    public App()
    {
        _logger = LogManager.GetCurrentClassLogger();
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

#if DEBUG
        this.AttachDeveloperTools();
#endif
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime applicationLifetime)
            return;

        var main = new Views.Main();

        _main = main.ViewModel;
        _window = main;

        applicationLifetime.MainWindow = main;

        main.Show();

        base.OnFrameworkInitializationCompleted();
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    public static async Task CheckForUpdatesAsync()
    {
        if (Current is not App app)
            throw new InvalidOperationException();

        try
        {
            if (_updateManager.IsInstalled)
            {
                app._main.Message = GetText("Text.Main.CheckingForUpdates");

                var updateInfo = await _updateManager.CheckForUpdatesAsync();

                if (updateInfo is null)
                {
                    app._main.Message = GetText("Text.Main.NoUpdatesFound");

                    await Task.Delay(500);

                    app._main.Message = string.Empty;
                }
                else
                {
                    await _updateManager.DownloadUpdatesAsync(updateInfo, p =>
                    {
                        app._main.Message = GetText("Text.Main.Downloading", updateInfo.TargetFullRelease.Version, p);
                    });

                    app._main.Message = GetText("Text.Main.Relaunching");

                    await Task.Delay(500);

                    _updateManager.ApplyUpdatesAndRestart(updateInfo);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            app._logger.Error(ex, "Error checking for updates");
            AddNotification(GetText("Text.Main.UpdateError"), true);
        }
    }

    public static Window GetWindow()
    {
        if (Current is not App app)
            throw new InvalidOperationException();

        return app._window;
    }

    public static string GetText(string key, params object?[] args)
    {
        if (Current is not App)
            throw new InvalidOperationException();

        if (Current.FindResource(key) is not string text)
            return $"#{key}";

        return string.Format(text, args);
    }

    public static void AddNotification(string message, bool isError = false)
    {
        if (Current is not App app)
            throw new InvalidOperationException();

        var notice = new Notification
        {
            IsError = isError,
            Message = message
        };

        app._logger.Log(isError ? LogLevel.Error : LogLevel.Info, message);
        app._main.OnReceiveNotification(notice);
    }

    public static void ShowPopup(Popup popup, bool process = false)
    {
        if (Current is not App app)
            throw new InvalidOperationException();

        if (app._main.Popup?.InProgress ?? false)
            return;

        app._main.Popup = popup;

        if (process)
            ProcessPopup();
    }

    public static async void ProcessPopup()
    {
        if (Current is not App app)
            throw new InvalidOperationException();

        try
        {
            if (app._main.Popup is null)
                return;

            if (!app._main.Popup.Validate())
                return;

            app._main.Popup.InProgress = true;

            var finished = await app._main.Popup.ProcessAsync();

            if (finished)
                app._main.Popup = null;
            else
                app._main.Popup.InProgress = false;
        }
        catch (Exception ex)
        {
            // Catch this, because it's a fire and forget method, to prevent crashes.
            Debug.WriteLine(ex);
        }
    }

    public static void CancelPopup()
    {
        if (Current is not App app)
            throw new InvalidOperationException();

        if (app._main.Popup?.InProgress ?? true)
            return;

        app._main.Popup = null;
    }
}
