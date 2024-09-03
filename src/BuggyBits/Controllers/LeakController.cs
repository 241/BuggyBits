using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Collections.Generic;

namespace BuggyBits.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LeakController : Controller
    {
        private static int _callCount = 0;
        private static int _threadCount = 0;
        private static List<byte[]> _memoryLeaks = new List<byte[]>();

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("LeakConnections")]
        public IActionResult LeakConnections()
        {
            _callCount = 0;
            MakeRequest();
            return View("LeakConnections");
        }

        private void MakeRequest()
        {
            if (_callCount >= 500)
                return;

            _callCount++;

            // Buggy code: HttpClient is not disposed
            HttpClient client = new HttpClient();
            client.GetAsync("https://www.bing.com").GetAwaiter().GetResult();

            // Wait for 1 second before making the next request
            Thread.Sleep(1000);
            Console.WriteLine($"Request {_callCount} completed");
            // Recursive call
            MakeRequest();
        }

        [HttpGet("LeakThreads")]
        public IActionResult LeakThreads()
        {
            _threadCount = 0;
            CreateNewThread();
            return View("LeakThreads");
        }

        private void CreateNewThread()
        {
            if (_threadCount >= 500)
                return;

            _threadCount++;

            // Start the stopwatch to measure thread start duration
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Create a new thread
            Thread newThread = new Thread(() =>
            {
                // Simulate some work
                Thread.Sleep(100000); // Sleep for 10 seconds
            });

            newThread.Start();
            // Stop the stopwatch and log the duration
            stopwatch.Stop();
            Console.WriteLine($"Thread {_threadCount} created in {stopwatch.ElapsedMilliseconds} ms");
            CreateNewThread();
        }

        [HttpGet("LeakMemory")]
        public IActionResult LeakMemory()
        {
            // Allocate 100 MB of memory
            byte[] memoryLeak = new byte[100 * 1024 * 1024];
            _memoryLeaks.Add(memoryLeak);

            Console.WriteLine($"Allocated 100 MB, total leaks: {_memoryLeaks.Count * 100} MB");

            return View("LeakMemory");
        }
    }
}