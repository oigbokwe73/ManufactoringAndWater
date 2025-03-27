The client wanted to integrate CDMA based water meter with Azure cloud to create a smart water metering architecture capable to track real-time water consumption and data analytics. The data was to be collected from domestic water meters across US through Automatic Meter Reading (AMR) via CDMA chips and Verizon wireless network. With about 500,000 water meters installed, the core challenge meant collecting water flow readings from meters in minimum time utilized for two-way communication and hence increasing the life of battery powering the smart water meters.

To enable data collection at large with minimized infrastructure cost, the client wanted to implement a cloud based data analytics solution. This system will integrate water meter CDMA chips with cloud based data analytical system creating  a Internet of Things (IoT) environment. This IoT system should utilize Azure Cloud services to integrate with sensors placed on Smart Water Meters providing data analytics to both customers as well as the client to determine water consumption at real-time; screen defaulter from over consumption of water resource; load balancing based on water consumption need in different regions/groups; frame stratagems for saving water resource across the US region.


Here’s a comprehensive breakdown of the **scenario** and the **SQL database schema** you would need to support it.

---

## ✅ **Scenario Overview**

- **Data Source**: 500,000+ CDMA-enabled water meters, transmitting data via Verizon's network through AMR.
- **Objective**: Real-time data collection, long battery life, low latency two-way communication.
- **Solution Goals**:
  - Ingest water consumption data via IoT hub and push to Azure SQL DB.
  - Real-time analytics (alerts, over-consumption).
  - Customer and asset management.
  - Region-based consumption optimization.

---

## 🗂️ SQL Database Tables

### 1. **Customers**

```sql
CREATE TABLE Customers (
    CustomerID INT PRIMARY KEY IDENTITY,
    FirstName NVARCHAR(50),
    LastName NVARCHAR(50),
    Email NVARCHAR(100),
    PhoneNumber NVARCHAR(20),
    Address NVARCHAR(255),
    RegionID INT,
    CreatedDate DATETIME DEFAULT GETDATE()
);
```

---

### 2. **Regions**

```sql
CREATE TABLE Regions (
    RegionID INT PRIMARY KEY IDENTITY,
    RegionName NVARCHAR(100),
    State NVARCHAR(50),
    WaterAllocationLimit DECIMAL(10,2),
    Description NVARCHAR(255)
);
```

---

### 3. **WaterMeters**

```sql
CREATE TABLE WaterMeters (
    MeterID INT PRIMARY KEY IDENTITY,
    SerialNumber NVARCHAR(50) UNIQUE,
    CustomerID INT FOREIGN KEY REFERENCES Customers(CustomerID),
    RegionID INT FOREIGN KEY REFERENCES Regions(RegionID),
    MeterType NVARCHAR(50), -- e.g., CDMA, GSM, LoRaWAN
    InstallationDate DATETIME,
    BatteryLifePercent INT,
    FirmwareVersion NVARCHAR(20),
    IsActive BIT,
    LastMaintenanceDate DATETIME
);
```

---

### 4. **WaterConsumptionReadings**

```sql
CREATE TABLE WaterConsumptionReadings (
    ReadingID BIGINT PRIMARY KEY IDENTITY,
    MeterID INT FOREIGN KEY REFERENCES WaterMeters(MeterID),
    ReadingTimestamp DATETIME,
    WaterVolumeLiters DECIMAL(10,2), -- Water consumed in that time frame
    SignalStrength INT, -- Optional: CDMA signal strength
    BatteryStatus INT, -- Optional: % at time of reading
    SyncStatus NVARCHAR(20), -- e.g., 'Received', 'Processed', 'Error'
    CreatedDate DATETIME DEFAULT GETDATE()
);
```

---

### 5. **Alerts**

```sql
CREATE TABLE Alerts (
    AlertID INT PRIMARY KEY IDENTITY,
    MeterID INT FOREIGN KEY REFERENCES WaterMeters(MeterID),
    AlertType NVARCHAR(50), -- e.g., OverConsumption, LowBattery, NoSignal
    AlertTimestamp DATETIME,
    IsResolved BIT DEFAULT 0,
    ResolutionNotes NVARCHAR(255),
    CreatedDate DATETIME DEFAULT GETDATE()
);
```

---

### 6. **MaintenanceLogs**

```sql
CREATE TABLE MaintenanceLogs (
    LogID INT PRIMARY KEY IDENTITY,
    MeterID INT FOREIGN KEY REFERENCES WaterMeters(MeterID),
    MaintenanceDate DATETIME,
    TechnicianName NVARCHAR(100),
    Notes NVARCHAR(255)
);
```

---

### 7. **ConsumptionThresholds**

```sql
CREATE TABLE ConsumptionThresholds (
    ThresholdID INT PRIMARY KEY IDENTITY,
    RegionID INT FOREIGN KEY REFERENCES Regions(RegionID),
    DailyLimit DECIMAL(10,2), -- Liters
    WeeklyLimit DECIMAL(10,2),
    MonthlyLimit DECIMAL(10,2),
    EffectiveFrom DATETIME,
    EffectiveTo DATETIME
);
```

---

### 8. **MeterCommandsQueue**

```sql
CREATE TABLE MeterCommandsQueue (
    CommandID INT PRIMARY KEY IDENTITY,
    MeterID INT FOREIGN KEY REFERENCES WaterMeters(MeterID),
    CommandType NVARCHAR(50), -- e.g., 'Restart', 'FirmwareUpdate', 'StatusCheck'
    CommandStatus NVARCHAR(20), -- e.g., 'Pending', 'Sent', 'Failed'
    IssuedDate DATETIME,
    ExecutedDate DATETIME NULL
);
```

---

### 9. **UsageAnalytics**

> *(This can be either a materialized summary table or a view)*

```sql
CREATE VIEW DailyUsageSummary AS
SELECT 
    MeterID,
    CONVERT(DATE, ReadingTimestamp) AS UsageDate,
    SUM(WaterVolumeLiters) AS TotalDailyUsage
FROM WaterConsumptionReadings
GROUP BY MeterID, CONVERT(DATE, ReadingTimestamp);
```

---

## 🔄 **IoT Flow (Simplified)**

1. **Meter reading is transmitted** → CDMA → Verizon → IoT Hub.
2. **Azure Function / Stream Analytics** processes message:
   - Logs water usage to `WaterConsumptionReadings`.
   - Triggers alerts into `Alerts` if usage > threshold.
   - Updates battery % in `WaterMeters`.
3. **Power BI dashboards**:
   - Pull from `DailyUsageSummary`, `Alerts`, and `Regions`.

---

Would you like a Mermaid diagram of the data flow and schema for visual aid?
