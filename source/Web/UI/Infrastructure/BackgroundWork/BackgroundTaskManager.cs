using AspNetSkeleton.Service.Host.Core.Infrastructure.BackgroundWork;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace AspNetSkeleton.UI.Infrastructure.BackgroundWork
{
    // Based on: https://github.com/StephenCleary/AspNetBackgroundTasks
    //The MIT License (MIT)

    //Copyright(c) 2014 StephenCleary

    //Permission is hereby granted, free of charge, to any person obtaining a copy of
    //this software and associated documentation files (the "Software"), to deal in
    //the Software without restriction, including without limitation the rights to
    //use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
    //the Software, and to permit persons to whom the Software is furnished to do so,
    //subject to the following conditions:

    //The above copyright notice and this permission notice shall be included in all
    //copies or substantial portions of the Software.

    //THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    //IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
    //FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
    //COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
    //IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
    //CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

    /// <summary>
    /// A type that tracks background operations and notifies ASP.NET that they are still in progress.
    /// </summary>
    public sealed class BackgroundTaskManager : IRegisteredObject, IShutDownTokenAccessor
    {
        static readonly BackgroundTaskManager instance = new BackgroundTaskManager();
        
        public static BackgroundTaskManager Current => instance;

        /// <summary>
        /// A cancellation token that is set when ASP.NET is shutting down the app domain.
        /// </summary>
        private readonly CancellationTokenSource _shutdownCts;

        /// <summary>
        /// A countdown event that is incremented each time a task is registered and decremented each time it completes. When it reaches zero, we are ready to shut down the app domain. 
        /// </summary>
        private readonly CountdownEvent _ce;

        /// <summary>
        /// A task that completes after <see cref="_ce"/> reaches zero and the object has been unregistered.
        /// </summary>
        private readonly Task _done;

        /// <summary>
        /// Creates an instance that is registered with the ASP.NET runtime.
        /// </summary>
        public BackgroundTaskManager()
        {
            // Start the count at 1 and decrement it when ASP.NET notifies us we're shutting down.
            _ce = new CountdownEvent(1);

            _shutdownCts = new CancellationTokenSource();
            _shutdownCts.Token.Register(() => _ce.Decrease(), useSynchronizationContext: false);

            // Register the object.
            HostingEnvironment.RegisterObject(this);

            // When the count reaches zero (all tasks have completed and ASP.NET has notified us we are shutting down),
            //  then unregister this object, and then the _done task is completed.
            _done = _ce.WaitAsync().ContinueWith(
                _ => HostingEnvironment.UnregisterObject(this),
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }

        /// <summary>
        /// Gets a cancellation token that is set when ASP.NET is shutting down the app domain.
        /// </summary>
        public CancellationToken ShutDownToken => _shutdownCts.Token;

        void IRegisteredObject.Stop(bool immediate)
        {
            _shutdownCts.Cancel();

            if (immediate)
                _done.Wait();
        }

        /// <summary>
        /// Registers a task with the ASP.NET runtime. The task is unregistered when it completes.
        /// </summary>
        /// <param name="task">The task to register.</param>
        private void Register(Task task)
        {
            _ce.Increase();

            task.ContinueWith(
                _ => _ce.Decrease(),
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }

        /// <summary>
        /// Executes a background operation, registering it with ASP.NET.
        /// </summary>
        /// <param name="operation">The background operation.</param>
        public void Run(Action operation)
        {
            Register(Task.Run(operation));
        }

        public void Run(Func<Task> taskFactory)
        {
            Register(Task.Run(taskFactory));
        }
    }
}
