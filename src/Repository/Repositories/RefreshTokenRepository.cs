using MetroShip.Repository.Base;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models.Identity;
using AppDbContext = MetroShip.Repository.Infrastructure.AppDbContext;

namespace MetroShip.Repository.Repositories;

public class RefreshTokenRepository(AppDbContext context)
    : BaseRepository<RefreshToken>(context), IRefreshTokenRepository;