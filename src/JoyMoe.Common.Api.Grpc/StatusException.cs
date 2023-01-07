using Grpc.Core;

namespace JoyMoe.Common.Api.Grpc;

public class StatusException : RpcException
{
    public StatusException(StatusCode status, string message) : base(new Status(status, message)) { }

    public StatusException(StatusCode status, string message, Metadata metadata) :
        base(new Status(status, message), metadata) { }
}
