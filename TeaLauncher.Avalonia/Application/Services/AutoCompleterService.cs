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
using System.Collections.Generic;
using TeaLauncher.Avalonia.Domain.Interfaces;

namespace TeaLauncher.Avalonia.Application.Services;

/// <summary>
/// Service responsible for auto-completion functionality.
/// Implements the auto-completion logic extracted from AutoCompleteMachine,
/// following the Single Responsibility Principle.
/// This is a stateless service that maintains only the word list for completion.
/// </summary>
public class AutoCompleterService : IAutoCompleter
{
    private List<string> _words = new List<string>();

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoCompleterService"/> class.
    /// No dependencies required as this is a pure logic component.
    /// </summary>
    public AutoCompleterService()
    {
    }

    /// <inheritdoc />
    public string AutoCompleteWord(string chars)
    {
        var candidates = GetCandidates(chars);

        if (candidates.Count == 0)
            return "";

        // Find the longest common prefix among all candidates
        string sample = candidates[0];
        int length = 0;

        for (int i = 0; i < sample.Length; i++)
        {
            bool allMatch = true;
            string partOfSample = sample.Substring(0, i + 1);

            foreach (string candidate in candidates)
            {
                if (!candidate.StartsWith(partOfSample, StringComparison.OrdinalIgnoreCase))
                {
                    allMatch = false;
                    break;
                }
            }

            if (allMatch)
                length = i + 1;
            else
                break;
        }

        return sample.Substring(0, length);
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetCandidates(string prefix)
    {
        // Find all words that start with the prefix (case-insensitive)
        var candidates = _words.FindAll(word =>
            word.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

        return candidates.AsReadOnly();
    }

    /// <inheritdoc />
    public void UpdateWordList(IEnumerable<string> words)
    {
        _words.Clear();
        _words.AddRange(words);
    }
}
