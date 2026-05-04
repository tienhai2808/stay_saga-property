using Grpc.Core;
using Grpc.Property;
using PropertyGrpc = Grpc.Property.PropertyService;

namespace PropertyService.Services;

public sealed class GrpcService : PropertyGrpc.PropertyServiceBase
{
    public override Task<PongResponse> Ping(PingRequest request, ServerCallContext context)
    {
        return Task.FromResult(new PongResponse());
    }
}