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

using Microsoft.Extensions.DependencyInjection;
using TeaLauncher.Avalonia.Application.Orchestration;
using TeaLauncher.Avalonia.Application.Services;
using TeaLauncher.Avalonia.Domain.Interfaces;
using TeaLauncher.Avalonia.Infrastructure.Configuration;
using TeaLauncher.Avalonia.Views;

#if WINDOWS
using TeaLauncher.Avalonia.Infrastructure.Platform;
#endif

namespace TeaLauncher.Avalonia;

/// <summary>
/// Configures dependency injection services for the application.
/// This class centralizes all service registrations with appropriate lifetimes.
/// </summary>
public static class ServiceConfiguration
{
    /// <summary>
    /// Configures all application services with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The configured service collection for method chaining.</returns>
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        // Register Domain/Application services as Singletons (stateful services)
        services.AddSingleton<IAutoCompleter, AutoCompleterService>();
        services.AddSingleton<ICommandRegistry, CommandRegistryService>();
        services.AddSingleton<ICommandExecutor, CommandExecutorService>();

        // Register Infrastructure services as Singletons
        services.AddSingleton<IConfigurationLoader, YamlConfigLoaderService>();

#if WINDOWS
        services.AddSingleton<IHotkeyManager, WindowsHotkeyService>();
        services.AddSingleton<IIMEController, WindowsIMEControllerService>();
#endif

        // Register Orchestration and UI components as Transient (per-operation/request)
        services.AddTransient<ApplicationOrchestrator>();
        services.AddTransient<MainWindow>();

        return services;
    }
}
