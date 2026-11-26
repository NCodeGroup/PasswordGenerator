// Copyright © NCode Group
// SPDX-License-Identifier: Apache-2.0

using System.Buffers;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace NCode.PasswordGenerator;

// Sealed, unlike other internal implementations: Moq proxies only virtual members and this type has none, tests
// substitute it through PasswordGenerator.CreatePinnedBuffer, and no derived type can change how Dispose zeroes it.
/// <summary>
/// Owns a password buffer on the pinned object heap, so the garbage collector never copies it, and zeroes the buffer
/// when disposed.
/// </summary>
/// <param name="length">The number of characters the buffer holds.</param>
internal sealed class PinnedPasswordBuffer(int length) : IMemoryOwner<char>
{
    private char[]? _buffer = GC.AllocateUninitializedArray<char>(length, pinned: true);

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
        if (buffer is null)
            return;

        var chars = buffer.AsSpan();
        var bytes = MemoryMarshal.AsBytes(chars);
        CryptographicOperations.ZeroMemory(bytes);
    }
}
