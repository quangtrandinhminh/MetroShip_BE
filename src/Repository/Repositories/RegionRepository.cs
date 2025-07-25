﻿using MetroShip.Repository.Base;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using AppDbContext = MetroShip.Repository.Infrastructure.AppDbContext;

namespace MetroShip.Repository.Repositories;

public class RegionRepository : BaseRepository<Region> , IRegionRepository
{
    public RegionRepository(AppDbContext context) : base(context)
    {
    }
}