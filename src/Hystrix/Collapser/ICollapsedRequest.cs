namespace Netflix.Hystrix
{
    using System;

    /// <summary>
    /// A request argument RequestArgumentType that was collapsed for batch processing and needs a response ResponseType set on it by the <code>executeBatch</code> implementation.
    /// </summary>
    /// <typeparam name="TResponseType"></typeparam>
    /// <typeparam name="TRequestArgumentType"></typeparam>
    public interface ICollapsedRequest<TResponseType, TRequestArgumentType>
    {
        /// <summary>
        /// The request argument passed into the {@link HystrixCollapser} instance constructor which was then collapsed.
        /// </summary>
        TRequestArgumentType Argument { get; }

        /// <summary>
        /// When set any client thread blocking on get() will immediately be unblocked and receive the response.
        /// </summary>
        /// <param name="response"></param>
        void SetResponse(TResponseType response);

        /// <summary>
        /// When set any client thread blocking on get() will immediately be unblocked and receive the exception.
        /// </summary>
        /// <param name="exception"></param>
        void SetException(System.Exception exception);
    }
}
