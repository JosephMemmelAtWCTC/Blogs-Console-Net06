﻿using Microsoft.IdentityModel.Tokens;
using NLog;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

// create instance of Logger
// NLog.Logger logger = UserInteractions.getLogger();
LoggerWithColors logger = new LoggerWithColors();

logger.Info("Main program is running and log manager is started, program is running on a " + (LoggerWithColors.IS_UNIX ? "" : "non-") + "unix-based device.\n");



logger.Info("Program started");


string[] MAIN_MENU_OPTIONS_IN_ORDER = { enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Display_All_Blogs),
                                        enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Add_Blog),
                                        enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Edit_Blog),
                                        enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Deleate_Blog),
                                        enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Create_Post),
                                        enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Display_Posts),
                                        enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Exit)};


string menuCheckCommand;

// MAIN MENU LOOP
do
{
    menuCheckCommand = UserInteractions.OptionsSelector(MAIN_MENU_OPTIONS_IN_ORDER);

    logger.Info($"User choice: \"{menuCheckCommand}\"");

    if (menuCheckCommand == enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Exit))
    {
        logger.Info("Program quitting...");
    }
    else if (menuCheckCommand == enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Display_All_Blogs))
    {
        // Display all blogs
        Console.WriteLine("All blogs located in the database (by name):\n");
        try
        {
            var query = getAllBlogs();
            Console.ForegroundColor = UserInteractions.resultsColor;

            foreach (var item in query)
            {
                Console.WriteLine(item.Name);
            }
            Console.ForegroundColor = UserInteractions.defaultColor;
            int itemCount = query.Count();
            Console.Write($"\nThere were ");
            Console.ForegroundColor = UserInteractions.resultsColor;
            Console.Write($"{itemCount} ");
            Console.ForegroundColor = UserInteractions.defaultColor;
            Console.Write($"blog{(itemCount==1?"":"s")}");
            Console.ForegroundColor = UserInteractions.defaultColor;
            Console.WriteLine(" located in the database.");
        }
        catch (Exception ex)
        {
            logger.Error(ex.Message);
        }
    }
    else if (menuCheckCommand == enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Add_Blog))
    {
        // Create and save a new Blog
        Console.Write("Enter a name for the new Blog: ");
        var name = Console.ReadLine();
        try
        {
            var blog = new Blog { Name = name };

            var db = new BloggingContext();
            ValidationContext context = new ValidationContext(blog, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(blog, context, results, true);
            if (isValid)
            {
                // check for unique name
                if (db.Blogs.Any(b => b.Name == blog.Name))
                {
                    // generate validation error
                    isValid = false;
                    results.Add(new ValidationResult("Blog name exists", new string[] { "Name" }));
                }
                else
                {
                    logger.Info("Validation passed");
                    // save blog to db
                    db.AddBlog(blog);
                    logger.Info($"Blog added - {blog.Name}");
                }
            }
            if (!isValid)
            {
                foreach (var result in results)
                {
                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                }
            }

            //TODO: Display all blogs?
        }
        catch (Exception ex)
        {
            logger.Error(ex.Message);
        }
    }
    else if (menuCheckCommand == enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Create_Post))
    {
        Blog selectedBlog = selectBlog("Please select a blog for your new post: ");
        
        string postTitle = UserInteractions.UserCreatedStringObtainer("Please enter the title of the new post", 1, false, false);
        string postContent = UserInteractions.UserCreatedStringObtainer("Please enter the content for the new post", 1, false, false);
        try
        {
            Post post = new Post {
                Title = postTitle,
                Content = postContent,
                BlogId = selectedBlog.BlogId
            };

            var db = new BloggingContext();
            db.AddPost(post);
            logger.Info($"Post added to blog \"{selectedBlog}\" - {postTitle}");

            //TODO: Display all posts?
        }
        catch (Exception ex)
        {
            logger.Error(ex.Message);
        }
    }
    else if (menuCheckCommand == enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Display_Posts))
    {
        Blog selectedBlog = selectBlog("Please select a blog whose posts you wish to view: ");
        int postsCount;
        try{
            postsCount = selectedBlog.Posts.Count;
        }catch(NullReferenceException error){
            //Just make it equal to 0 as count fails if it is empty
            postsCount = 0;
        }

        Console.Write("You have selected blog ");
        Console.ForegroundColor = UserInteractions.resultsColor;
        Console.Write($"{selectedBlog.Name}");
        Console.ForegroundColor = UserInteractions.defaultColor;
        Console.Write(" which contains ");
        Console.ForegroundColor = UserInteractions.resultsColor;
        Console.Write($"{postsCount}");
        Console.ForegroundColor = UserInteractions.defaultColor;
        Console.WriteLine($" post{(postsCount==1?"":"s")}.\n");

        Console.ForegroundColor = UserInteractions.resultsColor;
        if(postsCount > 0){
            Console.WriteLine($"Blog: \"{selectedBlog.Name}\"");
            foreach(Post post in selectedBlog.Posts){
                string titleLine = $"{selectedBlog.Name} - Post: ";
                string innerIndentLine = new string[titleLine.Length-8].Aggregate((c, n) => $"{c} ");

                Console.WriteLine($"{titleLine}{post.Title}");
                Console.WriteLine($"{innerIndentLine}Content: {post.Content}");
                Console.WriteLine();
            }
        }else{
            Console.WriteLine($"Blog \"{selectedBlog.Name}\" does not contain any posts.");
        }
        Console.ForegroundColor = UserInteractions.defaultColor;

    // Once the Blog is selected, all Posts related to the selected blog should be display as well as the number of Posts
    // For each Post, display the Blog name, Post title and Post content

    }
    else if (menuCheckCommand == enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Deleate_Blog))
    {
        Blog selectedBlog = selectBlog("Please select a blog to delete: ");
        if (selectedBlog != null)
        {
            var db = new BloggingContext();
            // Deleate the blog
            db.DeleteBlog(selectedBlog);
            logger.Info($"Blog (id: {selectedBlog.BlogId}) deleted");
        }
    }
    else if (menuCheckCommand == enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Edit_Blog))
    {
        Blog selectedBlog = selectBlog("Please select a blog to edit: ");
        if (selectedBlog != null)
        {
            var db = new BloggingContext();
            // Edit the blog
                Blog UpdatedBlog = InputBlog(db, logger);
                if (UpdatedBlog != null)
                {
                    UpdatedBlog.BlogId = selectedBlog.BlogId;
                    db.EditBlog(UpdatedBlog);
                    logger.Info($"Blog (id: {selectedBlog.BlogId}) updated");
                }
        }
    }    else
    {
        logger.Warn("That menu option is not available, please try again.");
    }

} while (menuCheckCommand != enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Exit)); //If user intends to exit the program

logger.Info("Program ended");





IOrderedQueryable<Blog> getAllBlogs(){
    var db = new BloggingContext();
    // Get all Blogs from the database
    var query = db.Blogs.Include("Posts").OrderBy(b => b.Name);
    // var query = db.Blogs.OrderBy(b => b.Name);
    
    return query;
}

Blog selectBlog(string selectionMessage){
    Blog[] allBlogs = getAllBlogs().ToArray();
    string[] allBlogKeys = new string[allBlogs.Count()];

    for(int i = 0; i < allBlogKeys.Length; i++){
        allBlogKeys[i] = allBlogs[i].Name;
    }

    string selectedBlogNameKey = UserInteractions.OptionsSelector(allBlogKeys, selectionMessage);
    Blog selectedBlog;

    // Find the blog that matches the key
    foreach(Blog blog in allBlogs)
    {
        if(blog.Name == selectedBlogNameKey){
            selectedBlog = blog;
            return blog;
        }
    }
    return new Blog();
}


static Blog InputBlog(BloggingContext db, LoggerWithColors logger)
{
    Blog blog = new Blog();
    blog.Name = UserInteractions.UserCreatedStringObtainer("Please enter the updated blog name", 1, false, false);

    ValidationContext context = new ValidationContext(blog, null, null);
    List<ValidationResult> results = new List<ValidationResult>();

    var isValid = Validator.TryValidateObject(blog, context, results, true);
    if (isValid)
    {
        return blog;
    }
    else
    {
        foreach (var result in results)
        {
            logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
        }
    }
    return null;
}


string enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS mainMenuEnum)
{


    return mainMenuEnum switch
    {
        MAIN_MENU_OPTIONS.Exit => "Quit program",
        MAIN_MENU_OPTIONS.Display_All_Blogs => $"Display all blogs", // on file (display max amount is {UserInteractions.PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT / 11:N0})"
        MAIN_MENU_OPTIONS.Add_Blog => "Add Blog",
        MAIN_MENU_OPTIONS.Edit_Blog => "Edit Blog",
        MAIN_MENU_OPTIONS.Deleate_Blog => "Deleate a blog",
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
    Display_Posts,
    Deleate_Blog,
    Edit_Blog,
}
