﻿using Darjeeling.Interfaces.Repositories;
using Darjeeling.Models.Entities;
using Microsoft.Extensions.Logging;

namespace Darjeeling.DataContext.Repositories;

public class FCGuildRoleRepository : IFCGuildRoleRepository
{
    private readonly Darjeeling.Repositories.DataContext _context;
    private readonly ILogger<FCGuildRoleRepository> _logger;
    
    public FCGuildRoleRepository(Darjeeling.Repositories.DataContext context, ILogger<FCGuildRoleRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task AddAsync(FCGuildRole role)
    {
        await _context.FCGuildRoles.AddRangeAsync(role);
    }
    
    public async Task RemoveAsync(FCGuildRole role)
    {
        _context.FCGuildRoles.RemoveRange(role);
    }
    
    public async Task<FCGuildRole?> GetGuildRoleByDiscordGuildUid(string discordGuildUid)
    {
        return _context.FCGuildRoles.FirstOrDefault(fcg => fcg.DiscordGuildUid == discordGuildUid);
    }
}