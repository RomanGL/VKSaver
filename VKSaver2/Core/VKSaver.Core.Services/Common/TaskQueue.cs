using System;
using System.Threading;
using System.Threading.Tasks;

namespace VKSaver.Core.Services.Common
{
    public sealed class TaskQueue
    {    
        public TaskQueue()
        {
            _semaphore = new SemaphoreSlim(1);
        }

        public async Task<T> Enqueue<T>(Func<Task<T>> taskGenerator)
        {
            await _semaphore.WaitAsync();
            try
            {
                return await taskGenerator();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private readonly SemaphoreSlim _semaphore;
    }
}
