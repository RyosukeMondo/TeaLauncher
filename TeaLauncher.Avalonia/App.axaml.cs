/*
 * TeaLauncher. Simple command launcher.
 * Copyright (C) Toshiyuki Hirooka <toshi.hirooka@gmail.com> http://wasabi.in/
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 */

using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using TeaLauncher.Avalonia.Domain.Interfaces;
using TeaLauncher.Avalonia.Views;
using AvaloniaApplication = Avalonia.Application;

namespace TeaLauncher.Avalonia;

public partial class App : AvaloniaApplication
{
    /// <summary>
    /// Gets or sets the service provider for dependency injection.
    /// This is set during application startup in Program.cs.
    /// </summary>
    public static IServiceProvider? ServiceProvider { get; set; }

    public override void Initialize()
    {
        var logger = Infrastructure.Logging.FileLogger.Instance;
        logger.Info("App.Initialize() called");

        try
        {
            AvaloniaXamlLoader.Load(this);
            logger.Info("Avalonia XAML loaded successfully");
        }
        catch (Exception ex)
        {
            logger.Error("Failed to load Avalonia XAML", ex);
            throw;
        }
    }

    private MainWindow CreateMainWindow()
    {
        var logger = Infrastructure.Logging.FileLogger.Instance;

        if (ServiceProvider != null)
        {
            logger.Info("Resolving MainWindow from ServiceProvider");
            return ServiceProvider.GetRequiredService<MainWindow>();
        }
        else
        {
            logger.Warning("ServiceProvider is null - using direct instantiation");
            return new MainWindow();
        }
    }

    private void HandleInitialization(IClassicDesktopStyleApplicationLifetime desktop, string configFilePath)
    {
        var logger = Infrastructure.Logging.FileLogger.Instance;
        logger.Info("Starting first-time initialization...");

        var initService = new Infrastructure.Initialization.InitializationService();
        var settingsService = new Infrastructure.Settings.SettingsService();
        var initWindow = new Views.InitializationWindow(initService, settingsService);

        logger.Info("Showing initialization window");
        initWindow.Show();
        initWindow.Activate();

        initWindow.Closed += (s, e) =>
        {
            logger.Info($"Initialization window closed. Completed: {initWindow.InitializationCompleted}");

            if (!initWindow.InitializationCompleted)
            {
                logger.Warning("Initialization not completed - shutting down application");
                desktop.Shutdown();
                return;
            }

            logger.Info("Initialization completed successfully - creating main window");
            desktop.MainWindow = CreateMainWindow();
            logger.Info("Main window created successfully");
        };

        logger.Info("Waiting for initialization to complete...");
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var logger = Infrastructure.Logging.FileLogger.Instance;
        logger.Info("App.OnFrameworkInitializationCompleted() called");

        try
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                logger.Info("Desktop application lifetime detected");

                // Read command-line arguments, default to "commands.yaml" if not provided
                string configFilePath = "commands.yaml";

                if (desktop.Args != null && desktop.Args.Length > 0)
                {
                    configFilePath = desktop.Args[0];
                    logger.Info($"Config file path from args: {configFilePath}");
                }
                else
                {
                    logger.Info($"Using default config file path: {configFilePath}");
                }

                // Check if initialization is needed (first-time setup)
                var initService = new Infrastructure.Initialization.InitializationService();
                bool needsInit = initService.IsInitializationNeeded(configFilePath);
                logger.Info($"Initialization needed: {needsInit}");

                if (needsInit)
                {
                    HandleInitialization(desktop, configFilePath);
                }
                else
                {
                    logger.Info("No initialization needed - creating main window directly");
                    desktop.MainWindow = CreateMainWindow();
                    logger.Info("Main window created successfully");
                }
            }
            else
            {
                logger.Warning("ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime");
            }

            base.OnFrameworkInitializationCompleted();
            logger.Info("Framework initialization completed");
        }
        catch (Exception ex)
        {
            logger.Error("Error in OnFrameworkInitializationCompleted", ex);
            throw;
        }
    }
}
