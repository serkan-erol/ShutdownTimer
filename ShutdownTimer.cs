/* A simple console application to schedule or cancel a system shutdown with a timer.
 * It allows users to specify a shutdown time in minutes or cancel a scheduled shutdown.
 * It uses the shutdown.exe command-line utility available in Windows.
 * Now, with restart and restart to BIOS options!
 */

using System.Diagnostics;

// String array for the main options
string[] mainOptions = new string[5]
{ 
    " 1 - Schedule shutdown",
    " 2 - Cancel shutdown",
    " 3 - Schedule restart",
    " 4 - Restart now, and go to BIOS (Requires 'Run as administrator')",
    " 5 - Exit (ESC works, too!)"
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
    ConsoleKey.Escape   // Escape key to exit, as well
};

// To use as a parameter for some functions
const string callerMainName = "shutdown/restart";

// Display the main menu / Ask the user for an operation
Console.WriteLine($"\n Note: Trying to schedule a new {callerMainName} (options 1, 3, and 4) ");
Console.WriteLine($" will cancel the existing {callerMainName} first, if there is one");
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine(" Choose the operation you want to do:");
Console.ResetColor();

foreach (string mainOption in mainOptions)
{
    Console.WriteLine(mainOption);
}

// Read user input (a single key press) without displaying it on the console and get the Key info (Enum of ConsoleKeyInfo.Key and/or ConsoleKey)
// Also, validate input for the main menu using the InputValidation function
ConsoleKey option = await InputValidation(mainKeys, mainOptions);

// Clear the console before next steps
Console.Clear();

// Forward to the corresponding option
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
        /* Since the user called it directly, we can not know if it's a shutdown or restart 
         * that, they are trying to cancel. 
         * So we use both in the message.
         */
        await CancelShutdown(callerMainName); 
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

    // Exit option - will exit the application immediately
    case ConsoleKey.D5:
    case ConsoleKey.NumPad5:
    case ConsoleKey.Escape:
    default:
        Environment.Exit(0); // Directly exit the application and clear the console
        break;
}

// Exiting message for options other than 5 (exit)
Console.ResetColor();
Console.Write("\n Press any key to exit...");
Console.ReadKey(); // Wait for a key press before closing the console
Console.Clear();

// ------------------------------------------------------------------------------------------------------------- //
// ------------------------------------------------- FUNCTIONS ------------------------------------------------- //
// ------------------------------------------------------------------------------------------------------------- //

// Function to schedule a shutdown
async Task ScheduleShutdown()
{
    const string functionName = "shutdown";

    try
    {
        // First, call the CancelShutdown function to cancel any existing scheduled shutdown or restart
        await CancelShutdown(functionName);

        // Read user input and validate it for the timer
        int shutdownTimeMins = await TimerInputValidation(functionName); // Call it with "shutdown" as the caller function name
        // Convert minutes to seconds because the shutdown.exe takes the parameter as time in seconds 
        int shutdownTimeSecs = shutdownTimeMins * 60;

        // Create and configure a shutdown process
        Process shutdownProcess = new Process();
        shutdownProcess.StartInfo.FileName = "shutdown.exe";
        shutdownProcess.StartInfo.Arguments = $"/s /t {shutdownTimeSecs}";

        // Settings to catch the output of the process to check if there was not a shutdown scheduled
        shutdownProcess.StartInfo.RedirectStandardError = true;  // Redirect the error output
        shutdownProcess.StartInfo.UseShellExecute = false; // Required for redirection
        shutdownProcess.StartInfo.CreateNoWindow = true; // Hide the process window

        // Start the shutdown process
        shutdownProcess.Start();

        // Read the error output of the process to catch any errors
        string errorOutput = await shutdownProcess.StandardError.ReadToEndAsync();
        shutdownProcess.WaitForExit();

        // Check if there was any error output
        if (!string.IsNullOrWhiteSpace(errorOutput))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($" Warning: An unexpected error occurred while trying to schedule the shutdown.");
            Console.WriteLine($" Process exit code: {shutdownProcess.ExitCode}");
            Console.WriteLine($" Error output: {errorOutput}");
            return; // Exit the function if there was an error
        }
        // Successful shutdown scheduling. Inform the user about the successful scheduling and print the timer info.
        else if (shutdownProcess.ExitCode == 0)
        {
            // Let the user know a shutdown is scheduled and print the timer info in hours/minutes format
            PrintTimer(shutdownTimeMins, functionName);
        }
        else // If there is an unexpected error, inform the user
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($" Warning: An unexpected error occurred while trying to schedule the {functionName}.");
            Console.WriteLine($" Process exit code: {shutdownProcess.ExitCode}");
            Console.WriteLine($" Error output: {errorOutput}");
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($" Error: {ex.Message}");
    }
    finally
    {
        Console.ResetColor();
    }
}

// ------------------------------------------------------------------------------------------------------------- //

// Function to cancel a scheduled shutdown
async Task CancelShutdown(string callerFunctionName)
{
    try
    {
        // Let the user know what is happening
        Console.WriteLine($"\n Checking if there is already a {callerFunctionName} scheduled...");
        Console.WriteLine(" If there is, it will be cancelled!");
        Console.WriteLine($" Cancelling {callerFunctionName}...");

        // Create and configure a shutdown process
        Process cancelShutdownProcess = new Process();
        cancelShutdownProcess.StartInfo.FileName = "shutdown.exe";
        cancelShutdownProcess.StartInfo.Arguments = "/a";

        // Settings to catch the output of the process to check if there was not a shutdown scheduled
        cancelShutdownProcess.StartInfo.RedirectStandardError = true;  // Redirect the error output
        cancelShutdownProcess.StartInfo.UseShellExecute = false; // Required for redirection
        cancelShutdownProcess.StartInfo.CreateNoWindow = true; // Hide the process window

        cancelShutdownProcess.Start();

        // Read the output of the process
        string errorOutput = await cancelShutdownProcess.StandardError.ReadToEndAsync();
        cancelShutdownProcess.WaitForExit();

        // Check if the error output contains any message
        if (errorOutput.Contains("Unable to abort the system shutdown because no shutdown was in progress.(1116)"))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($" Warning: {errorOutput}");
            Console.WriteLine($" There was no scheduled {callerFunctionName} to cancel.");
        }
        // Successful cancellation will return an exit code of 0 and no error output. Inform the user about the successful cancellation.
        else if (cancelShutdownProcess.ExitCode == 0)
        {
            // Capitalize the first letter of the callerFunctionName for better UX
            string callerFunctionNameUpper = char.ToUpper(callerFunctionName[0]) + callerFunctionName.Substring(1);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($" {callerFunctionNameUpper} cancellation completed successfully.");
        }
        else // If there is an unexpected error (not the "no shutdown was in progress" message), print it to the user
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($" Warning: An unexpected error occurred while trying to cancel the {callerFunctionName}.");
            Console.WriteLine($" Process exit code: {cancelShutdownProcess.ExitCode}");
            Console.WriteLine($" Error output: {errorOutput}");
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($" Error: {ex.Message}");
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
    const string functionName = "restart";

    try
    {
        // First, call the CancelShutdown function to cancel any existing scheduled shutdown or restart
        await CancelShutdown(functionName);

        int restartMins = 0;
        int restartSecs = 0;

        // If it is a regular restart, it can be scheduled with a timer. So, we ask the user for the timer input.
        // Otherwise, Restart to BIOS option restarts immediately with restartSecs = 0
        if (!biosFlag)
        {
            // Read user input and validate it for the timer
            restartMins = await TimerInputValidation(functionName); // Call it with "restart" as the caller function name
            restartSecs = restartMins * 60;
        }

        Console.WriteLine("\n Scheduling a restart...");
        // Create and configure a restart process
        Process restartProcess = new Process();
        restartProcess.StartInfo.FileName = "shutdown.exe";

        // Build the arguments string based on biosFlag
        string restartArguments = "/r " + (biosFlag ? "/fw" : "/f") + $" /t {restartSecs}";

        restartProcess.StartInfo.Arguments = restartArguments;

        // Redirect the output to capture any system messages
        restartProcess.StartInfo.RedirectStandardError = true;
        restartProcess.StartInfo.UseShellExecute = false; // Required for redirection
        restartProcess.StartInfo.CreateNoWindow = true; // Hide the process window

        Console.ForegroundColor = ConsoleColor.Red;
        restartProcess.Start();

        // Read the error output of the process to catch any errors
        string errorOutput = await restartProcess.StandardError.ReadToEndAsync();
        restartProcess.WaitForExit();

        // Check if there was any error output
        if (errorOutput.Contains("A required privilege is not held by the client.(1314)"))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($" Warning: {errorOutput}");
            Console.WriteLine(" Restart scheduling failed.");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(" If you are trying to restart to BIOS, please make sure to launch the program with");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(" `Run as administrator'");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(" option\n");
        }
        else if (restartProcess.ExitCode == 0) // Successful restart scheduling
        {
            // Let the user know a restart is scheduled and print the timer info in hours/minutes format
            PrintTimer(restartMins, functionName);

            // Inform the user if the restart will go to BIOS or not
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine((biosFlag
                ? " Computer will restart and go to BIOS."
                : " Computer will restart normally."));
        }
        else // If there is an unexpected error, inform the user
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($" Warning: An unexpected error occurred while trying to cancel the {functionName}.");
            Console.WriteLine($" Process exit code: {restartProcess.ExitCode}");
            Console.WriteLine($" Error output: {errorOutput}");
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($" Error: {ex.Message}");
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
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n Please choose a valid option.");
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
        Console.WriteLine($" Error: {ex.Message}");
        Console.ResetColor();
        return ConsoleKey.Escape; // Return Escape in case of an exception to exit the application
    }
    finally
    {
        Console.ResetColor();
    }
}

// Function get valid timer input from the user
async Task<int> TimerInputValidation(string callerFunctionName) // callerFunctionName is only used for console outputs and better UX
{
    try
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n How many minutes do you need before {callerFunctionName}?");
        Console.WriteLine($" Enter 0 (zero) for immediate {callerFunctionName}.");
        Console.ResetColor();
        Console.Write(" Minutes: ");

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
            Console.WriteLine("\n Please enter a valid positive number or 0 (zero).");
            Console.WriteLine($" Entering 0 will {callerFunctionName} immediately.");
            Console.ResetColor();
            Console.Write("Minutes: ");
            shutdownTimeMins = int.TryParse(Console.ReadLine(), out shutdownTimeMins) ? shutdownTimeMins : -1;
        }

        return shutdownTimeMins;
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($" Error: {ex.Message}");
        Console.ResetColor();
        return -1; // Return -1 in case of error
    }
    finally
    {
        Console.ResetColor();
    }
}

// Function to print the timer info of a shutdown or a restart
void PrintTimer(int timeInMins, string callerFunctionName)
{
    try
    {
        // Capitalize the first letter of the callerFunctionName for better UX
        string callerFunctionNameUpper = char.ToUpper(callerFunctionName[0]) + callerFunctionName.Substring(1);

        // Inform the user which type of timer has been set
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n {callerFunctionNameUpper} scheduling completed.");

        // Print the timer info in hours/minutes format
        switch (timeInMins)
        {
            // Less than 60 minutes. So, print only minutes
            case < 60:
                Console.WriteLine($" Computer will {callerFunctionName} in {timeInMins} minute(s).");
                break;
            // Hours + remaining minutes
            default:
                Console.WriteLine($" Computer will {callerFunctionName} in {timeInMins / 60} hour(s)" +
                                  $" and {timeInMins % 60} minute(s).");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($" Error: {ex.Message}");
    }
    finally
    {
        Console.ResetColor();
    }
}