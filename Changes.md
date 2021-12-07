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
Updating Blog.Create
	injected usermanager and imageservice for later use
	Added authorization for admins/authors
		Added Author BlogRole enum
	Captured bloguserid from currently logged in user / removed field from view form
	Added Bindproperty to blogimage

