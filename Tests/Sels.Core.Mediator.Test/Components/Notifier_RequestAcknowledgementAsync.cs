using Microsoft.Extensions.DependencyInjection;
using Sels.Core.Mediator.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Mediator.Test.Components
{
    public class Notifier_RequestAcknowledgementAsync
    {
        [Test]
        [Timeout(60000)]
        public async Task RequestIsRaisedToGlobalHandler()
        {
            // Arrange
            var request = "Hi I'm a request";
            var handlerMock = new Mock<IRequestHandler<string>>();
            handlerMock.Setup(x => x.TryAcknowledgeAsync(It.IsAny<IRequestHandlerContext>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                       .Returns(Task.FromResult(RequestAcknowledgement.Acknowledge()));
            var provider = TestHelper.GetTestContainer(x => x.AddRequestHandler(handlerMock.Object));
            var notifier = provider.GetRequiredService<INotifier>();

            // Act
            var result = await notifier.RequestAcknowledgementAsync<string>(this, request);

            // Assert
            Assert.That(result.Acknowledged, Is.True);
            handlerMock.Verify(x => x.TryAcknowledgeAsync(It.IsAny<IRequestHandlerContext>(), It.Is<string>(x => x.Equals(request)), It.IsAny<CancellationToken>()));
        }

        [Test]
        [Timeout(60000)]
        public async Task RequestIsRaisedToRuntimeHandler()
        {
            // Arrange
            var request = "Hi I'm a request";
            var handlerMock = new Mock<IRequestHandler<string>>();
            handlerMock.Setup(x => x.TryAcknowledgeAsync(It.IsAny<IRequestHandlerContext>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                       .Returns(Task.FromResult(RequestAcknowledgement.Acknowledge()));
            var provider = TestHelper.GetTestContainer();
            var notifier = provider.GetRequiredService<INotifier>();
            var subscriber = provider.GetRequiredService<IRequestSubscriptionManager>();

            // Act
            using var subscription = subscriber.Subscribe(handlerMock.Object);
            var result = await notifier.RequestAcknowledgementAsync<string>(this, request);

            // Assert
            Assert.That(result.Acknowledged, Is.True);
            handlerMock.Verify(x => x.TryAcknowledgeAsync(It.IsAny<IRequestHandlerContext>(), It.Is<string>(x => x.Equals(request)), It.IsAny<CancellationToken>()));
        }

        [Test]
        [Timeout(60000)]
        public async Task UnsubscribingStopsRuntimeHandlerFromReceivingRequests()
        {
            // Arrange
            var request = "Hi I'm a request";
            var handlerMock = new Mock<IRequestHandler<string>>();
            handlerMock.Setup(x => x.TryAcknowledgeAsync(It.IsAny<IRequestHandlerContext>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                       .Returns(Task.FromResult(RequestAcknowledgement.Acknowledge()));
            var provider = TestHelper.GetTestContainer();
            var notifier = provider.GetRequiredService<INotifier>();
            var subscriber = provider.GetRequiredService<IRequestSubscriptionManager>();

            // Act
            var subscription = subscriber.Subscribe(handlerMock.Object);
            subscription.Dispose();
            var result = await notifier.RequestAcknowledgementAsync<string>(this, request);

            // Assert
            Assert.That(result.Acknowledged, Is.False);
            handlerMock.Verify(x => x.TryAcknowledgeAsync(It.IsAny<IRequestHandlerContext>(), It.Is<string>(x => x.Equals(request)), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        [Timeout(60000)]
        public async Task CancellingTokenCancelsRequest()
        {
            // Arrange
            var request = "Hi I'm a request";
            var provider = TestHelper.GetTestContainer();
            var notifier = provider.GetRequiredService<INotifier>();
            var subscriber = provider.GetRequiredService<IRequestSubscriptionManager>();
            Exception exception = null;
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            // Act
            using var subscription = subscriber.Subscribe<string>(async (x, e, t) =>
            {
                await Task.Delay(5000, t);
                return RequestAcknowledgement.Reject();
            });
            var result = await notifier.RequestAsync<string, bool>(this, request);
            try
            {
                tokenSource.CancelAfter(1000);
                await notifier.RequestAcknowledgementAsync<string>(this, request, tokenSource.Token);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
        }

        [Test]
        [Timeout(60000)]
        public async Task RequestHandlersAreCalledInTheCorrectOrder()
        {
            // Arrange
            var priorities = new ushort?[] { null, 10, 3, 4, 0, 1, null };
            var expected = new uint?[] { 0, 1, 3, 4, 10, null, null };
            var results = new List<uint?>();
            var request = "Hi I'm a request";
            var provider = TestHelper.GetTestContainer(x => {
                foreach (var priority in priorities)
                {
                    var handlerPriority = priority;
                    x.AddRequestHandler<string>((c, e, t) =>
                    {
                        results.Add(handlerPriority);
                        return Task.FromResult(RequestAcknowledgement.Reject());
                    }, handlerPriority);
                }
            });
            var notifier = provider.GetRequiredService<INotifier>();

            // Act
            var result = await notifier.RequestAcknowledgementAsync<string>(this, request);

            // Assert
            Assert.That(result.Acknowledged, Is.False);
            CollectionAssert.AreEqual(expected, results);
        }

        [Test]
        [Timeout(60000)]
        public async Task OnlyOneHandlerCanRespondToRequest()
        {
            // Arrange
            var respondedCount = 0;
            var request = "Hi I'm a request";
            var provider = TestHelper.GetTestContainer(x => {
                x.AddRequestHandler<string>((c, e, t) =>
                {
                    var response = RequestAcknowledgement.Acknowledge();
                    respondedCount++;
                    return Task.FromResult(response);
                }, null)
                .AddRequestHandler<string>((c, e, t) =>
                {
                    var response = RequestAcknowledgement.Acknowledge();
                    respondedCount++;
                    return Task.FromResult(response);
                }, 5)
                .AddRequestHandler<string>((c, e, t) =>
                {
                    var response = RequestAcknowledgement.Acknowledge();
                    respondedCount++;
                    return Task.FromResult(response);
                }, 1);
            });
            var notifier = provider.GetRequiredService<INotifier>();

            // Act
            var result = await notifier.RequestAcknowledgementAsync<string>(this, request);

            // Assert
            Assert.That(result.Acknowledged, Is.True);
            Assert.That(respondedCount, Is.EqualTo(1));
        }
    }
}
