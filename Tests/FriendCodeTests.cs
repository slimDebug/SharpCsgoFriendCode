using NUnit.Framework;
using SharpCsgoFriendCode;

namespace Tests;

public class FriendCodeTests
{
    [Test]
    public void EncodeSteam64Id()
    {
        Assert.That(FriendCode.Encode(76561197960287930UL), Is.EqualTo("SUCVS-FADA"));
    }

    [Test]
    public void DecodeFriendCode()
    {
        Assert.That(FriendCode.Decode("SUCVS-FADA"), Is.EqualTo(76561197960287930UL));
    }

    [Test]
    public void DecodeLowerCaseFriendCode()
    {
        Assert.That(FriendCode.Decode("sucvs-fada"), Is.EqualTo(76561197960287930UL));
    }

    [Test]
    public void DecodeWrongFriendCode()
    {
        Assert.Throws<ArgumentException>(() => FriendCode.Decode("test"));
    }

    [Test]
    public void DecodeMultipleFriendCodes()
    {
        Assert.Throws<ArgumentException>(() => FriendCode.Decode("SUCVS-FADA SUCVS-FADA"));
    }
}