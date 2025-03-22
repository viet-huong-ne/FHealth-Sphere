using Contract.Repositories.Entity;
using Contract.Repositories.Interface;
using FHealthSphere.Services.Services;
using Firebase.Database;
using Firebase.Database.Query;
using Google.Apis.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using ModelViews.HealthRecordModelViews;
using ModelViews.MetricModelViews;
using ModelViews.RecordMetricItemModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Services.Service
{
    public class FirebasePollingService
    {
        private readonly FirebaseClient _firebaseClient;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<HealthRecordService> _logger;
        private readonly Timer _timer;
        private string lastKey = "-OLtOFFi7AvzNmuNVWTU";
        public FirebasePollingService(IServiceScopeFactory scopeFactory, ILogger<HealthRecordService> logger)
        {
            _firebaseClient = new FirebaseClient("https://fhealth-sphere---login-default-rtdb.asia-southeast1.firebasedatabase.app/");
            _timer = new Timer(async _ => await ToolFireBase(), null, TimeSpan.Zero, TimeSpan.FromMinutes(3)); // Quét mỗi 5 p
            _scopeFactory = scopeFactory;
            _logger = logger;
        }
        private async Task<List<CreateHealthRecordCombinedModel>> CheckForUpdates()
        {
            try
            {
                // Lấy toàn bộ dữ liệu
                var data = await _firebaseClient
           .Child("healthRecords")
           .OnceAsync<CreateHealthRecordCombinedModel>();
                if (string.IsNullOrEmpty(lastKey))
                {
                    if (data.Any())
                    {
                        lastKey = data.Last().Key; // Lưu lại key cuối cùng
                        Console.WriteLine($"First scan - LastKey set: {lastKey}");
                    }

                    return data.Select(r => r.Object).ToList();
                }
                else
                {
                    //// Lấy dữ liệu từ Firebase bắt đầu từ lastKey
                    //var data1 = await _firebaseClient
                    //    .Child("healthRecords")
                    //    .OrderByKey()
                    //    .StartAt(lastKey) // Lấy từ lastKey trở đi
                    //    .OnceAsync<CreateHealthRecordCombinedModel>();

                    //// Bỏ qua phần tử đầu tiên (vì đó là lastKey của lần trước)
                    //var newData = data1.Skip(1).ToList();
                    // Lọc ra các phần tử mới dựa trên lastKey
                    var newData = data
                        .SkipWhile(x => x.Key != lastKey) // Bỏ qua các phần tử cũ
                        .Skip(1) // Bỏ luôn phần tử có lastKey
                        .ToList();
                    if (newData.Any())
                    {
                        lastKey = newData.Last().Key; // Cập nhật lastKey với bản ghi mới nhất
                        Console.WriteLine($"New scan - LastKey updated: {lastKey}");
                    }

                    return newData.Select(r => r.Object).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching data: {ex.Message}");
                return new List<CreateHealthRecordCombinedModel>(); // Trả về danh sách rỗng nếu lỗi
            }

        }
        private async Task ToolFireBase()
        {
            var HealthRecords = CheckForUpdates().Result;

            if (HealthRecords != null && HealthRecords.Any())
            {
                // Tạo từ điển để lưu dữ liệu phân loại theo bệnh nhân
                Dictionary<int?, List<CreateHealthRecordCombinedModel>> patientRecords = new();

                foreach (var record in HealthRecords)
                {
                    int? patientId = record.PatientId; // Giả sử có thuộc tính PatientId

                    if (!patientRecords.ContainsKey(patientId))
                    {
                        patientRecords[patientId] = new List<CreateHealthRecordCombinedModel>();
                    }

                    patientRecords[patientId].Add(record);
                }
                // Xuất danh sách từng bệnh nhân
                foreach (var patient in patientRecords)
                {
                    Console.WriteLine($"Patient ID: {patient.Key}, Record Count: {patient.Value.Count}");
                    decimal diastolicAverage = 0; // tâm trương
                    decimal systolicAverage = 0; // tâm thu
                    int diastolicCount = 0;
                    int systolicCount = 0;
                    foreach (var record in patient.Value)
                    {
                        decimal diastolic = 0;
                        decimal systolic = 0;
                        foreach (var metric in record.RecordMetricItems)
                        {

                            if (metric.MetricId == 1) // Tâm thu
                            {
                                systolic += metric.Value;
                                systolicAverage += metric.Value;
                                systolicCount++;
                            }
                            else if (metric.MetricId == 4) // Tâm truong
                            {
                                diastolic = metric.Value;
                                diastolicAverage += metric.Value;
                                diastolicCount++;
                            }
                        }
                        var ResultList = CheckOverAsync(systolic, diastolic); 
                        foreach (var result in ResultList.Result) 
                        {
                            //TODO 
                            if (result.Status.Equals("Over"))
                            {
                                Console.WriteLine("User ID: " + record.PatientId + " " + record.BandId + " " + result.CurrentValue);
                                Console.WriteLine(result.MetricName);
                                Console.WriteLine("Note" + record.GhiChu);
                            }
                        }
                    }
                    if (diastolicCount > 0)
                        diastolicAverage /= diastolicCount;
                    if (systolicCount > 0)
                        systolicAverage /= systolicCount;
                    // Tạo object mới và gán giá trị
                    var healthRecord = new CreateHealthRecordCombinedModel
                    {
                        PatientId = patient.Key,
                        BandId = patient.Value.First().BandId, // Nếu bạn có dữ liệu về BandId, hãy gán vào đây
                        GhiChu = "Tổng hợp trung bình chỉ số",
                        RecordMetricItems = new List<CreateRecordMetricItemModel>
                        {
                            new CreateRecordMetricItemModel { MetricId = 1, Value = diastolicAverage }, // Tâm trương
                            new CreateRecordMetricItemModel { MetricId = 4, Value = systolicAverage }  // Tâm thu
                        }
                    };
                    await ProcessDataAsync(healthRecord); // Save into DB
                }
            }
            else
            {
                Console.WriteLine("No new data found.");
            }
        }
        public async Task ProcessDataAsync(CreateHealthRecordCombinedModel healthRecord)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                try
                {
                    // Lấy dữ liệu từ Firebase
                    var scannedData = new HealthRecord
                    {
                        PatientId = healthRecord.PatientId,
                        BandId = healthRecord.BandId, // Nếu bạn có dữ liệu về BandId, hãy gán vào đây
                        GhiChu = "Tổng hợp trung bình chỉ số",
                        RecordMetricItems = new List<RecordMetricItem>
                        {
                            new RecordMetricItem {
                                MetricId = healthRecord.RecordMetricItems.First().MetricId,
                                Value = healthRecord.RecordMetricItems.First().Value,
                                CreatedTime = DateTimeOffset.Now,
                                LastUpdatedTime = DateTimeOffset.Now
                            }, // Tâm trương
                            new RecordMetricItem {
                                MetricId = healthRecord.RecordMetricItems.Last().MetricId,
                                Value = healthRecord.RecordMetricItems.Last().Value,
                                CreatedTime = DateTimeOffset.Now,
                                LastUpdatedTime = DateTimeOffset.Now
                            }  // Tâm thu
                        }
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
        public async Task<List<CheckResult>> CheckOverAsync(decimal systolic, decimal diastolic)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                try
                {

                    var metric = unitOfWork.GetRepository<Metric>().Entities.Where(n => n.Name.Equals("Tam Thu") || n.Name.Equals("Tam Truong"));
                    List<CheckResult> results = new List<CheckResult>();
                    bool systolicCheck = false;
                    bool diastolicCheck = false;
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

    }
}
