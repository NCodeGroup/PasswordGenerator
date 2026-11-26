// Copyright © NCode Group
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.InteropServices;

namespace NCode.PasswordGenerator.Tests;

public class PinnedPasswordBufferTests
{
    #region Memory Property Tests

    [Fact]
    public void Memory_GivenLength_ThenSameLength()
    {
        using var buffer = new PinnedPasswordBuffer(12);

        Assert.Equal(12, buffer.Memory.Length);
    }

    [Fact]
    public void Memory_WhenDisposed_ThenThrows()
    {
        var buffer = new PinnedPasswordBuffer(12);
        buffer.Dispose();

        Assert.Throws<ObjectDisposedException>(() => buffer.Memory);
    }

    #endregion

    #region Dispose Tests

    [Fact]
    public void Dispose_GivenPassword_ThenZeroed()
    {
        var buffer = new PinnedPasswordBuffer(12);
        buffer.Memory.Span.Fill('x');

        Assert.True(MemoryMarshal.TryGetArray<char>(buffer.Memory, out var segment));
        var array = segment.Array;
        Assert.NotNull(array);

        buffer.Dispose();

        Assert.All(array, c => Assert.Equal('\0', c));
    }

    [Fact]
    public void Dispose_WhenDisposedTwice_ThenNoThrow()
    {
        var buffer = new PinnedPasswordBuffer(12);
        buffer.Dispose();

        var exception = Record.Exception(buffer.Dispose);

        Assert.Null(exception);
    }

    #endregion
}
