using System.Diagnostics;

try
{
    Console.WriteLine("How many minutes do you need before shutdown? : ");

    // Read and validate input
    // If the input can't be parsed to an integer or is negative, the user is prompted to enter a valid number.
    if (!int.TryParse(Console.ReadLine(), out int shutdownTimeMins) || shutdownTimeMins < 0)
    {
        Console.WriteLine("Please enter a valid positive number.");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
        return;
    }

    // Convert minutes to seconds to give to shutdown.exe
    int shutdownTimeSecs =  shutdownTimeMins * 60;
    
    // Create and configure a shutdown process
    Process shutdownProcess = new Process();
    shutdownProcess.StartInfo.FileName = "shutdown.exe";
    shutdownProcess.StartInfo.Arguments = $"/s /t {shutdownTimeSecs}";

    shutdownProcess.Start();
    Console.WriteLine("Shutdown timer has been set.");
    
    // Still use shutdownTimeMins for the message to avoid unnecessary division
    switch (shutdownTimeMins)
    {
        case < 60:
            Console.WriteLine($"Computer will shutdown in {shutdownTimeMins} minutes");
            break;
        default:
            Console.WriteLine($"Computer will shutdown in {shutdownTimeMins / 60} hours " +
                              $"and {shutdownTimeMins % 60} minutes");
            break;
    }
    
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}