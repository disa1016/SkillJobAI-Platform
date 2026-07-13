using Microsoft.EntityFrameworkCore;
using SkillJobAI.Api.Data;
using SkillJobAI.Api.Entities;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;

namespace SkillJobAI.Api.Services;

public class JobService : IJobService
{
    private readonly AppDbContext _context;

    public JobService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResponse<JobResponse>> GetJobsAsync(
        int page,
        int pageSize,
        string? search)
    {
        if (page < 1)
            page = 1;

        if (pageSize < 1)
            pageSize = 10;

        if (pageSize > 50)
            pageSize = 50;

        var query = _context.Jobs
            .Include(j => j.Company)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.ToLower();

            query = query.Where(j =>
                j.Title.ToLower().Contains(searchTerm) ||
                j.Description.ToLower().Contains(searchTerm) ||
                j.Location.ToLower().Contains(searchTerm) ||
                (j.Company != null && j.Company.Name.ToLower().Contains(searchTerm)));
        }

        var totalItems = await query.CountAsync();

        var jobs = await query
            .OrderByDescending(j => j.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(j => new JobResponse
            {
                Id = j.Id,
                Title = j.Title,
                Description = j.Description,
                Location = j.Location,
                Salary = j.Salary,
                CreatedAt = j.CreatedAt,
                CompanyId = j.CompanyId,
                CompanyName = j.Company != null ? j.Company.Name : "",
                Company = j.Company == null ? null : new JobCompanyResponse
                {
                    Id = j.Company.Id,
                    Name = j.Company.Name,
                    Location = j.Company.Location ?? ""
                }
            })
            .ToListAsync();

        return new PagedResponse<JobResponse>
        {
            Items = jobs,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<JobResponse?> GetJobByIdAsync(int id)
    {
        return await _context.Jobs
            .Include(j => j.Company)
            .Where(j => j.Id == id)
            .Select(j => new JobResponse
            {
                Id = j.Id,
                Title = j.Title,
                Description = j.Description,
                Location = j.Location,
                Salary = j.Salary,
                CreatedAt = j.CreatedAt,
                CompanyId = j.CompanyId,
                CompanyName = j.Company != null ? j.Company.Name : "",
                Company = j.Company == null ? null : new JobCompanyResponse
                {
                    Id = j.Company.Id,
                    Name = j.Company.Name,
                    Description = j.Company.Description ?? "",
                    WebsiteUrl = j.Company.WebsiteUrl ?? "",
                    LogoUrl = j.Company.LogoUrl ?? "",
                    Location = j.Company.Location ?? ""
                }
            })
            .FirstOrDefaultAsync();
    }

    public async Task<JobResponse?> CreateJobAsync(JobRequest request)
    {
        if (request.CompanyId == null)
            return null;

        var company = await _context.Companies
            .FirstOrDefaultAsync(c => c.Id == request.CompanyId.Value);

        if (company == null)
            return null;

        var job = new Job
        {
            Title = request.Title,
            Description = request.Description,
            Location = request.Location,
            Salary = request.Salary,
            CompanyId = request.CompanyId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Jobs.Add(job);
        await _context.SaveChangesAsync();

        return new JobResponse
        {
            Id = job.Id,
            Title = job.Title,
            Description = job.Description,
            Location = job.Location,
            Salary = job.Salary,
            CreatedAt = job.CreatedAt,
            CompanyId = job.CompanyId,
            CompanyName = company.Name,
            Company = new JobCompanyResponse
            {
                Id = company.Id,
                Name = company.Name,
                Description = company.Description ?? "",
                WebsiteUrl = company.WebsiteUrl ?? "",
                LogoUrl = company.LogoUrl ?? "",
                Location = company.Location ?? ""
            }
        };
    }

    public async Task<JobResponse?> UpdateJobAsync(int id, JobRequest request)
    {
        var job = await _context.Jobs.FindAsync(id);

        if (job == null || request.CompanyId == null)
            return null;

        var company = await _context.Companies
            .FirstOrDefaultAsync(c => c.Id == request.CompanyId.Value);

        if (company == null)
            return null;

        job.Title = request.Title;
        job.Description = request.Description;
        job.Location = request.Location;
        job.Salary = request.Salary;
        job.CompanyId = request.CompanyId;

        await _context.SaveChangesAsync();

        return new JobResponse
        {
            Id = job.Id,
            Title = job.Title,
            Description = job.Description,
            Location = job.Location,
            Salary = job.Salary,
            CreatedAt = job.CreatedAt,
            CompanyId = job.CompanyId,
            CompanyName = company.Name,
            Company = new JobCompanyResponse
            {
                Id = company.Id,
                Name = company.Name,
                Description = company.Description ?? "",
                WebsiteUrl = company.WebsiteUrl ?? "",
                LogoUrl = company.LogoUrl ?? "",
                Location = company.Location ?? ""
            }
        };
    }

    public async Task<bool> DeleteJobAsync(int id)
    {
        var job = await _context.Jobs.FindAsync(id);

        if (job == null)
            return false;

        _context.Jobs.Remove(job);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CompanyExistsAsync(int companyId)
    {
        return await _context.Companies.AnyAsync(c => c.Id == companyId);
    }

    public async Task<int?> GetJobCompanyIdAsync(int jobId)
    {
        return await _context.Jobs
            .Where(j => j.Id == jobId)
            .Select(j => j.CompanyId)
            .FirstOrDefaultAsync();
    }
}