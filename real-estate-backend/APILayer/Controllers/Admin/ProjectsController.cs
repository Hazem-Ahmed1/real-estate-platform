using Microsoft.AspNetCore.Mvc;

namespace APILayer.Controllers.Admin;

[Route("api/admin/projects")]
public class ProjectsController : ApiController
{
    // Admin specific WRITE operations (POST, PUT, DELETE) will go here.
    // Read operations are shared via the Public ProjectsController to avoid redundancy.
}
