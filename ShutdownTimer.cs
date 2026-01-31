/* A simple console application to set or cancel a system shutdown timer.
 * It allows users to specify a shutdown time in minutes or cancel a scheduled shutdown.
 * It uses the shutdown.exe command-line utility available in Windows.
 */

using System.Diagnostics;

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("\nChoose the operation you want to do:");
Console.ResetColor();
Console.WriteLine("1 - Schedule shutdown with a timer");
Console.WriteLine("2 - Cancel shutdown\n");

// Read user input (a single key press) without displaying it on the console
ConsoleKeyInfo input = Console.ReadKey(true);

// Get the pressed key (Enum of ConsoleKey)
var option = input.Key; // D1 or NumPad1 for option 1, D2 or NumPad2 for option 2

// Validate input: loop until the user provides a valid option (1 or 2)
while (option != ConsoleKey.D1 &&       // Digit 1
       option != ConsoleKey.D2 &&       // Digit 2
       option != ConsoleKey.NumPad1 &&  // NumPad 1
       option != ConsoleKey.NumPad2)    // NumPad 2
{
    // Clear the console before re-displaying the menu
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Yellow;

    Console.WriteLine("\nPlease choose a valid option.");
    Console.ResetColor();
    Console.WriteLine("1 - Set shutdown with a timer");
    Console.WriteLine("2 - Cancel shutdown\n");

    input = Console.ReadKey(true);
    option = input.Key;
}

// Clear the console before next steps
Console.Clear();

if (option == ConsoleKey.D1 || 
    option == ConsoleKey.NumPad1)
{
    try
    {
        Console.WriteLine("Checking if there is already a shutdown scheduled...");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("If there is, it will be cancelled!");
        
        Console.ForegroundColor = ConsoleColor.Green;
        // Check and cancel any existing shutdown timer
        Process cancelExistingShutdownProcess = new Process();
        cancelExistingShutdownProcess.StartInfo.FileName = "shutdown.exe";
        cancelExistingShutdownProcess.StartInfo.Arguments = "/a";
        cancelExistingShutdownProcess.Start();
        await Task.Delay(300); // Small delay to ensure the process has time to complete
        Console.WriteLine("Cancellation completed.");
        Console.ResetColor();


        Console.WriteLine("\nHow many minutes do you need before shutdown?");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Entering '0' will shutdown the PC immediately!");
        Console.ResetColor();
        Console.Write("Minutes: ");

        // Read, parse, and validate the input
        // If the input can't be parsed to an integer shutdownTimeMins will be -1
        int shutdownTimeMins = int.TryParse(Console.ReadLine(), out shutdownTimeMins) ? shutdownTimeMins : -1;

        /* If the input can't be parsed to an integer (user entered a non-number value, like a String) 
         * or the user enters a negative value
         * the user is prompted to enter a valid number.
         */
        while (shutdownTimeMins < 0)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nPlease enter a valid positive number.");
            Console.ResetColor();
            Console.Write("Minutes: ");
            shutdownTimeMins = int.TryParse(Console.ReadLine(), out shutdownTimeMins) ? shutdownTimeMins : -1;
        }

        Console.Clear();

        // Convert minutes to seconds because the shutdown.exe takes the parameter as time in seconds 
        int shutdownTimeSecs = shutdownTimeMins * 60;

        // Create and configure a shutdown process
        Process shutdownProcess = new Process();
        shutdownProcess.StartInfo.FileName = "shutdown.exe";
        shutdownProcess.StartInfo.Arguments = $"/s /t {shutdownTimeSecs}";

        shutdownProcess.Start();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\nShutdown timer has been set.");

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
        Console.ResetColor();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message}");
        Console.ResetColor();
    }
    finally
    {
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
else if(option == ConsoleKey.D2 || 
        option == ConsoleKey.NumPad2) // option == 2
{
    try
    {
        Console.WriteLine("Cancelling shutdown...");

        // Create and configure a shutdown process
        Process cancelShutdownProcess = new Process();
        cancelShutdownProcess.StartInfo.FileName = "shutdown.exe";
        cancelShutdownProcess.StartInfo.Arguments = "/a";

        cancelShutdownProcess.Start();

        Console.ForegroundColor = ConsoleColor.Green;
        await Task.Delay(300); // Small delay to ensure the process has time to complete
        Console.WriteLine("Cancellation completed.");
        Console.ResetColor();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message}");
        Console.ResetColor();
    }
    finally
    {
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
Console.Clear();