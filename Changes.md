Install postgres / PG4 admin
Install nuget package: Npgsql.EntityFrameworkCore.PostgreSQL
Update connection string in appsettings.json
Update Program.cs to use new database/connection string
npm console update-database to test
*ERROR* nvarchar(256) in AspNetRoles throwing error
	caused by using old sql server migrations
	deleted old migrations and used npm: add-migration "Initial" -o "Data/Migrations"
	view in pgAdmin to verify

Turned off codelens to make initial bits more readable (tools-options-text editor-all languages-codelens)
Testing inheritance with database -> sticking with old way both users and authors as bloguser
Testing inheritance with database -> sticking with old way both users and authors as bloguser but may change later
Initial DB setup
	Added models needed for database
		Add DataAnnotations: [Required][DisplayName(Name="")][StringLength[100, ErrorMessage="{0}{1}"]]
	Added minimum models as DBSet<> to ApplicationDBContext
	Added enums : ReadyStatus
		Later on will add ModerationType and BlogRole
	npm: add-migration 002 -o "Data/Migrations" -> update-database
	Will have errors with nullable types if database hasn't been manually created yet -> make DBSet<>s nullable for tempfix
	Scaffold out Blog for testing
		Update Blog.Create(blog) to use DateTime.Now
		When using Postgres, error will be thrown about using incompatible date format (local vs timezone)
			Update data annotation on Blog to use [Column(TypeName = "timestamp without time zone")]
			npm: add-migration 003 -o "Data/Migrations" -> update-database
		Unable to save changes on a create new until we either remove some form items or seed the database with initial values to populate drop downs
		Create DataService
			Apply migrations programatically
			Seed Roles from BlogRole's enum
			Seed Users programatically (later on change this to seed database based on preset json or even a database backup)
Looked at other blog sites to see what kinda of data we should capture and bought a template (comparisons later on using bootstrap 5/HTML templates vs react templates)
Started doing some forms editing with the template and trimming out the fat for css/javascript (can be branched off and used as a mini-series)
Use BlogUser as identity
	Inherit BlogUser : IdentityUser
	update Program.cs (old startup.cs)
		old : builder.Services.AddDefaultIdentity<IdentityUser>
		new : builder.Services.AddIdentity<BlogUser, IdentityRole>
		add : .AddDefaultUI()
		add : .AddDefaultTokenProviders()
	Inject DataService
		builder.Services.AddScoped<DataService>();
		...
		var app = builder.Build();
		...
		await app.Services
			.CreateScope()
			.ServiceProvider
			.GetRequiredService<DataService>()
			.SeedDatabaseAsync();
		Will throw an error about IdentityUser conflict with UserManager on DataService
			Since we are changing our IdentityRole to using a BlogUser... have to update types where Class<IdentityUser> was called
				Update _LoginPartial.cshtml injection: @inject UserManager<BlogUser> UserManager
				Do same for SignInManager
				Will have to update Area pages later as well
				Update ApplicationDBContext declared as public class ApplicationDbContext : IdentityDbContext<BlogUser> 
Updated DataService seeding to use same username as email (will have to completly revise the login areas to accomodate non-email logins since that is what it validates against)
Migrations are no longer necessary since the  await dbContext.Database.EnsureCreatedAsync(); will update database based on current Model (update for later is to migrate this into the model snapshot to seed the data)
Made a small mistake of not logging out/in after doing a database drop/create. Identity would persist until (showing logged in name) until 
Previously used two fields to manage Images, but decided to make an Image Model as an owned class. (Good for code readability, bad for how it looks in the database)
Scaffolded Blogs, Comments, Posts and Tags
	When scaffolding, have to drop/recreate database to reflect changes since we aren't using migrations
Changed the Areas/Identity/Pages/Account/Index to use the custom Identity model
	Be careful with identity updates since it pulls from usermanager not the dbcontext
Revised landing page (_layout and index) 
Updating Blogs.Create
	injected usermanager and imageservice for later use
	Added authorization for admins/authors
		Added Author BlogRole enum
	Captured bloguserid from currently logged in user / removed field from view form
	Added Bindproperty to blogimage
	Suppressing nullable warnings with !
Updated Blogs.Index to use cards instead of table
Updated Blogs.Edit
Deleted Blogs.Details (previous references direct to Posts.Index which will be revised later)
Refactored out using an image service... used an extension method instead for simplicity. Deleted all associated fluff to support the old image service.
Seeded database users with a json file
Changed to seeding database with .csv files
	Must seed in the correct order
	Have to create typeconverters for bool fields in database
	Must ignore null date fields (TODO for later see if there is an elegant work around)
	Must save each table seed to database before seeding the next to ensure FK compliance
Edited visuals for Blog.Index and had to remove Asp-For when sending formfield as a parameter (was invalidating model when null)
Added tiny cloud mce / linked scripts
Updated sequences for PK's via sql file
Post.Create 
	Edited viewbag for BlogId
	Removed calculated fields on view
	Populated readystatus from enum on view
	Must make BlogUserId nullable since we save it programmatically (TODO for later refactor to AuthorId)
	Created slug service for url-friendly routing, implemented it for post creation
Post.Edit
	Edited form fields
	Setting a form field disabled does not post data on form submission
Blog.Index
	Created Posts.PostsByBlogIndex for filtering
	Updated links to route to new Posts.PostsByBlogIndex
Routing to Post.Details via slugs
	Updated app.MapControllerRoute
	Updated PostByBlogIndex View asp-route
	Changed Post.Details to use slug instead of ID 
	Updated return route to use filtered Blog.Index for Post.Details and Post.Edit views and Post.Edit Action
Made details page only show the html from content
Implemented conditional elements on views
Home.Index
	Created ViewModel for showing posts and blogs on homepage
	Updated controller to populate top 3 posts and all blogs (may filter down later)
	Fixed links to Post.Details to use slug route (have to specify asp-controller since partial view)
Checked authorization to show edit/modify links
Refactored PostByBlogIndex to simple pass filtered data to Index View
UI update
	Changed Blog and Post Index cards to use <a class="stretched-link"> to make entire div clickable
	Updated Blogs.Index to include Posts (since it uses count)
	Updated navbar, admin login -> New Blog, author login -> create Post
Updated roles, added author to admin and mod emails so they can make new posts, assigned mod email one blog
Implemented Prism.js
	Made hidden element for binding Post.Content
	Updated tinymce.init setup to change element value when editor changes (have to verify this does not cut off last change when submitting)
Footer is cutting off bottom elements due to fixed-bottom, added a div with fixed height before (TODO: add fancy css to calculate space needed)
Added delete to Post.Edit. Button/anchor must be outside of form otherwise form submit will occur
Hiding posts that aren't ReadyStatus.Production unless logged in as the blog author
Added edit link to Blog.Index
	Setting Z-Index allows for clickable links in stretched-link cards
Updated Blog.Edit to change Blog ownership via email select list (may need to make this a searchable item later)

*******************************************************************************
TODO:
*******************************************************************************
Calculate the Post abstract by parsing out the html
Implement more hardened security for authorization (people navigating directly to edit page etc)
Make a page to show all authors




*******************************************************************************
Way Later:
*******************************************************************************
Make a load more function for long list of posts (paging then render incrementally?)
Make overflow background a different color