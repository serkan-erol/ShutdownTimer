ShutdownTimer

        - A simple shutdown scheduler to help with shutting down or restarting
    your PC after a timer. You can schedule it to shutdown/restart after a download, 
    an update or something similar.
        
        - It uses Windows' own shutdown.exe
    It is not really different from creating a shortcut of said shutdown.exe 
    and entering command line arguments in its properties tab. Only plus side, 
    with this one, you do not have to go to properties everytime you want to change
    the time setting, and you can enter the amount of time in minutes instead of seconds. 
    Saving precious seconds!

        - Now it also has an option to cancel a scheduled shutdown/restart! 

        - Now, restart and restart to BIOS options have been added, and they support
    timers, as well. Restart to BIOS requires admin privileges to work. 
    So, if you want to go straight to BIOS after a restart, run the program as administrator.
    In total, it does the job of 4 shortcuts in one simple program.

        - If you have already scheduled a shutdown/restart via this program or 
    shutdown.exe, and you run this program again to schedule another shutdown or
    restart, this action will override* the previous scheduled shutdown.
    *Note: It will cancel the old one and schedule a new one.        

        - It is self-contained, meaning you do not need anything else for it to work. 
    It is 64-bit Windows only. You can find the exe file at: 
    bin/Release/net9.0/win-x64/publish