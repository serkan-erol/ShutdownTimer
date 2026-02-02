/* A simple console application to set or cancel a system shutdown timer.
 * It allows users to specify a shutdown time in minutes or cancel a scheduled shutdown.
 * It uses the shutdown.exe command-line utility available in Windows.
 */

using System.Diagnostics;

// String array for the main options
string[] mainOptions = new string[5]
{ 
    "1 - Schedule shutdown",
    "2 - Cancel shutdown",
    "3 - Restart computer",
    "4 - Restart and go to BIOS (Requires 'Run as administrator')",
    "5 - Exit (ESC works too)\n"
};

// ConsoleKey array for valid main menu options
ConsoleKey[] mainKeys = new ConsoleKey[11]
{
    ConsoleKey.D1,
    ConsoleKey.NumPad1, // Digit 1 and NumPad 1 for option 1
    ConsoleKey.D2,
    ConsoleKey.NumPad2,
    ConsoleKey.D3,
    ConsoleKey.NumPad3,
    ConsoleKey.D4,
    ConsoleKey.NumPad4,
    ConsoleKey.D5,
    ConsoleKey.NumPad5, // Digit 5 and NumPad 5 for exit option
    ConsoleKey.Escape  // Escape key to exit, as well
};

// Display the main menu / Ask the user for an operation
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("\nChoose the operation you want to do:");
Console.ResetColor();
foreach (string mainOption in mainOptions)
{
    Console.WriteLine(mainOption);
}

// Read user input (a single key press) without displaying it on the console and get the Key info (Enum of ConsoleKey)
// Also, validate input for the main menu using the InputValidation function
ConsoleKey option = await InputValidation(mainKeys, mainOptions);

// Clear the console before next steps
//aaa Console.Clear();

// Schedule shutdown option
switch (option)
{
    // Call the ScheduleShutdown function
    case ConsoleKey.D1:
    case ConsoleKey.NumPad1:
        await ScheduleShutdown();
        break;

    // Call the CancelShutdown function
    case ConsoleKey.D2:
    case ConsoleKey.NumPad2:
        await CancelShutdown();
        break;

    // Call the RestartComputer function. Call it with biosFlag = false
    case ConsoleKey.D3:
    case ConsoleKey.NumPad3:
        await RestartComputer(false);
        break;

    // Call the RestartComputer function with the 'go to BIOS' option. Call it with biosFlag = true
    case ConsoleKey.D4:
    case ConsoleKey.NumPad4:
        await RestartComputer(true);
        break;

    // Exit option - do nothing, will exit the application
    case ConsoleKey.D5:
    case ConsoleKey.NumPad5:
    case ConsoleKey.Escape:
    default:
        Console.WriteLine("Exiting application...");
        break;
}

//aaa await Task.Delay(300); // Small delay to ensure the user kinda sees the final message
Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();
//aaa Console.Clear();

// ------------------------------------------------------------------------------------------------------------- //
// ------------------------------------------------- FUNCTIONS ------------------------------------------------- //
// ------------------------------------------------------------------------------------------------------------- //

// Function to schedule a shutdown
async Task ScheduleShutdown()
{
    try
    {
        // First, call the CancelShutdown function to cancel any existing scheduled shutdown or restart
        await CancelShutdown();

        // Read user input and validate it
        int shutdownTimeMins = await TimerInputValidation();
        // Convert minutes to seconds because the shutdown.exe takes the parameter as time in seconds 
        int shutdownTimeSecs = shutdownTimeMins * 60;

        // Create and configure a shutdown process
        Process shutdownProcess = new Process();
        shutdownProcess.StartInfo.FileName = "shutdown.exe";
        shutdownProcess.StartInfo.Arguments = $"/s /t {shutdownTimeSecs}";

        // Start the shutdown process
        shutdownProcess.Start();

        // Inform the user that the shutdown timer has been set
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
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message}");
    }
    finally
    {
        Console.ResetColor();
    }
}

// ------------------------------------------------------------------------------------------------------------- //

// Function to cancel a scheduled shutdown
async Task CancelShutdown()
{
    try
    {
        // Cancel any existing shutdown first
        Console.WriteLine("\nChecking if there is already a shutdown scheduled...");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("If there is, it will be cancelled!");
        Console.WriteLine("Cancelling shutdown...");

        // Create and configure a shutdown process
        Process cancelShutdownProcess = new Process();
        cancelShutdownProcess.StartInfo.FileName = "shutdown.exe";
        cancelShutdownProcess.StartInfo.Arguments = "/a";

        cancelShutdownProcess.Start();

        Console.ForegroundColor = ConsoleColor.Green;
        await Task.Delay(300); // Small delay to ensure the process has time to complete
        Console.WriteLine("Cancellation completed.");
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message}");
    }
    finally
    {
        Console.ResetColor();
    }
}

// ------------------------------------------------------------------------------------------------------------- //

// Function to restart the computer
async Task RestartComputer(bool biosFlag)
{
    try
    {
        // First, call the CancelShutdown function to cancel any existing scheduled shutdown or restart
        await CancelShutdown();

        // Read user input (a single key press) without displaying it on the console
        int restartMins = await TimerInputValidation();
        int restartSecs = restartMins * 60;

        Console.WriteLine("\nRestarting computer...");
        // Create and configure a restart process
        Process restartProcess = new Process();
        restartProcess.StartInfo.FileName = "shutdown.exe";

        // Build the arguments string based on biosFlag and immediateFlag
        string restartArguments = "/r" + (biosFlag ? " /fw" : " /f") + $" /t {restartSecs}";

        restartProcess.StartInfo.Arguments = restartArguments;

        Console.ForegroundColor = ConsoleColor.Red;
        restartProcess.Start();
        await Task.Delay(300); // Small delay to ensure the process has time to complete

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Restart scheduling completed.");
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message}");
    }
    finally
    {
        Console.ResetColor();
    }
}

// ------------------------------------------------------------------------------------------------------------- //

// Function to get valid input from the user with specified range and options
async Task<ConsoleKey> InputValidation(ConsoleKey[] validKeys, string[] inputOptions)
{
    try
    {
        // Read user input (a single key press) without displaying it on the console and get the Key info (Enum of ConsoleKey)
        ConsoleKey option = Console.ReadKey(true).Key;

        // Validate input: loop until the user provides a valid option
        while (Array.IndexOf(validKeys, option) == -1)
        {
            // Clear the console before re-displaying the menu
            //aaa Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nPlease choose a valid option.");
            Console.ResetColor();
            foreach (string inputOption in inputOptions)
            {
                Console.WriteLine(inputOption);
            }
            option = Console.ReadKey(true).Key;
        }
        return option;
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message}");
        Console.ResetColor();
        return ConsoleKey.Escape; // Return Escape in case of error
    }
    finally
    {
        Console.ResetColor();
    }
}

// Function get valid timer input from the user
async Task<int> TimerInputValidation()
{
    try
    {
        Console.WriteLine("\nHow many minutes do you need before shutdown/restart?");
        Console.WriteLine("Enter 0 (zero) for immediate shutdown/restart.");
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
            //aaa Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nPlease enter a valid positive number or 0 (zero).");
            Console.WriteLine("Entering 0 will shutdown/restart immediately.");
            Console.ResetColor();
            Console.Write("Minutes: ");
            shutdownTimeMins = int.TryParse(Console.ReadLine(), out shutdownTimeMins) ? shutdownTimeMins : -1;
        }

        return shutdownTimeMins;
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message}");
        Console.ResetColor();
        return -1; // Return -1 in case of error
    }
    finally
    {
        Console.ResetColor();
    }
}