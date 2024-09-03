using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Collections.Generic;
using System.Net;

namespace BuggyBits.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LeakController : Controller
    {
        private static int _callCount = 0;
        private static int _threadCount = 0;
        private static List<byte[]> _memoryLeaks = new List<byte[]>();
        private static List<int> _allocatedSizes = new List<int>();
        private static long _totalMemoryLeaks = 0;

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

            // Buggy code: WebClient is not disposed
            WebClient client = new WebClient();
            var response = client.DownloadString("https://www.bing.com");

            // Wait for 100 ms before making the next request
            Thread.Sleep(100);
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
            // Generate a random size between 85KB and 100MB
            Random random = new Random();
            int size = random.Next(85 * 1024, 100 * 1024 * 1024);

            // Allocate memory with the generated size
            byte[] memoryLeak = new byte[size];
            _memoryLeaks.Add(memoryLeak);
            _allocatedSizes.Add(size);
            _totalMemoryLeaks += size;

            // Log the size of the created array and the total memory leaks
            Console.WriteLine($"Allocated {size / 1024.0 / 1024.0:F2} MB, total leaks: {_totalMemoryLeaks / 1024.0 / 1024.0:F2} MB");

            // Pass the allocated sizes and total memory leaks to the view
            ViewBag.AllocatedSizes = _allocatedSizes;
            ViewBag.TotalMemoryLeaks = _totalMemoryLeaks;

            return View("LeakMemory");
        }
    }
}