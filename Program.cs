using NLog;

// See https://aka.ms/new-console-template for more information
const bool IS_UNIX = true;

string loggerPath = Directory.GetCurrentDirectory() + (IS_UNIX ? "/" : "\\") + "nlog.config";

// create instance of Logger
NLog.Logger logger = LogManager.Setup().LoadConfigurationFromFile(loggerPath).GetCurrentClassLogger();
logger.Info("Program started");

Console.WriteLine("Hello World!");

logger.Info("Program ended");


try
{

    // Create and save a new Blog
    Console.Write("Enter a name for a new Blog: ");
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