﻿using NLog;

// create instance of Logger
NLog.Logger logger = UserInteractions.getLogger();
logger.Info("Main program is running and log manager is started, program is running on a " + (UserInteractions.IS_UNIX ? "" : "non-") + "unix-based device.\n");



logger.Info("Program started");


string[] MAIN_MENU_OPTIONS_IN_ORDER = { enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Display_All_Blogs),
                                        enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Add_Blog),
                                        enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Create_Post),
                                        enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Display_Posts),
                                        enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Exit)};


string menuCheckCommand;

// MAIN MENU LOOP
do
{
    menuCheckCommand = UserInteractions.OptionsSelector(MAIN_MENU_OPTIONS_IN_ORDER);

    logger.Info($"User choice: \"{menuCheckCommand}\"");

    if (menuCheckCommand == enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Display_All_Blogs))
    {

    }
    else if (menuCheckCommand == enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Add_Blog))
    {
        try
        {

            // Create and save a new Blog
            Console.Write("Enter a name for the new Blog: ");
            var name = Console.ReadLine();

            var blog = new Blog { Name = name };

            var db = new BloggingContext();
            db.AddBlog(blog);
            logger.Info("Blog added - {name}", name);

            // Display all Blogs from the database
            var query = db.Blogs.OrderBy(b => b.Name);

            Console.WriteLine("All blogs in the database:");
            foreach (var item in query)
            {
                Console.WriteLine(item.Name);
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex.Message);
        }
    }
    else
    {
        logger.Fatal("Somehow menuCheckCommand was selected that did not fall under the the existing commands, this should never have been triggered. Improper menuCheckCommand is getting through");
    }

} while (menuCheckCommand != enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Exit)); //If user intends to exit the program
logger.Info("Program quitting...");


logger.Info("Program ended");
string enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS mainMenuEnum)
{
    return mainMenuEnum switch
    {
        MAIN_MENU_OPTIONS.Exit => "Quit program",
        MAIN_MENU_OPTIONS.Display_All_Blogs => $"View all blogs", // on file (display max amount is {UserInteractions.PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT / 11:N0})"
        MAIN_MENU_OPTIONS.Add_Blog => "Add Blog",
        MAIN_MENU_OPTIONS.Create_Post => "Create New Post",
        MAIN_MENU_OPTIONS.Display_Posts => "Display Posts",
        _ => "ERROR_MAIN_MENU_OPTION_DOES_NOT_EXIST"
    };
}

public enum MAIN_MENU_OPTIONS
{
    Exit,
    Display_All_Blogs,
    Add_Blog,
    Create_Post,
    Display_Posts
}
