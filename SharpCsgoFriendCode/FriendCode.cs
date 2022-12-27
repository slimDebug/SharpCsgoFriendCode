using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SharpCsgoFriendCode;

public static class FriendCode
{
    // CS:GO uses a simplified alphabet to encode friend codes
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const string EncoderMaskString = "CSGO\0\0\0\0";
    private const ushort FriendCodeLength = 10;
    private static readonly Regex FriendCodeRegex = new($@"[{Alphabet}]{{5}}-[{Alphabet}]{{4}}", RegexOptions.Compiled);

    private static readonly ulong EncoderMask =
        BinaryPrimitives.ReadUInt64BigEndian(Encoding.ASCII.GetBytes(EncoderMaskString));

    public static ulong Decode(string friendCode)
    {
        friendCode = friendCode.ToUpperInvariant();
        var matches = FriendCodeRegex.Matches(friendCode);

        if (!matches.Any())
            throw new ArgumentException("No match for friend code pattern XXXXX-XXXX found.", nameof(friendCode));

        if (matches.Count > 1)
            throw new ArgumentException("More than one friend code pattern found.", nameof(friendCode));

        // extend to full form & remove dashes
        friendCode = $"AAAA{friendCode.Replace("-", "")}";

        var mask = 0UL;

        // alphabet mapping + encoding with 5 bits
        for (var i = 0; i < friendCode.Length; i++)
            mask |= (ulong) Alphabet.IndexOf(friendCode[i]) << (i * 5);

        // reverse endianness
        mask = BinaryPrimitives.ReverseEndianness(mask);

        var accountId = 0UL;
        for (var i = 0; i < 8; i++)
        {
            mask >>= 1;
            var nibble = mask & 0xF;

            mask >>= 4;

            accountId <<= 4;

            accountId |= nibble;
        }

        return ToSteam64Id(accountId);
    }

    public static string Encode(ulong steam64Id)
    {
        var accountId = ToAccountId(steam64Id) | EncoderMask;
        // first byte of the MD5 hash as uint
        var hash = GetFirstPartOfHash(accountId);

        var mask = 0UL;
        for (var i = 0; i < 8; i++)
        {
            var nibble = steam64Id & 0xF;
            var hashNibble = (hash >> i) & 1;
            var temp = (mask << 4) | nibble;

            steam64Id >>= 4;

            // creates an ulong using hi | lo
            mask = ((mask >> 28) << 32) | temp;
            mask = ((mask >> 31) << 32) | (temp << 1) | hashNibble;
        }

        mask = BinaryPrimitives.ReverseEndianness(mask);

        return EncodeMask(mask);
    }

    private static uint GetFirstPartOfHash(ulong accountId)
    {
        var accountIdBytes = BitConverter.GetBytes(accountId);
        using var md5 = MD5.Create();
        return BinaryPrimitives.ReadUInt32LittleEndian(md5.ComputeHash(accountIdBytes));
    }

    private static string EncodeMask(ulong mask)
    {
        var friendCode = string.Empty;

        // skips the AAAA- part from the friend code
        for (var i = 0; i < 4; i++) mask >>= 5;

        try
        {
            for (var i = 0; i < FriendCodeLength; i++)
            {
                if (i == 5)
                {
                    friendCode += '-';
                    continue;
                }

                // this is kinda risky ngl
                friendCode += Alphabet[(int) (mask & 0x1F)];
                mask >>= 5;
            }
        }
        catch
        {
            throw new ArgumentException("Provided SteamID 64 was invalid.");
        }

        return friendCode;
    }

    private static uint ToAccountId(ulong steam64Id)
    {
        // https://developer.valvesoftware.com/wiki/SteamID
        return (uint) (steam64Id & 0xFFFFFFFFUL);
    }

    private static ulong ToSteam64Id(ulong accountId)
    {
        // https://developer.valvesoftware.com/wiki/SteamID
        return accountId | 0x110000100000000UL;
    }
}