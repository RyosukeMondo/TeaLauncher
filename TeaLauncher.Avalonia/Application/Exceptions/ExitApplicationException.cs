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

namespace TeaLauncher.Avalonia.Application.Exceptions;

/// <summary>
/// Exception thrown when the !exit special command is executed.
/// This signals to the application that it should terminate gracefully.
/// </summary>
public class ExitApplicationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExitApplicationException"/> class.
    /// </summary>
    public ExitApplicationException()
        : base("Application exit requested via !exit command")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExitApplicationException"/> class with a custom message.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public ExitApplicationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExitApplicationException"/> class with a custom message and inner exception.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ExitApplicationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
