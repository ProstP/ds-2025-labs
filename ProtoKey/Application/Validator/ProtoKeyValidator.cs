using System.Text.RegularExpressions;

namespace ProtoKey.Application.Validator;

public class ProtoKeyValidator
{
    private static readonly Regex _keyRegex = new(@"^[a-zA-Z0-9_\-\.]{1,1000}$");

    public bool IsValidKey(string key)
    {
        return _keyRegex.IsMatch(key);
    }

    public bool IsValidValue(int? value)
    {
        return value is int validValue && int.MinValue <= validValue && validValue <= int.MaxValue;
    }
}