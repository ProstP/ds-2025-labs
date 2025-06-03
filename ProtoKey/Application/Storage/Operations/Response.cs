using System.Diagnostics.CodeAnalysis;

namespace ProtoKey.Application.Storage.Operations;

public enum ResponseType
{
    SET,
    GET,
    KEYS,
    ERROR,
}

public struct Response
{
    public ResponseType Type;
    public int? Value;
    public string[] Keys;
    public string ErrorMessage;

    public static Response CreateSet()
    {
        return new Response()
        {
            Type = ResponseType.SET,
            Value = null,
            Keys = null,
            ErrorMessage = null,
        };
    }
    public static Response CreateGet(int value)
    {
        return new Response()
        {
            Type = ResponseType.GET,
            Value = value,
            Keys = null,
            ErrorMessage = null,
        };
    }
    public static Response CreateKeys(string[] keys)
    {
        return new Response()
        {
            Type = ResponseType.KEYS,
            Value = null,
            Keys = keys,
            ErrorMessage = null,
        };
    }
    public static Response CreateError(string errorMessage)
    {
        return new Response()
        {
            Type = ResponseType.ERROR,
            Value = null,
            Keys = null,
            ErrorMessage = errorMessage,
        };
    }

    public override int GetHashCode()
    {
        return (Type, Value, Keys, ErrorMessage).GetHashCode();
    }
    public override bool Equals([NotNullWhen(true)] object obj)
    {
        if (obj is Response response)
        {
            return Type == response.Type && Value == response.Value
                && Keys.SequenceEqual(response.Keys) && ErrorMessage == response.ErrorMessage;
        }

        return false;
    }
}