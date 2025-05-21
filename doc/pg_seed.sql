-- Database: MetroShip

-- Roles
/*INSERT INTO public."Roles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp") VALUES ('010b0aba-03fc-436f-a104-86ef3b5a59b9', 'Staff', 'STAFF', 'd8942f4f-f178-40cd-8e54-52f1a38c3990');
INSERT INTO public."Roles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp") VALUES ('1e0b9237-6bbc-4949-a4ac-1ae98aa50387', 'Admin', 'ADMIN', 'a3f7b082-50aa-40f3-809c-02675a35bbf7');
INSERT INTO public."Roles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp") VALUES ('adb62054-ba71-4499-b7bf-cf08953ef7be', 'Customer', 'CUSTOMER', '25e8f05e-e858-46e3-b099-5909fcd4314c');
*/

-- Regions
INSERT INTO public."Regions"
("Id", "RegionCode", "RegionName", "CreatedBy", "LastUpdatedBy", "DeletedBy", "CreatedAt", "LastUpdatedAt", "DeletedAt")
VALUES
    ('d75d6e5a-34b5-4396-b0b9-2947250ed138','HN','Hà Nội',NULL,NULL, NULL, NOW(), NOW(), NULL),
    ('c29464b1-6b74-4cde-9e9c-51bf0ecc522f','HCMC','Thành phố Hồ Chí Minh',NULL,NULL, NULL, NOW(), NOW(), NULL);
-- ───────────────────────────────────────────────────────────────────────────────
-- Metro Line 1: Bến Thành – Suối Tiên
-- ───────────────────────────────────────────────────────────────────────────────
INSERT INTO public."MetroLines"
("Id","RegionId","LineNameVi","LineNameEn","LineCode","TotalKm","TotalStations", "LineType",
 "LineOwner","CarriagesPerTrain",
 "CarriageLenghtMeter","CarriageWidthMeter", "CarriageHeightMeter",
 "CarriageWeightTons","BasePriceVndPerKm",
 "MinHeadwayMin","MaxHeadwayMin","RouteTimeMin","DwellTimeMin","ColorHex","TopSpeedKmH","TopSpeedUdgKmH",
 "IsActive","CreatedBy","LastUpdatedBy","DeletedBy","CreatedAt","LastUpdatedAt","DeletedAt")
VALUES
    (
        'e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',  -- GUID
        'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',                           -- existing RegionId
        'Tuyến 1: Bến Thành – Suối Tiên',
        'Metro 1: Ben Thanh – Suoi Tien',
        'L1',19.70,14,
        'Đường sắt đô thị Thành Phố Hồ Chí Minh',
        'Ban Quản lý Đường sắt đô thị Thành phố Hồ Chí Minh (MAUR)',
        3,20.25,2.95,4.08,
        27, 5000,
        8,12,34,1,
        '#0079B8',
        100,80,
        TRUE,
        NULL, NULL, NULL,
        NOW(), NOW(), NULL
    );

-- ───────────────────────────────────────────────────────────────────────────────
-- Stations on Line 1 (3 underground, 11 elevated)
-- ───────────────────────────────────────────────────────────────────────────────
INSERT INTO public."Stations"
("Id","StationCode","StationNameVi","StationNameEn","Address","IsUnderground","IsActive","RegionId","Latitude","Longitude",
 "CreatedBy","LastUpdatedBy","DeletedBy","CreatedAt","LastUpdatedAt","DeletedAt")
VALUES
    -- Underground
    ('5e9f3c27-8d6b-4c44-9a14-dfd6a3e2e8f0','L1-01','Bến Thành','Ben Thanh',
     'Quách Thị Trang, Phường Bến Thành, Quận 1, TP. Hồ Chí Minh, Việt Nam',
     TRUE,TRUE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',
     10.77060,106.69738,NULL,NULL,NULL,NOW(),NOW(),NULL),  -- :contentReference[oaicite:2]{index=2}
    ('1a2b3c4d-5e6f-7081-92a3-b4c5d6e7f8a9','L1-02','Nhà hát Thành phố','Opera House',
     'Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh, Việt Nam',
     TRUE,TRUE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',
     10.77547,106.70215,NULL,NULL,NULL,NOW(),NOW(),NULL),  -- :contentReference[oaicite:3]{index=3}
    ('2b3c4d5e-6f70-8192-a3b4-c5d6e7f8a9b0','L1-03','Ba Son','Ba Son',
     'Đại lộ Tôn Đức Thắng, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh, Việt Nam',
     TRUE,TRUE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',
     10.78160,106.70800,NULL,NULL,NULL,NOW(),NOW(),NULL),  -- :contentReference[oaicite:4]{index=4}

    -- Elevated
    ('3c4d5e6f-7081-92a3-b4c5-d6e7f8a9b0c1','L1-04','Công viên Văn Thánh','Van Thanh Park',
     'Quận Bình Thạnh, TP. Hồ Chí Minh, Việt Nam',
     FALSE,TRUE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',
     10.79613,106.71554,NULL,NULL,NULL,NOW(),NOW(),NULL),  -- :contentReference[oaicite:5]{index=5}
    ('4d5e6f70-8192-a3b4-c5d6-e7f8a9b0c1d2','L1-05','Tân Cảng','Tan Cang',
     'Đường Điện Biên Phủ, Phường 22, Quận Bình Thạnh, TP. Hồ Chí Minh, Việt Nam',
     FALSE,TRUE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',
     10.79862,106.72330,NULL,NULL,NULL,NOW(),NOW(),NULL),  -- :contentReference[oaicite:6]{index=6}
    ('5e6f7081-92a3-b4c5-d6e7-f8a9b0c1d2e3','L1-06','Thảo Điền','Thao Dien',
     'Công viên Cầu Sài Gòn, Phường Thảo Điền, TP. Thủ Đức, Việt Nam',
     FALSE,TRUE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',
     10.80053,106.73368,NULL,NULL,NULL,NOW(),NOW(),NULL),  -- :contentReference[oaicite:7]{index=7}
    ('6f708192-a3b4-c5d6-e7f8-a9b0c1d2e3f4','L1-07','An Phú','An Phu',
     'Phường Thảo Điền, TP. Thủ Đức, Việt Nam',
     FALSE,TRUE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',
     10.80215,106.74234,NULL,NULL,NULL,NOW(),NOW(),NULL),  -- :contentReference[oaicite:8]{index=8}
    ('708192a3-b4c5-d6e7-f8a9-b0c1d2e3f4a5','L1-08','Rạch Chiếc','Rach Chiec',
     'Phường An Phú, TP. Thủ Đức, Việt Nam',
     FALSE,TRUE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',
     10.80854,106.75529,NULL,NULL,NULL,NOW(),NOW(),NULL),  -- :contentReference[oaicite:9]{index=9}
    ('8192a3b4-c5d6-e7f8-a9b0-c1d2e3f4a5b6','L1-09','Phước Long','Phuoc Long',
     'Phường Trường Thọ, TP. Thủ Đức, Việt Nam',
     FALSE,TRUE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',
     10.82146,106.75820,NULL,NULL,NULL,NOW(),NOW(),NULL),  -- :contentReference[oaicite:10]{index=10}
    ('92a3b4c5-d6e7-f8a9-b0c1-d2e3f4a5b6c7','L1-10','Bình Thái','Binh Thai',
     'Phường Trường Thọ, TP. Thủ Đức, Việt Nam',
     FALSE,TRUE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',
     10.83266,106.76389,NULL,NULL,NULL,NOW(),NOW(),NULL),  -- :contentReference[oaicite:11]{index=11}
    ('a3b4c5d6-e7f8-a9b0-c1d2-e3f4a5b6c7d8','L1-11','Thủ Đức','Thu Duc',
     'Phường Bình Thọ, TP. Thủ Đức, Việt Nam',
     FALSE,TRUE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',
     10.84641,106.77167,NULL,NULL,NULL,NOW(),NOW(),NULL),  -- :contentReference[oaicite:12]{index=12}
    ('b4c5d6e7-f8a9-b0c1-d2e3-f4a5b6c7d8e9','L1-12','Khu Công nghệ Cao','Hi-Tech Park',
     'Xa lộ Hà Nội, Phường Linh Trung, TP. Thủ Đức, Việt Nam',
     FALSE,TRUE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',
     10.85905,106.78889,NULL,NULL,NULL,NOW(),NOW(),NULL),  -- :contentReference[oaicite:13]{index=13}
    ('c5d6e7f8-a9b0-c1d2-e3f4-a5b6c7d8e9f0','L1-13','Đại học Quốc Gia','National University',
     'Xa lộ Hà Nội, Phường Linh Trung, TP. Thủ Đức, Việt Nam',
     FALSE,TRUE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',
     10.86634,106.80126,NULL,NULL,NULL,NOW(),NOW(),NULL),  -- :contentReference[oaicite:14]{index=14}
    ('d6e7f8a9-b0c1-d2e3-f4a5-b6c7d8e9f0a1','L1-14','Bến xe Suối Tiên','Suoi Tien Terminal',
     'Xa lộ Hà Nội, Long Bình, TP. Thủ Đức & Phường Bình Thắng, TP. Dĩ An, Bình Dương',
     FALSE,TRUE,'c29464b1-6b74-4cde-9e9c-51bf0ecc522f',
     10.87952,106.81411,NULL,NULL,NULL,NOW(),NOW(),NULL);  -- :contentReference[oaicite:15]{index=15}

-- ───────────────────────────────────────────────────────────────────────────────
-- Routes between each adjacent station (13 segments)
-- ───────────────────────────────────────────────────────────────────────────────
INSERT INTO public."Routes"
("Id","RouteCode","LineId","FromStationId","ToStationId","RouteNameVi","RouteNameEn","Direction",
 "SeqOrder","TravelTimeMin","LengthKm",
 "CreatedBy","LastUpdatedBy","DeletedBy","CreatedAt","LastUpdatedAt","DeletedAt")
VALUES
    -- Forward (Direction = 0)
    ('fa1d2c3b-4e5f-6a7b-8c9d-e0f1a2b3c4d5','L1-01-02','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '5e9f3c27-8d6b-4c44-9a14-dfd6a3e2e8f0','1a2b3c4d-5e6f-7081-92a3-b4c5d6e7f8a9',
     'Bến Thành – Nhà hát Thành phố','Ben Thanh – Opera House',0,  1,2, 0.715, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('1b2c3d4e-5f6a-7b8c-9d0e-f1a2b3c4d5e6','L1-02-03','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '1a2b3c4d-5e6f-7081-92a3-b4c5d6e7f8a9','2b3c4d5e-6f70-8192-a3b4-c5d6e7f8a9b0',
     'Nhà hát Thành phố – Ba Son','Opera House – Ba Son',           0,  2, 2,0.991, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('2c3d4e5f-6a7b-8c9d-0e1f-a2b3c4d5e6f7','L1-03-04','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '2b3c4d5e-6f70-8192-a3b4-c5d6e7f8a9b0','3c4d5e6f-7081-92a3-b4c5-d6e7f8a9b0c1',
     'Ba Son – Công viên Văn Thánh','Ba Son – Van Thanh Park',     0,  3, 3,1.814, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('3d4e5f6a-7b8c-9d0e-1f2a-b3c4d5e6f7a8','L1-04-05','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '3c4d5e6f-7081-92a3-b4c5-d6e7f8a9b0c1','4d5e6f70-8192-a3b4-c5d6-e7f8a9b0c1d2',
     'Công viên Văn Thánh – Tân Cảng','Van Thanh Park – Tan Cang',0,  4, 2,0.918, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('4e5f6a7b-8c9d-0e1f-2a3b-c4d5e6f7a8b9','L1-05-06','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '4d5e6f70-8192-a3b4-c5d6-e7f8a9b0c1d2','5e6f7081-92a3-b4c5-d6e7-f8a9b0c1d2e3',
     'Tân Cảng – Thảo Điền','Tan Cang – Thao Dien',                0,  5, 2,1.158, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('5f6a7b8c-9d0e-1f2a-3b4c-d5e6f7a8b9c0','L1-06-07','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '5e6f7081-92a3-b4c5-d6e7-f8a9b0c1d2e3','6f708192-a3b4-c5d6-e7f8-a9b0c1d2e3f4',
     'Thảo Điền – An Phú','Thao Dien – An Phu',                  0,  6, 2,0.957, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('6a7b8c9d-0e1f-2a3b-4c5d-e6f7a8b9c0d1','L1-07-08','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '6f708192-a3b4-c5d6-e7f8-a9b0c1d2e3f4','708192a3-b4c5-d6e7-f8a9-b0c1d2e3f4a5',
     'An Phú – Rạch Chiếc','An Phu – Rach Chiec',              0,  7, 3,1.654, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('7b8c9d0e-1f2a-3b4c-5d6e-f7a8b9c0d1e2','L1-08-09','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '708192a3-b4c5-d6e7-f8a9-b0c1d2e3f4a5','8192a3b4-c5d6-e7f8-a9b0-c1d2e3f4a5b6',
     'Rạch Chiếc – Phước Long','Rach Chiec – Phuoc Long',        0,  8,3, 1.466, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('8c9d0e1f-2b3c-4d5e-6f7a-8b9c0d1e2f3a','L1-09-10','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '8192a3b4-c5d6-e7f8-a9b0-c1d2e3f4a5b6','92a3b4c5-d6e7-f8a9-b0c1-d2e3f4a5b6c7',
     'Phước Long – Bình Thái','Phuoc Long – Binh Thai',         0,  9, 3,1.393, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('9d0e1f2a-3b4c-5d6e-7f8a-b9c0d1e2f3a4','L1-10-11','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '92a3b4c5-d6e7-f8a9-b0c1-d2e3f4a5b6c7','a3b4c5d6-e7f8-a9b0-c1d2-e3f4a5b6c7d8',
     'Bình Thái – Thủ Đức','Binh Thai – Thu Duc',              0, 10, 3,1.744, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('0e1f2a3b-4c5d-6e7f-8a9b-c0d1e2f3a4b5','L1-11-12','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     'a3b4c5d6-e7f8-a9b0-c1d2-e3f4a5b6c7d8','b4c5d6e7-f8a9-b0c1-d2e3-f4a5b6c7d8e9',
     'Thủ Đức – Hi-Tech Park','Thu Duc – Hi-Tech Park',       0, 11, 3,2.380, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('1f2a3b4c-5d6e-7f8a-9b0c-d1e2f3a4b5c6','L1-12-13','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     'b4c5d6e7-f8a9-b0c1-d2e3-f4a5b6c7d8e9','c5d6e7f8-a9b0-c1d2-e3f4-a5b6c7d8e9f0',
     'Hi-Tech Park – Đại học Quốc Gia','Hi-Tech Park – National University',  0, 12, 3,1.575, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('2a3b4c5d-6e7f-8a9b-0c1d-e2f3a4b5c6d7','L1-13-14','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     'c5d6e7f8-a9b0-c1d2-e3f4-a5b6c7d8e9f0','d6e7f8a9-b0c1-d2e3-f4a5-b6c7d8e9f0a1',
     'Đại học Quốc Gia – Bến xe Suối Tiên','National University – Suoi Tien Terminal', 0, 13, 3,2.056, NULL,NULL,NULL,NOW(),NOW(),NULL),

    -- Reverse (Direction = 1)
    ('ca14f6bb-71a8-4f9b-8c15-1d2b8992c437','L1-14-13','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     'd6e7f8a9-b0c1-d2e3-f4a5-b6c7d8e9f0a1','c5d6e7f8-a9b0-c1d2-e3f4-a5b6c7d8e9f0',
     'Bến xe Suối Tiên – Đại học Quốc Gia','Suoi Tien Terminal – National University', 1,  1, 3,2.056, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('f053780b-1c99-45b4-9cb7-e8daea0a90d8','L1-13-12','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     'c5d6e7f8-a9b0-c1d2-e3f4-a5b6c7d8e9f0','b4c5d6e7-f8a9-b0c1-d2e3-f4a5b6c7d8e9',
     'Đại học Quốc Gia – Hi-Tech Park','National University – Hi-Tech Park',         1,  2, 3,1.575, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('9020fe12-5630-46c1-9f10-cd9c3f919e3e','L1-12-11','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     'b4c5d6e7-f8a9-b0c1-d2e3-f4a5b6c7d8e9','a3b4c5d6-e7f8-a9b0-c1d2-e3f4a5b6c7d8',
     'Hi-Tech Park – Thủ Đức','Hi-Tech Park – Thu Duc',            1,  3, 3,2.380,NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('e881211f-390c-4488-ab8f-d93a9e204372','L1-11-10','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     'a3b4c5d6-e7f8-a9b0-c1d2-e3f4a5b6c7d8','92a3b4c5-d6e7-f8a9-b0c1-d2e3f4a5b6c7',
     'Thủ Đức – Bình Thái','Thu Duc – Binh Thai',                1,  4, 3,1.744,NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('a4b2bc3f-a169-4c09-aadb-e4625d04fc8c','L1-10-09','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '92a3b4c5-d6e7-f8a9-b0c1-d2e3f4a5b6c7','8192a3b4-c5d6-e7f8-a9b0-c1d2e3f4a5b6',
     'Bình Thái – Phước Long','Binh Thai – Phuoc Long',         1,  5, 3,1.393, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('af4fe5e6-b57d-45d3-8caf-d053cb14ffa9','L1-09-08','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '8192a3b4-c5d6-e7f8-a9b0-c1d2e3f4a5b6','708192a3-b4c5-d6e7-f8a9-b0c1d2e3f4a5',
     'Phước Long – Rạch Chiếc','Phuoc Long – Rach Chiec',       1,  6, 3,1.466, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('eae9c4c1-1239-4dcd-ae22-5f4266972add','L1-08-07','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '708192a3-b4c5-d6e7-f8a9-b0c1d2e3f4a5','6f708192-a3b4-c5d6-e7f8-a9b0c1d2e3f4',
     'Rạch Chiếc – An Phú','Rach Chiec – An Phu',               1,  7, 3,1.654, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('49d3b912-8f57-4323-af0a-3d02f5e941e6','L1-07-06','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '6f708192-a3b4-c5d6-e7f8-a9b0c1d2e3f4','5e6f7081-92a3-b4c5-d6e7-f8a9b0c1d2e3',
     'An Phú – Thảo Điền','An Phu – Thao Dien',                1,  8, 2,0.957, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('0c4cc394-fc83-494b-80c5-04b9d2ecc2aa','L1-06-05','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '5e6f7081-92a3-b4c5-d6e7-f8a9b0c1d2e3','4d5e6f70-8192-a3b4-c5d6-e7f8a9b0c1d2',
     'Thảo Điền – Tân Cảng','Thao Dien – Tan Cang',            1,  9, 2,1.158, NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('9bbf80e5-26a8-4aa8-9860-5abad379917b','L1-05-04','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '4d5e6f70-8192-a3b4-c5d6-e7f8a9b0c1d2','3c4d5e6f-7081-92a3-b4c5-d6e7f8a9b0c1',
     'Tân Cảng – Công viên Văn Thánh','Tan Cang – Van Thanh Park',1, 10,2,0.918,NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('9ab91376-fa23-493b-9ca7-0adc65818357','L1-04-03','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '3c4d5e6f-7081-92a3-b4c5-d6e7f8a9b0c1','2b3c4d5e-6f70-8192-a3b4-c5d6e7f8a9b0',
     'Công viên Văn Thánh – Ba Son','Van Thanh Park – Ba Son',1, 11,3,1.814,NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('2f146d84-60b2-4d4c-b609-856f89cfcd0b','L1-03-02','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '2b3c4d5e-6f70-8192-a3b4-c5d6e7f8a9b0','1a2b3c4d-5e6f-7081-92a3-b4c5d6e7f8a9',
     'Ba Son – Nhà hát Thành phố','Ba Son – Opera House',        1, 12,2,0.991,NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('1c07cfde-3ef9-4add-8289-37cff5941a5e','L1-02-01','e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     '1a2b3c4d-5e6f-7081-92a3-b4c5d6e7f8a9','5e9f3c27-8d6b-4c44-9a14-dfd6a3e2e8f0',
     'Nhà hát Thành phố – Bến Thành','Opera House – Ben Thanh', 1, 13,2,0.715,NULL,NULL,NULL,NOW(),NOW(),NULL);

-- ───────────────────────────────────────────────────────────────────────────────
-- Default daily time slot for Line 1
-- ───────────────────────────────────────────────────────────────────────────────
INSERT INTO public."MetroTimeSlots"
("Id","DayOfWeek","SpecialDate","IsAbnormal","OpenTime","CloseTime","Shift",
"CreatedBy","LastUpdatedBy","DeletedBy","CreatedAt","LastUpdatedAt","DeletedAt")
VALUES
    ('a1b2c3d4-e5f6-7a8b-9c0d-e1f2a3b4c5d6',NULL,NULL,FALSE,'08:00:00','11:00:00',1,NULL,NULL,NULL,NOW(),NOW(),NULL),  -- Morning
    ('b2c3d4e5-f6a7-8b9c-0d1e-f2a3b4c5d6e7',NULL,NULL,FALSE,'13:00:00','16:00:00',2,NULL,NULL,NULL,NOW(),NOW(),NULL),  -- Afternoon
    ('c3d4e5f6-a7b8-9c0d-1e2f-a3b4c5d6e7f8',NULL,NULL,FALSE,'18:00:00','21:00:00',3,NULL,NULL,NULL,NOW(),NOW(),NULL),  -- Evening
    ('d4e5f6a7-b8c9-0d1e-2f3a-b4c5d6e7f8a9',NULL,NULL,FALSE,'22:00:00','03:00:00',4,NULL,NULL,NULL,NOW(),NOW(),NULL); -- Night

-- ───────────────────────────────────────────────────────────────────────────────
-- Link Line 1 to its default time slot
-- ───────────────────────────────────────────────────────────────────────────────
INSERT INTO public."MetroBasePrices"
("Id","LineId","TimeSlotId","BasePriceVndPerKm","IsActive",
 "CreatedBy","LastUpdatedBy","DeletedBy","CreatedAt","LastUpdatedAt","DeletedAt")
VALUES
    -- Morning (08:00–11:00) – full base price
    ('d5f6a7b8-c9d0-1e2f-3a4b-5c6d7e8f9a0b',
     'e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     'a1b2c3d4-e5f6-7a8b-9c0d-e1f2a3b4c5d6',
     5000.00, TRUE,
     NULL, NULL, NULL,
     NOW(), NOW(), NULL),

    -- Afternoon (13:00–16:00) – full base price
    ('e6f7a8b9-c0d1-2e3f-4a5b-6c7d8e9f0a1b',
     'e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     'b2c3d4e5-f6a7-8b9c-0d1e-f2a3b4c5d6e7',
     5000.00, TRUE,
     NULL, NULL, NULL,
     NOW(), NOW(), NULL),

    -- Evening (18:00–21:00) – full base price
    ('f7a8b9c0-d1e2-3f4a-5b6c-7d8e9f0a1b2c',
     'e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     'c3d4e5f6-a7b8-9c0d-1e2f-a3b4c5d6e7f8',
     5000.00, TRUE,
     NULL, NULL, NULL,
     NOW(), NOW(), NULL),

    -- Night (22:00–03:00) – 20% discount
    ('a8b9c0d1-e2f3-4a5b-6c7d-8e9f0a1b2c3d',
     'e4d1d6b2-4f3a-4de0-9efa-8c7f9f1a0b1c',
     'd4e5f6a7-b8c9-0d1e-2f3a-b4c5d6e7f8a9',
     4000.00, TRUE,
     NULL, NULL, NULL,
     NOW(), NOW(), NULL);

-- ───────────────────────────────────────────────────────────────────────────────
-- Parcel Category
-- ───────────────────────────────────────────────────────────────────────────────
INSERT INTO public."ParcelCategories"
("Id","CategoryName","Description","IsBulk","WeightLimitKg","VolumeLimitCm3",
 "LengthLimitCm","WidthLimitCm","HeightLimitCm","IsActive",
 "CreatedBy","LastUpdatedBy","DeletedBy","CreatedAt","LastUpdatedAt","DeletedAt")
VALUES
    -- Hàng hóa chung / General cargo
    ('0a1b2c3d-4e5f-6789-abcd-0fedcba98765',
     'Hàng hóa chung',
     'Hàng bình thường, không cần xử lý đặc biệt',
     FALSE, NULL,   NULL,
     NULL, NULL, NULL,
     TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL),

    -- Hàng nguy hiểm / Dangerous goods
    ('1b2c3d4e-5f6a-7890-bcde-1fedcba98765',
     'Hàng nguy hiểm',
     'Vật phẩm nguy hiểm, tuân thủ quy định an toàn',
     FALSE, NULL,   NULL,
     NULL, NULL, NULL,
     TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL),

    -- Động vật sống / Live animals
    ('2c3d4e5f-6a7b-8901-cdef-2fedcba98765',
     'Động vật sống',
     'Vận chuyển động vật theo quy định chuyên biệt',
     FALSE, NULL,   NULL,
     NULL, NULL, NULL,
     TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL),

    -- Hàng dễ hư hỏng / Perishable goods
    ('3d4e5f6a-7b8c-9012-def0-3fedcba98765',
     'Hàng dễ hư hỏng',
     'Hàng cần bảo quản lạnh hoặc kiểm soát nhiệt độ',
     FALSE, NULL,   NULL,
     NULL, NULL, NULL,
     TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL),

    -- Hàng giá trị / Valuable goods
    ('4e5f6a7b-8c9d-0123-ef01-4fedcba98765',
     'Hàng giá trị',
     'Hàng giá trị cao, cần bảo mật và giám sát',
     FALSE, NULL,   NULL,
     NULL, NULL, NULL,
     TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL),

    -- Hành lý tiêu chuẩn / Standard luggage
    ('5f6a7b8c-9d0e-1234-f012-5fedcba98765',
     'Hành lý tiêu chuẩn',
     'Hành lý của khách: ≤70 kg và 100×60×25 cm',
     FALSE, 70.00, NULL,
     100.00,60.00,25.00,
     TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL),

    -- Hàng cồng kềnh / Bulky article
    ('6a7b8c9d-0e1f-2345-0123-6fedcba98765',
     'Hàng cồng kềnh',
     'Vượt 100 kg hoặc kích thước >100×100×70 cm',
     TRUE, 100.00, NULL,
     100.00,100.00,70.00,
     TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL),

    -- Hàng pallet / Pallet shipment
    ('7b8c9d0e-1f2a-3456-1234-7fedcba98765',
     'Hàng pallet',
     'Đóng pallet: ≤2500 kg và 240×180×220 cm',
     TRUE,2500.00, NULL,
     240.00,180.00,220.00,
     TRUE,
     NULL,NULL,NULL,NOW(),NOW(),NULL);

INSERT INTO public."SystemConfigs"
("Id","ConfigKey","ConfigValue","Description","ConfigType",
 "CreatedBy","LastUpdatedBy","DeletedBy","CreatedAt","LastUpdatedAt","DeletedAt")
VALUES
    ('a3f5b9e1-4c12-4fa8-9b2a-7d5e1c2f3a4b','CONFIRMATION_HOUR','24','Số giờ cho phép xác nhận đơn hàng',3,
     NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('b4d6c8f2-5d23-5gb9-0c3b-8e6f2d3g4h5c','PAYMENT_REQUEST_HOUR','24','Số giờ yêu cầu thanh toán',3,
     NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('c5e7d9g3-6e34-6hc0-1d4c-9f7g3e4h5i6d','ALLOW_CANCEL_BEFORE_HOUR','24','Cho phép hủy trước bao nhiêu giờ',3,
     NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('d6f8e0h4-7f45-7id1-2e5d-0g8h4f5i6j7e','REFUND_PERCENT','80','Phần trăm hoàn tiền khi hủy',2,
     NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('e7g9f1i5-8g56-8je2-3f6e-1h9i5g6j7k8f','SURCHARGE_AFTER_DELIVERED_HOUR','48','Phụ phí sau thời gian giao',3,
     NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('f8h0g2j6-9h67-9kf3-4g7f-2i0j6h7k8l9g','SURCHARGE_PER_DAY_VND','10000','Phụ phí lưu kho mỗi ngày (VND)',2,
     NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('2e9e0869-85e5-4c23-bedc-126c27f50076','MAX_SCHEDULE_SHIPMENT_DAY','15','Số ngày tối đa khách có thể đặt trước cho kiện hàng',3,
     NULL,NULL,NULL,NOW(),NOW(),NULL),
    ('09i1h3k7-0i78-0lg4-5h8g-3j1k7i8l9m0h','FREE_STORAGE_AFTER_DAY','30','Số ngày thanh lý hàng tồn kho',3,
     NULL,NULL,NULL,NOW(),NOW(),NULL);

/*INSERT INTO public."SystemConfigs"
("Id","ConfigKey","ConfigValue","Description","ConfigType",
 "CreatedBy","LastUpdatedBy","DeletedBy","CreatedAt","LastUpdatedAt","DeletedAt") VALUES
    ('2e9e0869-85e5-4c23-bedc-126c27f50076','MAX_SCHEDULE_SHIPMENT_DAY','15','Số ngày tối đa khách có thể đặt trước cho kiện hàng',3,
    NULL,NULL,NULL,NOW(),NOW(),NULL);*/

-- ───────────────────────────────────────────────────────────────────────────────
-- insert default user
-- ───────────────────────────────────────────────────────────────────────────────
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
        '2025-05-19T15:48:50.1689055+00:00', NULL, NULL,
        'admin@example.com', FALSE, 'Admin User', NULL,
        '2025-05-19T15:48:50.1689055+00:00',
        FALSE, NULL, 'ADMIN', 'ADMIN',
        NULL, '$2a$12$xGMlYdHQXreJRAsPVtWVueg2x0MFDeOl472DgWkhKJ.fFX5gvsY5m',
        NULL, FALSE, FALSE,
        'admin', '2025-05-19T15:48:50.1689062+00:00'
    ),
    (
        '64b4c4c2-48ab-4a97-af1c-b25f1aa86362', 0, NULL, NULL, NULL,
        'https://via.placeholder.com/150', NULL, NULL,
        '9f6e38a8-f982-46fb-b031-0e717614d8eb', NULL,
        '2025-05-19T15:48:50.1689069+00:00', NULL, NULL,
        'staff@example.com', FALSE, 'Staff', NULL,
        '2025-05-19T15:48:50.1689069+00:00',
        FALSE, NULL, 'STAFF', 'STAFF',
        NULL, '$2a$12$xGMlYdHQXreJRAsPVtWVueg2x0MFDeOl472DgWkhKJ.fFX5gvsY5m',
        NULL, FALSE, FALSE,
        'staff', '2025-05-19T15:48:50.1689071+00:00'
    );

-- Seed UserRoles
INSERT INTO public."UserRoles"
("RoleId","UserId","Discriminator")
VALUES
    ('021239ef-1d1f-4676-a402-aff9fd24c0c8','5ca7e417-15c3-4bf4-b31c-d57b861e4ab3','UserRoleEntity'),
    ('d76a73b5-f94f-4fc9-9009-00a8f1716b7d','64b4c4c2-48ab-4a97-af1c-b25f1aa86362','UserRoleEntity');


