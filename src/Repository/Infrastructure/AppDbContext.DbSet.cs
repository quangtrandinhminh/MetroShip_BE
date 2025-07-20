using MetroShip.Repository.Models;
using MetroShip.Repository.Models.Identity;
using Microsoft.EntityFrameworkCore;

namespace MetroShip.Repository.Infrastructure;

public sealed partial class AppDbContext
{
    //public DbSet<RefreshToken> RefreshTokens { get; set; }

    public DbSet<UserRoleEntity> UserRoleEntity { get; set; }

    public DbSet<RoleEntity> Role { get; set; }

    public  DbSet<MetroLine> MetroLines { get; set; }

    public DbSet<MetroTrain> MetroTrains { get; set; }

    public DbSet<MetroTimeSlot> MetroTimeSlots { get; set; }

    public DbSet<Parcel> Parcels { get; set; }

    public DbSet<ParcelTracking> ParcelTrackings { get; set; }

    public DbSet<ParcelCategory> ParcelCategories { get; set; }

    public DbSet<Region> Regions { get; set; }

    public DbSet<Report> Reports { get; set; }

    public DbSet<Route> Routes { get; set; }

    public DbSet<Shipment> Shipments { get; set; }

    public DbSet<ShipmentMedia> ShipmentImages { get; set; }

    public DbSet<Transaction> Transactions { get; set; }

    public DbSet<Notification> Notifications { get; set; }

    public DbSet<ShipmentItinerary> ShipmentItineraries { get; set; }

    public DbSet<Station> Stations { get; set; }

    public DbSet<StaffAssignment> StaffAssignments { get; set; }

    public DbSet<SupportingTicket> SupportingTickets { get; set; }

    public DbSet<SystemConfig> SystemConfigs { get; set; }
}