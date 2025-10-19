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

using System.Collections.Generic;

namespace TeaLauncher.Avalonia.Domain.Interfaces;

/// <summary>
/// Defines the contract for auto-completion functionality.
/// This interface represents the auto-completion responsibility, extracted from AutoCompleteMachine.
/// </summary>
public interface IAutoCompleter
{
    /// <summary>
    /// Auto-completes a word based on the registered word list.
    /// Returns the longest common prefix among all words that start with the input.
    /// </summary>
    /// <param name="chars">The input characters to auto-complete.</param>
    /// <returns>The auto-completed word, or an empty string if no matches are found.</returns>
    string AutoCompleteWord(string chars);

    /// <summary>
    /// Gets all candidate words that start with the specified prefix.
    /// </summary>
    /// <param name="prefix">The prefix to search for.</param>
    /// <returns>A list of all words that start with the prefix.</returns>
    IReadOnlyList<string> GetCandidates(string prefix);

    /// <summary>
    /// Updates the word list used for auto-completion.
    /// This replaces the entire word list with the provided list.
    /// </summary>
    /// <param name="words">The new list of words to use for auto-completion.</param>
    void UpdateWordList(IEnumerable<string> words);
}
