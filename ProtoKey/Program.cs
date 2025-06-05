using System.Threading.Channels;
using ProtoKey.Application.Save;
using ProtoKey.Application.Storage;
using ProtoKey.Application.Storage.Operations;
using ProtoKey.Application.Validator;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

Channel<Command> commandChannel = Channel.CreateUnbounded<Command>();
Channel<Command> saveChannel = Channel.CreateUnbounded<Command>();
Channel<Response> responseChannel = Channel.CreateUnbounded<Response>();
ProtoKeyValidator protoKeyValidator = new();

builder.Services.AddSingleton(commandChannel);
builder.Services.AddSingleton(responseChannel);
builder.Services.AddSingleton(protoKeyValidator);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

ProtoKeyStorage storage = new(
    commandChannel,
    saveChannel,
    responseChannel,
    protoKeyValidator);
SaveService saveService = new(saveChannel);

storage.InitStorage(saveService.LoadData());

Task.Run(storage.RunAsync);
Task.Run(saveService.RunAsync);

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
