﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Sels.Core.Async.Test.Components.Queue
{
	public class WorkerQueue_EnqueueAsync
	{
		[Test, Timeout(10000)]
		public async Task DoesNotThrowWhenQueueIsNotAtMaxSize()
		{
			// Arrange
			var provider = TestHelper.GetTaskManagerContainer();
			await using var scope = provider.CreateAsyncScope();
			provider = scope.ServiceProvider;
			var taskManager = provider.GetRequiredService<ITaskManager>();
			await using var queue = new WorkerQueue<int>(taskManager, 1);
			Exception exception = null;

			// Act
			try
			{
				await queue.EnqueueAsync(1);
			}
			catch (Exception ex)
			{
				exception = ex;
			}

			// Assert
			Assert.IsNull(exception);
		}
		[Test, Timeout(10000)]
		public async Task DoesNotThrowWhenQueueDoesNotHaveMaxSize()
		{
			// Arrange
			var provider = TestHelper.GetTaskManagerContainer();
			await using var scope = provider.CreateAsyncScope();
			provider = scope.ServiceProvider;
			var taskManager = provider.GetRequiredService<ITaskManager>();
			await using var queue = new WorkerQueue<int>(taskManager);
			Exception exception = null;

            // Act
            try
            {
                await queue.EnqueueAsync(1);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(exception);
        }
		[Test, Timeout(10000)]
		public async Task ThrowsWhenQueueIsAtMaxSize()
		{
			// Arrange
			var provider = TestHelper.GetTaskManagerContainer();
			await using var scope = provider.CreateAsyncScope();
			provider = scope.ServiceProvider;
			var taskManager = provider.GetRequiredService<ITaskManager>();
			await using var queue = new WorkerQueue<int>(taskManager, 1);
			Exception exception = null;

			// Act
			_ = await queue.TryEnqueueAsync(1);
            try
            {
                await queue.EnqueueAsync(1);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
			Assert.That(exception, Is.AssignableTo<InvalidOperationException>());
        }
		[Test, Timeout(10000)]
		public async Task AddingToQueueIncreasesCount()
		{
			// Arrange
			const int amount = 5;
			var provider = TestHelper.GetTaskManagerContainer();
			await using var scope = provider.CreateAsyncScope();
			provider = scope.ServiceProvider;
			var taskManager = provider.GetRequiredService<ITaskManager>();
			await using var queue = new WorkerQueue<int>(taskManager);

			// Act
			for (int i = 0; i < amount; i++)
			{
				await queue.EnqueueAsync(i);
			}

			// Assert
			Assert.AreEqual(amount, queue.Count);
		}
		[Test, Timeout(10000)]
		public async Task ThrowsOperationCanceledExceptionWhenTokenIsCancelled()
		{
			// Arrange
			var provider = TestHelper.GetTaskManagerContainer();
			await using var scope = provider.CreateAsyncScope();
			provider = scope.ServiceProvider;
			var taskManager = provider.GetRequiredService<ITaskManager>();
			await using var queue = new WorkerQueue<int>(taskManager);
			var tokenSource = new CancellationTokenSource();
			Exception exception = null;

			// Act
			try
			{
				tokenSource.Cancel();
				await queue.EnqueueAsync(1, tokenSource.Token);
			}
			catch (Exception ex)
			{
				exception = ex;
			}

			// Assert
			Assert.IsNotNull(exception);
			Assert.That(exception, Is.AssignableTo<OperationCanceledException>());
		}
	}
}