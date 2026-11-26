// Copyright © NCode Group
// SPDX-License-Identifier: Apache-2.0

using System.Buffers;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace NCode.PasswordGenerator;

/// <summary>
/// Owns a password buffer on the pinned object heap, so the garbage collector never copies it, and zeroes the buffer
/// when disposed.
/// </summary>
internal sealed class PinnedPasswordBuffer : IMemoryOwner<char>
{
    private char[]? _buffer;

    /// <summary>
    /// Initializes a new instance of the <see cref="PinnedPasswordBuffer"/> class with a buffer of
    /// <paramref name="length"/> characters.
    /// </summary>
    /// <param name="length">The number of characters the buffer holds.</param>
    public PinnedPasswordBuffer(int length)
    {
        _buffer = GC.AllocateUninitializedArray<char>(length, pinned: true);
    }

    /// <inheritdoc />
    public Memory<char> Memory
    {
        get
        {
            var buffer = _buffer;
            ObjectDisposedException.ThrowIf(buffer is null, this);
            return buffer;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        var buffer = Interlocked.Exchange(ref _buffer, null);
        if (buffer is not null)
            CryptographicOperations.ZeroMemory(MemoryMarshal.AsBytes(buffer.AsSpan()));
    }
}
