using Capstone.DAL.DTOs;
using Capstone.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace Capstone.DAL.Repository;

public class Repository : IRepository
{
    private readonly HelpDeskDbContext _context;

    public Repository(HelpDeskDbContext context)
    {
        _context = context;
    }

    public bool Authenticate(User user)
    {
        if (string.IsNullOrWhiteSpace(user.UserName) || string.IsNullOrWhiteSpace(user.Password))
        {
            return false;
        }

        return _context.Users
            .AsNoTracking()
            .Any(u => u.UserName == user.UserName && u.Password == user.Password);
    }

    public List<ServiceRequest> ViewRequests()
    {
        return _context.ServiceRequests
            .AsNoTracking()
            .Include(sr => sr.RaisedByUser)
                .ThenInclude(u => u!.Role)
            .Include(sr => sr.Status)
            .OrderByDescending(sr => sr.RaisedOn)
            .ToList();
    }

    public List<ServiceRequest> ViewRequests(string userName)
    {
        return _context.ServiceRequests
            .AsNoTracking()
            .Include(sr => sr.RaisedByUser)
                .ThenInclude(u => u!.Role)
            .Include(sr => sr.Status)
            .Where(sr => sr.RaisedBy == userName)
            .OrderByDescending(sr => sr.RaisedOn)
            .ToList();
    }

    public int RaiseRequest(ServiceRequest newRequest)
    {
        newRequest.RaisedOn = DateTime.UtcNow;
        if (newRequest.ReqStatus == 0)
        {
            newRequest.ReqStatus = 1;
        }

        _context.ServiceRequests.Add(newRequest);
        _context.SaveChanges();
        return newRequest.RequestId;
    }

    public ServiceRequest? GetRequestById(int requestId)
    {
        return _context.ServiceRequests
            .AsNoTracking()
            .Include(sr => sr.RaisedByUser)
                .ThenInclude(u => u!.Role)
            .Include(sr => sr.Status)
            .FirstOrDefault(sr => sr.RequestId == requestId);
    }

    public bool ReOpenRequest(ServiceRequest request)
    {
        var existing = _context.ServiceRequests.Find(request.RequestId);
        if (existing is null)
        {
            return false;
        }

        existing.ReqStatus = 4;
        _context.SaveChanges();
        return true;
    }

    public List<ServiceRequest> GetRequestBySP(string userName)
    {
        return _context.ServiceRequests
            .FromSqlInterpolated($"SELECT * FROM ServiceRequest WHERE RaisedBy = {userName}")
            .AsNoTracking()
            .ToList();
    }

    public bool CloseRequest(int requestId)
    {
        var request = _context.ServiceRequests.Find(requestId);
        if (request is null)
        {
            return false;
        }

        request.ReqStatus = 3;
        _context.SaveChanges();
        return true;
    }

    public bool DeleteRequest(int requestId)
    {
        var request = _context.ServiceRequests.Find(requestId);
        if (request is null)
        {
            return false;
        }

        _context.ServiceRequests.Remove(request);
        _context.SaveChanges();
        return true;
    }

    public User? GetUser(string userName)
    {
        return _context.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .FirstOrDefault(u => u.UserName == userName);
    }

    public async Task<SummaryResponse> GetSummaryRequestByUserAsync(string userName)
    {
        var requests = await _context.ServiceRequests
            .AsNoTracking()
            .Include(sr => sr.Status)
            .Where(sr => sr.RaisedBy == userName)
            .OrderByDescending(sr => sr.RaisedOn)
            .ToListAsync();

        return new SummaryResponse
        {
            UserName = userName,
            TotalRequests = requests.Count,
            OpenRequests = requests.Count(sr => sr.ReqStatus == 1),
            InProgressRequests = requests.Count(sr => sr.ReqStatus == 2),
            ClosedRequests = requests.Count(sr => sr.ReqStatus == 3),
            ReOpenedRequests = requests.Count(sr => sr.ReqStatus == 4),
            Requests = requests.Select(sr => new RequestSummaryItem
            {
                RequestId = sr.RequestId,
                Description = sr.Description ?? string.Empty,
                Status = sr.Status?.Description ?? string.Empty,
                RaisedOn = sr.RaisedOn
            }).ToList()
        };
    }
}