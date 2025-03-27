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

Set Up Steps 

Creating a serverless API using Azure that leverages Service Bus to communicate with an SQL Database involves several steps. Here's a high-level overview of how you can set this up:

1. **Set Up Azure SQL Database**:
   - Create an Azure SQL Database instance.
   - Set up the necessary tables and schemas you'll need for your application.

2. **Create Azure Service Bus**:
   - Set up an Azure Service Bus namespace.
   - Within the namespace, create a queue or topic (based on your requirement).

3. **Deploy Serverless API using Azure Functions**:
   - Create a new Azure Function App.
   - Develop an HTTP-triggered function that will act as your API endpoint.
   - In this function, when data is received, send a message to the Service Bus queue or topic.

4. **Deploy 2 Service Bus Triggered Function**:
   - Create another Azure Function that is triggered by the Service Bus queue or topic.
   - This function will read the message from the Service Bus and process it. The processing might involve parsing the message and inserting the data into the Azure SQL Database.

5. **Deploy a Timer Triggered Function**:
   - Create another Azure Function that is triggered when a file is dropped in a container.
   - This function will stream in a file, read it and place on the service bus topic.

6. **Implement Error Handling**:
   - Ensure that you have error handling in place. If there's a failure in processing the message and inserting it into the database, you might want to log the error or move the message to a dead-letter queue.

7. **Secure Your Functions**:
   - Ensure that your HTTP-triggered function (API endpoint) is secured, possibly using Azure Active Directory or function keys.

8. **Optimize & Monitor**:
   - Monitor the performance of your functions using Azure Monitor and Application Insights.
   - Optimize the performance, scalability, and cost by adjusting the function's plan (Consumption Plan, Premium Plan, etc.) and tweaking the configurations.

9. **Deployment**:
   - Deploy your functions to the Azure environment. You can use CI/CD pipelines using tools like Azure DevOps or GitHub Actions for automated deployments.

By following these steps, you'll have a serverless API in Azure that uses Service Bus as a mediator to process data and store it in an SQL Database. This architecture ensures decoupling between data ingestion and processing, adding a layer of resilience and scalability to your solution.


## Appplication Setting 

|Key|Value | Comment|
|:----|:----|:----|
|AzureWebJobsStorage|[CONNECTION STRING]|RECOMMENDATION :  store in AzureKey Vault.|
|ConfigurationPath| [CONFIGURATION FOLDER PATH] |Folder is optional
|ApiKeyName|[API KEY NAME]|Will be passed in the header  :  the file name of the config.
|AppName| [APPLICATION NAME]| This is the name of the Function App, used in log analytics|
|StorageAcctName|[STORAGE ACCOUNT NAME]|Example  "AzureWebJobsStorage"|
|ServiceBusConnectionString|[SERVICE BUS CONNECTION STRING]|Example  "ServiceBusConnectionString".  Recommmended to store in Key vault.|
|DatabaseConnection|[DATABASE CONNECTION STRING]|Example  "DatabaseConnection". Recommmended to store in Key vault.|
|TimerInterval|[TIMER_INTERVAL]|Example  "0 */1 * * * *" 1 MIN|


> **Note:**  Look at the configuration file in the **Config** Folder and created a Table to record information.

## Configuration Files 

> **Note:** The **Configuration** is located in the  FunctionApp  in a **Config** Folder.

|FileName|Description|
|:----|:----|
|99F77BEF300E4660A63A939ADD0BCF68.json| **Upload File** Parse CSV file --> Write Batched Files To Storage|
|43EFE991E8614CFB9EDECF1B0FDED37A.json| **File Parser** Parse CSV file --> File received from SFTP will use this process to parse files|
|43EFE991E8614CFB9EDECF1B0FDED37D.json| **Upload File** Parse JSON/CSV Directly to NO SQL DB|
|43EFE991E8614CFB9EDECF1B0FDED37C.json| **Service Bus Trigger for SQL DB** | Receive JSON payload and insert into SQL DB|
|43EFE991E8614CFB9EDECF1B0FDED37F.json| **Service Bus Trigger for No SQL DB** | Receive JSON payload and insert into NO SQL DB|
|43EFE991E8614CFB9EDECF1B0FDED37E.json| **Blob Trigger** Send parsed/sharded file  to Send to Service Bus|
|43EFE991E8614CFB9EDECF1B0FDED37B.json| **Search Resullt from NO SQLDB** |
|43EFE991E8614CFB9EDECF1B0FDED37G.json| **Search SQL DB. Return resultset** |
|3FB620B0E0FD4E8F93C9E4D839D00E1E.json| **Copy File from SFTP into the pickup folder** |
|3FB620B0E0FD4E8F93C9E4D839D00E1F.json| **Create a new Record in NoSQL Database** |
|CC244934898F46789734A9437B6F76CA.json| Encode Payload Request |
|6B427917E36A4DA281D57F9A64AD9D55.json| Get reports from DB  |


> Create the following blob containers and share in azure storage

|ContainerName|Description|
|:----|:----|
|config|Location for the configuration files|
|pickup|Thes are files that are copied from the SFTP share and dropped in the pickup container |
|processed|These are files the have been parsed and dropped in th processed container|

|Table|Description|
|:----|:----|
|csvbatchfiles|Track the CSV parsed files|
|training[YYYYMMDD]|N0 SQL DataStore|


|Share|Description|
|:----|:----|
|training[YYYYMMDD]|Create a share location for SFTP to drop files|

## Service Bus Subscription information

|Subscription Name|Description|
|:----|:----|
|request|Create a Topic|
|nosqlmessage|Create a Subscription|
|sqlmessage|Create a Subscription|



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
