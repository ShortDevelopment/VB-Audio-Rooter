// See https://aka.ms/new-console-template for more information
using ShortDev.DirectShow.VirtualDevices;

Console.WriteLine("Hello, World!");

// FilterRegistry.RegisterFilter<TestFilter>(register: false);
FilterRegistry.RegisterFilter<TestFilter>(register: true);
// FilterRegistry.RegisterFilter<TestFilter>("Test LK");

Console.WriteLine("Ok!");