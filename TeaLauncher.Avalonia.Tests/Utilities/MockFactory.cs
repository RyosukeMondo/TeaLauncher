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

using Avalonia.Input;
using TeaLauncher.Avalonia.Configuration;
using TeaLauncher.Avalonia.Domain.Interfaces;
using TeaLauncher.Avalonia.Domain.Models;

namespace TeaLauncher.Avalonia.Tests.Utilities;

/// <summary>
/// Factory for creating preconfigured NSubstitute mocks of domain interfaces.
/// These mocks come with common setup to reduce boilerplate in tests.
/// </summary>
public static class MockFactory
{
    /// <summary>
    /// Creates a mock ICommandRegistry preconfigured with the specified commands.
    /// The mock will return these commands from GetAllCommands() and HasCommand().
    /// </summary>
    /// <param name="commands">Commands to preconfigure in the registry.</param>
    /// <returns>A configured mock ICommandRegistry.</returns>
    public static ICommandRegistry CreateMockCommandRegistry(params Command[] commands)
    {
        var mock = Substitute.For<ICommandRegistry>();

        // Configure GetAllCommands to return the provided commands
        mock.GetAllCommands().Returns(commands.ToList().AsReadOnly());

        // Configure HasCommand to check against provided commands (case-insensitive)
        mock.HasCommand(Arg.Any<string>()).Returns(callInfo =>
        {
            var commandName = callInfo.ArgAt<string>(0);
            return commands.Any(c => c.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));
        });

        return mock;
    }

    /// <summary>
    /// Creates a mock ICommandExecutor with basic configuration.
    /// ExecuteAsync returns a completed task by default.
    /// </summary>
    /// <returns>A configured mock ICommandExecutor.</returns>
    public static ICommandExecutor CreateMockCommandExecutor()
    {
        var mock = Substitute.For<ICommandExecutor>();

        // Configure ExecuteAsync to return completed task by default
        mock.ExecuteAsync(Arg.Any<string>()).Returns(Task.CompletedTask);

        return mock;
    }

    /// <summary>
    /// Creates a mock IAutoCompleter preconfigured with the specified word list.
    /// The mock will use these words for auto-completion.
    /// </summary>
    /// <param name="words">Words to use for auto-completion.</param>
    /// <returns>A configured mock IAutoCompleter.</returns>
    public static IAutoCompleter CreateMockAutoCompleter(params string[] words)
    {
        var mock = Substitute.For<IAutoCompleter>();

        // Configure GetCandidates to filter words by prefix (case-insensitive)
        mock.GetCandidates(Arg.Any<string>()).Returns(callInfo =>
        {
            var prefix = callInfo.ArgAt<string>(0);
            return words.Where(w => w.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();
        });

        // Configure AutoCompleteWord to find longest common prefix
        mock.AutoCompleteWord(Arg.Any<string>()).Returns(callInfo =>
        {
            var input = callInfo.ArgAt<string>(0);
            var candidates = words.Where(w => w.StartsWith(input, StringComparison.OrdinalIgnoreCase)).ToList();

            if (candidates.Count == 0)
                return input;

            if (candidates.Count == 1)
                return candidates[0];

            // Find longest common prefix among candidates
            var firstCandidate = candidates[0];
            var commonLength = firstCandidate.Length;

            for (int i = 1; i < candidates.Count; i++)
            {
                var currentLength = 0;
                while (currentLength < commonLength &&
                       currentLength < candidates[i].Length &&
                       char.ToLower(firstCandidate[currentLength]) == char.ToLower(candidates[i][currentLength]))
                {
                    currentLength++;
                }
                commonLength = currentLength;
            }

            return firstCandidate[..commonLength];
        });

        return mock;
    }

    /// <summary>
    /// Creates a mock IConfigurationLoader preconfigured to return the specified configuration.
    /// Both synchronous and asynchronous load methods are configured.
    /// </summary>
    /// <param name="config">The configuration to return when loading.</param>
    /// <returns>A configured mock IConfigurationLoader.</returns>
    public static IConfigurationLoader CreateMockConfigurationLoader(CommandsConfig config)
    {
        var mock = Substitute.For<IConfigurationLoader>();

        // Configure both sync and async load methods
        mock.LoadConfiguration(Arg.Any<string>()).Returns(config);
        mock.LoadConfigurationAsync(Arg.Any<string>()).Returns(Task.FromResult(config));

        return mock;
    }

    /// <summary>
    /// Creates a mock IHotkeyManager with basic configuration.
    /// RegisterHotkey stores the callback for later invocation in tests.
    /// </summary>
    /// <returns>A configured mock IHotkeyManager.</returns>
    public static IHotkeyManager CreateMockHotkeyManager()
    {
        var mock = Substitute.For<IHotkeyManager>();

        // Configure IsRegistered to return false by default
        mock.IsRegistered.Returns(false);

        // Configure RegisterHotkey to set IsRegistered to true when called
        mock.When(m => m.RegisterHotkey(Arg.Any<Key>(), Arg.Any<KeyModifiers>(), Arg.Any<Action>()))
            .Do(_ => mock.IsRegistered.Returns(true));

        return mock;
    }

    /// <summary>
    /// Creates a mock IIMEController with basic configuration.
    /// </summary>
    /// <returns>A configured mock IIMEController.</returns>
    public static IIMEController CreateMockIMEController()
    {
        var mock = Substitute.For<IIMEController>();

        // No special configuration needed - just return a basic mock
        return mock;
    }

    /// <summary>
    /// Creates a mock IConfigurationLoader that throws an exception when loading.
    /// Useful for testing error handling scenarios.
    /// </summary>
    /// <param name="exception">The exception to throw when loading.</param>
    /// <returns>A configured mock IConfigurationLoader that throws on load.</returns>
    public static IConfigurationLoader CreateFailingConfigurationLoader(Exception exception)
    {
        var mock = Substitute.For<IConfigurationLoader>();

        // Configure both methods to throw the specified exception
        mock.LoadConfiguration(Arg.Any<string>()).Returns(_ => throw exception);
        mock.LoadConfigurationAsync(Arg.Any<string>()).Returns<CommandsConfig>(_ => throw exception);

        return mock;
    }

    /// <summary>
    /// Creates a MockDialogService configured with a default confirm response.
    /// The mock records all dialog calls for verification in tests without requiring a display server.
    /// </summary>
    /// <param name="defaultConfirmResponse">The default response for ShowConfirmAsync (true for Yes, false for No). Default is true.</param>
    /// <returns>A configured MockDialogService.</returns>
    public static MockDialogService CreateMockDialogService(bool defaultConfirmResponse = true)
    {
        var mock = new MockDialogService();

        // Configure the default confirmation response
        mock.SetConfirmResponse(defaultConfirmResponse);

        return mock;
    }
}
