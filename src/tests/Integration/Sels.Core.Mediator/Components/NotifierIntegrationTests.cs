using Microsoft.Extensions.DependencyInjection;
using Sels.Core.Mediator.Request;
using Sels.Core.Models;
using System.Text;

namespace Sels.Core.Mediator.IntegrationTests.Components
{
    /// <summary>Integration tests for <see cref="Notifier"/> backed by the real container and memory cache.</summary>
    [Collection(MediatorIntegrationCollection.Name)]
    [Trait("Category", "Integration")]
    public class NotifierIntegrationTests : MediatorIntegrationTestBase
    {
        /// <summary>Verifies RaiseEventAsync for global generic listener: raises event.</summary>
        [Fact(Timeout = MediatorIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task RaiseEventAsync_GlobalGenericListener_RaisesEvent()
        {
            // Arrange
            var @event = "Hi I'm an event";
            var listener = Substitute.For<IEventListener<string>>();
            listener.HandleAsync(Arg.Any<IEventListenerContext>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            ConfigureTestServices(x => x.AddEventListener(listener));
            var provider = ServiceProvider;
            var notifier = provider.GetRequiredService<INotifier>();

            // Act
            var result = await notifier.RaiseEventAsync(this, @event);

            // Assert
            Assert.Equal(1, result);
            _ = listener.Received().HandleAsync(Arg.Any<IEventListenerContext>(), Arg.Is<string>(x => x.Equals(@event)), Arg.Any<CancellationToken>());
        }

        /// <summary>Verifies RaiseEventAsync for global listener: raises event.</summary>
        [Fact(Timeout = MediatorIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task RaiseEventAsync_GlobalListener_RaisesEvent()
        {
            // Arrange
            var @event = "Hi I'm an event";
            var listener = Substitute.For<IEventListener>();
            listener.HandleAsync(Arg.Any<IEventListenerContext>(), Arg.Any<object>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            ConfigureTestServices(x => x.AddEventListener(listener));
            var provider = ServiceProvider;
            var notifier = provider.GetRequiredService<INotifier>();

            // Act
            var result = await notifier.RaiseEventAsync(this, @event);

            // Assert
            Assert.Equal(1, result);
            _ = listener.Received().HandleAsync(Arg.Any<IEventListenerContext>(), Arg.Is<object>(x => x.Equals(@event)), Arg.Any<CancellationToken>());
        }

        /// <summary>Verifies RaiseEventAsync for runtime generic listener: raises event.</summary>
        [Fact(Timeout = MediatorIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task RaiseEventAsync_RuntimeGenericListener_RaisesEvent()
        {
            // Arrange
            var @event = "Hi I'm an event";
            var listener = Substitute.For<IEventListener<string>>();
            listener.HandleAsync(Arg.Any<IEventListenerContext>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            var provider = ServiceProvider;
            var notifier = provider.GetRequiredService<INotifier>();
            var subscriber = provider.GetRequiredService<IEventSubscriber<string>>();

            // Act
            using var subscription = subscriber.Subscribe(listener);
            var result = await notifier.RaiseEventAsync(this, @event);

            // Assert
            Assert.Equal(1, result);
            _ = listener.Received().HandleAsync(Arg.Any<IEventListenerContext>(), Arg.Is<string>(x => x.Equals(@event)), Arg.Any<CancellationToken>());
        }

        /// <summary>Verifies RaiseEventAsync for runtime listener: raises event.</summary>
        [Fact(Timeout = MediatorIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task RaiseEventAsync_RuntimeListener_RaisesEvent()
        {
            // Arrange
            var @event = "Hi I'm an event";
            var listener = Substitute.For<IEventListener>();
            listener.HandleAsync(Arg.Any<IEventListenerContext>(), Arg.Any<object>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            var provider = ServiceProvider;
            var notifier = provider.GetRequiredService<INotifier>();
            var subscriber = provider.GetRequiredService<IEventSubscriber>();

            // Act
            using var subscription = subscriber.Subscribe(listener);
            var result = await notifier.RaiseEventAsync(this, @event);

            // Assert
            Assert.Equal(1, result);
            _ = listener.Received().HandleAsync(Arg.Any<IEventListenerContext>(), Arg.Is<object>(x => x.Equals(@event)), Arg.Any<CancellationToken>());
        }

        /// <summary>Verifies RaiseEventAsync for generic runtime listener unsubscribed: does not raise event.</summary>
        [Fact(Timeout = MediatorIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task RaiseEventAsync_GenericRuntimeListenerUnsubscribed_DoesNotRaiseEvent()
        {
            // Arrange
            var @event = "Hi I'm an event";
            var listener = Substitute.For<IEventListener<string>>();
            listener.HandleAsync(Arg.Any<IEventListenerContext>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            var provider = ServiceProvider;
            var notifier = provider.GetRequiredService<INotifier>();
            var subscriber = provider.GetRequiredService<IEventSubscriber<string>>();

            // Act
            var subscription = subscriber.Subscribe(listener);
            subscription.Dispose();
            var result = await notifier.RaiseEventAsync(this, @event);

            // Assert
            Assert.Equal(0, result);
            _ = listener.DidNotReceive().HandleAsync(Arg.Any<IEventListenerContext>(), Arg.Is<string>(x => x.Equals(@event)), Arg.Any<CancellationToken>());
        }

        /// <summary>Verifies RaiseEventAsync for runtime listener unsubscribed: does not raise event.</summary>
        [Fact(Timeout = MediatorIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task RaiseEventAsync_RuntimeListenerUnsubscribed_DoesNotRaiseEvent()
        {
            // Arrange
            var @event = "Hi I'm an event";
            var listener = Substitute.For<IEventListener>();
            listener.HandleAsync(Arg.Any<IEventListenerContext>(), Arg.Any<object>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            var provider = ServiceProvider;
            var notifier = provider.GetRequiredService<INotifier>();
            var subscriber = provider.GetRequiredService<IEventSubscriber>();

            // Act
            var subscription = subscriber.Subscribe(listener);
            subscription.Dispose();
            var result = await notifier.RaiseEventAsync(this, @event);

            // Assert
            Assert.Equal(0, result);
            _ = listener.DidNotReceive().HandleAsync(Arg.Any<IEventListenerContext>(), Arg.Is<string>(x => x.Equals(@event)), Arg.Any<CancellationToken>());
        }

        /// <summary>Verifies RaiseEventAsync for enlisted event: raises enlisted event.</summary>
        [Fact(Timeout = MediatorIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task RaiseEventAsync_EnlistedEvent_RaisesEnlistedEvent()
        {
            // Arrange
            var @event = "Hi I'm an event";
            var secondEvent = 1;
            var listener = Substitute.For<IEventListener>();
            listener.HandleAsync(Arg.Any<IEventListenerContext>(), Arg.Any<object>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            ConfigureTestServices(x => x.AddEventListener(listener));
            var provider = ServiceProvider;
            var notifier = provider.GetRequiredService<INotifier>();

            // Act
            var result = await notifier.RaiseEventAsync(this, @event, x => x.Enlist(secondEvent));

            // Assert
            Assert.Equal(2, result);
            _ = listener.Received(1).HandleAsync(Arg.Any<IEventListenerContext>(), Arg.Is<string>(x => x.Equals(@event)), Arg.Any<CancellationToken>());
            _ = listener.Received(1).HandleAsync(Arg.Any<IEventListenerContext>(), Arg.Is<int>(x => x.Equals(secondEvent)), Arg.Any<CancellationToken>());
        }

        /// <summary>Verifies RaiseEventAsync for listener enlists event: raises enlisted event.</summary>
        [Fact(Timeout = MediatorIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task RaiseEventAsync_ListenerEnlistsEvent_RaisesEnlistedEvent()
        {
            // Arrange
            var @event = "Hi I'm an event";
            var secondEvent = 1;
            var listener = Substitute.For<IEventListener<int>>();
            listener.HandleAsync(Arg.Any<IEventListenerContext>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            ConfigureTestServices(x => x.AddEventListener((c, e, t) =>
            {
                c.EnlistEvent(secondEvent);
                return Task.CompletedTask;
            }).AddEventListener(listener));
            var provider = ServiceProvider;
            var notifier = provider.GetRequiredService<INotifier>();

            // Act
            var result = await notifier.RaiseEventAsync(this, @event);

            // Assert
            Assert.Equal(2, result);
            _ = listener.Received(1).HandleAsync(Arg.Any<IEventListenerContext>(), Arg.Is<int>(x => x.Equals(secondEvent)), Arg.Any<CancellationToken>());
        }

        /// <summary>Verifies RaiseEventAsync for derived event: raises event as base type.</summary>
        [Fact(Timeout = MediatorIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task RaiseEventAsync_DerivedEvent_RaisesEventAsBaseType()
        {
            // Arrange
            var @event = new List<string>() { "Hello I'm an event" };
            var listener = Substitute.For<IEventListener<List<string>>>();
            listener.HandleAsync(Arg.Any<IEventListenerContext>(), Arg.Any<List<string>>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            var baseListener = Substitute.For<IEventListener<IEnumerable<string>>>();
            baseListener.HandleAsync(Arg.Any<IEventListenerContext>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            ConfigureTestServices(x => x.AddEventListener(listener).AddEventListener(baseListener));
            var provider = ServiceProvider;
            var notifier = provider.GetRequiredService<INotifier>();

            // Act
            var result = await notifier.RaiseEventAsync(this, @event, x => x.AlsoRaiseAs<IEnumerable<string>>());

            // Assert
            Assert.Equal(2, result);
            _ = listener.Received(1).HandleAsync(Arg.Any<IEventListenerContext>(), Arg.Is<List<string>>(x => x.Equals(@event)), Arg.Any<CancellationToken>());
            _ = baseListener.Received(1).HandleAsync(Arg.Any<IEventListenerContext>(), Arg.Is<IEnumerable<string>>(x => x.Equals(@event)), Arg.Any<CancellationToken>());
        }

        /// <summary>Verifies RaiseEventAsync for token cancelled: cancels event.</summary>
        [Fact(Timeout = MediatorIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task RaiseEventAsync_TokenCancelled_CancelsEvent()
        {
            // Arrange
            var @event = "Hi I'm an event";
            var provider = ServiceProvider;
            var notifier = provider.GetRequiredService<INotifier>();
            var subscriber = provider.GetRequiredService<IEventSubscriber>();
            Exception exception = null;
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            // Act
            using var subscription = subscriber.Subscribe((x, e, t) => Task.Delay(5000, t));
            try
            {
                tokenSource.CancelAfter(1000);
                await notifier.RaiseEventAsync(this, @event, tokenSource.Token);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.NotNull(exception);
        }

        /// <summary>Verifies RaiseEventAsync for ignore exceptions option: ignores listener exceptions.</summary>
        [Fact(Timeout = MediatorIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task RaiseEventAsync_IgnoreExceptionsOption_IgnoresListenerExceptions()
        {
            // Arrange
            var @event = "Hi I'm an event";
            var provider = ServiceProvider;
            var notifier = provider.GetRequiredService<INotifier>();
            var subscriber = provider.GetRequiredService<IEventSubscriber>();
            Exception exception = null;
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            // Act
            using var subscription = subscriber.Subscribe((x, e, t) => Task.Delay(5000, t));
            try
            {
                tokenSource.CancelAfter(1000);
                await notifier.RaiseEventAsync(this, @event, x => x.WithOptions(EventOptions.IgnoreExceptions), tokenSource.Token);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.Null(exception);
        }

        /// <summary>Verifies RaiseEventAsync for fire and forget option: launches background task.</summary>
        [Fact(Timeout = MediatorIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task RaiseEventAsync_FireAndForgetOption_LaunchesBackgroundTask()
        {
            // Arrange
            var @event = "Hi I'm an event";
            var provider = ServiceProvider;
            var notifier = provider.GetRequiredService<INotifier>();
            var subscriber = provider.GetRequiredService<IEventSubscriber>();
            Exception exception = null;
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            // Act
            using var subscription = subscriber.Subscribe((x, e, t) => Task.Delay(5000, t));
            try
            {
                tokenSource.CancelAfter(1000);
                await notifier.RaiseEventAsync(this, @event, x => x.WithOptions(EventOptions.FireAndForget), tokenSource.Token);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.Null(exception);
        }

        /// <summary>Verifies RaiseEventAsync for prioritized listeners: calls listeners in order.</summary>
        [Fact(Timeout = MediatorIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task RaiseEventAsync_PrioritizedListeners_CallsListenersInOrder()
        {
            // Arrange
            var priorities = new byte?[] { null, 10, 3, 4, 0, 1, null };
            var expected = new uint?[] { 0, 1, 3, 4, 10, null, null };
            var @event = "Hi I'm an event";
            var results = new List<uint?>();
            ConfigureTestServices(x =>
            {
                foreach (var priority in priorities)
                {
                    var eventPriority = priority;
                    x.AddEventListener((c, e, t) =>
                    {
                        results.Add(eventPriority);
                        return Task.CompletedTask;
                    }, eventPriority);
                }
            });
            var provider = ServiceProvider;
            var notifier = provider.GetRequiredService<INotifier>();

            // Act
            var result = await notifier.RaiseEventAsync(this, @event);

            // Assert
            Assert.Equal(priorities.Length, result);
            Assert.Equal(expected, results);
        }

        /// <summary>Verifies RaiseEventAsync for parallel listeners: executes expected amount in parallel.</summary>
        [Theory(Timeout = MediatorIntegrationTestConstants.TestTimeoutMilliseconds)]
        [InlineData(1)]
        [InlineData(0)]
        [InlineData(10)]
        [InlineData(100)]
        public async Task RaiseEventAsync_ParallelListeners_ExecutesExpectedAmountInParallel(int amount)
        {
            // Arrange
            var @event = "Hi I'm an event";
            ConfigureTestServices(x =>
            {
                foreach (var i in Enumerable.Range(1, amount))
                {
                    x.AddEventListener((c, e, t) => c.WaitForCommitAsync());
                }
            });
            var provider = ServiceProvider;
            var notifier = provider.GetRequiredService<INotifier>();

            // Act
            var result = await notifier.RaiseEventAsync(this, @event, x => x.WithOptions(EventOptions.AllowParallelExecution));

            // Assert
            Assert.Equal(amount, result);
        }

        /// <summary>Verifies RaiseEventAsync for listeners wait for transaction: raises event.</summary>
        [Theory(Timeout = MediatorIntegrationTestConstants.TestTimeoutMilliseconds)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(7)]
        [InlineData(10)]
        public async Task RaiseEventAsync_ListenersWaitForTransaction_RaisesEvent(int amount)
        {
            // Arrange
            var @event = "Hi I'm an event";
            ConfigureTestServices(x =>
            {
                foreach (var i in Enumerable.Range(1, amount))
                {
                    var waitForTransaction = i % 2 == 0;
                    x.AddEventListener(async (c, e, t) => {
                        if (waitForTransaction) await c.WaitForCommitAsync();
                    });
                }
            });
            var provider = ServiceProvider;
            var notifier = provider.GetRequiredService<INotifier>();

            // Act
            var result = await notifier.RaiseEventAsync(this, @event, x => x.WithOptions(EventOptions.AllowParallelExecution));

            // Assert
            Assert.Equal(amount, result);
        }

        /// <summary>Verifies RaiseEventAsync for listener fails in transaction: cancels transaction.</summary>
        [Fact(Timeout = MediatorIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task RaiseEventAsync_ListenerFailsInTransaction_CancelsTransaction()
        {
            // Arrange
            var @event = "Hi I'm an event";
            bool completedAfterTransaction = false;
            ConfigureTestServices(x =>
            {
                x.AddEventListener(async (c, e, t) =>
                {
                    await c.WaitForCommitAsync();
                    completedAfterTransaction = true;
                }, 0)
                .AddEventListener((c, e, t) =>
                {
                    throw new InvalidOperationException();
                }, 1).AddEventListener(async (c, e, t) =>
                {
                    await c.WaitForCommitAsync();
                    completedAfterTransaction = true;
                }, null);
            });
            var provider = ServiceProvider;
            var notifier = provider.GetRequiredService<INotifier>();
            Exception exception = null;

            // Act
            try
            {
                await notifier.RaiseEventAsync(this, @event, x => x.WithOptions(EventOptions.AllowParallelExecution));
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.NotNull(exception);
            Assert.False(completedAfterTransaction);
        }

        /// <summary>Request without a response used by the acknowledgement tests.</summary>
        public class NullRequest : IRequest
        {

        }

        /// <summary>Verifies RequestAcknowledgementAsync for global handler: raises request.</summary>
        [Fact(Timeout = MediatorIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task RequestAcknowledgementAsync_GlobalHandler_RaisesRequest()
        {
            // Arrange
            var request = new NullRequest();
            var handler = Substitute.For<IRequestHandler<NullRequest>>();
            handler.TryAcknowledgeAsync(Arg.Any<IRequestHandlerContext>(), Arg.Any<NullRequest>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(RequestAcknowledgement.Acknowledge()));
            ConfigureTestServices(x => x.AddRequestHandler(handler));
            var provider = ServiceProvider;
            var notifier = provider.GetRequiredService<INotifier>();

            // Act
            var result = await notifier.RequestAcknowledgementAsync(this, request);

            // Assert
            Assert.True(result.Acknowledged);
            _ = handler.Received().TryAcknowledgeAsync(Arg.Any<IRequestHandlerContext>(), Arg.Is<NullRequest>(x => x.Equals(request)), Arg.Any<CancellationToken>());
        }

        /// <summary>Verifies RequestAcknowledgementAsync for runtime handler: raises request.</summary>
        [Fact(Timeout = MediatorIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task RequestAcknowledgementAsync_RuntimeHandler_RaisesRequest()
        {
            // Arrange
            var request = new NullRequest();
            var handler = Substitute.For<IRequestHandler<NullRequest>>();
            handler.TryAcknowledgeAsync(Arg.Any<IRequestHandlerContext>(), Arg.Any<NullRequest>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(RequestAcknowledgement.Acknowledge()));
            var provider = ServiceProvider;
            var notifier = provider.GetRequiredService<INotifier>();
            var subscriber = provider.GetRequiredService<IRequestSubscriptionManager>();

            // Act
            using var subscription = subscriber.Subscribe(handler);
            var result = await notifier.RequestAcknowledgementAsync(this, request);

            // Assert
            Assert.True(result.Acknowledged);
            _ = handler.Received().TryAcknowledgeAsync(Arg.Any<IRequestHandlerContext>(), Arg.Is<NullRequest>(x => x.Equals(request)), Arg.Any<CancellationToken>());
        }

        /// <summary>Verifies RequestAcknowledgementAsync for runtime handler unsubscribed: does not raise request.</summary>
        [Fact(Timeout = MediatorIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task RequestAcknowledgementAsync_RuntimeHandlerUnsubscribed_DoesNotRaiseRequest()
        {
            // Arrange
            var request = new NullRequest();
            var handler = Substitute.For<IRequestHandler<NullRequest>>();
            handler.TryAcknowledgeAsync(Arg.Any<IRequestHandlerContext>(), Arg.Any<NullRequest>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(RequestAcknowledgement.Acknowledge()));
            var provider = ServiceProvider;
            var notifier = provider.GetRequiredService<INotifier>();
            var subscriber = provider.GetRequiredService<IRequestSubscriptionManager>();

            // Act
            var subscription = subscriber.Subscribe(handler);
            subscription.Dispose();
            var result = await notifier.RequestAcknowledgementAsync(this, request);

            // Assert
            Assert.False(result.Acknowledged);
            _ = handler.DidNotReceive().TryAcknowledgeAsync(Arg.Any<IRequestHandlerContext>(), Arg.Is<NullRequest>(x => x.Equals(request)), Arg.Any<CancellationToken>());
        }

        /// <summary>Verifies RequestAcknowledgementAsync for token cancelled: cancels request.</summary>
        [Fact(Timeout = MediatorIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task RequestAcknowledgementAsync_TokenCancelled_CancelsRequest()
        {
            // Arrange
            var request = new NullRequest();
            var provider = ServiceProvider;
            var notifier = provider.GetRequiredService<INotifier>();
            var subscriber = provider.GetRequiredService<IRequestSubscriptionManager>();
            Exception exception = null;
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            // Act
            using var subscription = subscriber.Subscribe<NullRequest>(async (x, e, t) =>
            {
                await Task.Delay(5000, t);
                return RequestAcknowledgement.Reject();
            });
            var result = await notifier.RequestAcknowledgementAsync(this, request);
            try
            {
                tokenSource.CancelAfter(1000);
                await notifier.RequestAcknowledgementAsync(this, request, tokenSource.Token);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.NotNull(exception);
        }

        /// <summary>Verifies RequestAcknowledgementAsync for prioritized handlers: calls handlers in order.</summary>
        [Fact(Timeout = MediatorIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task RequestAcknowledgementAsync_PrioritizedHandlers_CallsHandlersInOrder()
        {
            // Arrange
            var priorities = new byte?[] { null, 10, 3, 4, 0, 1, null };
            var expected = new uint?[] { 0, 1, 3, 4, 10, null, null };
            var results = new List<uint?>();
            var request = new NullRequest();
            ConfigureTestServices(x => {
                foreach (var priority in priorities)
                {
                    var handlerPriority = priority;
                    x.AddRequestHandler<NullRequest>((c, e, t) =>
                    {
                        results.Add(handlerPriority);
                        return Task.FromResult(RequestAcknowledgement.Reject());
                    }, handlerPriority);
                }
            });
            var provider = ServiceProvider;
            var notifier = provider.GetRequiredService<INotifier>();

            // Act
            var result = await notifier.RequestAcknowledgementAsync(this, request);

            // Assert
            Assert.False(result.Acknowledged);
            Assert.Equal(expected, results);
        }

        /// <summary>Verifies RequestAcknowledgementAsync for multiple handlers: only one responds.</summary>
        [Fact(Timeout = MediatorIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task RequestAcknowledgementAsync_MultipleHandlers_OnlyOneResponds()
        {
            // Arrange
            var respondedCount = 0;
            var request = new NullRequest();
            ConfigureTestServices(x => {
                x.AddRequestHandler<NullRequest>((c, e, t) =>
                {
                    var response = RequestAcknowledgement.Acknowledge();
                    respondedCount++;
                    return Task.FromResult(response);
                }, null)
                .AddRequestHandler<NullRequest>((c, e, t) =>
                {
                    var response = RequestAcknowledgement.Acknowledge();
                    respondedCount++;
                    return Task.FromResult(response);
                }, 5)
                .AddRequestHandler<NullRequest>((c, e, t) =>
                {
                    var response = RequestAcknowledgement.Acknowledge();
                    respondedCount++;
                    return Task.FromResult(response);
                }, 1);
            });
            var provider = ServiceProvider;
            var notifier = provider.GetRequiredService<INotifier>();

            // Act
            var result = await notifier.RequestAcknowledgementAsync(this, request);

            // Assert
            Assert.True(result.Acknowledged);
            Assert.Equal(1, respondedCount);
        }

        /// <summary>Request with an empty response used by the request tests.</summary>
        public class NullRequest_RequestAsync : IRequest<Null>
        {

        }

        /// <summary>Verifies RequestAsync for global handler: raises request.</summary>
        [Fact(Timeout = MediatorIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task RequestAsync_GlobalHandler_RaisesRequest()
        {
            // Arrange
            var request = new NullRequest_RequestAsync();
            var handler = Substitute.For<IRequestHandler<NullRequest_RequestAsync, Null>>();
            handler.TryRespondAsync(Arg.Any<IRequestHandlerContext>(), Arg.Any<NullRequest_RequestAsync>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(RequestResponse<Null>.Success(Null.Value)));
            ConfigureTestServices(x => x.AddRequestHandler(handler));
            var provider = ServiceProvider;
            var notifier = provider.GetRequiredService<INotifier>();

            // Act
            var result = await notifier.RequestAsync(this, request);

            // Assert
            Assert.True(result.Completed);
            _ = handler.Received().TryRespondAsync(Arg.Any<IRequestHandlerContext>(), Arg.Is<NullRequest_RequestAsync>(x => x.Equals(request)), Arg.Any<CancellationToken>());
        }

        /// <summary>Verifies RequestAsync for runtime handler: raises request.</summary>
        [Fact(Timeout = MediatorIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task RequestAsync_RuntimeHandler_RaisesRequest()
        {
            // Arrange
            var request = new NullRequest_RequestAsync();
            var handler = Substitute.For<IRequestHandler<NullRequest_RequestAsync, Null>>();
            handler.TryRespondAsync(Arg.Any<IRequestHandlerContext>(), Arg.Any<NullRequest_RequestAsync>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(RequestResponse<Null>.Success(Null.Value)));
            var provider = ServiceProvider;
            var notifier = provider.GetRequiredService<INotifier>();
            var subscriber = provider.GetRequiredService<IRequestSubscriptionManager>();

            // Act
            using var subscription = subscriber.Subscribe(handler);
            var result = await notifier.RequestAsync(this, request);

            // Assert
            Assert.True(result.Completed);
            _ = handler.Received().TryRespondAsync(Arg.Any<IRequestHandlerContext>(), Arg.Is<NullRequest_RequestAsync>(x => x.Equals(request)), Arg.Any<CancellationToken>());
        }

        /// <summary>Verifies RequestAsync for runtime handler unsubscribed: does not raise request.</summary>
        [Fact(Timeout = MediatorIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task RequestAsync_RuntimeHandlerUnsubscribed_DoesNotRaiseRequest()
        {
            // Arrange
            var request = new NullRequest_RequestAsync();
            var handler = Substitute.For<IRequestHandler<NullRequest_RequestAsync, Null>>();
            handler.TryRespondAsync(Arg.Any<IRequestHandlerContext>(), Arg.Any<NullRequest_RequestAsync>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(RequestResponse<Null>.Success(Null.Value)));
            var provider = ServiceProvider;
            var notifier = provider.GetRequiredService<INotifier>();
            var subscriber = provider.GetRequiredService<IRequestSubscriptionManager>();

            // Act
            var subscription = subscriber.Subscribe(handler);
            subscription.Dispose();
            var result = await notifier.RequestAsync(this, request);

            // Assert
            Assert.False(result.Completed);
            _ = handler.DidNotReceive().TryRespondAsync(Arg.Any<IRequestHandlerContext>(), Arg.Is<NullRequest_RequestAsync>(x => x.Equals(request)), Arg.Any<CancellationToken>());
        }

        /// <summary>Verifies RequestAsync for token cancelled: cancels request.</summary>
        [Fact(Timeout = MediatorIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task RequestAsync_TokenCancelled_CancelsRequest()
        {
            // Arrange
            var request = new NullRequest_RequestAsync();
            var provider = ServiceProvider;
            var notifier = provider.GetRequiredService<INotifier>();
            var subscriber = provider.GetRequiredService<IRequestSubscriptionManager>();
            Exception exception = null;
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            // Act
            using var subscription = subscriber.Subscribe<NullRequest_RequestAsync, Null>(async (x, e, t) =>
            {
                await Task.Delay(5000, t);
                return RequestResponse<Null>.Reject();
            });
            var result = await notifier.RequestAsync(this, request);
            try
            {
                tokenSource.CancelAfter(1000);
                await notifier.RequestAsync(this, request, tokenSource.Token);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.NotNull(exception);
        }

        /// <summary>Verifies RequestAsync for prioritized handlers: calls handlers in order.</summary>
        [Fact(Timeout = MediatorIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task RequestAsync_PrioritizedHandlers_CallsHandlersInOrder()
        {
            // Arrange
            var priorities = new byte?[] { null, 10, 3, 4, 0, 1, null };
            var expected = new uint?[] { 0, 1, 3, 4, 10, null, null };
            var results = new List<uint?>();
            var request = new NullRequest_RequestAsync();
            ConfigureTestServices(x => {
                foreach (var priority in priorities)
                {
                    var handlerPriority = priority;
                    x.AddRequestHandler<NullRequest_RequestAsync, Null>((c, e, t) =>
                    {
                        results.Add(handlerPriority);
                        return Task.FromResult(RequestResponse<Null>.Reject());
                    }, handlerPriority);
                }
            });
            var provider = ServiceProvider;
            var notifier = provider.GetRequiredService<INotifier>();

            // Act
            var result = await notifier.RequestAsync(this, request);

            // Assert
            Assert.False(result.Completed);
            Assert.Equal(expected, results);
        }

        /// <summary>Verifies RequestAsync for multiple handlers: only one responds.</summary>
        [Fact(Timeout = MediatorIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task RequestAsync_MultipleHandlers_OnlyOneResponds()
        {
            // Arrange
            var respondedCount = 0;
            var request = new NullRequest_RequestAsync();
            ConfigureTestServices(x => {
                x.AddRequestHandler<NullRequest_RequestAsync, Null>((c, e, t) =>
                {
                    var response = RequestResponse<Null>.Success(Null.Value);
                    respondedCount++;
                    return Task.FromResult(response);
                }, null)
                .AddRequestHandler< NullRequest_RequestAsync, Null>((c, e, t) =>
                {
                    var response = RequestResponse<Null>.Success(Null.Value);
                    respondedCount++;
                    return Task.FromResult(response);
                }, 5)
                .AddRequestHandler<NullRequest_RequestAsync, Null>((c, e, t) =>
                {
                    var response = RequestResponse<Null>.Success(Null.Value);
                    respondedCount++;
                    return Task.FromResult(response);
                }, 1);
            });
            var provider = ServiceProvider;
            var notifier = provider.GetRequiredService<INotifier>();

            // Act
            var result = await notifier.RequestAsync(this, request);

            // Assert
            Assert.True(result.Completed);
            Assert.Equal(1, respondedCount);
        }
    }
}
