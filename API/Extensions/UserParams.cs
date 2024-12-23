using System;

namespace API.Extensions;

public class UserParams
{
    public const int MaxPageSize = 50;
    public int PageNumber { get; set; } = 1;
    private int _pageSize = 10;

    public int PageSize
    {
        // get { return _pageSize; }
        // set { _pageSize = (value > MaxPageSize) ? MaxPageSize : value;}
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }

    public string? Gender { get; set; }
    public string? CurrentUserName { get; set; }
    public int MinAge { get; set; } = 18;
    public int MaxAge { get; set; } = 100;
    public string OrderBy { get; set; } = "lastActive";



}
