-- Create database (jika belum ada)
IF DB_ID('assessmentdb') IS NULL
BEGIN
    CREATE DATABASE assessmentdb;
END;
GO

USE assessmentdb;
GO

-- Table ltcourierfee
IF OBJECT_ID('ltcourierfee', 'U') IS NOT NULL
    DROP TABLE ltcourierfee;
GO
CREATE TABLE ltcourierfee (
    WeightID int NOT NULL,
    CourierID int NOT NULL,
    StartKg int NOT NULL,
    EndKg int NULL,
    Price decimal(10,0) NULL
);
GO

INSERT INTO ltcourierfee (WeightID, CourierID, StartKg, EndKg, Price) VALUES
(1, 1, 1, 2, 8000),
(2, 1, 3, 4, 9500),
(3, 2, 1, 2, 7500),
(4, 2, 3, 4, 8500),
(5, 3, 1, 2, 10000),
(6, 3, 3, 4, 10000),
(7, 1, 5, 10, 10500),
(8, 2, 5, 10, 9500),
(9, 3, 5, 10, 12000);
GO

-- Table mscourier
IF OBJECT_ID('mscourier', 'U') IS NOT NULL
    DROP TABLE mscourier;
GO
CREATE TABLE mscourier (
    CourierID int NOT NULL PRIMARY KEY,
    CourierName varchar(50) NOT NULL
);
GO

INSERT INTO mscourier (CourierID, CourierName) VALUES
(1, 'JNE'),
(2, 'J&T'),
(3, 'Wahana');
GO

-- Table mspayment
IF OBJECT_ID('mspayment', 'U') IS NOT NULL
    DROP TABLE mspayment;
GO
CREATE TABLE mspayment (
    PaymentID int NOT NULL PRIMARY KEY,
    PaymentName varchar(50) NOT NULL
);
GO

INSERT INTO mspayment (PaymentID, PaymentName) VALUES
(1, 'Cash'),
(2, 'COD');
GO

-- Table msproduct
IF OBJECT_ID('msproduct', 'U') IS NOT NULL
    DROP TABLE msproduct;
GO
CREATE TABLE msproduct (
    ProductID int NOT NULL PRIMARY KEY,
    ProductName varchar(50) NOT NULL,
    Weight float NOT NULL,
    Price decimal(10,0) NOT NULL
);
GO

INSERT INTO msproduct (ProductID, ProductName, Weight, Price) VALUES
(1, 'Tepung', 1.5, 10000),
(7, 'Bluband', 0.25, 8000),
(9, 'Beras', 1, 64000),
(10, 'Eskrim', 0.5, 20000),
(11, 'Kentang', 1, 15000);
GO

-- Table mssales
IF OBJECT_ID('mssales', 'U') IS NOT NULL
    DROP TABLE mssales;
GO
CREATE TABLE mssales (
    SalesID int NOT NULL PRIMARY KEY,
    SalesName varchar(50) NOT NULL
);
GO

INSERT INTO mssales (SalesID, SalesName) VALUES
(1, 'Andy'),
(2, 'Jessica');
GO

-- Table trinvoice
IF OBJECT_ID('trinvoice', 'U') IS NOT NULL
    DROP TABLE trinvoice;
GO
CREATE TABLE trinvoice (
    InvoiceNo varchar(10) NOT NULL PRIMARY KEY,
    InvoiceDate datetime NOT NULL,
    InvoiceTo varchar(500) NOT NULL,
    ShipTo varchar(500) NOT NULL,
    SalesID int NOT NULL,
    CourierID int NOT NULL,
    PaymentType int NOT NULL,
    CourierFee decimal(10,0) NOT NULL
);
GO

INSERT INTO trinvoice (InvoiceNo, InvoiceDate, InvoiceTo, ShipTo, SalesID, CourierID, PaymentType, CourierFee) VALUES
('IN0001', '2015-06-23', 'Invoice Orang 1', 'Ship Orang 1', 1, 1, 1, 0),
('IN0002', '2019-02-27', 'Invoice Orang 2', 'Ship Orang 2', 2, 2, 2, 0);
GO

-- Table trinvoicedetail
IF OBJECT_ID('trinvoicedetail', 'U') IS NOT NULL
    DROP TABLE trinvoicedetail;
GO
CREATE TABLE trinvoicedetail (
    InvoiceNo varchar(10) NOT NULL,
    ProductID int NOT NULL,
    Weight float NOT NULL,
    Qty smallint NOT NULL,
    Price decimal(10,0) NOT NULL
);
GO

INSERT INTO trinvoicedetail (InvoiceNo, ProductID, Weight, Qty, Price) VALUES
('IN0001', 1, 1.5, 3, 10000),
('IN0001', 7, 0.25, 1, 8000),
('IN0001', 9, 2, 3, 64000),
('IN0002', 7, 0.25, 1, 8000),
('IN0002', 10, 0.5, 3, 20000),
('IN0002', 9, 2, 2, 64000);
GO
