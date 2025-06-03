using ProtoKey.Application.Validator;

namespace ProtoKeyTests.Unit;

[TestFixture]
public class ProtoKeyValidatorTests
{
    private ProtoKeyValidator _validator = new();

    [TestCase("a", true)]
    [TestCase("z", true)]
    [TestCase("A", true)]
    [TestCase("Z", true)]
    [TestCase("0", true)]
    [TestCase("9", true)]
    [TestCase("-", true)]
    [TestCase(".", true)]
    [TestCase("_", true)]
    [TestCase("Aa0-9._", true)]
    [TestCase("=", false)]
    [TestCase("=.", false)]
    public void IsValidKey_DifferentSymbolsInKey_ReturnExpected(string key, bool expected)
    {
        Assert.That(_validator.IsValidKey(key), Is.EqualTo(expected));
    }

    [TestCase(1, true)]
    [TestCase(1000, true)]
    [TestCase(0, false)]
    [TestCase(1001, false)]
    public void IsValidKey_DifferentKeyLength_ReturnExpected(int length, bool expected)
    {
        char ch = 'a';
        string key = new(ch, length);

        Assert.That(_validator.IsValidKey(key), Is.EqualTo(expected));
    }

    [TestCase(int.MinValue)]
    [TestCase(int.MaxValue)]
    [TestCase(0)]
    public void IsValidValue_ValueIsDigit_ReturnTrue(int value)
    {
        Assert.That(_validator.IsValidValue(value), Is.True);
    }

    [Test]
    public void IsValidValue_ValueIsNull_ReturnFalse()
    {
        Assert.That(_validator.IsValidValue(null), Is.False);
    }
}