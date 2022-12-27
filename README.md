# SharpCsgoFriendCode

C# library that can generate CSGO friend codes from Steam64IDs &amp; generate Steam64IDs from CSGO friend codes.

## Usage

```csharp
// Should be SUCVS-FADA
string friendCode = FriendCode.Encode(76561197960287930UL);

// Should be 76561197960287930UL
ulong steam64Id = FriendCode.Decode("SUCVS-FADA");
```

## Interesting read up & sources

[De- and encoding CS:GO friend codes](https://www.unknowncheats.me/forum/counterstrike-global-offensive/453555-de-encoding-cs-friend-codes.html)