/*using System.Linq.Expressions;
using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels.Graph;
using MetroShip.Service.ApiModels.Parcel;
using MetroShip.Service.ApiModels.Shipment;
using MetroShip.Service.BusinessModels;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Mapper;
using MetroShip.Service.Services;
using MetroShip.Service.Validations;
using MetroShip.Utility.Config;
using MetroShip.Utility.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace MetroShip.Test;

public class ShipmentPricingServiceTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ILogger> _mockLogger;
    private readonly Mock<ShipmentValidator> _mockValidator;
    private readonly Mock<ISystemConfigRepository> _mockSystemConfigRepo;
    private readonly Mock<IStationRepository> _mockStationRepo;
    private readonly Mock<IParcelCategoryRepository> _mockParcelCategoryRepo;
    private readonly Mock<MetroGraph> _mockMetroGraph;
    private readonly Mock<IMapperlyMapper> _mockMapper;
    private readonly SystemConfigSetting _systemConfigSetting;
    private readonly ShipmentService _service;

    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IShipmentRepository> _shipmentRepository;
    private readonly Mock<IShipmentItineraryRepository> _shipmentItineraryRepository;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
    private readonly Mock<IEmailService> _emailSender;
    private readonly Mock<IUserRepository> _userRepository;
    private bool _isInitialized = false;
    private MetroGraph _metroGraph;

    public ShipmentPricingServiceTests()
    {
        _mockLogger = new Mock<ILogger>();
        _mockValidator = new Mock<ShipmentValidator>();
        _mockSystemConfigRepo = new Mock<ISystemConfigRepository>();
        _mockStationRepo = new Mock<IStationRepository>();
        _mockParcelCategoryRepo = new Mock<IParcelCategoryRepository>();
        _mockMetroGraph = new Mock<MetroGraph>();
        _mockMapper = new Mock<IMapperlyMapper>();
        _systemConfigSetting = new SystemConfigSetting();

        _service = new ShipmentService(
            _serviceProviderMock.Object,
            _unitOfWork.Object,
            _shipmentRepository.Object,
            _shipmentItineraryRepository.Object,
            _mockStationRepo.Object,
            _mockSystemConfigRepo.Object,
            _emailSender.Object,
            _mockParcelCategoryRepo.Object,
            _userRepository.Object,
        );
    }

    [Fact]
    public async Task GetItineraryAndTotalPrice_WithValidRequest_ReturnsCorrectPricing()
    {
        // Arrange
        var request = CreateTestRequest();
        SetupMockRepositories();
        SetupMockPricingConfigs();

        // Act
        var result = await _service.GetItineraryAndTotalPrice(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Standard);
        Assert.True(result.Standard.TotalShippingFeeVnd > 0);
    }

    [Theory]
    [InlineData(3.0, 300, 25000)] // <5kg, 300km = 25000 VND
    [InlineData(7.0, 300, 52500)] // 7kg in tier 2, 300km = 7500 * 7 = 52500 VND
    [InlineData(25.0, 600, 262500)] // 25kg in tier 3, 600km = 10500 * 25 = 262500 VND
    public void PriceCalculationService_CalculatesCorrectPrices(double weight, int distance, decimal expectedPrice)
    {
        // Arrange
        var configs = CreateTestConfigs();
        var builder = new PricingTableBuilder();
        var pricingTable = builder.BuildPricingTable(configs);
        var priceService = new PriceCalculationService(pricingTable);

        // Act
        var actualPrice = priceService.CalculateShippingPrice((decimal)weight, distance);

        // Assert
        Assert.Equal(expectedPrice, actualPrice);
    }

    private TotalPriceCalcRequest CreateTestRequest()
    {
        return new TotalPriceCalcRequest
        {
            DepartureStationId = "station1",
            DestinationStationId = "station2",
            ScheduleShipmentDate = DateTime.UtcNow.AddDays(3),
            UserLatitude = 10.7769,
            UserLongitude = 106.7009,
            Parcels = new List<ParcelRequest>
            {
                new ParcelRequest
                {
                    WeightKg = 3.0m,
                    LengthCm = 20,
                    WidthCm = 15,
                    HeightCm = 10,
                    ParcelCategoryId = "cat1"
                }
            }
        };
    }

    private List<SystemConfig> CreateTestConfigs()
    {
        return new List<SystemConfig>
        {
            new SystemConfig { ConfigKey = "DISTANCE_STEP_KM", ConfigValue = "300" },
            new SystemConfig { ConfigKey = "PRICE_STEP_PERCENT_PER_DISTANCE_TIER", ConfigValue = "50" },
            new SystemConfig { ConfigKey = "DISTANCE_TIER_QUANTITY", ConfigValue = "7" },
            new SystemConfig { ConfigKey = "WEIGHT_TIER_1_MAX_KG", ConfigValue = "5" },
            new SystemConfig { ConfigKey = "PRICE_TIER_1_VND", ConfigValue = "25000" },
            new SystemConfig { ConfigKey = "WEIGHT_TIER_2_MAX_KG", ConfigValue = "10" },
            new SystemConfig { ConfigKey = "PRICE_TIER_2_VND_PER_KG", ConfigValue = "7500" },
            new SystemConfig { ConfigKey = "WEIGHT_TIER_3_MAX_KG", ConfigValue = "50" },
            new SystemConfig { ConfigKey = "PRICE_TIER_3_VND_PER_KG", ConfigValue = "7000" },
            new SystemConfig { ConfigKey = "WEIGHT_TIER_4_MAX_KG", ConfigValue = "100" },
            new SystemConfig { ConfigKey = "PRICE_TIER_4_VND_PER_KG", ConfigValue = "6500" },
            new SystemConfig { ConfigKey = "PRICE_TIER_5_VND_PER_KG", ConfigValue = "6000" }
        };
    }

    private void SetupMockRepositories()
    {
        _mockSystemConfigRepo.Setup(x => x.GetSystemConfigValueByKey(It.IsAny<string>()))
            .Returns("1000");

        _mockStationRepo.Setup(x => x.GetAllStationIdNearUser(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<string> { "station3", "station4" });

        _mockStationRepo.Setup(x => x.AreStationsInSameMetroLine(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        _mockMetroGraph.Setup(x => x.FindShortestPathByDijkstra(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new List<string> { "station1", "station2" });

        _mockMetroGraph.Setup(x => x.CreateResponseFromPath(It.IsAny<List<string>>(), It.IsAny<IMapperlyMapper>()))
            .Returns(new BestPathGraphResponse { 
                TotalShippingFeeVnd = 25000,
                TotalInsuranceFeeVnd = 300,
                TotalCostVnd = 60,
                TotalKm = 300
                TotalTime = 60,
                TotalStations = 2,
                TotalRoutes = 1,

            });
    }

    private void SetupMockPricingConfigs()
    {
        _mockSystemConfigRepo.Setup(x => x.GetAllSystemConfigs(ConfigTypeEnum.PriceStructure))
            .ReturnsAsync(CreateTestConfigs());
    }
}*/