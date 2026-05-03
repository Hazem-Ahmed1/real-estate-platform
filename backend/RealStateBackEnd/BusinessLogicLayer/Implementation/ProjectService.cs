using AutoMapper;
using BusinessLogicLayer.Contracts;
using BusinessLogicLayer.Dtos.ProjectModule;
using BusinessLogicLayer.Exceptions;
using BusinessLogicLayer.Specifications;
using BusinessLogicLayer.Specifications.Projects;
using DataAccessLayer.Common;
using DataAccessLayer.Contracts;
using DataAccessLayer.Entities.ProjectModule;

namespace BusinessLogicLayer.Implementation;

public class ProjectService(IUnitOfWork unitOfWork, IMapper mapper) : IProjectService
{
    public async Task<PaginatedResult<ProjectListDto>> GetProjectsAsync(ProjectSpecParams @params)
    {
        var spec = new ProjectWithBuildingsSpecification(@params);
        var countSpec = new ProjectWithBuildingsSpecification(@params, isCount: true);

        var totalItems = await unitOfWork.Repository<Project>().CountAsync(countSpec);
        var projects = await unitOfWork.Repository<Project>().GetAllAsync(spec);

        var data = mapper.Map<IEnumerable<ProjectListDto>>(projects);

        return new PaginatedResult<ProjectListDto>(@params.Page, @params.PageSize, totalItems, data);
    }

    public async Task<ProjectDetailsDto?> GetProjectByIdAsync(int id)
    {
        var spec = new ProjectWithBuildingsSpecification(id);
        var project = await unitOfWork.Repository<Project>().GetByIdAsync(spec);

        if (project == null)
            throw new NotFoundExpection(nameof(Project), id);

        return mapper.Map<ProjectDetailsDto>(project);
    }
}
