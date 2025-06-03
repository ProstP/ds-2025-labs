using System.Threading.Channels;
using Microsoft.AspNetCore.Mvc;
using ProtoKey.Application.Storage.Operations;
using ProtoKey.Application.Validator;
using ProtoKey.Controllers.Contract.Request;
using ProtoKey.Controllers.Contract.Response;

namespace ProtoKey.Controllers;

[ApiController]
[Route("[controller]")]
public class ProtoKeyController : ControllerBase
{
    private readonly ChannelWriter<Command> _commandWriter;
    private readonly ChannelReader<Response> _responseReader;
    private readonly ProtoKeyValidator _protoKeyValidator;

    public ProtoKeyController(
        Channel<Command> commandChannel,
        Channel<Response> responseChannel,
        ProtoKeyValidator protoKeyValidator)
    {
        _commandWriter = commandChannel.Writer;
        _responseReader = responseChannel.Reader;
        _protoKeyValidator = protoKeyValidator;
    }

    [HttpPost]
    [Route("set")]
    public async Task<ActionResult> Set([FromBody] SetRequest request)
    {
        if (!_protoKeyValidator.IsValidKey(request.Key) || !_protoKeyValidator.IsValidValue(request.Value))
        {
            return BadRequest("Key or value is not valid");
        }

        await _commandWriter.WriteAsync(Command.CreateSet(request.Key, request.Value));
        var response = await _responseReader.ReadAsync();

        if (response.Type == ResponseType.ERROR)
        {
            return BadRequest(response.ErrorMessage);
        }
        if (response.Type != ResponseType.SET)
        {
            return StatusCode(500, "Server reponse is incorrect");
        }

        return Ok();
    }

    [HttpGet]
    [Route("get/{key}")]
    public async Task<ActionResult<GetResponse>> Get([FromRoute] string key)
    {
        if (!_protoKeyValidator.IsValidKey(key))
        {
            return BadRequest("Key is not valid");
        }

        await _commandWriter.WriteAsync(Command.CreateGet(key));

        Response response = await _responseReader.ReadAsync();

        if (response.Type == ResponseType.ERROR)
        {
            return BadRequest(response.ErrorMessage);
        }
        if (response.Type != ResponseType.GET)
        {
            return StatusCode(500, "Server reponse is incorrect");
        }

        return Ok(new GetResponse
        {
            Value = (int)response.Value
        });
    }

    [HttpGet]
    [Route("keys")]
    public async Task<ActionResult<KeysResponse>> Keys([FromQuery] string prefix)
    {
        await _commandWriter.WriteAsync(Command.CreateKeys(prefix ?? ""));

        Response response = await _responseReader.ReadAsync();

        if (response.Type == ResponseType.ERROR)
        {
            return BadRequest(response.ErrorMessage);
        }
        if (response.Type != ResponseType.KEYS)
        {
            return StatusCode(500, "Server reponse is incorrect");
        }

        return Ok(new KeysResponse()
        {
            Keys = response.Keys
        });
    }
}