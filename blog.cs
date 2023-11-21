using System.ComponentModel.DataAnnotations;

public class Blog
{
    public int BlogId { get; set; }
    [Required(ErrorMessage = "A blog name is required.")]
    public string Name { get; set; }

    public List<Post> Posts { get; set; }
}
