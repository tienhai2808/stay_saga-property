using Grpc.Core;
using Grpc.Property;
using PropertyGrpc = Grpc.Property.PropertyService;

namespace PropertyService.GrpcServices;

public sealed class PropertyGrpcService : PropertyGrpc.PropertyServiceBase
{
    public override Task<PongResponse> Ping(PingRequest request, ServerCallContext context)
    {
        return Task.FromResult(new PongResponse());
    }
}
