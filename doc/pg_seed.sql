-- Database: MetroShip
-- Regions
INSERT INTO public."Regions"
("Id", "RegionCode", "RegionName", "CreatedBy", "LastUpdatedBy", "DeletedBy", "CreatedAt", "LastUpdatedAt", "DeletedAt")
VALUES
    ('d75d6e5a-34b5-4396-b0b9-2947250ed138','HN','Hà Nội',NULL,NULL, NULL,
     NOW(), NOW(), NULL),
    ('c29464b1-6b74-4cde-9e9c-51bf0ecc522f','HCMC','Thành phố Hồ Chí Minh',NULL,NULL,
     NULL, NOW(), NOW(), NULL);

-- Metro Time Slots
INSERT INTO public."MetroTimeSlots"
("Id","DayOfWeek","SpecialDate","IsAbnormal","OpenTime","CloseTime","Shift",
 "CreatedBy","LastUpdatedBy","DeletedBy","CreatedAt","LastUpdatedAt","DeletedAt")
VALUES
    ('a1b2c3d4-e5f6-7a8b-9c0d-e1f2a3b4c5d6',NULL,NULL,FALSE,'08:00:00','11:00:00',1,NULL,NULL,NULL,NOW(),NOW(),NULL),  -- Morning
    ('b2c3d4e5-f6a7-8b9c-0d1e-f2a3b4c5d6e7',NULL,NULL,FALSE,'13:00:00','16:00:00',2,NULL,NULL,NULL,NOW(),NOW(),NULL),  -- Afternoon
    ('c3d4e5f6-a7b8-9c0d-1e2f-a3b4c5d6e7f8',NULL,NULL,FALSE,'18:00:00','21:00:00',3,NULL,NULL,NULL,NOW(),NOW(),NULL),  -- Evening
    ('d4e5f6a7-b8c9-0d1e-2f3a-b4c5d6e7f8a9',NULL,NULL,FALSE,'23:00:00','02:00:00',4,NULL,NULL,NULL,NOW(),NOW(),NULL);

-- insert default user
-- If Roles already has data, clear it first
DO $$
    BEGIN
        IF EXISTS (SELECT 1 FROM public."Roles") THEN
            DELETE FROM public."Roles";
        END IF;
    END
$$;

INSERT INTO public."Roles"
("Id","ConcurrencyStamp","Name","NormalizedName")
VALUES
    ('021239ef-1d1f-4676-a402-aff9fd24c0c8','8f9fc771-6b87-441a-ba62-a9a68b6bc629','Admin','ADMIN'),
    ('d56ba56c-6469-4494-913a-88d3639f905e','8944cb35-777f-47f6-8202-423e1d6df65a','Customer','CUSTOMER'),
    ('d76a73b5-f94f-4fc9-9009-00a8f1716b7d','a8d5d83a-96d2-4121-bbd8-d5e85e4ffc90','Staff','STAFF');

-- Seed Users
INSERT INTO public."Users"
("Id","AccessFailedCount","AccountName","AccountNo","Address","Avatar","BankId","BirthDate",
 "ConcurrencyStamp","CreatedBy","CreatedTime","DeletedBy","DeletedTime",
 "Email","EmailConfirmed","FullName","LastUpdatedBy","LastUpdatedTime",
 "LockoutEnabled","LockoutEnd","NormalizedEmail","NormalizedUserName",
 "OTP","PasswordHash","PhoneNumber","PhoneNumberConfirmed","TwoFactorEnabled",
 "UserName","Verified")
VALUES
    (
        '5ca7e417-15c3-4bf4-b31c-d57b861e4ab3', 0, NULL, NULL, NULL,
        'https://via.placeholder.com/150', NULL, NULL,
        '6c4a58fc-041f-4677-9d88-45f5ca60e831', NULL,
        NOW(), NULL, NULL,
        'admin@example.com', FALSE, 'Admin User', NULL,
        NOW(),
        FALSE, NULL, 'admin@example.com', 'ADMIN',
        NULL, '$2a$12$xGMlYdHQXreJRAsPVtWVueg2x0MFDeOl472DgWkhKJ.fFX5gvsY5m',
        NULL, FALSE, FALSE,
        'admin', NOW()
    ),
    (
        '64b4c4c2-48ab-4a97-af1c-b25f1aa86362', 0, NULL, NULL, NULL,
        'https://via.placeholder.com/150', NULL, NULL,
        '9f6e38a8-f982-46fb-b031-0e717614d8eb', NULL,
        NOW(), NULL, NULL,
        'staff@example.com', FALSE, 'Staff', NULL,
        NOW(),
        FALSE, NULL, 'staff@example.com', 'STAFF',
        NULL, '$2a$12$xGMlYdHQXreJRAsPVtWVueg2x0MFDeOl472DgWkhKJ.fFX5gvsY5m',
        NULL, FALSE, FALSE,
        'staff', NOW()
    ),
    (
        '93155333-ab24-4410-b8f5-a77c77e81195', 0, NULL, NULL, NULL,
        'https://via.placeholder.com/150', NULL, NULL,
        '9f6e38a8-f982-46fb-b031-0e717614d8eb', NULL,
        NOW(), NULL, NULL,
        'quang@example.com', FALSE, 'Quang', NULL,
        NOW(),
        FALSE, NULL, 'quang@example.com', 'QUANGTDM',
        NULL, '$2a$12$xGMlYdHQXreJRAsPVtWVueg2x0MFDeOl472DgWkhKJ.fFX5gvsY5m',
        '012345678', FALSE, FALSE,
        'quangtdm', NOW()
    ),
    (
        'dfe95016-f1cd-4c6e-b03f-ee917550584e', 0, NULL, NULL, NULL,
        'https://via.placeholder.com/150', NULL, NULL,
        '9f6e38a8-f982-46fb-b031-0e717614d8eb', NULL,
        NOW(), NULL, NULL,
        'staff@example.com', FALSE, 'Staff cargo ben thanh', NULL,
        NOW(),
        FALSE, NULL, 'staff@example.com', 'STAFF-CG-BT',
        NULL, '$2a$12$xGMlYdHQXreJRAsPVtWVueg2x0MFDeOl472DgWkhKJ.fFX5gvsY5m',
        NULL, FALSE, FALSE,
        'staff-cg-bt', NOW()
    ),
    (
        'd87ac817-eaa4-4b90-8bc3-3c1e04175a15', 0, NULL, NULL, NULL,
        'https://via.placeholder.com/150', NULL, NULL,
        '9f6e38a8-f982-46fb-b031-0e717614d8eb', NULL,
        NOW(), NULL, NULL,
        'staff@example.com', FALSE, 'Staff checker ben thanh', NULL,
        NOW(),
        FALSE, NULL, 'staff@example.com', 'STAFF-CK-BT',
        NULL, '$2a$12$xGMlYdHQXreJRAsPVtWVueg2x0MFDeOl472DgWkhKJ.fFX5gvsY5m',
        NULL, FALSE, FALSE,
        'staff-ck-bt', NOW()
    );

-- Seed UserRoles
INSERT INTO public."UserRoles"
("RoleId","UserId","Discriminator")
VALUES
    ('021239ef-1d1f-4676-a402-aff9fd24c0c8','5ca7e417-15c3-4bf4-b31c-d57b861e4ab3','UserRoleEntity'),
    ('d76a73b5-f94f-4fc9-9009-00a8f1716b7d','64b4c4c2-48ab-4a97-af1c-b25f1aa86362','UserRoleEntity'),
    ('d56ba56c-6469-4494-913a-88d3639f905e','93155333-ab24-4410-b8f5-a77c77e81195','UserRoleEntity'),
    ('d76a73b5-f94f-4fc9-9009-00a8f1716b7d','dfe95016-f1cd-4c6e-b03f-ee917550584e','UserRoleEntity'),
    ('d76a73b5-f94f-4fc9-9009-00a8f1716b7d','d87ac817-eaa4-4b90-8bc3-3c1e04175a15','UserRoleEntity');

-- Clear all system configs
DO $$
    BEGIN
        IF EXISTS (SELECT 1 FROM public."SystemConfigs") THEN
            DELETE FROM public."SystemConfigs";
        END IF;
    END
$$;
DO $$
    BEGIN
        IF EXISTS (SELECT 1 FROM public."SystemConfigs") THEN
            Update public."SystemConfigs" SET "IsActive" = TRUE;
        END IF;
    END
$$;
-- Seed SystemConfigs
INSERT INTO public."SystemConfigs"
("Id","ConfigKey","ConfigValue","Description","ConfigType", "IsActive",
 "CreatedBy","LastUpdatedBy","DeletedBy","CreatedAt","LastUpdatedAt","DeletedAt")
VALUES
    -- System
    ('39f8a5d6-6c9e-4b4f-a5d6-6c9eb4f8a5d6','MAX_DISTANCE_IN_METERS','5000',
     'Bán kính phạm vi tìm kiếm các ga gần vị trí người dùng', 1,TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('49b4f8a5-d6c9-e4b4-f8a5-d6c9eb4f8a5d','MAX_COUNT_STATION_NEAR_USER','3',
     'Số ga gần vị trí khách hàng mà hệ thống cần tìm', 1,TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('c948e65c-616e-48a2-b953-0821d2544ac2','MAX_CAPACITY_PER_LINE_KG','75000',
     'Trọng lượng tối đa cho phép trên mỗi chuyến tàu (kg)', 1,TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('0caa5c76-65a1-4ff7-a336-be01b21a723d','MAX_CAPACITY_PER_LINE_M3','480',
     'Thể tích tối đa cho phép trên mỗi chuyến tàu (m3)', 1,TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL),

    -- policy
    ('a3f5b9e1-4c12-4fa8-9b2a-7d5e1c2f3a4b','CONFIRMATION_HOUR','24',
     'Số giờ cho phép xác nhận đơn hàng',2,TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('b4d6c8f2-5d23-5gb9-0c3b-8e6f2d3g4h5c','PAYMENT_REQUEST_HOUR','24',
     'Số giờ yêu cầu thanh toán',2,TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('c5e7d9g3-6e34-6hc0-1d4c-9f7g3e4h5i6d','ALLOW_CANCEL_BEFORE_HOUR','24',
     'Cho phép hủy trước bao nhiêu giờ',2,TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('d6f8e0h4-7f45-7id1-2e5d-0g8h4f5i6j7e','REFUND_PERCENT','80',
     'Phần trăm hoàn tiền khi hủy',2,TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('e7g9f1i5-8g56-8je2-3f6e-1h9i5g6j7k8f','SURCHARGE_AFTER_DELIVERED_HOUR','48',
     'Phụ phí sau thời gian giao',2,TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('f8h0g2j6-9h67-9kf3-4g7f-2i0j6h7k8l9g','SURCHARGE_PER_DAY_PERCENT','0.01',
     'Phụ phí lưu kho mỗi ngày (VND)',2,TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('2e9e0869-85e5-4c23-bedc-126c27f50076','MAX_SCHEDULE_SHIPMENT_DAY','15',
     'Số ngày tối đa khách có thể đặt trước cho kiện hàng',2,TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('09i1h3k7-0i78-0lg4-5h8g-3j1k7i8l9m0h','FREE_STORAGE_AFTER_DAY','30',
     'Số ngày thanh lý hàng tồn kho',2,TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('29e61663-34e5-457e-a6f8-121ee33ce4e4','SCHEDULE_BEFORE_SHIFT_MINUTES','30',
     'Số phần trăm ưu đãi cho các kiện hàng giao ca đêm',2,TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL),

    -- Price -- type=3
    ('ae2856fc-485b-42b8-b986-a94318869b9e','DISTANCE_STEP_KM','300',
     'Bước nhảy khoảng cách tính giá (km)', 3,TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('aa1bdb30-7df7-4efd-8fc0-663c9b0576f5','PRICE_STEP_PERCENT_PER_DISTANCE_TIER','0.5',
     'Phần trăm giá theo từng bậc khoảng cách', 3,TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('d5e99000-fd33-4eb6-b0ee-708c0ffbf5f7','DISTANCE_TIER_QUANTITY','10',
     'Số lượng bậc khoảng cách', 3,TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('91c3f6a9-6f9a-4150-a31d-2b51ea2e7c07','WEIGHT_TIER_1_MAX_KG','5',
     'Trọng lượng tối đa bậc 1 (kg)', 3,TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('a3dbe011-5d6f-4c51-9714-2aa9a5a9246d','PRICE_TIER_1_VND','25000',
     'Giá bậc 1 (VND)', 3,TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('8ebaa2ee-b418-426b-bd86-5e5698324c80','WEIGHT_TIER_2_MAX_KG','10',
     'Trọng lượng tối đa bậc 2 (kg)', 3,TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('7cc423fb-925b-4031-9f19-92cb6dabc2e7','PRICE_TIER_2_VND_PER_KG','7500',
     'Giá bậc 2 (VND/kg)', 3,TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('0631cfda-896c-4f77-bd96-2ad90e127521','WEIGHT_TIER_3_MAX_KG','50',
     'Trọng lượng tối đa bậc 3 (kg)', 3,TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('8f95513d-8b40-4f11-913a-9240fc8ab8bd','PRICE_TIER_3_VND_PER_KG','7000',
     'Giá bậc 3 (VND/kg)', 3,TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('49159b29-8477-4927-bb98-a2342919c55c','WEIGHT_TIER_4_MAX_KG','100',
     'Trọng lượng tối đa bậc 4 (kg)', 3,TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('3df009ac-6562-484e-9acd-299138b5aa42','PRICE_TIER_4_VND_PER_KG','6500',
     'Giá bậc 4 (VND/kg)', 3,TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('7000f9a3-1a1d-4f73-aacf-704ffcaa3ee6','PRICE_TIER_5_VND_PER_KG','6000',
     'Giá bậc 5 (VND/kg)', 3,TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL);

-- ───────────────────────────────────────────────────────────────
DO $$
    BEGIN
        IF EXISTS (SELECT 1 FROM public."ParcelCategories") THEN
            DELETE FROM public."ParcelCategories";
        END IF;
    END
$$;
-- Parcel Category
INSERT INTO public."ParcelCategories"
("Id","CategoryName","Description","WeightLimitKg","VolumeLimitCm3",
 "LengthLimitCm","WidthLimitCm","HeightLimitCm","IsActive",
 "CreatedBy","LastUpdatedBy","DeletedBy","CreatedAt","LastUpdatedAt","DeletedAt",
 "TotalSizeLimitCm", "InsuranceRate", "InsuranceFeeVnd", "IsInsuranceRequired")
VALUES
    -- Hàng hóa chung / General cargo
    ('0a1b2c3d-4e5f-6789-abcd-0fedcba98765',
     'Hàng hóa chung',
     'Hàng bình thường, không cần xử lý đặc biệt',
     NULL,   NULL,
     150, 150, 150,
     TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL,
     300, NULL, 5000, FALSE),

    -- Hàng dễ hư hỏng / Perishable goods
    ('3d4e5f6a-7b8c-9012-def0-3fedcba98765',
     'Hàng dễ hư hỏng',
     'Hàng cần bảo quản lạnh hoặc kiểm soát nhiệt độ',
     NULL,   NULL,
     150, 150, 150,
     TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL,
     300, NULL, 5000, FALSE),

    -- Hàng giá trị / Valuable goods
    ('4e5f6a7b-8c9d-0123-ef01-4fedcba98765',
     'Hàng giá trị',
     'Hàng giá trị cao, cần bảo mật và giám sát',
     NULL,   NULL,
     150, 150, 150,
     TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL,
     300, 0.005, NULL, TRUE),

    -- Hàng dễ vỡ
    ('9c50210e-5670-4608-b58a-c54d0b3cc249',
     'Hàng dễ vỡ',
     'Hàng dễ vỡ, cần đóng gói cẩn thận',
     NULL,   NULL,
     150, 150, 150,
     TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL,
     300, NULL, 20000, TRUE);

-- ─────────────────────────────────────────────────────────────────
-- 22/07/2025: CHANGE DATA
-- ─────────────────────────────────────────────────────────────────
-- Clear all transaction data
DO $$
    BEGIN
        IF EXISTS (SELECT 1 FROM public."Transactions") THEN
            DELETE FROM public."Transactions";
        END IF;
    END
$$;

-- Clear all shipment data
DO $$
    BEGIN
        IF EXISTS (SELECT 1 FROM public."ShipmentImages") THEN
            DELETE FROM public."ShipmentImages";
        END IF;
    END
$$;
DO $$
    BEGIN
        IF EXISTS (SELECT 1 FROM public."Shipments") THEN
            DELETE FROM public."Shipments";
        END IF;
    END
$$;
-- ─────────────────────────────────────────────────────────────────
-- Clear all routes, metro lines, and stations, staff assignments, trains
DO $$
    BEGIN
        IF EXISTS (SELECT 1 FROM public."MetroTrains") THEN
            DELETE FROM public."MetroTrains";
        END IF;
    END
$$;
DO $$
    BEGIN
        IF EXISTS (SELECT 1 FROM public."StaffAssignments") THEN
            DELETE FROM public."StaffAssignments";
        END IF;
    END
$$;
DO $$
    BEGIN
        IF EXISTS (SELECT 1 FROM public."Routes") THEN
            DELETE FROM public."Routes";
        END IF;
    END
$$;
DO $$
    BEGIN
        IF EXISTS (SELECT 1 FROM public."MetroLines") THEN
            DELETE FROM public."MetroLines";
        END IF;
    END
$$;
DO $$
    BEGIN
        IF EXISTS (SELECT 1 FROM public."Stations") THEN
            DELETE FROM public."Stations";
        END IF;
    END
$$;

-- Seed Metro Lines
-- Metro Line 1: Bến Thành – Suối Tiên
-- Delete Carriage & price
INSERT INTO public."MetroLines"
("Id","RegionId","LineNameVi","LineNameEn","LineCode","TotalKm","TotalStations", "LineType",
 "LineOwner", "StationListJSON",
/* "CarriagesPerTrain","CarriageLenghtMeter","CarriageWidthMeter", "CarriageHeightMeter",
 "CarriageWeightTons","BasePriceVndPerKm","MinHeadwayMin","MaxHeadwayMin","RouteTimeMin",*/
 "DwellTimeMin","ColorHex","IsActive","CreatedBy","LastUpdatedBy","DeletedBy","CreatedAt","LastUpdatedAt","DeletedAt")
VALUES
    (
        'e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',  -- GUID
        'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',                           -- existing RegionId
        'Tuyến 1: Bến Thành – Suối Tiên',
        'Metro 1: Ben Thanh – Suoi Tien',
        'HCMC-L1',19.70,14,
        'Đường sắt đô thị Thành Phố Hồ Chí Minh',
        'Ban Quản lý Đường sắt đô thị Thành phố Hồ Chí Minh (MAUR)',
        '[
          {
            "StationId": "5e9f3c27-8d6b-4c44-9a14-dfd6a3e2e8f0",
            "StationCode": "HCMC-L1-01"
          },
          {
            "StationId": "1a2b3c4d-5e6f-7081-92a3-b4c5d6e7f8a9",
            "StationCode": "HCMC-L1-02"
          },
          {
            "StationId": "2b3c4d5e-6f70-8192-a3b4-c5d6e7f8a9b0",
            "StationCode": "HCMC-L1-03"
          },
          {
            "StationId": "3c4d5e6f-7081-92a3-b4c5-d6e7f8a9b0c1",
            "StationCode": "HCMC-L1-04"
          },
          {
            "StationId": "4d5e6f70-8192-a3b4-c5d6-e7f8a9b0c1d2",
            "StationCode": "HCMC-L1-05"
          },
          {
            "StationId": "5e6f7081-92a3-b4c5-d6e7-f8a9b0c1d2e3",
            "StationCode": "HCMC-L1-06"
          },
          {
            "StationId": "6f708192-a3b4-c5d6-e7f8-a9b0c1d2e3f4",
            "StationCode": "HCMC-L1-07"
          },
          {
            "StationId": "708192a3-b4c5-d6e7-f8a9-b0c1d2e3f4a5",
            "StationCode": "HCMC-L1-08"
          },
          {
            "StationId": "8192a3b4-c5d6-e7f8-a9b0-c1d2e3f4a5b6",
            "StationCode": "HCMC-L1-09"
          },
          {
            "StationId": "92a3b4c5-d6e7-f8a9-b0c1-d2e3f4a5b6c7",
            "StationCode": "HCMC-L1-10"
          },
          {
            "StationId": "a3b4c5d6-e7f8-a9b0-c1d2-e3f4a5b6c7d8",
            "StationCode": "HCMC-L1-11"
          },
          {
            "StationId": "b4c5d6e7-f8a9-b0c1-d2e3-f4a5b6c7d8e9",
            "StationCode": "HCMC-L1-12"
          },
          {
            "StationId": "c5d6e7f8-a9b0-c1d2-e3f4-a5b6c7d8e9f0",
            "StationCode": "HCMC-L1-13"
          },
          {
            "StationId": "d6e7f8a9-b0c1-d2e3-f4a5-b6c7d8e9f0a1",
            "StationCode": "HCMC-L1-14"
          }
        ]',
        /*3,20.25,2.95,4.08,
        27, 5000,
        8,12,34,*/
        30,
        '#0079B8',
        TRUE,
        NULL, NULL, NULL,
        NOW(), NOW(), NULL
    );

-- Metro Line 1:  Seed Stations,
INSERT INTO public."Stations"
("Id","StationCode","StationNameVi","StationNameEn","Address","IsUnderground","IsActive","RegionId","Latitude","Longitude",
 "CreatedBy","LastUpdatedBy","DeletedBy","CreatedAt","LastUpdatedAt","DeletedAt", "IsMultiLine")
VALUES
    -- Underground
    ('5e9f3c27-8d6b-4c44-9a14-dfd6a3e2e8f0','HCMC-L1-01','Bến Thành','Ben Thanh',
     'Quách Thị Trang, Phường Bến Thành, Quận 1, TP. Hồ Chí Minh, Việt Nam',
     TRUE,TRUE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',
     10.77060,106.69738,NULL,NULL,NULL,NOW(),NOW(),
     NULL, TRUE),
    ('1a2b3c4d-5e6f-7081-92a3-b4c5d6e7f8a9','HCMC-L1-02','Nhà hát Thành phố','Opera House',
     'Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh, Việt Nam',
     TRUE,FALSE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',
     10.77547,106.70215,NULL,NULL,NULL,NOW(),NOW(),
     NULL, FALSE),
    ('2b3c4d5e-6f70-8192-a3b4-c5d6e7f8a9b0','HCMC-L1-03','Ba Son','Ba Son',
     'Đại lộ Tôn Đức Thắng, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh, Việt Nam',
     TRUE,FALSE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',
     10.78160,106.70800,NULL,NULL,NULL,NOW(),NOW(),
     NULL, FALSE),

    -- Elevated
    ('3c4d5e6f-7081-92a3-b4c5-d6e7f8a9b0c1','HCMC-L1-04','Công viên Văn Thánh','Van Thanh Park',
     'Quận Bình Thạnh, TP. Hồ Chí Minh, Việt Nam',
     FALSE,FALSE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',
     10.79613,106.71554,NULL,NULL,NULL,NOW(),NOW(),
     NULL, FALSE),
    ('4d5e6f70-8192-a3b4-c5d6-e7f8a9b0c1d2','HCMC-L1-05','Tân Cảng','Tan Cang',
     'Đường Điện Biên Phủ, Phường 22, Quận Bình Thạnh, TP. Hồ Chí Minh, Việt Nam',
     FALSE,FALSE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',
     10.79862,106.72330,NULL,NULL,NULL,NOW(),NOW(),
     NULL, FALSE),
    ('5e6f7081-92a3-b4c5-d6e7-f8a9b0c1d2e3','HCMC-L1-06','Thảo Điền','Thao Dien',
     'Công viên Cầu Sài Gòn, Phường Thảo Điền, TP. Thủ Đức, Việt Nam',
     FALSE,FALSE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',
     10.80053,106.73368,NULL,NULL,NULL,NOW(),NOW(),
     NULL, FALSE),
    ('6f708192-a3b4-c5d6-e7f8-a9b0c1d2e3f4','HCMC-L1-07','An Phú','An Phu',
     'Phường Thảo Điền, TP. Thủ Đức, Việt Nam',
     FALSE,FALSE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',
     10.80215,106.74234,NULL,NULL,NULL,NOW(),NOW(),
     NULL, FALSE),
    ('708192a3-b4c5-d6e7-f8a9-b0c1d2e3f4a5','HCMC-L1-08','Rạch Chiếc','Rach Chiec',
     'Phường An Phú, TP. Thủ Đức, Việt Nam',
     FALSE,FALSE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',
     10.80854,106.75529,NULL,NULL,NULL,NOW(),NOW(),
     NULL, FALSE),
    ('8192a3b4-c5d6-e7f8-a9b0-c1d2e3f4a5b6','HCMC-L1-09','Phước Long','Phuoc Long',
     'Phường Trường Thọ, TP. Thủ Đức, Việt Nam',
     FALSE,FALSE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',
     10.82146,106.75820,NULL,NULL,NULL,NOW(),NOW(),
     NULL, FALSE),
    ('92a3b4c5-d6e7-f8a9-b0c1-d2e3f4a5b6c7','HCMC-L1-10','Bình Thái','Binh Thai',
     'Phường Trường Thọ, TP. Thủ Đức, Việt Nam',
     FALSE,FALSE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',
     10.83266,106.76389,NULL,NULL,NULL,NOW(),NOW(),
     NULL, FALSE),
    ('a3b4c5d6-e7f8-a9b0-c1d2-e3f4a5b6c7d8','HCMC-L1-11','Thủ Đức','Thu Duc',
     'Phường Bình Thọ, TP. Thủ Đức, Việt Nam',
     FALSE,FALSE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',
     10.84641,106.77167,NULL,NULL,NULL,NOW(),NOW(),
     NULL, FALSE),
    ('b4c5d6e7-f8a9-b0c1-d2e3-f4a5b6c7d8e9','HCMC-L1-12','Khu Công nghệ Cao','Hi-Tech Park',
     'Xa lộ Hà Nội, Phường Linh Trung, TP. Thủ Đức, Việt Nam',
     FALSE,FALSE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',
     10.85905,106.78889,NULL,NULL,NULL,NOW(),NOW(),
     NULL, FALSE),
    ('c5d6e7f8-a9b0-c1d2-e3f4-a5b6c7d8e9f0','HCMC-L1-13','Đại học Quốc Gia','National University',
     'Xa lộ Hà Nội, Phường Linh Trung, TP. Thủ Đức, Việt Nam',
     FALSE,FALSE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',
     10.86634,106.80126,NULL,NULL,NULL,NOW(),NOW(),
     NULL, FALSE),
    ('d6e7f8a9-b0c1-d2e3-f4a5-b6c7d8e9f0a1','HCMC-L1-14','Bến xe Suối Tiên','Suoi Tien Terminal',
     'Xa lộ Hà Nội, Long Bình, TP. Thủ Đức & Phường Bình Thắng, TP. Dĩ An, Bình Dương',
     FALSE,TRUE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',
     10.87952,106.81411,NULL,NULL,NULL,NOW(),NOW(),
     NULL, FALSE);

-- Metro Line 1: Routes between each adjacent station (26 segments) - old
INSERT INTO public."Routes"
("Id","RouteCode","LineId","FromStationId","ToStationId","RouteNameVi","RouteNameEn","Direction",
 "SeqOrder","TravelTimeMin","LengthKm",
 "CreatedBy","LastUpdatedBy","DeletedBy","CreatedAt","LastUpdatedAt","DeletedAt")
VALUES
    -- Forward (Direction = 0)
    ('fa1d2c3b-4e5f-6a7b-8c9d-e0f1a2b3c4d5','HCMC-L1-01-02','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '5e9f3c27-8d6b-4c44-9a14-dfd6a3e2e8f0','1a2b3c4d-5e6f-7081-92a3-b4c5d6e7f8a9',
     'Bến Thành – Nhà hát Thành phố','Ben Thanh – Opera House',0,  1,2, 0.715, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('1b2c3d4e-5f6a-7b8c-9d0e-f1a2b3c4d5e6','HCMC-L1-02-03','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '1a2b3c4d-5e6f-7081-92a3-b4c5d6e7f8a9','2b3c4d5e-6f70-8192-a3b4-c5d6e7f8a9b0',
     'Nhà hát Thành phố – Ba Son','Opera House – Ba Son',           0,  2, 2,0.991, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('2c3d4e5f-6a7b-8c9d-0e1f-a2b3c4d5e6f7','HCMC-L1-03-04','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '2b3c4d5e-6f70-8192-a3b4-c5d6e7f8a9b0','3c4d5e6f-7081-92a3-b4c5-d6e7f8a9b0c1',
     'Ba Son – Công viên Văn Thánh','Ba Son – Van Thanh Park',     0,  3, 3,1.814, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('3d4e5f6a-7b8c-9d0e-1f2a-b3c4d5e6f7a8','HCMC-L1-04-05','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '3c4d5e6f-7081-92a3-b4c5-d6e7f8a9b0c1','4d5e6f70-8192-a3b4-c5d6-e7f8a9b0c1d2',
     'Công viên Văn Thánh – Tân Cảng','Van Thanh Park – Tan Cang',0,  4, 2,0.918, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('4e5f6a7b-8c9d-0e1f-2a3b-c4d5e6f7a8b9','HCMC-L1-05-06','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '4d5e6f70-8192-a3b4-c5d6-e7f8a9b0c1d2','5e6f7081-92a3-b4c5-d6e7-f8a9b0c1d2e3',
     'Tân Cảng – Thảo Điền','Tan Cang – Thao Dien',                0,  5, 2,1.158, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('5f6a7b8c-9d0e-1f2a-3b4c-d5e6f7a8b9c0','HCMC-L1-06-07','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '5e6f7081-92a3-b4c5-d6e7-f8a9b0c1d2e3','6f708192-a3b4-c5d6-e7f8-a9b0c1d2e3f4',
     'Thảo Điền – An Phú','Thao Dien – An Phu',                  0,  6, 2,0.957, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('6a7b8c9d-0e1f-2a3b-4c5d-e6f7a8b9c0d1','HCMC-L1-07-08','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '6f708192-a3b4-c5d6-e7f8-a9b0c1d2e3f4','708192a3-b4c5-d6e7-f8a9-b0c1d2e3f4a5',
     'An Phú – Rạch Chiếc','An Phu – Rach Chiec',              0,  7, 3,1.654, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('7b8c9d0e-1f2a-3b4c-5d6e-f7a8b9c0d1e2','HCMC-L1-08-09','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '708192a3-b4c5-d6e7-f8a9-b0c1d2e3f4a5','8192a3b4-c5d6-e7f8-a9b0-c1d2e3f4a5b6',
     'Rạch Chiếc – Phước Long','Rach Chiec – Phuoc Long',        0,  8,3, 1.466, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('8c9d0e1f-2b3c-4d5e-6f7a-8b9c0d1e2f3a','HCMC-L1-09-10','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '8192a3b4-c5d6-e7f8-a9b0-c1d2e3f4a5b6','92a3b4c5-d6e7-f8a9-b0c1-d2e3f4a5b6c7',
     'Phước Long – Bình Thái','Phuoc Long – Binh Thai',         0,  9, 3,1.393, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('9d0e1f2a-3b4c-5d6e-7f8a-b9c0d1e2f3a4','HCMC-L1-10-11','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '92a3b4c5-d6e7-f8a9-b0c1-d2e3f4a5b6c7','a3b4c5d6-e7f8-a9b0-c1d2-e3f4a5b6c7d8',
     'Bình Thái – Thủ Đức','Binh Thai – Thu Duc',              0, 10, 3,1.744, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('0e1f2a3b-4c5d-6e7f-8a9b-c0d1e2f3a4b5','HCMC-L1-11-12','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     'a3b4c5d6-e7f8-a9b0-c1d2-e3f4a5b6c7d8','b4c5d6e7-f8a9-b0c1-d2e3-f4a5b6c7d8e9',
     'Thủ Đức – Hi-Tech Park','Thu Duc – Hi-Tech Park',       0, 11, 3,2.380, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('1f2a3b4c-5d6e-7f8a-9b0c-d1e2f3a4b5c6','HCMC-L1-12-13','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     'b4c5d6e7-f8a9-b0c1-d2e3-f4a5b6c7d8e9','c5d6e7f8-a9b0-c1d2-e3f4-a5b6c7d8e9f0',
     'Hi-Tech Park – Đại học Quốc Gia','Hi-Tech Park – National University',  0, 12, 3,1.575, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('2a3b4c5d-6e7f-8a9b-0c1d-e2f3a4b5c6d7','HCMC-L1-13-14','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     'c5d6e7f8-a9b0-c1d2-e3f4-a5b6c7d8e9f0','d6e7f8a9-b0c1-d2e3-f4a5-b6c7d8e9f0a1',
     'Đại học Quốc Gia – Bến xe Suối Tiên','National University – Suoi Tien Terminal', 0, 13, 3,2.056, NULL,NULL,NULL,NOW(),NOW(),NULL),

    -- Reverse (Direction = 1)
    ('ca14f6bb-71a8-4f9b-8c15-1d2b8992c437','HCMC-L1-14-13','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     'd6e7f8a9-b0c1-d2e3-f4a5-b6c7d8e9f0a1','c5d6e7f8-a9b0-c1d2-e3f4-a5b6c7d8e9f0',
     'Bến xe Suối Tiên – Đại học Quốc Gia','Suoi Tien Terminal – National University', 1,  1, 3,2.056, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('f053780b-1c99-45b4-9cb7-e8daea0a90d8','HCMC-L1-13-12','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     'c5d6e7f8-a9b0-c1d2-e3f4-a5b6c7d8e9f0','b4c5d6e7-f8a9-b0c1-d2e3-f4a5b6c7d8e9',
     'Đại học Quốc Gia – Hi-Tech Park','National University – Hi-Tech Park',         1,  2, 3,1.575, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('9020fe12-5630-46c1-9f10-cd9c3f919e3e','HCMC-L1-12-11','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     'b4c5d6e7-f8a9-b0c1-d2e3-f4a5b6c7d8e9','a3b4c5d6-e7f8-a9b0-c1d2-e3f4a5b6c7d8',
     'Hi-Tech Park – Thủ Đức','Hi-Tech Park – Thu Duc',            1,  3, 3,2.380,NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('e881211f-390c-4488-ab8f-d93a9e204372','HCMC-L1-11-10','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     'a3b4c5d6-e7f8-a9b0-c1d2-e3f4a5b6c7d8','92a3b4c5-d6e7-f8a9-b0c1-d2e3f4a5b6c7',
     'Thủ Đức – Bình Thái','Thu Duc – Binh Thai',                1,  4, 3,1.744,NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('a4b2bc3f-a169-4c09-aadb-e4625d04fc8c','HCMC-L1-10-09','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '92a3b4c5-d6e7-f8a9-b0c1-d2e3f4a5b6c7','8192a3b4-c5d6-e7f8-a9b0-c1d2e3f4a5b6',
     'Bình Thái – Phước Long','Binh Thai – Phuoc Long',         1,  5, 3,1.393, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('af4fe5e6-b57d-45d3-8caf-d053cb14ffa9','HCMC-L1-09-08','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '8192a3b4-c5d6-e7f8-a9b0-c1d2e3f4a5b6','708192a3-b4c5-d6e7-f8a9-b0c1d2e3f4a5',
     'Phước Long – Rạch Chiếc','Phuoc Long – Rach Chiec',       1,  6, 3,1.466, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('eae9c4c1-1239-4dcd-ae22-5f4266972add','HCMC-L1-08-07','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '708192a3-b4c5-d6e7-f8a9-b0c1d2e3f4a5','6f708192-a3b4-c5d6-e7f8-a9b0c1d2e3f4',
     'Rạch Chiếc – An Phú','Rach Chiec – An Phu',               1,  7, 3,1.654, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('49d3b912-8f57-4323-af0a-3d02f5e941e6','HCMC-L1-07-06','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '6f708192-a3b4-c5d6-e7f8-a9b0c1d2e3f4','5e6f7081-92a3-b4c5-d6e7-f8a9b0c1d2e3',
     'An Phú – Thảo Điền','An Phu – Thao Dien',                1,  8, 2,0.957, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('0c4cc394-fc83-494b-80c5-04b9d2ecc2aa','HCMC-L1-06-05','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '5e6f7081-92a3-b4c5-d6e7-f8a9b0c1d2e3','4d5e6f70-8192-a3b4-c5d6-e7f8a9b0c1d2',
     'Thảo Điền – Tân Cảng','Thao Dien – Tan Cang',            1,  9, 2,1.158, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('9bbf80e5-26a8-4aa8-9860-5abad379917b','HCMC-L1-05-04','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '4d5e6f70-8192-a3b4-c5d6-e7f8a9b0c1d2','3c4d5e6f-7081-92a3-b4c5-d6e7f8a9b0c1',
     'Tân Cảng – Công viên Văn Thánh','Tan Cang – Van Thanh Park',1, 10,2,0.918,NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('9ab91376-fa23-493b-9ca7-0adc65818357','HCMC-L1-04-03','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '3c4d5e6f-7081-92a3-b4c5-d6e7f8a9b0c1','2b3c4d5e-6f70-8192-a3b4-c5d6e7f8a9b0',
     'Công viên Văn Thánh – Ba Son','Van Thanh Park – Ba Son',1, 11,3,1.814,NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('2f146d84-60b2-4d4c-b609-856f89cfcd0b','HCMC-L1-03-02','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '2b3c4d5e-6f70-8192-a3b4-c5d6e7f8a9b0','1a2b3c4d-5e6f-7081-92a3-b4c5d6e7f8a9',
     'Ba Son – Nhà hát Thành phố','Ba Son – Opera House',        1, 12,2,0.991,NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('1c07cfde-3ef9-4add-8289-37cff5941a5e','HCMC-L1-02-01','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '1a2b3c4d-5e6f-7081-92a3-b4c5d6e7f8a9','5e9f3c27-8d6b-4c44-9a14-dfd6a3e2e8f0',
     'Nhà hát Thành phố – Bến Thành','Opera House – Ben Thanh', 1, 13,2,0.715,NULL,NULL,NULL,NOW(),NOW(),NULL);

-- Metro Line 2: Ben Thanh – Tham Luong -- Delete Carriage & price
INSERT INTO public."MetroLines"
("Id","RegionId","LineNameVi","LineNameEn","LineCode","TotalKm","TotalStations","DwellTimeMin",
 "LineType","LineOwner","StationListJSON",
 "ColorHex","IsActive","CreatedBy","LastUpdatedBy","DeletedBy","CreatedAt","LastUpdatedAt","DeletedAt")
VALUES
    (
        '2f5c8e93-4b7a-4d2e-8f6c-1a9b5e7d3c2f',  -- Line 2 ID
        'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',  -- HCMC Region ID (existing)
        'Tuyến 2: Bến Thành – Tân Bình',
        'Metro 2: Ben Thanh – Tan Binh',
        'HCMC-L2', 10.23, 11,30,
        'Đường sắt đô thị Thành Phố Hồ Chí Minh',
        'Ban Quản lý Đường sắt đô thị Thành phố Hồ Chí Minh (MAUR)',
        '[
          {
            "StationId": "5e9f3c27-8d6b-4c44-9a14-dfd6a3e2e8f0",
            "StationCode": "HCMC-L2-01"
          },
          {
            "StationId": "8a4c6e91-2f5b-4d3e-9c7a-6b8d2f4e7c9a",
            "StationCode": "HCMC-L2-02"
          },
          {
            "StationId": "7b5d9f82-3e6c-4a2f-8d9b-5c7e3f6a8d1b",
            "StationCode": "HCMC-L2-03"
          },
          {
            "StationId": "6c8e4f73-1d7a-4b5c-9e8c-4f6a1d7b9e2c",
            "StationCode": "HCMC-L2-04"
          },
          {
            "StationId": "5d9f7e64-2c8b-4a6d-8f9d-7e5c2c8a6f3d",
            "StationCode": "HCMC-L2-05"
          },
          {
            "StationId": "4e8a6f55-3d9c-4b7e-9a8e-6f4d3d9b7a4e",
            "StationCode": "HCMC-L2-06"
          },
          {
            "StationId": "3f9b5e46-4c8d-4a6f-8b9f-5e3c4c8a6b5f",
            "StationCode": "HCMC-L2-07"
          },
          {
            "StationId": "2a7c4d37-5b9e-4c8a-9c7a-4d2b5b9c8a7a",
            "StationCode": "HCMC-L2-08"
          },
          {
            "StationId": "1b8d3c28-6a7f-4d9b-8d6b-3c1a6a7d9b8b",
            "StationCode": "HCMC-L2-09"
          },
          {
            "StationId": "9c6e2b19-7d8a-4e6c-7e9c-2b9d7d8e6c2c",
            "StationCode": "HCMC-L2-10"
          },
          {
            "StationId": "8d7f1a0a-5e9b-4f7d-6f8d-1a8e5e9f7d1d",
            "StationCode": "HCMC-L2-11"
          }
        ]',
        '#E60012',TRUE,
        NULL,
        NULL,
        NULL,NOW(),NOW(),NULL
    );

-- Metro Line 2: Stations Insert, -- Edit IsActive start - end station only to TRUE
INSERT INTO public."Stations"
("Id","StationCode","StationNameVi","StationNameEn","Address","IsUnderground","IsActive","RegionId","Latitude","Longitude",
 "CreatedBy","LastUpdatedBy","DeletedBy","CreatedAt","LastUpdatedAt","DeletedAt", "IsMultiLine")
VALUES
-- L2-01: Ben Thanh (use existing station)
-- L2-02: Tao Dan
('8a4c6e91-2f5b-4d3e-9c7a-6b8d2f4e7c9a','HCMC-L2-02','Tao Đàn','Tao Dan',
 'Cạnh Công viên Tao Đàn, P. Bến Thành, Q.1, TP.HCM',TRUE,TRUE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.7725,106.691111,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL, TRUE),

-- L2-03: Dan Chu
('7b5d9f82-3e6c-4a2f-8d9b-5c7e3f6a8d1b','HCMC-L2-03','Dân Chủ','Dan Chu',
 'Ngã tư Cách Mạng Tháng 8 - Nguyễn Thượng Hiền, P.6, Q.3, TP.HCM',TRUE,FALSE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.778056,106.681111,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL, FALSE),

-- L2-04: Hoa Hung
('6c8e4f73-1d7a-4b5c-9e8c-4f6a1d7b9e2c','HCMC-L2-04','Hòa Hưng','Hoa Hung',
 'Gần đường Cách Mạng Tháng 8, P.13, Q.10, TP.HCM',TRUE,FALSE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.781111,106.675,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL, FALSE),

-- L2-05: Le Thi Rieng
('5d9f7e64-2c8b-4a6d-8f9d-7e5c2c8a6f3d','HCMC-L2-05','Lê Thị Riêng','Le Thi Rieng',
 'Cạnh Công viên Lê Thị Riêng, P.15, Q.10, TP.HCM',TRUE,FALSE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.786111,106.665556,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL, FALSE),

-- L2-06: Pham Van Hai
('4e8a6f55-3d9c-4b7e-9a8e-6f4d3d9b7a4e','HCMC-L2-06','Phạm Văn Hai','Pham Van Hai',
 'Ngã tư Phạm Văn Hai - Cách Mạng Tháng 8, P.3, Q.Tân Bình, TP.HCM',TRUE,FALSE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.789167,106.66,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL, FALSE),

-- L2-07: Bay Hien
('3f9b5e46-4c8d-4a6f-8b9f-5e3c4c8a6b5f','HCMC-L2-07','Bảy Hiền','Bay Hien',
 'Ngã tư Bảy Hiền, P.11, Q.Tân Bình, TP.HCM',TRUE,FALSE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.792778,106.653333,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL, FALSE),

-- L2-08: Nguyen Hong Dao
('2a7c4d37-5b9e-4c8a-9c7a-4d2b5b9c8a7a','HCMC-L2-08','Nguyễn Hồng Đào','Nguyen Hong Dao',
 'Gần đường Nguyễn Hồng Đào, P.14, Q.Tân Bình, TP.HCM',TRUE,FALSE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.799444,106.640556,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL, FALSE),

-- L2-09: Ba Queo
('1b8d3c28-6a7f-4d9b-8d6b-3c1a6a7d9b8b','HCMC-L2-09','Bà Quẹo','Ba Queo',
 'Ngã tư Bà Quẹo, P.14, Q.Tân Bình, TP.HCM',TRUE,FALSE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.805444,106.635278,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL, FALSE),

-- L2-10: Pham Van Bach
('9c6e2b19-7d8a-4e6c-7e9c-2b9d7d8e6c2c','HCMC-L2-10','Phạm Văn Bạch','Pham Van Bach',
 'Đường Phạm Văn Bạch, P.15, Q.Tân Bình, TP.HCM',TRUE,FALSE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.813028,106.632944,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL, FALSE),

-- L2-11: Tan Binh (elevated)
('8d7f1a0a-5e9b-4f7d-6f8d-1a8e5e9f7d1d','HCMC-L2-11','Tân Bình','Tan Binh',
 'Gần cầu vượt Tân Bình, P.15, Q.Tân Bình, TP.HCM',FALSE,TRUE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.821111,106.630556,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL, FALSE);

-- Metro Line 2: Routes Insert (Forward and Reverse) - old
INSERT INTO public."Routes"
("Id","RouteCode","LineId","FromStationId","ToStationId","RouteNameVi","RouteNameEn","Direction",
 "SeqOrder","TravelTimeMin","LengthKm","CreatedBy","LastUpdatedBy","DeletedBy","CreatedAt","LastUpdatedAt","DeletedAt")
VALUES
-- FORWARD DIRECTION (0)
('f3d8c1e4-2a6b-4f9d-8c2f-6b4a9d1e8c3f','HCMC-L2-01-02','2f5c8e93-4b7a-4d2e-8f6c-1a9b5e7d3c2f',
 '5e9f3c27-8d6b-4c44-9a14-dfd6a3e2e8f0','8a4c6e91-2f5b-4d3e-9c7a-6b8d2f4e7c9a',
 'Bến Thành – Tao Đàn','Ben Thanh – Tao Dan',0,1,2,0.69,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

('e4c9b2d5-3f7a-4e8c-9b3e-7a5f8c2b9e4c','HCMC-L2-02-03','2f5c8e93-4b7a-4d2e-8f6c-1a9b5e7d3c2f',
 '8a4c6e91-2f5b-4d3e-9c7a-6b8d2f4e7c9a','7b5d9f82-3e6c-4a2f-8d9b-5c7e3f6a8d1b',
 'Tao Đàn – Dân Chủ','Tao Dan – Dan Chu',0,2,3,1.25,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

('d5a8c3f6-4e9b-4d7a-8c4d-9b6e7a3c8f5d','HCMC-L2-03-04','2f5c8e93-4b7a-4d2e-8f6c-1a9b5e7d3c2f',
 '7b5d9f82-3e6c-4a2f-8d9b-5c7e3f6a8d1b','6c8e4f73-1d7a-4b5c-9e8c-4f6a1d7b9e2c',
 'Dân Chủ – Hòa Hưng','Dan Chu – Hoa Hung',0,3,1,0.62,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

('c6b9d4e7-5f8a-4c6b-9d5c-8a7f6b4d9e6c','HCMC-L2-04-05','2f5c8e93-4b7a-4d2e-8f6c-1a9b5e7d3c2f',
 '6c8e4f73-1d7a-4b5c-9e8c-4f6a1d7b9e2c','5d9f7e64-2c8b-4a6d-8f9d-7e5c2c8a6f3d',
 'Hòa Hưng – Lê Thị Riêng','Hoa Hung – Le Thi Rieng',0,4,3,1.21,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

('b7c8e5f8-6a9b-4b7c-8e6b-9a8f7c5e8b7b','HCMC-L2-05-06','2f5c8e93-4b7a-4d2e-8f6c-1a9b5e7d3c2f',
 '5d9f7e64-2c8b-4a6d-8f9d-7e5c2c8a6f3d','4e8a6f55-3d9c-4b7e-9a8e-6f4d3d9b7a4e',
 'Lê Thị Riêng – Phạm Văn Hai','Le Thi Rieng – Pham Van Hai',0,5,1,0.57,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

('a8d9f6e9-7b8c-4a8d-9f7a-8c9f8d6f9a8a','HCMC-L2-06-07','2f5c8e93-4b7a-4d2e-8f6c-1a9b5e7d3c2f',
 '4e8a6f55-3d9c-4b7e-9a8e-6f4d3d9b7a4e','3f9b5e46-4c8d-4a6f-8b9f-5e3c4c8a6b5f',
 'Phạm Văn Hai – Bảy Hiền','Pham Van Hai – Bay Hien',0,6,2,0.77,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

('97e8a7fa-8c9d-497e-8a8c-9d8a7f9e8a97','HCMC-L2-07-08','2f5c8e93-4b7a-4d2e-8f6c-1a9b5e7d3c2f',
 '3f9b5e46-4c8d-4a6f-8b9f-5e3c4c8a6b5f','2a7c4d37-5b9e-4c8a-9c7a-4d2b5b9c8a7a',
 'Bảy Hiền – Nguyễn Hồng Đào','Bay Hien – Nguyen Hong Dao',0,7,3,1.33,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

('86f9b8eb-9d8e-486f-9b9d-8e9b8e9f9b86','HCMC-L2-08-09','2f5c8e93-4b7a-4d2e-8f6c-1a9b5e7d3c2f',
 '2a7c4d37-5b9e-4c8a-9c7a-4d2b5b9c8a7a','1b8d3c28-6a7f-4d9b-8d6b-3c1a6a7d9b8b',
 'Nguyễn Hồng Đào – Bà Quẹo','Nguyen Hong Dao – Ba Queo',0,8,2,0.78,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

('75a8c9dc-8e9f-475a-8c8e-9f8c9f8a8c75','HCMC-L2-09-10','2f5c8e93-4b7a-4d2e-8f6c-1a9b5e7d3c2f',
 '1b8d3c28-6a7f-4d9b-8d6b-3c1a6a7d9b8b','9c6e2b19-7d8a-4e6c-7e9c-2b9d7d8e6c2c',
 'Bà Quẹo – Phạm Văn Bạch','Ba Queo – Pham Van Bach',0,9,2,0.93,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

('64b9d8ed-9f8a-464b-9d9f-8a9d8a9b9d64','HCMC-L2-10-11','2f5c8e93-4b7a-4d2e-8f6c-1a9b5e7d3c2f',
 '9c6e2b19-7d8a-4e6c-7e9c-2b9d7d8e6c2c','8d7f1a0a-5e9b-4f7d-6f8d-1a8e5e9f7d1d',
 'Phạm Văn Bạch – Tân Bình','Pham Van Bach – Tan Binh',0,10,2,0.98,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- REVERSE DIRECTION (1)
('53c8e7fe-8f9a-453c-8e8f-9a8e9a8c8e53','HCMC-L2-02-01','2f5c8e93-4b7a-4d2e-8f6c-1a9b5e7d3c2f',
 '8a4c6e91-2f5b-4d3e-9c7a-6b8d2f4e7c9a','5e9f3c27-8d6b-4c44-9a14-dfd6a3e2e8f0',
 'Tao Đàn – Bến Thành','Tao Dan – Ben Thanh',1,1,2,0.69,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

('42d9f6af-7e8b-442d-9f7e-8b7f8b7d9f42','HCMC-L2-03-02','2f5c8e93-4b7a-4d2e-8f6c-1a9b5e7d3c2f',
 '7b5d9f82-3e6c-4a2f-8d9b-5c7e3f6a8d1b','8a4c6e91-2f5b-4d3e-9c7a-6b8d2f4e7c9a',
 'Dân Chủ – Tao Đàn','Dan Chu – Tao Dan',1,2,3,1.25,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

('31e8a5ba-6f7c-431e-8a6f-7c6a7c6e8a31','HCMC-L2-04-03','2f5c8e93-4b7a-4d2e-8f6c-1a9b5e7d3c2f',
 '6c8e4f73-1d7a-4b5c-9e8c-4f6a1d7b9e2c','7b5d9f82-3e6c-4a2f-8d9b-5c7e3f6a8d1b',
 'Hòa Hưng – Dân Chủ','Hoa Hung – Dan Chu',1,3,1,0.62,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

('20f9b4cb-5e6d-420f-9b5e-6d5b6d5f9b20','HCMC-L2-05-04','2f5c8e93-4b7a-4d2e-8f6c-1a9b5e7d3c2f',
 '5d9f7e64-2c8b-4a6d-8f9d-7e5c2c8a6f3d','6c8e4f73-1d7a-4b5c-9e8c-4f6a1d7b9e2c',
 'Lê Thị Riêng – Hòa Hưng','Le Thi Rieng – Hoa Hung',1,4,3,1.21,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

('1fa8c3dc-4d5e-41fa-8c4d-5e4c5e4a8c1f','HCMC-L2-06-05','2f5c8e93-4b7a-4d2e-8f6c-1a9b5e7d3c2f',
 '4e8a6f55-3d9c-4b7e-9a8e-6f4d3d9b7a4e','5d9f7e64-2c8b-4a6d-8f9d-7e5c2c8a6f3d',
 'Phạm Văn Hai – Lê Thị Riêng','Pham Van Hai – Le Thi Rieng',1,5,1,0.57,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

('0eb9d2ed-3c4f-40eb-9d3c-4f3d4f3b9d0e','HCMC-L2-07-06','2f5c8e93-4b7a-4d2e-8f6c-1a9b5e7d3c2f',
 '3f9b5e46-4c8d-4a6f-8b9f-5e3c4c8a6b5f','4e8a6f55-3d9c-4b7e-9a8e-6f4d3d9b7a4e',
 'Bảy Hiền – Phạm Văn Hai','Bay Hien – Pham Van Hai',1,6,2,0.77,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

('9dc8e1fe-2b3a-49dc-8e2b-3a2e3a2c8e9d','HCMC-L2-08-07','2f5c8e93-4b7a-4d2e-8f6c-1a9b5e7d3c2f',
 '2a7c4d37-5b9e-4c8a-9c7a-4d2b5b9c8a7a','3f9b5e46-4c8d-4a6f-8b9f-5e3c4c8a6b5f',
 'Nguyễn Hồng Đào – Bảy Hiền','Nguyen Hong Dao – Bay Hien',1,7,3,1.33,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

('8cd7f0af-1a29-48cd-7f1a-291f291d7f8c','HCMC-L2-09-08','2f5c8e93-4b7a-4d2e-8f6c-1a9b5e7d3c2f',
 '1b8d3c28-6a7f-4d9b-8d6b-3c1a6a7d9b8b','2a7c4d37-5b9e-4c8a-9c7a-4d2b5b9c8a7a',
 'Bà Quẹo – Nguyễn Hồng Đào','Ba Queo – Nguyen Hong Dao',1,8,2,0.78,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

('7bc6efba-0918-47bc-6ef0-180e180c6f7b','HCMC-L2-10-09','2f5c8e93-4b7a-4d2e-8f6c-1a9b5e7d3c2f',
 '9c6e2b19-7d8a-4e6c-7e9c-2b9d7d8e6c2c','1b8d3c28-6a7f-4d9b-8d6b-3c1a6a7d9b8b',
 'Phạm Văn Bạch – Bà Quẹo','Pham Van Bach – Ba Queo',1,9,2,0.93,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

('6ab5decb-8071-46ab-5de8-071d071b5e6a','HCMC-L2-11-10','2f5c8e93-4b7a-4d2e-8f6c-1a9b5e7d3c2f',
 '8d7f1a0a-5e9b-4f7d-6f8d-1a8e5e9f7d1d','9c6e2b19-7d8a-4e6c-7e9c-2b9d7d8e6c2c',
 'Tân Bình – Phạm Văn Bạch','Tan Binh – Pham Van Bach',1,10,2,0.98,
 NULL,NULL,NULL,NOW(),NOW(),NULL);

-- Metro Line 3A: Ben Thanh – Tan Kien, deleted carriage & price
-- Generated on: 2025-06-11 07:09:19 UTC
-- Official source: http://www.maur.hochiminhcity.gov.vn/web/en/the-metro-line-3a
INSERT INTO public."MetroLines"
("Id","RegionId","LineNameVi","LineNameEn","LineCode","TotalKm","TotalStations", "DwellTimeMin",
 "LineType","LineOwner","StationListJSON",
 "ColorHex","IsActive","CreatedBy","LastUpdatedBy","DeletedBy","CreatedAt","LastUpdatedAt","DeletedAt")
VALUES
    (
        '8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',  -- Line 3A ID
        'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',  -- HCMC Region ID (existing)
        'Tuyến 3A: Bến Thành – Tân Kiên',
        'Metro 3A: Ben Thanh – Tan Kien',
        'HCMC-L3A', 19.80, 18,30,
        'Đường sắt đô thị Thành Phố Hồ Chí Minh',
        'Ban Quản lý Đường sắt đô thị Thành phố Hồ Chí Minh (MAUR)',
        '[
          {
            "StationId": "5e9f3c27-8d6b-4c44-9a14-dfd6a3e2e8f0",
            "StationCode": "HCMC-L3A-01"
          },
          {
            "StationId": "b5f8e2a1-4c7d-4b5f-8e2a-1c7d4b5f8e2a",
            "StationCode": "HCMC-L3A-02"
          },
          {
            "StationId": "c6a9f3b2-5d8e-4c6a-9f3b-2d8e5c6a9f3b",
            "StationCode": "HCMC-L3A-03"
          },
          {
            "StationId": "d7b8a4c3-6e9f-4d7b-8a4c-3e9f6d7b8a4c",
            "StationCode": "HCMC-L3A-04"
          },
          {
            "StationId": "e8c9b5d4-7f8a-4e8c-9b5d-4f8a7e8c9b5d",
            "StationCode": "HCMC-L3A-05"
          },
          {
            "StationId": "f9d8c6e5-8a9b-4f9d-8c6e-5a9b8f9d8c6e",
            "StationCode": "HCMC-L3A-06"
          },
          {
            "StationId": "a8e7d9f6-9b8c-4a8e-7d9f-6b8c9a8e7d9f",
            "StationCode": "HCMC-L3A-07"
          },
          {
            "StationId": "b9f6e8a7-8c9d-4b9f-6e8a-7c9d8b9f6e8a",
            "StationCode": "HCMC-L3A-08"
          },
          {
            "StationId": "c8a7f9b8-9d8e-4c8a-7f9b-8d8e9c8a7f9b",
            "StationCode": "HCMC-L3A-09"
          },
          {
            "StationId": "d9b8a8c9-8e9f-4d9b-8a8c-9e9f8d9b8a8c",
            "StationCode": "HCMC-L3A-10"
          },
          {
            "StationId": "e8c9b9d8-9f8a-4e8c-9b9d-8f8a9e8c9b9d",
            "StationCode": "HCMC-L3A-11"
          },
          {
            "StationId": "f9d8c8e9-8a9b-4f9d-8c8e-9a9b8f9d8c8e",
            "StationCode": "HCMC-L3A-12"
          },
          {
            "StationId": "a8e9d9f8-9b8c-4a8e-9d9f-8b8c9a8e9d9f",
            "StationCode": "HCMC-L3A-13"
          },
          {
            "StationId": "b9f8e8a9-8c9d-4b9f-8e8a-9c9d8b9f8e8a",
            "StationCode": "HCMC-L3A-14"
          },
          {
            "StationId": "c8a9f9b8-9d8e-4c8a-9f9b-8d8e9c8a9f9b",
            "StationCode": "HCMC-L3A-15"
          },
          {
            "StationId": "0455c97c-2013-4631-afc6-a6be62315481",
            "StationCode": "HCMC-L3A-16"
          },
          {
            "StationId": "c5873750-eed1-48f9-aa62-75ed00acde87",
            "StationCode": "HCMC-L3A-17"
          },
          {
            "StationId": "d0a40d5f-1e84-459e-bfeb-3f2105db3e0b",
            "StationCode": "HCMC-L3A-18"
          }
        ]',
        '#00A651', TRUE,
        NULL,  -- quangtrandinhminh user ID
        NULL,
        NULL,NOW(),NOW(),NULL
    );

-- Metro Line 3A: Stations Insert (excluding Ben Thanh which already exists) , Edited IsActive for start - end station only to TRUE
INSERT INTO public."Stations"
("Id","StationCode","StationNameVi","StationNameEn","Address","IsUnderground","IsActive","RegionId","Latitude","Longitude",
 "CreatedBy","LastUpdatedBy","DeletedBy","CreatedAt","LastUpdatedAt","DeletedAt")
VALUES
-- L3A-02: Cho Thai Binh
('b5f8e2a1-4c7d-4b5f-8e2a-1c7d4b5f8e2a','HCMC-L3A-02','Chợ Thái Bình','Cho Thai Binh',
 'Khu vực Chợ Thái Bình, P. Phạm Ngũ Lão, Q.1, TP.HCM',TRUE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.766313,106.688331,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL),

-- L3A-03: Cong Hoa
('c6a9f3b2-5d8e-4c6a-9f3b-2d8e5c6a9f3b','HCMC-L3A-03','Cộng Hòa','Cong Hoa',
 'Ngã 6 Cộng Hòa, P.4, Q.3, TP.HCM',TRUE,TRUE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.773347,106.684341,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL),

-- L3A-04: Cong vien Hoa Binh
('d7b8a4c3-6e9f-4d7b-8a4c-3e9f6d7b8a4c','HCMC-L3A-04','Công viên Hòa Bình','Hoa Binh Park',
 'Công viên Hòa Bình, P.3, Q.3, TP.HCM',TRUE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.760773,106.671044,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L3A-05: Dai Hoc Y Duoc
('e8c9b5d4-7f8a-4e8c-9b5d-4f8a7e8c9b5d','HCMC-L3A-05','Đại Học Y Dược','Medical University',
 'Đại học Y Dược TP.HCM, P.11, Q.5, TP.HCM',TRUE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.757131,106.660143,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L3A-06: Thuan Kieu Plaza
('f9d8c6e5-8a9b-4f9d-8c6e-5a9b8f9d8c6e','HCMC-L3A-06','Thuận Kiều Plaza','Thuan Kieu Plaza',
 'Thuận Kiều Plaza, P.12, Q.5, TP.HCM',TRUE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.752358,106.657754,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L3A-07: Ben Xe Cho Lon
('a8e7d9f6-9b8c-4a8e-7d9f-6b8c9a8e7d9f','HCMC-L3A-07','Bến Xe Chợ Lớn','Cho Lon Bus Station',
 'Bến xe Chợ Lớn, P.11, Q.5, TP.HCM',TRUE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.750836,106.651711,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L3A-08: Cay Go
('b9f6e8a7-8c9d-4b9f-6e8a-7c9d8b9f6e8a','HCMC-L3A-08','Cây Gõ','Cay Go',
 'Khu vực Cây Gõ, P.7, Q.6, TP.HCM',TRUE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.749533,106.643321,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L3A-09: Phu Lam
('c8a7f9b8-9d8e-4c8a-7f9b-8d8e9c8a7f9b','HCMC-L3A-09','Phú Lâm','Phu Lam',
 'Khu vực Phú Lâm, P.Phú Lâm, Q.6, TP.HCM',TRUE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.747192,106.634628,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L3A-10: Cong Vien Phu Lam (elevated)
('d9b8a8c9-8e9f-4d9b-8a8c-9e9f8d9b8a8c','HCMC-L3A-10','Công Viên Phú Lâm','Phu Lam Park',
 'Công viên Phú Lâm, P.Phú Lâm, Q.6, TP.HCM',FALSE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.742813,106.626354,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L3A-11: Ben Xe Mien Tay (elevated)
('e8c9b9d8-9f8a-4e8c-9b9d-8f8a9e8c9b9d','HCMC-L3A-11','Bến Xe Miền Tây','Mien Tay Bus Terminal',
 'Bến xe Miền Tây, P.An Lạc, Q.Bình Tân, TP.HCM',FALSE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.740156,106.618012,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L3A-12: Khu Y Te Ky thuat cao (elevated)
('f9d8c8e9-8a9b-4f9d-8c8e-9a9b8f9d8c8e','HCMC-L3A-12','Khu Y Tế Kỹ thuật cao','High-Tech Medical Zone',
 'Khu Y tế Kỹ thuật cao, P.An Lạc A, Q.Bình Tân, TP.HCM',FALSE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.735811,106.606764,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L3A-13: Ho Hoc Lam (elevated)
('a8e9d9f8-9b8c-4a8e-9d9f-8b8c9a8e9d9f','HCMC-L3A-13','Hồ Học Lãm','Ho Hoc Lam',
 'Khu vực Hồ Học Lãm, P.An Lạc A, Q.Bình Tân, TP.HCM',FALSE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.728953,106.601138,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L3A-14: An Lac (elevated)
('b9f8e8a9-8c9d-4b9f-8e8a-9c9d8b9f8e8a','HCMC-L3A-14','An Lạc','An Lac',
 'Khu vực An Lạc, P.An Lạc, Q.Bình Tân, TP.HCM',FALSE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.720165,106.594191,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L3A-15: Hung Nhon (elevated)
('c8a9f9b8-9d8e-4c8a-9f9b-8d8e9c8a9f9b','HCMC-L3A-15','Hưng Nhơn','Hung Nhon',
 'Khu vực Hưng Nhơn, P.Bình Hưng Hòa B, Q.Bình Tân, TP.HCM',FALSE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.713606,106.583344,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L3A-16: Bau Goc (elevated)
('0455c97c-2013-4631-afc6-a6be62315481','HCMC-L3A-16','Bàu Gốc','Bau Goc',
 'Khu vực Bàu Gốc, P.Bình Hưng Hòa B, Q.Bình Tân, TP.HCM',FALSE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.704123,106.567998,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L3A-17: Nguyen Cuu Phu (elevated)
('c5873750-eed1-48f9-aa62-75ed00acde87','HCMC-L3A-17','Nguyễn Cửu Phú','Nguyen Cuu Phu',
 'Khu vực Nguyễn Cửu Phú, P.Tân Tạo A, Q.Bình Tân, TP.HCM',FALSE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.698375,106.556482,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L3A-18: Tan Kien (elevated)
('d0a40d5f-1e84-459e-bfeb-3f2105db3e0b','HCMC-L3A-18','Tân Kiên','Tan Kien',
 'Ga Tân Kiên, X.Tân Kiên, H.Bình Chánh, TP.HCM',FALSE,TRUE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.686521,106.535847,
 NULL,NULL,NULL,NOW(),NOW(),NULL);

-- Metro Line 3A: Routes Insert (Forward and Reverse) - Calculated using Haversine + 5% curve adjustment - old
INSERT INTO public."Routes"
("Id","RouteCode","LineId","FromStationId","ToStationId","RouteNameVi","RouteNameEn","Direction",
 "SeqOrder","TravelTimeMin","LengthKm","CreatedBy","LastUpdatedBy","DeletedBy","CreatedAt","LastUpdatedAt","DeletedAt")
VALUES
-- FORWARD DIRECTION (0)
('a1b2c3d4-5e6f-7a8b-9c0d-e1f2a3b4c5d6','HCMC-L3A-01-02','8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',
 '5e9f3c27-8d6b-4c44-9a14-dfd6a3e2e8f0','b5f8e2a1-4c7d-4b5f-8e2a-1c7d4b5f8e2a',
 'Bến Thành – Chợ Thái Bình','Ben Thanh – Cho Thai Binh',0,1,2,0.84,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

('b2c3d4e5-6f7a-8b9c-0d1e-f2a3b4c5d6e7','HCMC-L3A-02-03','8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',
 'b5f8e2a1-4c7d-4b5f-8e2a-1c7d4b5f8e2a','c6a9f3b2-5d8e-4c6a-9f3b-2d8e5c6a9f3b',
 'Chợ Thái Bình – Cộng Hòa','Cho Thai Binh – Cong Hoa',0,2,2,0.92,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

('c3d4e5f6-7a8b-9c0d-1e2f-a3b4c5d6e7f8','HCMC-L3A-03-04','8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',
 'c6a9f3b2-5d8e-4c6a-9f3b-2d8e5c6a9f3b','d7b8a4c3-6e9f-4d7b-8a4c-3e9f6d7b8a4c',
 'Cộng Hòa – Công viên Hòa Bình','Cong Hoa – Hoa Binh Park',0,3,3,1.54,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

('d4e5f6a7-8b9c-0d1e-2f3a-b4c5d6e7f8a9','HCMC-L3A-04-05','8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',
 'd7b8a4c3-6e9f-4d7b-8a4c-3e9f6d7b8a4c','e8c9b5d4-7f8a-4e8c-9b5d-4f8a7e8c9b5d',
 'Công viên Hòa Bình – Đại Học Y Dược','Hoa Binh Park – Medical University',0,4,2,1.15,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

('e5f6a7b8-9c0d-1e2f-3a4b-c5d6e7f8a9b0','HCMC-L3A-05-06','8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',
 'e8c9b5d4-7f8a-4e8c-9b5d-4f8a7e8c9b5d','f9d8c6e5-8a9b-4f9d-8c6e-5a9b8f9d8c6e',
 'Đại Học Y Dược – Thuận Kiều Plaza','Medical University – Thuan Kieu Plaza',0,5,1,0.25,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

('f6a7b8c9-0d1e-2f3a-4b5c-d6e7f8a9b0c1','HCMC-L3A-06-07','8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',
 'f9d8c6e5-8a9b-4f9d-8c6e-5a9b8f9d8c6e','a8e7d9f6-9b8c-4a8e-7d9f-6b8c9a8e7d9f',
 'Thuận Kiều Plaza – Bến Xe Chợ Lớn','Thuan Kieu Plaza – Cho Lon Bus Station',0,6,2,0.63,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

('a7b8c9d0-1e2f-3a4b-5c6d-e7f8a9b0c1d2','HCMC-L3A-07-08','8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',
 'a8e7d9f6-9b8c-4a8e-7d9f-6b8c9a8e7d9f','b9f6e8a7-8c9d-4b9f-6e8a-7c9d8b9f6e8a',
 'Bến Xe Chợ Lớn – Cây Gõ','Cho Lon Bus Station – Cay Go',0,7,2,0.88,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

('b8c9d0e1-2f3a-4b5c-6d7e-f8a9b0c1d2e3','HCMC-L3A-08-09','8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',
 'b9f6e8a7-8c9d-4b9f-6e8a-7c9d8b9f6e8a','c8a7f9b8-9d8e-4c8a-7f9b-8d8e9c8a7f9b',
 'Cây Gõ – Phú Lâm','Cay Go – Phu Lam',0,8,2,0.91,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

('c9d0e1f2-3a4b-5c6d-7e8f-a9b0c1d2e3f4','HCMC-L3A-09-10','8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',
 'c8a7f9b8-9d8e-4c8a-7f9b-8d8e9c8a7f9b','d9b8a8c9-8e9f-4d9b-8a8c-9e9f8d9b8a8c',
 'Phú Lâm – Công Viên Phú Lâm','Phu Lam – Phu Lam Park',0,9,2,0.87,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

('d0e1f2a3-4b5c-6d7e-8f9a-b0c1d2e3f4a5','HCMC-L3A-10-11','8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',
 'd9b8a8c9-8e9f-4d9b-8a8c-9e9f8d9b8a8c','e8c9b9d8-9f8a-4e8c-9b9d-8f8a9e8c9b9d',
 'Công Viên Phú Lâm – Bến Xe Miền Tây','Phu Lam Park – Mien Tay Bus Terminal',0,10,2,0.88,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

('e1f2a3b4-5c6d-7e8f-9a0b-c1d2e3f4a5b6','HCMC-L3A-11-12','8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',
 'e8c9b9d8-9f8a-4e8c-9b9d-8f8a9e8c9b9d','f9d8c8e9-8a9b-4f9d-8c8e-9a9b8f9d8c8e',
 'Bến Xe Miền Tây – Khu Y Tế Kỹ thuật cao','Mien Tay Bus Terminal – High-Tech Medical Zone',0,11,2,1.19,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

('f2a3b4c5-6d7e-8f9a-0b1c-d2e3f4a5b6c7','HCMC-L3A-12-13','8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',
 'f9d8c8e9-8a9b-4f9d-8c8e-9a9b8f9d8c8e','a8e9d9f8-9b8c-4a8e-9d9f-8b8c9a8e9d9f',
 'Khu Y Tế Kỹ thuật cao – Hồ Học Lãm','High-Tech Medical Zone – Ho Hoc Lam',0,12,2,0.59,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

('a3b4c5d6-7e8f-9a0b-1c2d-e3f4a5b6c7d8','HCMC-L3A-13-14','8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',
 'a8e9d9f8-9b8c-4a8e-9d9f-8b8c9a8e9d9f','b9f8e8a9-8c9d-4b9f-8e8a-9c9d8b9f8e8a',
 'Hồ Học Lãm – An Lạc','Ho Hoc Lam – An Lac',0,13,2,0.87,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

('b4c5d6e7-8f9a-0b1c-2d3e-f4a5b6c7d8e9','HCMC-L3A-14-15','8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',
 'b9f8e8a9-8c9d-4b9f-8e8a-9c9d8b9f8e8a','c8a9f9b8-9d8e-4c8a-9f9b-8d8e9c8a9f9b',
 'An Lạc – Hưng Nhơn','An Lac – Hung Nhon',0,14,2,1.13,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

('c5d6e7f8-9a0b-1c2d-3e4f-a5b6c7d8e9f0','HCMC-L3A-15-16','8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',
 'c8a9f9b8-9d8e-4c8a-9f9b-8d8e9c8a9f9b','0455c97c-2013-4631-afc6-a6be62315481',
 'Hưng Nhơn – Bàu Gốc','Hung Nhon – Bau Goc',0,15,2,1.68,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

('d6e7f8a9-0b1c-2d3e-4f5a-b6c7d8e9f0a1','HCMC-L3A-16-17','8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',
 '0455c97c-2013-4631-afc6-a6be62315481','c5873750-eed1-48f9-aa62-75ed00acde87',
 'Bàu Gốc – Nguyễn Cửu Phú','Bau Goc – Nguyen Cuu Phu',0,16,2,1.25,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

('e7f8a9b0-1c2d-3e4f-5a6b-c7d8e9f0a1b2','HCMC-L3A-17-18','8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',
 'c5873750-eed1-48f9-aa62-75ed00acde87','d0a40d5f-1e84-459e-bfeb-3f2105db3e0b',
 'Nguyễn Cửu Phú – Tân Kiên','Nguyen Cuu Phu – Tan Kien',0,17,3,2.17,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- REVERSE DIRECTION (1) - Copy all forward routes with reversed stations and direction=1
-- L3A-18-17: Tan Kien to Nguyen Cuu Phu
('f8a9b0c1-2d3e-4f5a-6b7c-8d9e0f1a2b3c','HCMC-L3A-18-17','8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',
 'd0a40d5f-1e84-459e-bfeb-3f2105db3e0b','c5873750-eed1-48f9-aa62-75ed00acde87',
 'Tân Kiên – Nguyễn Cửu Phú','Tan Kien – Nguyen Cuu Phu',1,1,3,2.17,
 NULL,NULL,NULL,'2025-06-11 07:18:48+00:00','2025-06-11 07:18:48+00:00',NULL),

-- L3A-17-16: Nguyen Cuu Phu to Bau Goc
('699c6790-26fb-4111-9c57-8d919a45ca10','HCMC-L3A-17-16','8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',
 'c5873750-eed1-48f9-aa62-75ed00acde87','0455c97c-2013-4631-afc6-a6be62315481',
 'Nguyễn Cửu Phú – Bàu Gốc','Nguyen Cuu Phu – Bau Goc',1,2,2,1.25,
 NULL,NULL,NULL,'2025-06-11 07:18:48+00:00','2025-06-11 07:18:48+00:00',NULL),

-- L3A-16-15: Bau Goc to Hung Nhon
('fc60305b-259e-4ebc-94b0-2c0a0bc1ffa7','HCMC-L3A-16-15','8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',
 '0455c97c-2013-4631-afc6-a6be62315481','c8a9f9b8-9d8e-4c8a-9f9b-8d8e9c8a9f9b',
 'Bàu Gốc – Hưng Nhơn','Bau Goc – Hung Nhon',1,3,2,1.68,
 NULL,NULL,NULL,'2025-06-11 07:18:48+00:00','2025-06-11 07:18:48+00:00',NULL),

-- L3A-15-14: Hung Nhon to An Lac
('72ee393d-39d3-49e6-8003-7a9e132c26ee','HCMC-L3A-15-14','8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',
 'c8a9f9b8-9d8e-4c8a-9f9b-8d8e9c8a9f9b','b9f8e8a9-8c9d-4b9f-8e8a-9c9d8b9f8e8a',
 'Hưng Nhơn – An Lạc','Hung Nhon – An Lac',1,4,2,1.13,
 NULL,NULL,NULL,'2025-06-11 07:18:48+00:00','2025-06-11 07:18:48+00:00',NULL),

-- L3A-14-13: An Lac to Ho Hoc Lam
('abc63a10-d970-4f87-89f8-3fd684ec7da4','HCMC-L3A-14-13','8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',
 'b9f8e8a9-8c9d-4b9f-8e8a-9c9d8b9f8e8a','a8e9d9f8-9b8c-4a8e-9d9f-8b8c9a8e9d9f',
 'An Lạc – Hồ Học Lãm','An Lac – Ho Hoc Lam',1,5,2,0.87,
 NULL,NULL,NULL,'2025-06-11 07:18:48+00:00','2025-06-11 07:18:48+00:00',NULL),

-- L3A-13-12: Ho Hoc Lam to Khu Y Te Ky thuat cao
('cbabf3eb-c85d-4509-84bf-18214f1016ea','HCMC-L3A-13-12','8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',
 'a8e9d9f8-9b8c-4a8e-9d9f-8b8c9a8e9d9f','f9d8c8e9-8a9b-4f9d-8c8e-9a9b8f9d8c8e',
 'Hồ Học Lãm – Khu Y Tế Kỹ thuật cao','Ho Hoc Lam – High-Tech Medical Zone',1,6,2,0.59,
 NULL,NULL,NULL,'2025-06-11 07:18:48+00:00','2025-06-11 07:18:48+00:00',NULL),

-- L3A-12-11: Khu Y Te Ky thuat cao to Ben Xe Mien Tay
('39ecc0a9-9591-46c2-a3e8-0f82ea53ae23','HCMC-L3A-12-11','8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',
 'f9d8c8e9-8a9b-4f9d-8c8e-9a9b8f9d8c8e','e8c9b9d8-9f8a-4e8c-9b9d-8f8a9e8c9b9d',
 'Khu Y Tế Kỹ thuật cao – Bến Xe Miền Tây','High-Tech Medical Zone – Mien Tay Bus Terminal',1,7,2,1.19,
 NULL,NULL,NULL,'2025-06-11 07:18:48+00:00','2025-06-11 07:18:48+00:00',NULL),

-- L3A-11-10: Ben Xe Mien Tay to Cong Vien Phu Lam
('f13d98fb-9f83-4013-b5cc-632e50b909c6','HCMC-L3A-11-10','8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',
 'e8c9b9d8-9f8a-4e8c-9b9d-8f8a9e8c9b9d','d9b8a8c9-8e9f-4d9b-8a8c-9e9f8d9b8a8c',
 'Bến Xe Miền Tây – Công Viên Phú Lâm','Mien Tay Bus Terminal – Phu Lam Park',1,8,2,0.88,
 NULL,NULL,NULL,'2025-06-11 07:18:48+00:00','2025-06-11 07:18:48+00:00',NULL),

-- L3A-10-09: Cong Vien Phu Lam to Phu Lam
('29560d4d-fb9e-44c4-97eb-0b61718ba124','HCMC-L3A-10-09','8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',
 'd9b8a8c9-8e9f-4d9b-8a8c-9e9f8d9b8a8c','c8a7f9b8-9d8e-4c8a-7f9b-8d8e9c8a7f9b',
 'Công Viên Phú Lâm – Phú Lâm','Phu Lam Park – Phu Lam',1,9,2,0.87,
 NULL,NULL,NULL,'2025-06-11 07:18:48+00:00','2025-06-11 07:18:48+00:00',NULL),

-- L3A-09-08: Phu Lam to Cay Go
('83044e81-53be-4874-8c6d-cf995608ecb2','HCMC-L3A-09-08','8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',
 'c8a7f9b8-9d8e-4c8a-7f9b-8d8e9c8a7f9b','b9f6e8a7-8c9d-4b9f-6e8a-7c9d8b9f6e8a',
 'Phú Lâm – Cây Gõ','Phu Lam – Cay Go',1,10,2,0.91,
 NULL,NULL,NULL,'2025-06-11 07:18:48+00:00','2025-06-11 07:18:48+00:00',NULL),

-- L3A-08-07: Cay Go to Ben Xe Cho Lon
('96b5ffe2-777c-4065-b7b8-724b5020d3c8','HCMC-L3A-08-07','8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',
 'b9f6e8a7-8c9d-4b9f-6e8a-7c9d8b9f6e8a','a8e7d9f6-9b8c-4a8e-7d9f-6b8c9a8e7d9f',
 'Cây Gõ – Bến Xe Chợ Lớn','Cay Go – Cho Lon Bus Station',1,11,2,0.88,
 NULL,NULL,NULL,'2025-06-11 07:18:48+00:00','2025-06-11 07:18:48+00:00',NULL),

-- L3A-07-06: Ben Xe Cho Lon to Thuan Kieu Plaza
('3c763175-bd0b-49d9-ac8b-c70a6557c14f','HCMC-L3A-07-06','8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',
 'a8e7d9f6-9b8c-4a8e-7d9f-6b8c9a8e7d9f','f9d8c6e5-8a9b-4f9d-8c6e-5a9b8f9d8c6e',
 'Bến Xe Chợ Lớn – Thuận Kiều Plaza','Cho Lon Bus Station – Thuan Kieu Plaza',1,12,2,0.63,
 NULL,NULL,NULL,'2025-06-11 07:18:48+00:00','2025-06-11 07:18:48+00:00',NULL),

-- L3A-06-05: Thuan Kieu Plaza to Dai Hoc Y Duoc
('e7637aca-01fd-4ddc-a5d4-cd6f96fc76cd','HCMC-L3A-06-05','8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',
 'f9d8c6e5-8a9b-4f9d-8c6e-5a9b8f9d8c6e','e8c9b5d4-7f8a-4e8c-9b5d-4f8a7e8c9b5d',
 'Thuận Kiều Plaza – Đại Học Y Dược','Thuan Kieu Plaza – Medical University',1,13,1,0.25,
 NULL,NULL,NULL,'2025-06-11 07:18:48+00:00','2025-06-11 07:18:48+00:00',NULL),

-- L3A-05-04: Dai Hoc Y Duoc to Cong vien Hoa Binh
('aabdb7d4-fb0c-4ab2-850b-912b5a7d3ada','HCMC-L3A-05-04','8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',
 'e8c9b5d4-7f8a-4e8c-9b5d-4f8a7e8c9b5d','d7b8a4c3-6e9f-4d7b-8a4c-3e9f6d7b8a4c',
 'Đại Học Y Dược – Công viên Hòa Bình','Medical University – Hoa Binh Park',1,14,2,1.15,
 NULL,NULL,NULL,'2025-06-11 07:18:48+00:00','2025-06-11 07:18:48+00:00',NULL),

-- L3A-04-03: Cong vien Hoa Binh to Cong Hoa
('b59e875f-afa9-4330-a334-f57359f2dc7f','HCMC-L3A-04-03','8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',
 'd7b8a4c3-6e9f-4d7b-8a4c-3e9f6d7b8a4c','c6a9f3b2-5d8e-4c6a-9f3b-2d8e5c6a9f3b',
 'Công viên Hòa Bình – Cộng Hòa','Hoa Binh Park – Cong Hoa',1,15,3,1.54,
 NULL,NULL,NULL,'2025-06-11 07:18:48+00:00','2025-06-11 07:18:48+00:00',NULL),

-- L3A-03-02: Cong Hoa to Cho Thai Binh
('8816083c-7df5-4a90-80f1-7bd7f94a6944','HCMC-L3A-03-02','8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',
 'c6a9f3b2-5d8e-4c6a-9f3b-2d8e5c6a9f3b','b5f8e2a1-4c7d-4b5f-8e2a-1c7d4b5f8e2a',
 'Cộng Hòa – Chợ Thái Bình','Cong Hoa – Cho Thai Binh',1,16,2,0.92,
 NULL,NULL,NULL,'2025-06-11 07:18:48+00:00','2025-06-11 07:18:48+00:00',NULL),

-- L3A-02-01: Cho Thai Binh to Ben Thanh
('12af8dc4-ebe3-4772-a118-78aab89783a8','HCMC-L3A-02-01','8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',
 'b5f8e2a1-4c7d-4b5f-8e2a-1c7d4b5f8e2a','5e9f3c27-8d6b-4c44-9a14-dfd6a3e2e8f0',
 'Chợ Thái Bình – Bến Thành','Cho Thai Binh – Ben Thanh',1,17,2,0.84,
 NULL,NULL,NULL,'2025-06-11 07:18:48+00:00','2025-06-11 07:18:48+00:00',NULL);

-- Metro Line 3B: Cong Hoa – Hiep Binh, deleted carriage & price
-- Generated on: 2025-06-11 08:40:31 UTC
-- Official source: http://www.maur.hochiminhcity.gov.vn/web/en/the-metro-line-3b
INSERT INTO public."MetroLines"
("Id","RegionId","LineNameVi","LineNameEn","LineCode","TotalKm","TotalStations", "DwellTimeMin",
 "LineType","LineOwner","StationListJSON",
 "ColorHex","IsActive","CreatedBy","LastUpdatedBy","DeletedBy","CreatedAt","LastUpdatedAt","DeletedAt")
VALUES
    (
        'a1f4e7c8-6b2d-4a1f-e7c8-6b2da1f4e7c8',  -- Line 3B ID
        'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',  -- HCMC Region ID (existing)
        'Tuyến 3B: Cộng Hòa – Hiệp Bình Phước',
        'Metro 3B: Cong Hoa – Hiep Binh Phuoc',
        'HCMC-L3B', 12.2, 10,30,
        'Đường sắt đô thị Thành Phố Hồ Chí Minh',
        'Ban Quản lý Đường sắt đô thị Thành phố Hồ Chí Minh (MAUR)',
        '[
          {
            "StationId": "c6a9f3b2-5d8e-4c6a-9f3b-2d8e5c6a9f3b",
            "StationCode": "HCMC-L3B-01"
          },
          {
            "StationId": "b2e5f8d9-7c3a-4b2e-f8d9-7c3ab2e5f8d9",
            "StationCode": "HCMC-L3B-02"
          },
          {
            "StationId": "8a4c6e91-2f5b-4d3e-9c7a-6b8d2f4e7c9a",
            "StationCode": "HCMC-L3B-03"
          },
          {
            "StationId": "c3f6a9e8-8d4b-4c3f-a9e8-8d4bc3f6a9e8",
            "StationCode": "HCMC-L3B-04"
          },
          {
            "StationId": "d4a7b8f7-9e5c-4d4a-b8f7-9e5cd4a7b8f7",
            "StationCode": "HCMC-L3B-05"
          },
          {
            "StationId": "e5b8c9a6-8f6d-4e5b-c9a6-8f6de5b8c9a6",
            "StationCode": "HCMC-L3B-06"
          },
          {
            "StationId": "f6c9d8b5-9a7e-4f6c-d8b5-9a7ef6c9d8b5",
            "StationCode": "HCMC-L3B-07"
          },
          {
            "StationId": "a7d8e9c4-8b6f-4a7d-e9c4-8b6fa7d8e9c4",
            "StationCode": "HCMC-L3B-08"
          },
          {
            "StationId": "b8e9f7d3-9c8a-4b8e-f7d3-9c8ab8e9f7d3",
            "StationCode": "HCMC-L3B-09"
          },
          {
            "StationId": "c9f8a6e2-8d9b-4c9f-a6e2-8d9bc9f8a6e2",
            "StationCode": "HCMC-L3B-10"
          }
        ]',
        '#8E44AD',TRUE,
        NULL,NULL,NULL,NOW(),NOW(),NULL
    );

-- Metro Line 3B:  Stations Insert (excluding existing Cong Hoa and Tao Dan)
INSERT INTO public."Stations"
("Id","StationCode","StationNameVi","StationNameEn","Address","IsUnderground","IsActive","RegionId","Latitude","Longitude",
 "CreatedBy","LastUpdatedBy","DeletedBy","CreatedAt","LastUpdatedAt","DeletedAt")
VALUES
-- L3B-02: Tu Du
('b2e5f8d9-7c3a-4b2e-f8d9-7c3ab2e5f8d9','HCMC-L3B-02','Từ Dũ','Tu Du',
 'Khu vực Từ Dũ, P.Bến Nghé, Q.1, TP.HCM',TRUE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.771960,106.688849,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L3B-04: Ho Con Rua
('c3f6a9e8-8d4b-4c3f-a9e8-8d4bc3f6a9e8','HCMC-L3B-04','Hồ Con Rùa','Ho Con Rua',
 'Khu vực Hồ Con Rùa, P.Bến Nghé, Q.1, TP.HCM',TRUE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.781617,106.699042,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L3B-05: Hoa Lu
('d4a7b8f7-9e5c-4d4a-b8f7-9e5cd4a7b8f7','HCMC-L3B-05','Hoa Lư','Hoa Lu',
 'Khu vực Hoa Lư, P.Đa Kao, Q.1, TP.HCM',TRUE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.787728,106.703273,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L3B-06: Thi Nghe
('e5b8c9a6-8f6d-4e5b-c9a6-8f6de5b8c9a6','HCMC-L3B-06','Thị Nghè','Thi Nghe',
 'Khu vực Thị Nghè, P.6, Q.3, TP.HCM',TRUE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.791535,106.708819,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L3B-07: Hang Xanh
('f6c9d8b5-9a7e-4f6c-d8b5-9a7ef6c9d8b5','HCMC-L3B-07','Hàng Xanh','Hang Xanh',
 'Ngã tư Hàng Xanh, P.25, Q.Bình Thạnh, TP.HCM',TRUE,FALSE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.797241,106.712165,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L3B-08: Ben Xe Mien Dong
('a7d8e9c4-8b6f-4a7d-e9c4-8b6fa7d8e9c4','HCMC-L3B-08','Bến Xe Miền Đông','Mien Dong Bus Terminal',
 'Bến xe Miền Đông, P.25, Q.Bình Thạnh, TP.HCM',TRUE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.810565,106.712134,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L3B-09: Binh Trieu (elevated)
('b8e9f7d3-9c8a-4b8e-f7d3-9c8ab8e9f7d3','HCMC-L3B-09','Bình Triệu','Binh Trieu',
 '5, Kha Vạn Cân, P. Hiệp Bình Chánh, Q. Thủ Đức, TP.HCM',FALSE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.823188,106.716817,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L3B-10: Hiep Binh Phuoc (elevated)
('c9f8a6e2-8d9b-4c9f-a6e2-8d9bc9f8a6e2','HCMC-L3B-10','Hiệp Bình Phước','Hiep Binh Phuoc',
 'Khu vực Hiệp Bình Phước, P.Hiệp Bình Phước, TP.Thủ Đức, TP.HCM',FALSE,TRUE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.841231,106.714488,
 NULL,NULL,NULL,NOW(),NOW(),NULL);

-- Metro Line 3B: Routes Insert (calculated using Haversine + 5% curve adjustment) - old
INSERT INTO public."Routes"
("Id","RouteCode","LineId","FromStationId","ToStationId","RouteNameVi","RouteNameEn","Direction",
 "SeqOrder","TravelTimeMin","LengthKm","CreatedBy","LastUpdatedBy","DeletedBy","CreatedAt","LastUpdatedAt","DeletedAt")
VALUES
-- FORWARD DIRECTION (0)
-- L3B-01-02: Cong Hoa to Tu Du
('a1c4f7e8-2b5d-4a1c-f7e8-2b5da1c4f7e8','HCMC-L3B-01-02','a1f4e7c8-6b2d-4a1f-e7c8-6b2da1f4e7c8',
 'c6a9f3b2-5d8e-4c6a-9f3b-2d8e5c6a9f3b','b2e5f8d9-7c3a-4b2e-f8d9-7c3ab2e5f8d9',
 'Cộng Hòa – Từ Dũ','Cong Hoa – Tu Du',0,1,1,0.47,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL),

-- L3B-02-03: Tu Du to Tao Dan
('b2d5a8f9-3c6e-4b2d-a8f9-3c6eb2d5a8f9','HCMC-L3B-02-03','a1f4e7c8-6b2d-4a1f-e7c8-6b2da1f4e7c8',
 'b2e5f8d9-7c3a-4b2e-f8d9-7c3ab2e5f8d9','8a4c6e91-2f5b-4d3e-9c7a-6b8d2f4e7c9a',
 'Từ Dũ – Tao Đàn','Tu Du – Tao Dan',0,2,1,0.24,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L3B-03-04: Tao Dan to Ho Con Rua
('c3e6b9a8-4d7f-4c3e-b9a8-4d7fc3e6b9a8','HCMC-L3B-03-04','a1f4e7c8-6b2d-4a1f-e7c8-6b2da1f4e7c8',
 '8a4c6e91-2f5b-4d3e-9c7a-6b8d2f4e7c9a','c3f6a9e8-8d4b-4c3f-a9e8-8d4bc3f6a9e8',
 'Tao Đàn – Hồ Con Rùa','Tao Dan – Ho Con Rua',0,3,2,1.03,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L3B-04-05: Ho Con Rua to Hoa Lu
('d4f7c8b9-5e8a-4d4f-c8b9-5e8ad4f7c8b9','HCMC-L3B-04-05','a1f4e7c8-6b2d-4a1f-e7c8-6b2da1f4e7c8',
 'c3f6a9e8-8d4b-4c3f-a9e8-8d4bc3f6a9e8','d4a7b8f7-9e5c-4d4a-b8f7-9e5cd4a7b8f7',
 'Hồ Con Rùa – Hoa Lư','Ho Con Rua – Hoa Lu',0,4,2,0.75,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L3B-05-06: Hoa Lu to Thi Nghe
('e5a8d9c8-6f9b-4e5a-d9c8-6f9be5a8d9c8','HCMC-L3B-05-06','a1f4e7c8-6b2d-4a1f-e7c8-6b2da1f4e7c8',
 'd4a7b8f7-9e5c-4d4a-b8f7-9e5cd4a7b8f7','e5b8c9a6-8f6d-4e5b-c9a6-8f6de5b8c9a6',
 'Hoa Lư – Thị Nghè','Hoa Lu – Thi Nghe',0,5,2,0.58,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L3B-06-07: Thi Nghe to Hang Xanh
('f6b9e8d7-7a8c-4f6b-e8d7-7a8cf6b9e8d7','HCMC-L3B-06-07','a1f4e7c8-6b2d-4a1f-e7c8-6b2da1f4e7c8',
 'e5b8c9a6-8f6d-4e5b-c9a6-8f6de5b8c9a6','f6c9d8b5-9a7e-4f6c-d8b5-9a7ef6c9d8b5',
 'Thị Nghè – Hàng Xanh','Thi Nghe – Hang Xanh',0,6,2,0.65,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L3B-07-08: Hang Xanh to Ben Xe Mien Dong
('a7c8f9e6-8b9d-4a7c-f9e6-8b9da7c8f9e6','HCMC-L3B-07-08','a1f4e7c8-6b2d-4a1f-e7c8-6b2da1f4e7c8',
 'f6c9d8b5-9a7e-4f6c-d8b5-9a7ef6c9d8b5','a7d8e9c4-8b6f-4a7d-e9c4-8b6fa7d8e9c4',
 'Hàng Xanh – Bến Xe Miền Đông','Hang Xanh – Mien Dong Bus Terminal',0,7,3,1.48,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L3B-08-09: Ben Xe Mien Dong to Binh Trieu
('b8d9a7f5-9c8e-4b8d-a7f5-9c8eb8d9a7f5','HCMC-L3B-08-09','a1f4e7c8-6b2d-4a1f-e7c8-6b2da1f4e7c8',
 'a7d8e9c4-8b6f-4a7d-e9c4-8b6fa7d8e9c4','b8e9f7d3-9c8a-4b8e-f7d3-9c8ab8e9f7d3',
 'Bến Xe Miền Đông – Bình Triệu','Mien Dong Bus Terminal – Binh Trieu',0,8,2,1.34,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L3B-09-10: Binh Trieu to Hiep Binh Phuoc
('c9e8b6a4-8d9f-4c9e-b6a4-8d9fc9e8b6a4','HCMC-L3B-09-10','a1f4e7c8-6b2d-4a1f-e7c8-6b2da1f4e7c8',
 'b8e9f7d3-9c8a-4b8e-f7d3-9c8ab8e9f7d3','c9f8a6e2-8d9b-4c9f-a6e2-8d9bc9f8a6e2',
 'Bình Triệu – Hiệp Bình Phước','Binh Trieu – Hiep Binh Phuoc',0,9,3,2.01,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- REVERSE DIRECTION (1)
-- L3B-10-09: Hiep Binh Phuoc to Binh Trieu
('f8b9e3d1-9a8f-4f8b-e3d1-9a8ff8b9e3d1','HCMC-L3B-10-09','a1f4e7c8-6b2d-4a1f-e7c8-6b2da1f4e7c8',
 'c9f8a6e2-8d9b-4c9f-a6e2-8d9bc9f8a6e2','b8e9f7d3-9c8a-4b8e-f7d3-9c8ab8e9f7d3',
 'Hiệp Bình Phước – Bình Triệu','Hiep Binh Phuoc – Binh Trieu',1,2,3,2.01,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L3B-09-08: Binh Trieu to Ben Xe Mien Dong
('a9c8f2e8-8b9a-4a9c-f2e8-8b9aa9c8f2e8','HCMC-L3B-09-08','a1f4e7c8-6b2d-4a1f-e7c8-6b2da1f4e7c8',
 'b8e9f7d3-9c8a-4b8e-f7d3-9c8ab8e9f7d3','a7d8e9c4-8b6f-4a7d-e9c4-8b6fa7d8e9c4',
 'Bình Triệu – Bến Xe Miền Đông','Binh Trieu – Mien Dong Bus Terminal',1,3,2,1.34,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L3B-08-07: Ben Xe Mien Dong to Hang Xanh
('b8d9a1f7-9c8b-4b8d-a1f7-9c8bb8d9a1f7','HCMC-L3B-08-07','a1f4e7c8-6b2d-4a1f-e7c8-6b2da1f4e7c8',
 'a7d8e9c4-8b6f-4a7d-e9c4-8b6fa7d8e9c4','f6c9d8b5-9a7e-4f6c-d8b5-9a7ef6c9d8b5',
 'Bến Xe Miền Đông – Hàng Xanh','Mien Dong Bus Terminal – Hang Xanh',1,4,3,1.48,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L3B-07-06: Hang Xanh to Thi Nghe
('c7e8b9f6-8d7c-4c7e-b9f6-8d7cc7e8b9f6','HCMC-L3B-07-06','a1f4e7c8-6b2d-4a1f-e7c8-6b2da1f4e7c8',
 'f6c9d8b5-9a7e-4f6c-d8b5-9a7ef6c9d8b5','e5b8c9a6-8f6d-4e5b-c9a6-8f6de5b8c9a6',
 'Hàng Xanh – Thị Nghè','Hang Xanh – Thi Nghe',1,5,2,0.65,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L3B-06-05: Thi Nghe to Hoa Lu
('d6f9c8a5-9e6d-4d6f-c8a5-9e6dd6f9c8a5','HCMC-L3B-06-05','a1f4e7c8-6b2d-4a1f-e7c8-6b2da1f4e7c8',
 'e5b8c9a6-8f6d-4e5b-c9a6-8f6de5b8c9a6','d4a7b8f7-9e5c-4d4a-b8f7-9e5cd4a7b8f7',
 'Thị Nghè – Hoa Lư','Thi Nghe – Hoa Lu',1,6,2,0.58,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L3B-05-04: Hoa Lu to Ho Con Rua
('e5a8d7b4-8f5e-4e5a-d7b4-8f5ee5a8d7b4','HCMC-L3B-05-04','a1f4e7c8-6b2d-4a1f-e7c8-6b2da1f4e7c8',
 'd4a7b8f7-9e5c-4d4a-b8f7-9e5cd4a7b8f7','c3f6a9e8-8d4b-4c3f-a9e8-8d4bc3f6a9e8',
 'Hoa Lư – Hồ Con Rùa','Hoa Lu – Ho Con Rua',1,7,2,0.75,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L3B-04-03: Ho Con Rua to Tao Dan
('f4b9e6c3-9a4f-4f4b-e6c3-9a4ff4b9e6c3','HCMC-L3B-04-03','a1f4e7c8-6b2d-4a1f-e7c8-6b2da1f4e7c8',
 'c3f6a9e8-8d4b-4c3f-a9e8-8d4bc3f6a9e8','8a4c6e91-2f5b-4d3e-9c7a-6b8d2f4e7c9a',
 'Hồ Con Rùa – Tao Đàn','Ho Con Rua – Tao Dan',1,8,2,1.03,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L3B-03-02: Tao Dan to Tu Du
('a3c8f5d2-8b3a-4a3c-f5d2-8b3aa3c8f5d2','HCMC-L3B-03-02','a1f4e7c8-6b2d-4a1f-e7c8-6b2da1f4e7c8',
 '8a4c6e91-2f5b-4d3e-9c7a-6b8d2f4e7c9a','b2e5f8d9-7c3a-4b2e-f8d9-7c3ab2e5f8d9',
 'Tao Đàn – Từ Dũ','Tao Dan – Tu Du',1,9,1,0.24,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L3B-02-01: Tu Du to Cong Hoa
('b2d9a4e1-9c2b-4b2d-a4e1-9c2bb2d9a4e1','HCMC-L3B-02-01','a1f4e7c8-6b2d-4a1f-e7c8-6b2da1f4e7c8',
 'b2e5f8d9-7c3a-4b2e-f8d9-7c3ab2e5f8d9','c6a9f3b2-5d8e-4c6a-9f3b-2d8e5c6a9f3b',
 'Từ Dũ – Cộng Hòa','Tu Du – Cong Hoa',1,10,1,0.47,
 NULL,NULL,NULL,NOW(),NOW(),NULL);

-- Metro Line 4: Thanh Xuan – Ben Tau Hiep Phuoc
-- Generated on: 2025-06-11 08:58:39 UTC
-- Official source: http://www.maur.hochiminhcity.gov.vn/web/en/metro-line-4
INSERT INTO public."MetroLines"
("Id","RegionId","LineNameVi","LineNameEn","LineCode","TotalKm","TotalStations", "DwellTimeMin",
 "LineType","LineOwner","StationListJSON",
 "ColorHex","IsActive","CreatedBy","LastUpdatedBy","DeletedBy","CreatedAt","LastUpdatedAt","DeletedAt")
VALUES
    (
        'f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',  -- Line 4 ID
        'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',  -- HCMC Region ID (existing)
        'Tuyến 4: Thạnh Xuân – Bến Tàu Hiệp Phước',
        'Metro 4: Thanh Xuan – Ben Tau Hiep Phuoc',
        'HCMC-L4', 24.58, 32,30,
        'Đường sắt đô thị Thành Phố Hồ Chí Minh',
        'Ban Quản lý Đường sắt đô thị Thành phố Hồ Chí Minh (MAUR)',
        '[
          {
            "StationId": "a3e9f4c7-5b8d-4a3e-f4c7-5b8da3e9f4c7",
            "StationCode": "HCMC-L4-01"
          },
          {
            "StationId": "b4f8a5d6-6c9e-4b4f-a5d6-6c9eb4f8a5d6",
            "StationCode": "HCMC-L4-02"
          },
          {
            "StationId": "c5a9b6e7-7d8f-4c5a-b6e7-7d8fc5a9b6e7",
            "StationCode": "HCMC-L4-03"
          },
          {
            "StationId": "d6b8c7f8-8e9a-4d6b-c7f8-8e9ad6b8c7f8",
            "StationCode": "HCMC-L4-04"
          },
          {
            "StationId": "e7c9d8a9-9f8b-4e7c-d8a9-9f8be7c9d8a9",
            "StationCode": "HCMC-L4-05"
          },
          {
            "StationId": "f8d8e9b8-8a9c-4f8d-e9b8-8a9cf8d8e9b8",
            "StationCode": "HCMC-L4-06"
          },
          {
            "StationId": "a9e7f8c7-9b8d-4a9e-f8c7-9b8da9e7f8c7",
            "StationCode": "HCMC-L4-07"
          },
          {
            "StationId": "b8f6a9d6-8c9e-4b8f-a9d6-8c9eb8f6a9d6",
            "StationCode": "HCMC-L4-08"
          },
          {
            "StationId": "c9a7b8e5-9d8f-4c9a-b8e5-9d8fc9a7b8e5",
            "StationCode": "HCMC-L4-09"
          },
          {
            "StationId": "d8b6c9f4-8e9a-4d8b-c9f4-8e9ad8b6c9f4",
            "StationCode": "HCMC-L4-10"
          },
          {
            "StationId": "e7c5d8a3-9f8b-4e7c-d8a3-9f8be7c5d8a3",
            "StationCode": "HCMC-L4-11"
          },
          {
            "StationId": "f6d4e9b2-8a9c-4f6d-e9b2-8a9cf6d4e9b2",
            "StationCode": "HCMC-L4-12"
          },
          {
            "StationId": "c3f6a9e8-8d4b-4c3f-a9e8-8d4bc3f6a9e8",
            "StationCode": "HCMC-L4-13"
          },
          {
            "StationId": "5e9f3c27-8d6b-4c44-9a14-dfd6a3e2e8f0",
            "StationCode": "HCMC-L4-14"
          },
          {
            "StationId": "a5e3f8c1-9b6d-4a5e-f8c1-9b6da5e3f8c1",
            "StationCode": "HCMC-L4-15"
          },
          {
            "StationId": "b6f4a9d2-8c7e-4b6f-a9d2-8c7eb6f4a9d2",
            "StationCode": "HCMC-L4-16"
          },
          {
            "StationId": "c7a5b8e3-9d8f-4c7a-b8e3-9d8fc7a5b8e3",
            "StationCode": "HCMC-L4-17"
          },
          {
            "StationId": "572e6743-c3de-4374-a70f-52ae566c64b2",
            "StationCode": "HCMC-L4-18"
          },
          {
            "StationId": "e9c7d8a5-9f8b-4e9c-d8a5-9f8be9c7d8a5",
            "StationCode": "HCMC-L4-19"
          },
          {
            "StationId": "f8d6e9b4-8a9c-4f8d-e9b4-8a9cf8d6e9b4",
            "StationCode": "HCMC-L4-20"
          },
          {
            "StationId": "a9e5f8c3-9b7d-4a9e-f8c3-9b7da9e5f8c3",
            "StationCode": "HCMC-L4-21"
          },
          {
            "StationId": "b8f4a9d6-8c9e-4b8f-a9d6-8c9eb8f4a9d6",
            "StationCode": "HCMC-L4-22"
          },
          {
            "StationId": "c7a3b8e5-9d8f-4c7a-b8e5-9d8fc7a3b8e5",
            "StationCode": "HCMC-L4-23"
          },
          {
            "StationId": "d6b2c9f4-8e9a-4d6b-c9f4-8e9ad6b2c9f4",
            "StationCode": "HCMC-L4-24"
          },
          {
            "StationId": "e5c1d8a3-9f8b-4e5c-d8a3-9f8be5c1d8a3",
            "StationCode": "HCMC-L4-25"
          },
          {
            "StationId": "f4d8e9b2-8a9c-4f4d-e9b2-8a9cf4d8e9b2",
            "StationCode": "HCMC-L4-26"
          },
          {
            "StationId": "a3e7f8c1-9b6d-4a3e-f8c1-9b6da3e7f8c1",
            "StationCode": "HCMC-L4-27"
          },
          {
            "StationId": "b2f6a9d8-8c7e-4b2f-a9d8-8c7eb2f6a9d8",
            "StationCode": "HCMC-L4-28"
          },
          {
            "StationId": "c1a5b8e7-9d8f-4c1a-b8e7-9d8fc1a5b8e7",
            "StationCode": "HCMC-L4-29"
          },
          {
            "StationId": "d8b4c9f6-8e9a-4d8b-c9f6-8e9ad8b4c9f6",
            "StationCode": "HCMC-L4-30"
          },
          {
            "StationId": "e7c3d8a5-9f8b-4e7c-d8a5-9f8be7c3d8a5",
            "StationCode": "HCMC-L4-31"
          },
          {
            "StationId": "f6d2e9b4-8a9c-4f6d-e9b4-8a9cf6d2e9b4",
            "StationCode": "HCMC-L4-32"
          }
        ]',
        '#FF5722',  -- Deep Orange color for Line 4
        TRUE,
        NULL,NULL,NULL,NOW(),NOW(),NULL
    );

-- 2. Stations Insert (excluding existing Ho Con Rua and Ben Thanh)
INSERT INTO public."Stations"
("Id","StationCode","StationNameVi","StationNameEn","Address","IsUnderground","IsActive","RegionId","Latitude","Longitude",
 "CreatedBy","LastUpdatedBy","DeletedBy","CreatedAt","LastUpdatedAt","DeletedAt")
VALUES
-- L4-01: Thanh Xuan (elevated)
('a3e9f4c7-5b8d-4a3e-f4c7-5b8da3e9f4c7','HCMC-L4-01','Thạnh Xuân','Thanh Xuan',
 'Khu vực Thạnh Xuân, P.Thạnh Xuân, Q.12, TP.HCM',FALSE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.8673,106.6635,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-02: Giao Khau (elevated)
('b4f8a5d6-6c9e-4b4f-a5d6-6c9eb4f8a5d6','HCMC-L4-02','Giao Khẩu','Giao Khau',
 'Khu vực Giao Khẩu, P.Thạnh Xuân, Q.12, TP.HCM',FALSE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.8612,106.6701,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-03: Nga 4 Ga (elevated)
('c5a9b6e7-7d8f-4c5a-b6e7-7d8fc5a9b6e7','HCMC-L4-03','Ngã 4 Ga','Nga 4 Ga',
 'Ngã 4 Ga, P.Thạnh Xuân, Q.12, TP.HCM',FALSE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.8549,106.6728,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-04: An Loc (elevated)
('d6b8c7f8-8e9a-4d6b-c7f8-8e9ad6b8c7f8','HCMC-L4-04','An Lộc','An Loc',
 'Khu vực An Lộc, P.Thạnh Lộc, Q.12, TP.HCM',FALSE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.8491,106.6759,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-05: An Nhon (elevated)
('e7c9d8a9-9f8b-4e7c-d8a9-9f8be7c9d8a9','HCMC-L4-05','An Nhơn','An Nhon',
 'Khu vực An Nhơn, P.An Phú Đông, Q.12, TP.HCM',FALSE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.8385,106.6784,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-06: Lam Son (elevated)
('f8d8e9b8-8a9c-4f8d-e9b8-8a9cf8d8e9b8','HCMC-L4-06','Lam Sơn','Lam Son',
 'Khu vực Lam Sơn, P.6, Q.Gò Vấp, TP.HCM',FALSE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.8324,106.6811,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-07: Nga 6 Go Vap (elevated)
('a9e7f8c7-9b8d-4a9e-f8c7-9b8da9e7f8c7','HCMC-L4-07','Ngã 6 Gò Vấp','Nga 6 Go Vap',
 'Ngã 6 Gò Vấp, P.6, Q.Gò Vấp, TP.HCM',FALSE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.8225,106.6783,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-08: Benh Vien 175 (elevated)
('b8f6a9d6-8c9e-4b8f-a9d6-8c9eb8f6a9d6','HCMC-L4-08','Bệnh Viện 175','Hospital 175',
 'Bệnh viện 175, P.8, Q.Gò Vấp, TP.HCM',FALSE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.8159,106.6829,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-09: Cong Vien Gia Dinh (elevated)
('c9a7b8e5-9d8f-4c9a-b8e5-9d8fc9a7b8e5','HCMC-L4-09','Công Viên Gia Định','Gia Dinh Park',
 'Công viên Gia Định, P.1, Q.Gò Vấp, TP.HCM',FALSE,TRUE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.8106,106.6853,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-10: Nga 4 Phu Nhuan (elevated)
('d8b6c9f4-8e9a-4d8b-c9f4-8e9ad8b6c9f4','HCMC-L4-10','Ngã 4 Phú Nhuận','Nga 4 Phu Nhuan',
 'Ngã 4 Phú Nhuận, P.2, Q.Phú Nhuận, TP.HCM',FALSE,TRUE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.7989,106.6841,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-11: Cau Kieu (elevated)
('e7c5d8a3-9f8b-4e7c-d8a3-9f8be7c5d8a3','HCMC-L4-11','Cầu Kiệu','Cau Kieu',
 'Cầu Kiệu, P.2, Q.Phú Nhuận, TP.HCM',FALSE,TRUE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.7915,106.6893,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-12: Cong Vien Le Van Tam (underground)
('f6d4e9b2-8a9c-4f6d-e9b2-8a9cf6d4e9b2','HCMC-L4-12','Công Viên Lê Văn Tám','Le Van Tam Park',
 'Công viên Lê Văn Tám, P.6, Q.3, TP.HCM',TRUE,TRUE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.7871,106.6934,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-13: Ho Con Rua (underground)
-- L4-14: Ben Thanh (underground)

-- L4-15: Hoang Dieu (underground)
('a5e3f8c1-9b6d-4a5e-f8c1-9b6da5e3f8c1','HCMC-L4-15','Hoàng Diệu','Hoang Dieu',
 'Đường Hoàng Diệu, P.6, Q.4, TP.HCM',TRUE,TRUE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.7621,106.7028,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-16: Ton Dan (underground)
('b6f4a9d2-8c7e-4b6f-a9d2-8c7eb6f4a9d2','HCMC-L4-16','Tôn Đản','Ton Dan',
 'Đường Tôn Đản, P.6, Q.4, TP.HCM',TRUE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.7563,106.7081,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-17: Nguyen Thi Thap (underground)
('c7a5b8e3-9d8f-4c7a-b8e3-9d8fc7a5b8e3','HCMC-L4-17','Nguyễn Thị Thập','Nguyen Thi Thap',
 'Đường Nguyễn Thị Thập, P.Tân Phong, Q.7, TP.HCM',TRUE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.7441,106.7132,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-18: Nguyen Van Linh (underground)
('572e6743-c3de-4374-a70f-52ae566c64b2','HCMC-L4-18','Nguyễn Văn Linh','Nguyen Van Linh',
 'Đường Nguyễn Văn Linh, P.Tân Phong, Q.7, TP.HCM',TRUE,TRUE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.7328,106.7195,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-19: Phuoc Kieng (elevated)
('e9c7d8a5-9f8b-4e9c-d8a5-9f8be9c7d8a5','HCMC-L4-19','Phước Kiểng','Phuoc Kieng',
 'Khu vực Phước Kiểng, P.Phước Kiểng, H.Nhà Bè, TP.HCM',FALSE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.7215,106.7098,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-20: Vinh Phuoc (elevated)
('f8d6e9b4-8a9c-4f8d-e9b4-8a9cf8d6e9b4','HCMC-L4-20','Vĩnh Phước','Vinh Phuoc',
 'Khu vực Vĩnh Phước, P.Phước Kiểng, H.Nhà Bè, TP.HCM',FALSE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.7136,106.7053,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-21: Pham Huu Lau (elevated)
('a9e5f8c3-9b7d-4a9e-f8c3-9b7da9e5f8c3','HCMC-L4-21','Phạm Hữu Lầu','Pham Huu Lau',
 'Khu vực Phạm Hữu Lầu, P.Phước Kiểng, H.Nhà Bè, TP.HCM',FALSE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.6989,106.7112,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-22: Kho B (elevated)
('b8f4a9d6-8c9e-4b8f-a9d6-8c9eb8f4a9d6','HCMC-L4-22','Kho B','Kho B',
 'Khu vực Kho B, P.Phước Kiểng, H.Nhà Bè, TP.HCM',FALSE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.6891,106.7164,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-23: Long Kieng (elevated)
('c7a3b8e5-9d8f-4c7a-b8e5-9d8fc7a3b8e5','HCMC-L4-23','Long Kiểng','Long Kieng',
 'Khu vực Long Kiểng, P.Long Kiểng, H.Nhà Bè, TP.HCM',FALSE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.6807,106.7211,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-24: Ba Chiem (elevated)
('d6b2c9f4-8e9a-4d6b-c9f4-8e9ad6b2c9f4','HCMC-L4-24','Bà Chiêm','Ba Chiem',
 'Khu vực Bà Chiêm, P.Long Kiểng, H.Nhà Bè, TP.HCM',FALSE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.6701,106.7265,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-25: Long Thoi (elevated)
('e5c1d8a3-9f8b-4e5c-d8a3-9f8be5c1d8a3','HCMC-L4-25','Long Thới','Long Thoi',
 'Khu vực Long Thới, P.Long Thới, H.Nhà Bè, TP.HCM',FALSE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.6625,106.7303,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-26: Rach Doi (elevated)
('f4d8e9b2-8a9c-4f4d-e9b2-8a9cf4d8e9b2','HCMC-L4-26','Rạch Dơi','Rach Doi',
 'Khu vực Rạch Dơi, P.Hiệp Phước, H.Nhà Bè, TP.HCM',FALSE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.6509,106.7368,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-27: Hiep Phuoc (elevated)
('a3e7f8c1-9b6d-4a3e-f8c1-9b6da3e7f8c1','HCMC-L4-27','Hiệp Phước','Hiep Phuoc',
 'Khu vực Hiệp Phước, P.Hiệp Phước, H.Nhà Bè, TP.HCM',FALSE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.6398,106.7421,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-28: Cong Vien The Thao (elevated)
('b2f6a9d8-8c7e-4b2f-a9d8-8c7eb2f6a9d8','HCMC-L4-28','Công Viên Thể Thao','Sports Park',
 'Công viên Thể thao, P.Hiệp Phước, H.Nhà Bè, TP.HCM',FALSE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.6281,106.7479,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-29: Benh Vien Quoc Te (elevated)
('c1a5b8e7-9d8f-4c1a-b8e7-9d8fc1a5b8e7','HCMC-L4-29','Bệnh Viện Quốc Tế','International Hospital',
 'Bệnh viện Quốc tế, P.Hiệp Phước, H.Nhà Bè, TP.HCM',FALSE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.6199,106.7523,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-30: Thi Tran Hiep Phuoc (elevated)
('d8b4c9f6-8e9a-4d8b-c9f6-8e9ad8b4c9f6','HCMC-L4-30','Thị Trấn Hiệp Phước','Hiep Phuoc Town',
 'Thị trấn Hiệp Phước, P.Hiệp Phước, H.Nhà Bè, TP.HCM',FALSE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.6117,106.7568,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-31: Cong Vien Cay Xanh (elevated)
('e7c3d8a5-9f8b-4e7c-d8a5-9f8be7c3d8a5','HCMC-L4-31','Công Viên Cây Xanh','Green Park',
 'Công viên Cây xanh, P.Hiệp Phước, H.Nhà Bè, TP.HCM',FALSE,FALSE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.6035,106.7612,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-32: Ben Tau Hiep Phuoc (elevated)
('f6d2e9b4-8a9c-4f6d-e9b4-8a9cf6d2e9b4','HCMC-L4-32','Bến Tàu Hiệp Phước','Ben Tau Hiep Phuoc',
 'Bến tàu Hiệp Phước, P.Hiệp Phước, H.Nhà Bè, TP.HCM',FALSE,TRUE,
 'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',10.5928,106.7674,
 NULL,NULL,NULL,NOW(),NOW(),NULL);

-- Metro Line 4 Forward Routes (2 Direction) - old
INSERT INTO public."Routes"
("Id","RouteCode","LineId","FromStationId","ToStationId","RouteNameVi","RouteNameEn","Direction",
 "SeqOrder","TravelTimeMin","LengthKm",
 "CreatedBy","LastUpdatedBy","DeletedBy","CreatedAt","LastUpdatedAt","DeletedAt")
VALUES
-- L4-01-02: Thanh Xuan to Giao Khau
('1a2b3c4d-5e6f-7890-abcd-ef1234567890','HCMC-L4-01-02','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'a3e9f4c7-5b8d-4a3e-f4c7-5b8da3e9f4c7','b4f8a5d6-6c9e-4b4f-a5d6-6c9eb4f8a5d6',
 'Thạnh Xuân – Giao Khẩu','Thanh Xuan – Giao Khau',0,1,2,0.73,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL),

-- L4-02-03: Giao Khau to Nga 4 Ga
('2b3c4d5e-6f70-8901-bcde-f2345678901a','HCMC-L4-02-03','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'b4f8a5d6-6c9e-4b4f-a5d6-6c9eb4f8a5d6','c5a9b6e7-7d8f-4c5a-b6e7-7d8fc5a9b6e7',
 'Giao Khẩu – Ngã 4 Ga','Giao Khau – Nga 4 Ga',0,2,1,0.35,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL),

-- L4-03-04: Nga 4 Ga to An Loc
('3c4d5e6f-7081-9012-cdef-345678901abc','HCMC-L4-03-04','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'c5a9b6e7-7d8f-4c5a-b6e7-7d8fc5a9b6e7','d6b8c7f8-8e9a-4d6b-c7f8-8e9ad6b8c7f8',
 'Ngã 4 Ga – An Lộc','Nga 4 Ga – An Loc',0,3,1,0.40,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL),

-- L4-04-05: An Loc to An Nhon
('4d5e6f70-8192-a123-def0-456789012bcd','HCMC-L4-04-05','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'd6b8c7f8-8e9a-4d6b-c7f8-8e9ad6b8c7f8','e7c9d8a9-9f8b-4e7c-d8a9-9f8be7c9d8a9',
 'An Lộc – An Nhơn','An Loc – An Nhon',0,4,2,1.18,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL),

-- L4-05-06: An Nhon to Lam Son
('5e6f7081-92a3-b234-ef01-56789012cdef','HCMC-L4-05-06','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'e7c9d8a9-9f8b-4e7c-d8a9-9f8be7c9d8a9','f8d8e9b8-8a9c-4f8d-e9b8-8a9cf8d8e9b8',
 'An Nhơn – Lam Sơn','An Nhon – Lam Son',0,5,2,0.95,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL),

-- L4-06-07: Lam Son to Nga 6 Go Vap
('6f708192-a3b4-c345-f012-6789012def01','HCMC-L4-06-07','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'f8d8e9b8-8a9c-4f8d-e9b8-8a9cf8d8e9b8','a9e7f8c7-9b8d-4a9e-f8c7-9b8da9e7f8c7',
 'Lam Sơn – Ngã 6 Gò Vấp','Lam Son – Nga 6 Go Vap',0,6,2,1.25,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL),

-- L4-07-08: Nga 6 Go Vap to Benh Vien 175
('708192a3-b4c5-d456-0123-789012ef0123','HCMC-L4-07-08','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'a9e7f8c7-9b8d-4a9e-f8c7-9b8da9e7f8c7','b8f6a9d6-8c9e-4b8f-a9d6-8c9eb8f6a9d6',
 'Ngã 6 Gò Vấp – Bệnh Viện 175','Nga 6 Go Vap – Hospital 175',0,7,2,0.87,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL),

-- L4-08-09: Benh Vien 175 to Cong Vien Gia Dinh
('8192a3b4-c5d6-e567-1234-89012f012345','HCMC-L4-08-09','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'b8f6a9d6-8c9e-4b8f-a9d6-8c9eb8f6a9d6','c9a7b8e5-9d8f-4c9a-b8e5-9d8fc9a7b8e5',
 'Bệnh Viện 175 – Công Viên Gia Định','Hospital 175 – Gia Dinh Park',0,8,1,0.68,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL),

-- L4-09-10: Cong Vien Gia Dinh to Nga 4 Phu Nhuan
('92a3b4c5-d6e7-f678-2345-9012f0123456','HCMC-L4-09-10','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'c9a7b8e5-9d8f-4c9a-b8e5-9d8fc9a7b8e5','d8b6c9f4-8e9a-4d8b-c9f4-8e9ad8b6c9f4',
 'Công Viên Gia Định – Ngã 4 Phú Nhuận','Gia Dinh Park – Nga 4 Phu Nhuan',0,9,2,1.45,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL),

-- L4-10-11: Nga 4 Phu Nhuan to Cau Kieu
('a3b4c5d6-e7f8-a789-3456-012f01234567','HCMC-L4-10-11','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'd8b6c9f4-8e9a-4d8b-c9f4-8e9ad8b6c9f4','e7c5d8a3-9f8b-4e7c-d8a3-9f8be7c5d8a3',
 'Ngã 4 Phú Nhuận – Cầu Kiệu','Nga 4 Phu Nhuan – Cau Kieu',0,10,2,0.92,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL),

-- L4-11-12: Cau Kieu to Cong Vien Le Van Tam
('b4c5d6e7-f8a9-b890-4567-123f012345678','HCMC-L4-11-12','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'e7c5d8a3-9f8b-4e7c-d8a3-9f8be7c5d8a3','f6d4e9b2-8a9c-4f6d-e9b2-8a9cf6d4e9b2',
 'Cầu Kiệu – Công Viên Lê Văn Tám','Cau Kieu – Le Van Tam Park',0,11,2,0.85,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL),

-- L4-12-13: Cong Vien Le Van Tam to Ho Con Rua (interchange with L3B)
('c5d6e7f8-a9b0-c901-5678-234f0123456789','HCMC-L4-12-13','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'f6d4e9b2-8a9c-4f6d-e9b2-8a9cf6d4e9b2','c3f6a9e8-8d4b-4c3f-a9e8-8d4bc3f6a9e8',
 'Công Viên Lê Văn Tám – Hồ Con Rùa','Le Van Tam Park – Ho Con Rua',0,12,2,0.95,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL),

-- L4-13-14: Ho Con Rua to Ben Thanh (interchange with HCMC-L1, L2, L3A)
('d6e7f8a9-b0c1-d012-6789-345f01234567890','HCMC-L4-13-14','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'c3f6a9e8-8d4b-4c3f-a9e8-8d4bc3f6a9e8','5e9f3c27-8d6b-4c44-9a14-dfd6a3e2e8f0',
 'Hồ Con Rùa – Bến Thành','Ho Con Rua – Ben Thanh',0,13,1,0.68,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL),

-- L4-14-15: Ben Thanh to Hoang Dieu
('e7f8a9b0-c1d2-e123-789a-456f012345678901','HCMC-L4-14-15','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 '5e9f3c27-8d6b-4c44-9a14-dfd6a3e2e8f0','a5e3f8c1-9b6d-4a5e-f8c1-9b6da5e3f8c1',
 'Bến Thành – Hoàng Diệu','Ben Thanh – Hoang Dieu',0,14,2,0.83,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL),

-- L4-15-16: Hoang Dieu to Ton Dan
('f8a9b0c1-d2e3-f234-89ab-567f0123456789012','HCMC-L4-15-16','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'a5e3f8c1-9b6d-4a5e-f8c1-9b6da5e3f8c1','b6f4a9d2-8c7e-4b6f-a9d2-8c7eb6f4a9d2',
 'Hoàng Diệu – Tôn Đản','Hoang Dieu – Ton Dan',0,15,1,0.72,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL),

-- L4-16-17: Ton Dan to Nguyen Thi Thap
('a9b0c1d2-e3f4-a345-9abc-678f01234567890123','HCMC-L4-16-17','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'b6f4a9d2-8c7e-4b6f-a9d2-8c7eb6f4a9d2','c7a5b8e3-9d8f-4c7a-b8e3-9d8fc7a5b8e3',
 'Tôn Đản – Nguyễn Thị Thập','Ton Dan – Nguyen Thi Thap',0,16,2,1.38,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL),

-- L4-17-18: Nguyen Thi Thap to Nguyen Van Linh
('b0c1d2e3-f4a5-b456-abcd-789f012345678901234','HCMC-L4-17-18','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'c7a5b8e3-9d8f-4c7a-b8e3-9d8fc7a5b8e3','572e6743-c3de-4374-a70f-52ae566c64b2',
 'Nguyễn Thị Thập – Nguyễn Văn Linh','Nguyen Thi Thap – Nguyen Van Linh',0,17,2,1.25,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL),

-- L4-18-19: Nguyen Van Linh to Phuoc Kieng
('c1d2e3f4-a5b6-c567-bcde-89af0123456789012345','HCMC-L4-18-19','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 '572e6743-c3de-4374-a70f-52ae566c64b2','e9c7d8a5-9f8b-4e9c-d8a5-9f8be9c7d8a5',
 'Nguyễn Văn Linh – Phước Kiểng','Nguyen Van Linh – Phuoc Kieng',0,18,2,1.42,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL),

-- L4-19-20: Phuoc Kieng to Vinh Phuoc
('d2e3f4a5-b6c7-d678-cdef-9abf01234567890123456','HCMC-L4-19-20','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'e9c7d8a5-9f8b-4e9c-d8a5-9f8be9c7d8a5','f8d6e9b4-8a9c-4f8d-e9b4-8a9cf8d6e9b4',
 'Phước Kiểng – Vĩnh Phước','Phuoc Kieng – Vinh Phuoc',0,19,2,0.98,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL),

-- L4-20-21: Vinh Phuoc to Pham Huu Lau
('e3f4a5b6-c7d8-e789-def0-abcf012345678901234567','HCMC-L4-20-21','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'f8d6e9b4-8a9c-4f8d-e9b4-8a9cf8d6e9b4','a9e5f8c3-9b7d-4a9e-f8c3-9b7da9e5f8c3',
 'Vĩnh Phước – Phạm Hữu Lầu','Vinh Phuoc – Pham Huu Lau',0,20,2,1.68,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL),

-- L4-21-22: Pham Huu Lau to Kho B
('f4a5b6c7-d8e9-f890-ef01-bcdf0123456789012345678','HCMC-L4-21-22','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'a9e5f8c3-9b7d-4a9e-f8c3-9b7da9e5f8c3','b8f4a9d6-8c9e-4b8f-a9d6-8c9eb8f4a9d6',
 'Phạm Hữu Lầu – Kho B','Pham Huu Lau – Kho B',0,21,2,1.25,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL),

-- L4-22-23: Kho B to Long Kieng
('a5b6c7d8-e9f0-a901-f012-cdef01234567890123456789','HCMC-L4-22-23','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'b8f4a9d6-8c9e-4b8f-a9d6-8c9eb8f4a9d6','c7a3b8e5-9d8f-4c7a-b8e5-9d8fc7a3b8e5',
 'Kho B – Long Kiểng','Kho B – Long Kieng',0,22,2,1.15,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL),

-- L4-23-24: Long Kieng to Ba Chiem
('b6c7d8e9-f0a1-b012-0123-def0123456789012345678a','HCMC-L4-23-24','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'c7a3b8e5-9d8f-4c7a-b8e5-9d8fc7a3b8e5','d6b2c9f4-8e9a-4d6b-c9f4-8e9ad6b2c9f4',
 'Long Kiểng – Bà Chiêm','Long Kieng – Ba Chiem',0,23,2,1.32,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL),

-- L4-24-25: Ba Chiem to Long Thoi
('c7d8e9f0-a1b2-c123-1234-ef012345678901234567890ab','HCMC-L4-24-25','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'd6b2c9f4-8e9a-4d6b-c9f4-8e9ad6b2c9f4','e5c1d8a3-9f8b-4e5c-d8a3-9f8be5c1d8a3',
 'Bà Chiêm – Long Thới','Ba Chiem – Long Thoi',0,24,2,0.95,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL),

-- L4-25-26: Long Thoi to Rach Doi
('d8e9f0a1-b2c3-d234-2345-f01234567890123456789abc','HCMC-L4-25-26','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'e5c1d8a3-9f8b-4e5c-d8a3-9f8be5c1d8a3','f4d8e9b2-8a9c-4f4d-e9b2-8a9cf4d8e9b2',
 'Long Thới – Rạch Dơi','Long Thoi – Rach Doi',0,25,2,0.92,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL),

-- L4-26-27: Rach Doi to Hiep Phuoc
('e9f0a1b2-c3d4-e345-3456-0123456789012345678abcd','HCMC-L4-26-27','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'f4d8e9b2-8a9c-4f4d-e9b2-8a9cf4d8e9b2','a3e7f8c1-9b6d-4a3e-f8c1-9b6da3e7f8c1',
 'Rạch Dơi – Hiệp Phước','Rach Doi – Hiep Phuoc',0,26,2,1.18,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL),

-- L4-27-28: Hiep Phuoc to Cong Vien The Thao
('f0a1b2c3-d4e5-f456-4567-123456789012345678bcde','HCMC-L4-27-28','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'a3e7f8c1-9b6d-4a3e-f8c1-9b6da3e7f8c1','b2f6a9d8-8c7e-4b2f-a9d8-8c7eb2f6a9d8',
 'Hiệp Phước – Công Viên Thể Thao','Hiep Phuoc – Sports Park',0,27,2,1.05,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL),

-- L4-28-29: Cong Vien The Thao to Benh Vien Quoc Te
('a1b2c3d4-e5f6-a567-5678-23456789012345678cdef','HCMC-L4-28-29','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'b2f6a9d8-8c7e-4b2f-a9d8-8c7eb2f6a9d8','c1a5b8e7-9d8f-4c1a-b8e7-9d8fc1a5b8e7',
 'Công Viên Thể Thao – Bệnh Viện Quốc Tế','Sports Park – International Hospital',0,28,2,0.88,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL),

-- L4-29-30: Benh Vien Quoc Te to Thi Tran Hiep Phuoc
('b2c3d4e5-f6a7-b678-6789-3456789012345678def0','HCMC-L4-29-30','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'c1a5b8e7-9d8f-4c1a-b8e7-9d8fc1a5b8e7','d8b4c9f6-8e9a-4d8b-c9f6-8e9ad8b4c9f6',
 'Bệnh Viện Quốc Tế – Thị Trấn Hiệp Phước','International Hospital – Hiep Phuoc Town',0,29,2,0.93,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL),

-- L4-30-31: Thi Tran Hiep Phuoc to Cong Vien Cay Xanh
('c3d4e5f6-a7b8-c789-789a-456789012345678ef01','HCMC-L4-30-31','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'd8b4c9f6-8e9a-4d8b-c9f6-8e9ad8b4c9f6','e7c3d8a5-9f8b-4e7c-d8a5-9f8be7c3d8a5',
 'Thị Trấn Hiệp Phước – Công Viên Cây Xanh','Hiep Phuoc Town – Green Park',0,30,2,0.89,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL),

-- L4-31-32: Cong Vien Cay Xanh to Ben Tau Hiep Phuoc
('d4e5f6a7-b8c9-d890-89ab-56789012345678f012','HCMC-L4-31-32','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'e7c3d8a5-9f8b-4e7c-d8a5-9f8be7c3d8a5','f6d2e9b4-8a9c-4f6d-e9b4-8a9cf6d2e9b4',
 'Công Viên Cây Xanh – Bến Tàu Hiệp Phước','Green Park – Ben Tau Hiep Phuoc',0,31,2,1.15,
 NULL,NULL,NULL,NOW(),NOW(),
 NULL),

-- L4-32-31: Ben Tau Hiep Phuoc to Cong Vien Cay Xanh
('11f6d2e9-b4a9-4c11-f6d2-e9b4a9c11f6d','HCMC-L4-32-31','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'f6d2e9b4-8a9c-4f6d-e9b4-8a9cf6d2e9b4','e7c3d8a5-9f8b-4e7c-d8a5-9f8be7c3d8a5',
 'Bến Tàu Hiệp Phước – Công Viên Cây Xanh','Ben Tau Hiep Phuoc – Green Park',1,1,2,1.15,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-31-30: Cong Vien Cay Xanh to Thi Tran Hiep Phuoc
('22e7c3d8-a59f-4822-e7c3-d8a59f8b22e7','HCMC-L4-31-30','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'e7c3d8a5-9f8b-4e7c-d8a5-9f8be7c3d8a5','d8b4c9f6-8e9a-4d8b-c9f6-8e9ad8b4c9f6',
 'Công Viên Cây Xanh – Thị Trấn Hiệp Phước','Green Park – Hiep Phuoc Town',1,2,2,0.89,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-30-29: Thi Tran Hiep Phuoc to Benh Vien Quoc Te
('33d8b4c9-f68e-4933-d8b4-c9f68e9a33d8','HCMC-L4-30-29','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'd8b4c9f6-8e9a-4d8b-c9f6-8e9ad8b4c9f6','c1a5b8e7-9d8f-4c1a-b8e7-9d8fc1a5b8e7',
 'Thị Trấn Hiệp Phước – Bệnh Viện Quốc Tế','Hiep Phuoc Town – International Hospital',1,3,2,0.93,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-29-28: Benh Vien Quoc Te to Cong Vien The Thao
('44c1a5b8-e79d-4444-c1a5-b8e79d8f44c1','HCMC-L4-29-28','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'c1a5b8e7-9d8f-4c1a-b8e7-9d8fc1a5b8e7','b2f6a9d8-8c7e-4b2f-a9d8-8c7eb2f6a9d8',
 'Bệnh Viện Quốc Tế – Công Viên Thể Thao','International Hospital – Sports Park',1,4,2,0.88,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-28-27: Cong Vien The Thao to Hiep Phuoc
('55b2f6a9-d88c-4555-b2f6-a9d88c7e55b2','HCMC-L4-28-27','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'b2f6a9d8-8c7e-4b2f-a9d8-8c7eb2f6a9d8','a3e7f8c1-9b6d-4a3e-f8c1-9b6da3e7f8c1',
 'Công Viên Thể Thao – Hiệp Phước','Sports Park – Hiep Phuoc',1,5,2,1.05,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-27-26: Hiep Phuoc to Rach Doi
('66a3e7f8-c19b-4666-a3e7-f8c19b6d66a3','HCMC-L4-27-26','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'a3e7f8c1-9b6d-4a3e-f8c1-9b6da3e7f8c1','f4d8e9b2-8a9c-4f4d-e9b2-8a9cf4d8e9b2',
 'Hiệp Phước – Rạch Dơi','Hiep Phuoc – Rach Doi',1,6,2,1.18,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-26-25: Rach Doi to Long Thoi
('77f4d8e9-b28a-4777-f4d8-e9b28a9c77f4','HCMC-L4-26-25','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'f4d8e9b2-8a9c-4f4d-e9b2-8a9cf4d8e9b2','e5c1d8a3-9f8b-4e5c-d8a3-9f8be5c1d8a3',
 'Rạch Dơi – Long Thới','Rach Doi – Long Thoi',1,7,2,0.92,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-25-24: Long Thoi to Ba Chiem
('88e5c1d8-a39f-4888-e5c1-d8a39f8b88e5','HCMC-L4-25-24','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'e5c1d8a3-9f8b-4e5c-d8a3-9f8be5c1d8a3','d6b2c9f4-8e9a-4d6b-c9f4-8e9ad6b2c9f4',
 'Long Thới – Bà Chiêm','Long Thoi – Ba Chiem',1,8,2,0.95,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-24-23: Ba Chiem to Long Kieng
('99d6b2c9-f48e-4999-d6b2-c9f48e9a99d6','HCMC-L4-24-23','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'd6b2c9f4-8e9a-4d6b-c9f4-8e9ad6b2c9f4','c7a3b8e5-9d8f-4c7a-b8e5-9d8fc7a3b8e5',
 'Bà Chiêm – Long Kiểng','Ba Chiem – Long Kieng',1,9,2,1.32,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-23-22: Long Kieng to Kho B
('aac7a3b8-e59d-4aaa-c7a3-b8e59d8faac7','HCMC-L4-23-22','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'c7a3b8e5-9d8f-4c7a-b8e5-9d8fc7a3b8e5','b8f4a9d6-8c9e-4b8f-a9d6-8c9eb8f4a9d6',
 'Long Kiểng – Kho B','Long Kieng – Kho B',1,10,2,1.15,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-22-21: Kho B to Pham Huu Lau
('bbb8f4a9-d68c-4bbb-b8f4-a9d68c9ebbb8','HCMC-L4-22-21','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'b8f4a9d6-8c9e-4b8f-a9d6-8c9eb8f4a9d6','a9e5f8c3-9b7d-4a9e-f8c3-9b7da9e5f8c3',
 'Kho B – Phạm Hữu Lầu','Kho B – Pham Huu Lau',1,11,2,1.25,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-21-20: Pham Huu Lau to Vinh Phuoc
('cca9e5f8-c39b-4ccc-a9e5-f8c39b7dcca9','HCMC-L4-21-20','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'a9e5f8c3-9b7d-4a9e-f8c3-9b7da9e5f8c3','f8d6e9b4-8a9c-4f8d-e9b4-8a9cf8d6e9b4',
 'Phạm Hữu Lầu – Vĩnh Phước','Pham Huu Lau – Vinh Phuoc',1,12,2,1.68,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-20-19: Vinh Phuoc to Phuoc Kieng
('ddf8d6e9-b48a-4ddd-f8d6-e9b48a9cddf8','HCMC-L4-20-19','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'f8d6e9b4-8a9c-4f8d-e9b4-8a9cf8d6e9b4','e9c7d8a5-9f8b-4e9c-d8a5-9f8be9c7d8a5',
 'Vĩnh Phước – Phước Kiểng','Vinh Phuoc – Phuoc Kieng',1,13,2,0.98,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-19-18: Phuoc Kieng to Nguyen Van Linh
('eee9c7d8-a59f-4eee-e9c7-d8a59f8beee9','HCMC-L4-19-18','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'e9c7d8a5-9f8b-4e9c-d8a5-9f8be9c7d8a5','572e6743-c3de-4374-a70f-52ae566c64b2',
 'Phước Kiểng – Nguyễn Văn Linh','Phuoc Kieng – Nguyen Van Linh',1,14,2,1.42,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-18-17: Nguyen Van Linh to Nguyen Thi Thap
('fffd8b6c-9f48-4fff-d8b6-c9f48e9afffd','HCMC-L4-18-17','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 '572e6743-c3de-4374-a70f-52ae566c64b2','c7a5b8e3-9d8f-4c7a-b8e3-9d8fc7a5b8e3',
 'Nguyễn Văn Linh – Nguyễn Thị Thập','Nguyen Van Linh – Nguyen Thi Thap',1,15,2,1.25,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-17-16: Nguyen Thi Thap to Ton Dan
('111c7a5b-8e39-4111-c7a5-b8e39d8f111c','HCMC-L4-17-16','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'c7a5b8e3-9d8f-4c7a-b8e3-9d8fc7a5b8e3','b6f4a9d2-8c7e-4b6f-a9d2-8c7eb6f4a9d2',
 'Nguyễn Thị Thập – Tôn Đản','Nguyen Thi Thap – Ton Dan',1,16,2,1.38,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-16-15: Ton Dan to Hoang Dieu
('222b6f4a-9d28-4222-b6f4-a9d28c7e222b','HCMC-L4-16-15','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'b6f4a9d2-8c7e-4b6f-a9d2-8c7eb6f4a9d2','a5e3f8c1-9b6d-4a5e-f8c1-9b6da5e3f8c1',
 'Tôn Đản – Hoàng Diệu','Ton Dan – Hoang Dieu',1,17,1,0.72,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-15-14: Hoang Dieu to Ben Thanh
('333a5e3f-8c19-4333-a5e3-f8c19b6d333a','HCMC-L4-15-14','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'a5e3f8c1-9b6d-4a5e-f8c1-9b6da5e3f8c1','5e9f3c27-8d6b-4c44-9a14-dfd6a3e2e8f0',
 'Hoàng Diệu – Bến Thành','Hoang Dieu – Ben Thanh',1,18,2,0.83,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-14-13: Ben Thanh to Ho Con Rua
('4445e9f3-c278-4444-5e9f-3c278d6b4445','HCMC-L4-14-13','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 '5e9f3c27-8d6b-4c44-9a14-dfd6a3e2e8f0','c3f6a9e8-8d4b-4c3f-a9e8-8d4bc3f6a9e8',
 'Bến Thành – Hồ Con Rùa','Ben Thanh – Ho Con Rua',1,19,1,0.68,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-13-12: Ho Con Rua to Cong Vien Le Van Tam
('555c3f6a-9e88-4555-c3f6-a9e88d4b555c','HCMC-L4-13-12','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'c3f6a9e8-8d4b-4c3f-a9e8-8d4bc3f6a9e8','f6d4e9b2-8a9c-4f6d-e9b2-8a9cf6d4e9b2',
 'Hồ Con Rùa – Công Viên Lê Văn Tám','Ho Con Rua – Le Van Tam Park',1,20,2,0.95,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-12-11: Cong Vien Le Van Tam to Cau Kieu
('666f6d4e-9b28-4666-f6d4-e9b28a9c666f','HCMC-L4-12-11','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'f6d4e9b2-8a9c-4f6d-e9b2-8a9cf6d4e9b2','e7c5d8a3-9f8b-4e7c-d8a3-9f8be7c5d8a3',
 'Công Viên Lê Văn Tám – Cầu Kiệu','Le Van Tam Park – Cau Kieu',1,21,2,0.85,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-11-10: Cau Kieu to Nga 4 Phu Nhuan
('777e7c5d-8a39-4777-e7c5-d8a39f8b777e','HCMC-L4-11-10','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'e7c5d8a3-9f8b-4e7c-d8a3-9f8be7c5d8a3','d8b6c9f4-8e9a-4d8b-c9f4-8e9ad8b6c9f4',
 'Cầu Kiệu – Ngã 4 Phú Nhuận','Cau Kieu – Nga 4 Phu Nhuan',1,22,2,0.92,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-10-09: Nga 4 Phu Nhuan to Cong Vien Gia Dinh
('888d8b6c-9f48-4888-d8b6-c9f48e9a888d','HCMC-L4-10-09','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'd8b6c9f4-8e9a-4d8b-c9f4-8e9ad8b6c9f4','c9a7b8e5-9d8f-4c9a-b8e5-9d8fc9a7b8e5',
 'Ngã 4 Phú Nhuận – Công Viên Gia Định','Nga 4 Phu Nhuan – Gia Dinh Park',1,23,2,1.45,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-09-08: Cong Vien Gia Dinh to Benh Vien 175
('999c9a7b-8e59-4999-c9a7-b8e59d8f999c','HCMC-L4-09-08','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'c9a7b8e5-9d8f-4c9a-b8e5-9d8fc9a7b8e5','b8f6a9d6-8c9e-4b8f-a9d6-8c9eb8f6a9d6',
 'Công Viên Gia Định – Bệnh Viện 175','Gia Dinh Park – Hospital 175',1,24,1,0.68,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-08-07: Benh Vien 175 to Nga 6 Go Vap
('aaab8f6a-9d68-4aaa-b8f6-a9d68c9eaaab','HCMC-L4-08-07','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'b8f6a9d6-8c9e-4b8f-a9d6-8c9eb8f6a9d6','a9e7f8c7-9b8d-4a9e-f8c7-9b8da9e7f8c7',
 'Bệnh Viện 175 – Ngã 6 Gò Vấp','Hospital 175 – Nga 6 Go Vap',1,25,2,0.87,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-07-06: Nga 6 Go Vap to Lam Son
('bbba9e7f-8c79-4bbb-a9e7-f8c79b8dbbba','HCMC-L4-07-06','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'a9e7f8c7-9b8d-4a9e-f8c7-9b8da9e7f8c7','f8d8e9b8-8a9c-4f8d-e9b8-8a9cf8d8e9b8',
 'Ngã 6 Gò Vấp – Lam Sơn','Nga 6 Go Vap – Lam Son',1,26,2,1.25,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-06-05: Lam Son to An Nhon
('cccf8d8e-9b88-4ccc-f8d8-e9b88a9ccccf','HCMC-L4-06-05','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'f8d8e9b8-8a9c-4f8d-e9b8-8a9cf8d8e9b8','e7c9d8a9-9f8b-4e7c-d8a9-9f8be7c9d8a9',
 'Lam Sơn – An Nhơn','Lam Son – An Nhon',1,27,2,0.95,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-05-04: An Nhon to An Loc
('ddde7c9d-8a99-4ddd-e7c9-d8a99f8bddde','HCMC-L4-05-04','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'e7c9d8a9-9f8b-4e7c-d8a9-9f8be7c9d8a9','d6b8c7f8-8e9a-4d6b-c7f8-8e9ad6b8c7f8',
 'An Nhơn – An Lộc','An Nhon – An Loc',1,28,2,1.18,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-04-03: An Loc to Nga 4 Ga
('eeed6b8c-7f88-4eee-d6b8-c7f88e9aeeee','HCMC-L4-04-03','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'd6b8c7f8-8e9a-4d6b-c7f8-8e9ad6b8c7f8','c5a9b6e7-7d8f-4c5a-b6e7-7d8fc5a9b6e7',
 'An Lộc – Ngã 4 Ga','An Loc – Nga 4 Ga',1,29,1,0.40,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-03-02: Nga 4 Ga to Giao Khau
('fffc5a9b-6e77-4fff-c5a9-b6e77d8fffff','HCMC-L4-03-02','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'c5a9b6e7-7d8f-4c5a-b6e7-7d8fc5a9b6e7','b4f8a5d6-6c9e-4b4f-a5d6-6c9eb4f8a5d6',
 'Ngã 4 Ga – Giao Khẩu','Nga 4 Ga – Giao Khau',1,30,1,0.35,
 NULL,NULL,NULL,NOW(),NOW(),NULL),

-- L4-02-01: Giao Khau to Thanh Xuan
('000b4f8a-5d66-4000-b4f8-a5d66c9e0000','HCMC-L4-02-01','f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
 'b4f8a5d6-6c9e-4b4f-a5d6-6c9eb4f8a5d6','a3e9f4c7-5b8d-4a3e-f4c7-5b8da3e9f4c7',
 'Giao Khẩu – Thạnh Xuân','Giao Khau – Thanh Xuan',1,31,2,0.73,
 NULL,NULL,NULL,NOW(),NOW(),NULL);

UPDATE public."Stations"
SET "IsActive" = TRUE;

-- update station Ben Thành IsMultiline to TRUE
UPDATE public."Stations"
SET "IsMultiLine" = TRUE, "StationCodeListJSON" = '[
  {
    "RouteId": "e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c",
    "StationCode": "HCMC‑L1‑01"
  },
  {
    "RouteId": "2f5c8e93-4b7a-4d2e-8f6c-1a9b5e7d3c2f",
    "StationCode": "HCMC‑L2‑01"
  },
  {
    "RouteId": "8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45",
    "StationCode": "HCMC‑L3A‑01"
  },
  {
    "RouteId": "f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9",
    "StationCode": "HCMC‑L4‑14"
  }
]'
WHERE "Id" = '5e9f3c27-8d6b-4c44-9a14-dfd6a3e2e8f0';

-- update station Cong hoa IsMultiline to TRUE - HCMC-L2 - HCMC-L3B
UPDATE public."Stations"
SET "IsMultiLine" = TRUE, "StationCodeListJSON" = '[
  {
    "RouteId": "8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45",
    "StationCode": "HCMC-L3A-03"
  },
  {
    "RouteId": "a1f4e7c8-6b2d-4a1f-e7c8-6b2da1f4e7c8",
    "StationCode": "HCMC-L3B-01"
  }
]'
WHERE "Id" = 'c6a9f3b2-5d8e-4c6a-9f3b-2d8e5c6a9f3b';

-- update station Tao Dan IsMultiline to TRUE - HCMC-L2 - HCMC-L3B
UPDATE public."Stations"
SET "IsMultiLine" = TRUE, "StationCodeListJSON" = '[
  {
    "RouteId": "2f5c8e93-4b7a-4d2e-8f6c-1a9b5e7d3c2f",
    "StationCode": "HCMC-L2-02"
  },
  {
    "RouteId": "a1f4e7c8-6b2d-4a1f-e7c8-6b2da1f4e7c8",
    "StationCode": "HCMC-L3B-03"
  }
]'
WHERE "Id" = '8a4c6e91-2f5b-4d3e-9c7a-6b8d2f4e7c9a';

-- Update station Ho Con Rua IsMultiLine to TRUE - HCMC-L3B - HCMC-L4
UPDATE public."Stations"
SET "IsMultiLine" = TRUE, "StationCodeListJSON" = '[
  {
    "RouteId": "a1f4e7c8-6b2d-4a1f-e7c8-6b2da1f4e7c8",
    "StationCode": "HCMC-L3B-04"
  },
  {
    "RouteId": "f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9",
    "StationCode": "HCMC-L4-13"
  }
]'
WHERE "Id" = 'c3f6a9e8-8d4b-4c3f-a9e8-8d4bc3f6a9e8';
-- ───────────────────────────────────────────────────────────────
-- Staff Assignments Seed Data
INSERT INTO public."StaffAssignments"
("Id", "StaffId", "StationId","AssignedRole","FromTime", "IsActive",
 "CreatedBy", "LastUpdatedBy", "DeletedBy", "CreatedAt", "LastUpdatedAt", "DeletedAt")
VALUES
    ('a242c7cb-6443-4d31-8cf9-f82a98101754',
     'dfe95016-f1cd-4c6e-b03f-ee917550584e', '5e9f3c27-8d6b-4c44-9a14-dfd6a3e2e8f0',
     3, NOW(), TRUE,
     NULL, NULL, NULL, NOW(),NOW(),NULL),
    ('ba35c7a8-255b-487f-86b2-f04a1ceecb32',
     'd87ac817-eaa4-4b90-8bc3-3c1e04175a15', '5e9f3c27-8d6b-4c44-9a14-dfd6a3e2e8f0',
     2, NOW(), TRUE,
     NULL, NULL, NULL, NOW(),NOW(),NULL);
-- ───────────────────────────────────────────────────────────────
-- Train Seed Data
INSERT INTO public."MetroTrains"
("Id", "TrainCode", "ModelName", "LineId","IsActive", "NumberOfCarriages", "CurrentStationId",
 "CreatedBy", "LastUpdatedBy", "DeletedBy", "CreatedAt", "LastUpdatedAt", "DeletedAt")
VALUES
    ('2b3c4d5e-6f70-8192-a3b4-c5d6e7f8a9b0', 'HCMC-L1-T01','HITACHI',
     'e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     TRUE, 3, '5e9f3c27-8d6b-4c44-9a14-dfd6a3e2e8f0',
     NULL, NULL, NULL, NOW(), NOW(), NULL),
    ('87eca124-25e8-4994-a07d-8715bfdef44b', 'HCMC-L1-T02','HITACHI',
     'e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     TRUE, 3, 'd6e7f8a9-b0c1-d2e3-f4a5-b6c7d8e9f0a1',
     NULL, NULL, NULL, NOW(), NOW(), NULL),
    ('f8f195af-4db7-4786-a1a7-acf8812b4e71', 'HCMC-L2-T01','HITACHI',
     '2f5c8e93-4b7a-4d2e-8f6c-1a9b5e7d3c2f',
     TRUE, 3, '5e9f3c27-8d6b-4c44-9a14-dfd6a3e2e8f0',
     NULL, NULL, NULL, NOW(), NOW(), NULL),
    ('8d466e7b-df3e-421b-9080-9f13ae4fc0cc', 'HCMC-L2-T02','HITACHI',
     '2f5c8e93-4b7a-4d2e-8f6c-1a9b5e7d3c2f',
     TRUE, 3, '8d7f1a0a-5e9b-4f7d-6f8d-1a8e5e9f7d1d',
     NULL, NULL, NULL, NOW(), NOW(), NULL),
    ('f0e189a5-e649-47f6-aa77-7ba543208f42', 'HCMC-L3A-T01','HITACHI',
     '8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',
     TRUE, 3, '5e9f3c27-8d6b-4c44-9a14-dfd6a3e2e8f0',
     NULL, NULL, NULL, NOW(), NOW(), NULL),
    ('76c5f4b0-1b32-4284-b9a6-7a0deeccba64', 'HCMC-L3A-T02','HITACHI',
     '8a7f3c45-9e2b-4d8a-b6f1-5c3e9a7f3c45',
     TRUE, 3, 'd0a40d5f-1e84-459e-bfeb-3f2105db3e0b',
     NULL, NULL, NULL, NOW(), NOW(), NULL),
    ('7d26d6af-fe31-4564-8930-0f3d8d28b526', 'HCMC-L3B-T01','HITACHI',
     'a1f4e7c8-6b2d-4a1f-e7c8-6b2da1f4e7c8',
     TRUE, 3, 'c6a9f3b2-5d8e-4c6a-9f3b-2d8e5c6a9f3b',
     NULL, NULL, NULL, NOW(), NOW(), NULL),
    ('66243ad8-7a3d-40b9-986e-a6d258ba6e97', 'HCMC-L3B-T02','HITACHI',
     'a1f4e7c8-6b2d-4a1f-e7c8-6b2da1f4e7c8',
     TRUE, 3, 'c9f8a6e2-8d9b-4c9f-a6e2-8d9bc9f8a6e2',
     NULL, NULL, NULL, NOW(), NOW(), NULL),
    ('97ac027f-c0ba-4bb5-a70f-79aeb4f9caef', 'HCMC-L4-T01','HITACHI',
     'f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
     TRUE, 3, 'a3e9f4c7-5b8d-4a3e-f4c7-5b8da3e9f4c7',
     NULL, NULL, NULL, NOW(), NOW(), NULL),
    ('89145ee1-6a8a-4df6-a485-acb2853a83ac', 'HCMC-L4-T02','HITACHI',
     'f2a8d5c9-3e7b-4f2a-d5c9-3e7bf2a8d5c9',
     TRUE, 3, 'f6d2e9b4-8a9c-4f6d-e9b4-8a9cf6d2e9b4',
     NULL, NULL, NULL, NOW(), NOW(), NULL)
;

-- L1-T01, L2-T01, L3A-T01, L4-T01 : Ben Thanh
UPDATE public."MetroTrains"
SET
    "Latitude" = s."Latitude",
    "Longitude" = s."Longitude",
    "CurrentStationId" = s."Id"
FROM public."Stations" s
WHERE s."Id" = '5e9f3c27-8d6b-4c44-9a14-dfd6a3e2e8f0'
AND public."MetroTrains"."Id" IN (
    '2b3c4d5e-6f70-8192-a3b4-c5d6e7f8a9b0',
    'f8f195af-4db7-4786-a1a7-acf8812b4e71',
    'f0e189a5-e649-47f6-aa77-7ba543208f42',
    '97ac027f-c0ba-4bb5-a70f-79aeb4f9caef'
);

-- L1-T02: Suoi Tien Terminal
UPDATE public."MetroTrains"
SET
    "Latitude" = s."Latitude",
    "Longitude" = s."Longitude",
    "CurrentStationId" = s."Id"
FROM public."Stations" s
WHERE s."Id" = 'd6e7f8a9-b0c1-d2e3-f4a5-b6c7d8e9f0a1'
AND public."MetroTrains"."Id" IN (
'87eca124-25e8-4994-a07d-8715bfdef44b'
);

-- L2-T02: Tan Binh
UPDATE public."MetroTrains"
SET
    "Latitude" = s."Latitude",
    "Longitude" = s."Longitude",
    "CurrentStationId" = s."Id"
FROM public."Stations" s
WHERE s."Id" = '8d7f1a0a-5e9b-4f7d-6f8d-1a8e5e9f7d1d'
  AND public."MetroTrains"."Id" IN (
    '8d466e7b-df3e-421b-9080-9f13ae4fc0cc'
    );

-- L3A-T02: Tan Kien
UPDATE public."MetroTrains"
SET
    "Latitude" = s."Latitude",
    "Longitude" = s."Longitude",
    "CurrentStationId" = s."Id"
FROM public."Stations" s
WHERE s."Id" = 'd0a40d5f-1e84-459e-bfeb-3f2105db3e0b'
  AND public."MetroTrains"."Id" IN (
    '76c5f4b0-1b32-4284-b9a6-7a0deeccba64'
    );

-- L3B-T01: Cong Hoa
UPDATE public."MetroTrains"
SET
    "Latitude" = s."Latitude",
    "Longitude" = s."Longitude",
    "CurrentStationId" = s."Id"
FROM public."Stations" s
WHERE s."Id" = 'c6a9f3b2-5d8e-4c6a-9f3b-2d8e5c6a9f3b'
  AND public."MetroTrains"."Id" IN (
    '7d26d6af-fe31-4564-8930-0f3d8d28b526'
    );

-- L3B-T02: Hiep Binh Phuoc
UPDATE public."MetroTrains"
SET
    "Latitude" = s."Latitude",
    "Longitude" = s."Longitude",
    "CurrentStationId" = s."Id"
FROM public."Stations" s
WHERE s."Id" = 'c9f8a6e2-8d9b-4c9f-a6e2-8d9bc9f8a6e2'
  AND public."MetroTrains"."Id" IN (
    '66243ad8-7a3d-40b9-986e-a6d258ba6e97'
    );

-- L4-T02: Ben Tau Hiep Phuoc
UPDATE public."MetroTrains"
SET
    "Latitude" = s."Latitude",
    "Longitude" = s."Longitude",
    "CurrentStationId" = s."Id"
FROM public."Stations" s
WHERE s."Id" = 'f6d2e9b4-8a9c-4f6d-e9b4-8a9cf6d2e9b4'
  AND public."MetroTrains"."Id" IN (
    '89145ee1-6a8a-4df6-a485-acb2853a83ac'
    );

-- ───────────────────────────────────────────────────────────────
-- Pricing Config Seed Data
INSERT INTO public."PricingConfig"
("Id", "IsActive", "EffectiveFrom", "CreatedBy", "LastUpdatedBy", "DeletedBy", "CreatedAt", "LastUpdatedAt", "DeletedAt")
VALUES
    ('f6c0e0bf-7be8-4d5d-be84-8ed7047150dd', TRUE, NOW(),
     NULL, NULL, NULL, NOW(), NOW(), NULL);

INSERT INTO public."WeightTier"
("Id", "PricingConfigId", "TierOrder", "MaxWeightKg", "BasePriceVndPerKmPerKg","IsPricePerKmAndKg",
 "CreatedBy", "LastUpdatedBy", "DeletedBy", "CreatedAt", "LastUpdatedAt", "DeletedAt")
VALUES
    ('6862aa75-8eb1-47e2-b0c8-5deb9f7a60fc', 'f6c0e0bf-7be8-4d5d-be84-8ed7047150dd',
     1,10, 30,TRUE,
     NULL, NULL, NULL, NOW(), NOW(), NULL),
    ('d9d532a0-e10c-4100-a4ed-e1d4ba6ab384', 'f6c0e0bf-7be8-4d5d-be84-8ed7047150dd',
     2,50, 25,TRUE,
     NULL, NULL, NULL, NOW(), NOW(), NULL),
    ('77730cd8-8bf8-4347-a827-7cf53abfab3e', 'f6c0e0bf-7be8-4d5d-be84-8ed7047150dd',
     3,100, 22,TRUE,
     NULL, NULL, NULL, NOW(), NOW(), NULL),
    ('9a59e690-7b65-4826-8bb1-cd401de857e7', 'f6c0e0bf-7be8-4d5d-be84-8ed7047150dd',
    4,200, 20,TRUE,
    NULL, NULL, NULL, NOW(), NOW(), NULL);

INSERT INTO public."DistanceTier"
("Id", "PricingConfigId", "TierOrder","MaxDistanceKm", "BasePriceVnd","BasePriceVndPerKm","IsPricePerKm",
 "CreatedBy", "LastUpdatedBy", "DeletedBy", "CreatedAt", "LastUpdatedAt", "DeletedAt")
VALUES
    ('d0f42872-8da6-4fc5-b11a-8e72ecf09d80', 'f6c0e0bf-7be8-4d5d-be84-8ed7047150dd',
     1,7, 5000,NULL,FALSE,
     NULL, NULL, NULL, NOW(), NOW(), NULL),
    ('57aa1280-27aa-41b7-8e83-fc0380b2e920', 'f6c0e0bf-7be8-4d5d-be84-8ed7047150dd',
     2,100, NULL,800,TRUE,
     NULL, NULL, NULL, NOW(), NOW(), NULL);

-- 11/08/2025: Update BaseSurchargePerDayVnd, FreeStoreDays, RefundRate, RefundForCancellationBeforeScheduledHours
UPDATE public."PricingConfig"
SET "BaseSurchargePerDayVnd" = 2000, "FreeStoreDays" = 2,
    "RefundRate" = 0.8, "RefundForCancellationBeforeScheduledHours" = 24
WHERE "Id" = 'f6c0e0bf-7be8-4d5d-be84-8ed7047150dd';

-- ───────────────────────────────────────────────────────────────
-- InsurancePolicy Seed Data
INSERT INTO public."InsurancePolicy"
("Id", "Name", "BaseFeeVnd", "MaxParcelValueVnd", "InsuranceFeeRateOnValue",
 "StandardCompensationValueVnd","MaxCompensationRateOnValue", "MinCompensationRateOnValue", "MaxCompensationRateOnShippingFee",
 "ValidFrom","IsActive",
 "CreatedBy", "LastUpdatedBy", "DeletedBy", "CreatedAt", "LastUpdatedAt", "DeletedAt")
VALUES
    ('5f55489c-c92a-4329-b848-b37dae74c252',
     'Chính sách bảo hiểm và bồi thường từ 1/8/2025',
     5000, 100000000, 0.005,
     500000,1,0.3, 4,
     '2025-08-01',TRUE,
     NULL, NULL, NULL, NOW(), NOW(), NULL);

INSERT INTO public."CategoryInsurance"
("Id", "ParcelCategoryId", "InsurancePolicyId", "IsActive",
 "CreatedBy", "LastUpdatedBy", "DeletedBy", "CreatedAt", "LastUpdatedAt", "DeletedAt")
VALUES
    ('b75c76cb-6c42-47c7-b7d3-c4e1ed333b30',
     '0a1b2c3d-4e5f-6789-abcd-0fedcba98765', '5f55489c-c92a-4329-b848-b37dae74c252',
     TRUE, NULL, NULL, NULL, NOW(), NOW(), NULL),
    ('8e5b26b9-b8c8-495f-956c-ee5149ba5b93',
     '3d4e5f6a-7b8c-9012-def0-3fedcba98765', '5f55489c-c92a-4329-b848-b37dae74c252',
     TRUE, NULL, NULL, NULL, NOW(), NOW(), NULL),
    ('dce2eb72-2449-47b6-8afb-b8844b5b9919',
     '4e5f6a7b-8c9d-0123-ef01-4fedcba98765', '5f55489c-c92a-4329-b848-b37dae74c252',
     TRUE, NULL, NULL, NULL, NOW(), NOW(), NULL),
    ('be213d73-ba20-4bcd-84fc-99901f934010',
     '9c50210e-5670-4608-b58a-c54d0b3cc249', '5f55489c-c92a-4329-b848-b37dae74c252',
     TRUE, NULL, NULL, NULL, NOW(), NOW(), NULL);


