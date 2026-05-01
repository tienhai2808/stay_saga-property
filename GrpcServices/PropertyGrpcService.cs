using Grpc.Core;
using StaySaga.GRPC.Property;
using PropertyGrpc = StaySaga.GRPC.Property.PropertyService;

namespace PropertyService.GrpcServices;

public sealed class PropertyGrpcService : PropertyGrpc.PropertyServiceBase
{
    public override Task<PongResponse> Ping(PingRequest request, ServerCallContext context)
    {
        return Task.FromResult(new PongResponse());
    }
}
