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
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Read command-line arguments, default to "commands.yaml" if not provided
            // TODO: Pass config file path to MainWindow when refactoring is complete
            // For now, MainWindow uses the default "commands.yaml"
            string configFilePath = "commands.yaml";

            if (desktop.Args != null && desktop.Args.Length > 0)
            {
                configFilePath = desktop.Args[0];
            }

            // Resolve MainWindow from the dependency injection container
            // Note: MainWindow is registered as Transient, so we get a new instance
            if (ServiceProvider != null)
            {
                desktop.MainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            }
            else
            {
                // Fallback to direct instantiation if ServiceProvider is not available
                // (e.g., in design mode or testing)
                // Note: This will use the parameterless constructor which has null! for dialogService
                desktop.MainWindow = new MainWindow();
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}
