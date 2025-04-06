using Contract.Repositories.Entity;
using Contract.Repositories.Interface;
using Contract.Services.Interface;
using Core.Utils;
using Firebase.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelViews.BandBrandModelViews;
using ModelViews.HealthRecordModelViews;
using ModelViews.MetricModelViews;
using ModelViews.RecordMetricItemModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Service
{
    public class FirebasePollingServiceV2 : BackgroundService
    {
        private readonly FirebaseClient _firebaseClient;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<FirebasePollingServiceV2> _logger;
        //private readonly BandBrandService _bandBrandService;
        private readonly string _lastKeyFile = Path.Combine(Directory.GetCurrentDirectory(), "Config", "lastkey.txt");
        private string lastKey = "";
        private List<CreateHealthRecordModel> _hourlyRecords = new();
        private DateTime _lastSaveTime = CoreHelper.SystemTimeNows;

        public FirebasePollingServiceV2(IServiceScopeFactory scopeFactory, ILogger<FirebasePollingServiceV2> logger)
        {
            _firebaseClient = new FirebaseClient("https://fhealth-sphere---login-default-rtdb.asia-southeast1.firebasedatabase.app/");
            _scopeFactory = scopeFactory;
            _logger = logger;
            
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("FirebasePollingService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //await PollFirebase();

                    if (CoreHelper.SystemTimeNows - _lastSaveTime >= TimeSpan.FromSeconds(10))
                    {
                        await SaveHourlyData();
                        _lastSaveTime = CoreHelper.SystemTimeNows;
                    }
                    //using var scope = _scopeFactory.CreateScope();
                    //var _bandBrandService = scope.ServiceProvider.GetRequiredService<IBandBrandService>();                    
                    //await _bandBrandService.GetBandBrandById(1);
                    //await _bandBrandService.UpdateBandBrand(1, new UpdateBandBrandModel
                    //{
                    //    Name = "Invite"
                    //});
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi trong ExecuteAsync.");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }

            _logger.LogInformation("FirebasePollingService is stopping.");
        }

        private async Task PollFirebase()
        {
            var newRecords = await CheckForUpdates();
            Console.WriteLine(lastKey);
            if (newRecords != null && newRecords.Any())
            {
                _hourlyRecords.AddRange(newRecords);
                await ToolFireBaseV2(newRecords);
            }
            else Console.WriteLine("NO RECORD");
        }

        private async Task SaveHourlyData()
        {
            if (_hourlyRecords.Any())
            {
                var groupedRecords = _hourlyRecords.GroupBy(r => r.PatientId).ToList();

                foreach (var patientRecords in groupedRecords)
                {
                    var averagedRecord = AverageRecords(patientRecords.ToList());
                    await ProcessDataAsync(averagedRecord);
                    _logger.LogInformation($"Saved averaged data for Patient ID: {averagedRecord.PatientId} to DB.");
                }

                _hourlyRecords.Clear();
            }
        }
        public async Task ProcessDataAsync(CreateHealthRecordModel healthRecord)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                try
                {
                    var RecordMetrics = new List<RecordMetricItem>();
                    foreach (var record in healthRecord.RecordMetricItems)
                    {
                        var model = new RecordMetricItem
                        {
                            MetricId = record.MetricId,
                            Value = record.Value,
                            CreatedTime = CoreHelper.SystemTimeNow,
                            LastUpdatedTime = CoreHelper.SystemTimeNow
                        };
                        RecordMetrics.Add(model);

                    }
                    // Lấy dữ liệu từ Firebase
                    var scannedData = new HealthRecord
                    {
                        PatientId = healthRecord.PatientId,
                        //BandId = healthRecord.BandId, // Nếu bạn có dữ liệu về BandId, hãy gán vào đây
                        GhiChu = "Tổng hợp trung bình chỉ số",
                        CreatedTime = CoreHelper.SystemTimeNow,
                        LastUpdatedTime = CoreHelper.SystemTimeNow,
                        RecordMetricItems = RecordMetrics
                    };
                    // Lưu dữ liệu vào database
                    await unitOfWork.GetRepository<HealthRecord>().InsertAsync(scannedData);
                    await unitOfWork.SaveAsync();

                    _logger.LogInformation("Dữ liệu đã được lưu thành công.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi lưu dữ liệu.");
                }
            }
        }
        private async Task ToolFireBaseV2(List<CreateHealthRecordModel> HealthRecords)
        {

            if (HealthRecords != null && HealthRecords.Any())
            {
                // Tạo từ điển để lưu dữ liệu phân loại theo bệnh nhân
                Dictionary<int?, List<CreateHealthRecordModel>> patientRecords = new();

                foreach (var record in HealthRecords)
                {
                    int? patientId = record.PatientId; // Giả sử có thuộc tính PatientId

                    if (!patientRecords.ContainsKey(patientId))
                    {
                        patientRecords[patientId] = new List<CreateHealthRecordModel>();
                    }

                    patientRecords[patientId].Add(record);
                }
                // Xuất danh sách từng bệnh nhân
                foreach (var patient in patientRecords)
                {
                    Console.WriteLine($"Patient ID: {patient.Key}, Record Count: {patient.Value.Count}");
                    foreach (var record in patient.Value)
                    {
                        decimal diastolic = 0; // tâm trương
                        decimal systolic = 0; // tâm thu
                        decimal heartbeat = 0; // nhịp tim 
                        foreach (var metric in record.RecordMetricItems)
                        {

                            if (metric.MetricId == 1) // Tâm thu
                            {
                                systolic = metric.Value;
                            }
                            else if (metric.MetricId == 2) // Tâm truong
                            {
                                diastolic = metric.Value;
                            }
                            else if (metric.MetricId == 3) // Nhịp tim
                            {
                                heartbeat = metric.Value;
                            }
                        }
                        var ResultList = CheckOverAsync(systolic, diastolic, heartbeat);
                        foreach (var result in ResultList.Result)
                        {
                            //TODO 
                            if (result.Status.Equals("Over"))
                            {
                                Console.WriteLine("User ID: " + record.PatientId + " " + record.BandId + " " + result.CurrentValue);
                                Console.WriteLine(result.MetricName);
                                Console.WriteLine("Note" + record.GhiChu);
                                var _token = await GetFCMToken(record.PatientId);
                                await CallNotification(result, record.PatientId, _token);
                            }
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("No new data found.");
            }
        }
        public async Task<List<CheckResult>> CheckOverAsync(decimal systolic, decimal diastolic, decimal heartbeat)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                try
                {

                    var metric = unitOfWork.GetRepository<Metric>().Entities.Where(n => n.Name.Equals("Tam Thu") || n.Name.Equals("Tam Truong") || n.Name.Equals("Nhip Tim"));
                    List<CheckResult> results = new List<CheckResult>();
                    foreach (Metric mt in metric)
                    {
                        if (mt.Name.Equals("Tam Thu"))
                        {
                            bool isOutOfRange = mt.MinValue > systolic || mt.MaxValue < systolic;
                            results.Add(new CheckResult
                            {
                                MetricName = "Tâm Thu",
                                CurrentValue = systolic,
                                Status = isOutOfRange ? "Over" : "Normal"
                            });
                        }
                        if (mt.Name.Equals("Tam Truong"))
                        {
                            bool isOutOfRange = mt.MinValue > diastolic || mt.MaxValue < diastolic;
                            results.Add(new CheckResult
                            {
                                MetricName = "Tâm Trương",
                                CurrentValue = diastolic,
                                Status = isOutOfRange ? "Over" : "Normal"
                            });
                        }
                        if (mt.Name.Equals("Nhip Tim"))
                        {
                            bool isOutOfRange = mt.MinValue > heartbeat || mt.MaxValue < heartbeat;
                            results.Add(new CheckResult
                            {
                                MetricName = "Nhịp Tim",
                                CurrentValue = heartbeat,
                                Status = isOutOfRange ? "Over" : "Normal"
                            });
                        }
                    }
                    return results;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi lưu dữ liệu.");
                    return new List<CheckResult>
                    {
                        new CheckResult
                        {
                            MetricName = "Lỗi hệ thống",
                            Status = $"Lỗi: {ex.Message}"
                        }
                    };
                }
            }
        }
        public async Task CallNotification(CheckResult result, int? patientId, string FCMToken)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _metricService = scope.ServiceProvider.GetRequiredService<IMetricService>();
                var _notiService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                var title = "Metric Value Alert";
                var message = $"Default value {result.CurrentValue} is out of range for metric {result.MetricName}.";
                await _metricService.SendNotificationAsync(title, message, FCMToken);
                await _notiService.CreateNotification(title, message, patientId);
            }
        }
        public async Task<string> GetFCMToken(int? patientId)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var accounts = unitOfWork.GetRepository<Account>().Entities.FirstOrDefaultAsync(n => n.Id == patientId);
                return accounts.Result.FCMToken;
            }
        }
        private CreateHealthRecordModel AverageRecords(List<CreateHealthRecordModel> records)
        {
            if (!records.Any()) return null;

            var patientId = records.First().PatientId;
            //var bandId = records.First().BandId;

            var averagedMetrics = records
                .SelectMany(r => r.RecordMetricItems)
                .GroupBy(item => item.MetricId)
                .Select(group => new CreateRecordMetricItemModel
                {
                    MetricId = group.Key,
                    Value = group.Average(item => item.Value)
                })
                .ToList();

            return new CreateHealthRecordModel
            {
                PatientId = patientId,
                //BandId = bandId,
                GhiChu = "Averaged hourly data",
                RecordMetricItems = averagedMetrics
            };
        }

        private async Task<List<CreateHealthRecordModel>> CheckForUpdates()
        {
            try
            {
                var data = await _firebaseClient
                    .Child("healthRecords")
                    .OnceAsync<CreateHealthRecordModel>();

                if (string.IsNullOrEmpty(lastKey))
                {
                    //lastKey = await LoadLastKeyAsync();
                    lastKey = LoadLastKeyOnDeploy();
                }

                if (string.IsNullOrEmpty(lastKey))
                {
                    if (data.Any())
                    {
                        lastKey = data.Last().Key;
                        //await SaveLastKeyAsync(lastKey);
                        SaveLastKeyAsync(lastKey);
                    }
                    return data.Select(r => r.Object).ToList();
                }
                else
                {
                    var newData = data.SkipWhile(x => x.Key != lastKey).Skip(1).ToList();

                    if (newData.Any())
                    {
                        lastKey = newData.Last().Key;
                        //await SaveLastKeyAsync(lastKey);
                        SaveLastKeyAsync(lastKey);
                    }

                    return newData.Select(r => r.Object).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching data from Firebase.");
                return new List<CreateHealthRecordModel>();
            }
        }
        //private async Task SaveLastKeyAsync(string lastKey)
        //{
        //    await File.WriteAllTextAsync(_lastKeyFile, lastKey);
        //}

        //private async Task<string> LoadLastKeyAsync()
        //{
        //    if (!File.Exists(_lastKeyFile))
        //    {
        //        Console.WriteLine("File lastkey.txt không tồn tại.");
        //        return string.Empty;
        //    }

        //    using (var reader = new StreamReader(_lastKeyFile))
        //    {
        //        var key = await reader.ReadToEndAsync();
        //        Console.WriteLine($"LastKey loaded: {key}");
        //        return key.Trim();
        //    }
        //}
        //private async Task<string> LoadLastKeyOnDeploy()
        //{
        //    try
        //    {
        //        if (File.Exists(_lastKeyFile))
        //        {
        //            return await File.ReadAllTextAsync(_lastKeyFile);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error loading last key on deploy.");
        //    }

        //    return string.Empty;
        //}

        private string LoadLastKeyOnDeploy()
        {
            return Environment.GetEnvironmentVariable("LAST_KEY") ?? string.Empty;
        }

        private void SaveLastKeyAsync(string key)
        {
            Environment.SetEnvironmentVariable("LAST_KEY", key);
        }
    }
}
