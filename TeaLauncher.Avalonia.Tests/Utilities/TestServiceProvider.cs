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
using NSubstitute;
using TeaLauncher.Avalonia.Application.Orchestration;
using TeaLauncher.Avalonia.Application.Services;
using TeaLauncher.Avalonia.Domain.Interfaces;
using TeaLauncher.Avalonia.Infrastructure.Configuration;

namespace TeaLauncher.Avalonia.Tests.Utilities;

/// <summary>
/// Provides helper methods for creating test service providers with different configurations.
/// This utility simplifies dependency injection setup in tests.
/// </summary>
public static class TestServiceProvider
{
    /// <summary>
    /// Creates a service provider with all domain interfaces registered as NSubstitute mocks.
    /// Use this for unit tests where you need full control over dependencies.
    /// </summary>
    /// <returns>A service provider with mocked dependencies.</returns>
    public static IServiceProvider CreateWithMocks()
    {
        var services = new ServiceCollection();

        // Register all domain interfaces as mocks
        services.AddSingleton(Substitute.For<ICommandRegistry>());
        services.AddSingleton(Substitute.For<ICommandExecutor>());
        services.AddSingleton(Substitute.For<IAutoCompleter>());
        services.AddSingleton(Substitute.For<IConfigurationLoader>());
        services.AddSingleton(Substitute.For<IHotkeyManager>());
        services.AddSingleton(Substitute.For<IIMEController>());

        // Register MockDialogService for headless test compatibility
        services.AddTransient<IDialogService, MockDialogService>();

        // Register orchestrator as transient (will receive mocked dependencies)
        services.AddTransient<ApplicationOrchestrator>();

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Creates a service provider with real service implementations.
    /// Platform-specific services (IHotkeyManager, IIMEController) are mocked for cross-platform testing.
    /// Use this for integration tests where you want to test real service interactions.
    /// </summary>
    /// <returns>A service provider with real service implementations.</returns>
    public static IServiceProvider CreateWithRealServices()
    {
        var services = new ServiceCollection();

        // Register real Application services
        services.AddSingleton<IAutoCompleter, AutoCompleterService>();
        services.AddSingleton<ICommandRegistry, CommandRegistryService>();
        services.AddSingleton<ICommandExecutor, CommandExecutorService>();

        // Register real Infrastructure services (where available)
        services.AddSingleton<IConfigurationLoader, YamlConfigLoaderService>();

        // Mock platform-specific services for cross-platform test compatibility
        services.AddSingleton(Substitute.For<IHotkeyManager>());
        services.AddSingleton(Substitute.For<IIMEController>());

        // Register MockDialogService (headless mode cannot show real Avalonia dialogs)
        services.AddTransient<IDialogService, MockDialogService>();

        // Register orchestrator
        services.AddTransient<ApplicationOrchestrator>();

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Creates a custom service provider with user-defined configuration.
    /// Use this when you need a mix of real and mocked services or custom service configuration.
    /// </summary>
    /// <param name="configure">Action to configure the service collection.</param>
    /// <returns>A service provider configured by the provided action.</returns>
    /// <example>
    /// var provider = TestServiceProvider.CreateCustom(services =>
    /// {
    ///     services.AddSingleton&lt;ICommandRegistry&gt;(mockRegistry);
    ///     services.AddSingleton&lt;ICommandExecutor, CommandExecutorService&gt;();
    /// });
    /// </example>
    public static IServiceProvider CreateCustom(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();
        configure(services);
        return services.BuildServiceProvider();
    }
}
