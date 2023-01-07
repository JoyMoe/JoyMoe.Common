using Grpc.Core;

namespace JoyMoe.Common.Api.Grpc;

public class GrpcValidationAsyncStreamReader<TRequest> : IAsyncStreamReader<TRequest>
{
    private readonly IAsyncStreamReader<TRequest> _inner;
    private readonly Func<TRequest, Task>         _validator;

    public GrpcValidationAsyncStreamReader(IAsyncStreamReader<TRequest> inner, Func<TRequest, Task> validator) {
        _inner     = inner;
        _validator = validator;
    }

    public async Task<bool> MoveNext(CancellationToken cancellationToken) {
        var success = await _inner.MoveNext(cancellationToken).ConfigureAwait(false);
        if (success) {
            await _validator.Invoke(Current).ConfigureAwait(false);
        }

        return success;
    }

    public TRequest Current => _inner.Current;
}
