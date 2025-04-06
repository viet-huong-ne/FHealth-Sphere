//using Contract.Repositories.Entity;
//using Contract.Repositories.Interface;
//using Contract.Services.Interface;
//using Core.Utils;
//using FHealthSphere.Services.Services;
//using Firebase.Database;
//using Firebase.Database.Query;
//using Google.Apis.Util;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using Microsoft.Identity.Client;
//using Microsoft.IdentityModel.Tokens;
//using ModelViews.HealthRecordModelViews;
//using ModelViews.MetricModelViews;
//using ModelViews.RecordMetricItemModelViews;

//namespace Services.Service
//{
//    public class FirebasePollingService 
//    {
//        private readonly FirebaseClient _firebaseClient;
//        private readonly IServiceScopeFactory _scopeFactory;
//        private readonly ILogger<HealthRecordService> _logger;
//        private readonly string _lastKeyFile = Path.Combine(Directory.GetCurrentDirectory(), "Config", "lastkey.txt"); // Đường dẫn file
//        private readonly Timer _timer;
//        private string lastKey = "";
//        private List<CreateHealthRecordCombinedModel> _hourlyRecords = new();
//        private DateTime _lastSaveTime = CoreHelper.SystemTimeNows;
//        public FirebasePollingService(IServiceScopeFactory scopeFactory, ILogger<HealthRecordService> logger)
//        {
//            _firebaseClient = new FirebaseClient("https://fhealth-sphere---login-default-rtdb.asia-southeast1.firebasedatabase.app/");
//            //_timer = new Timer(async _ => await TimerCallback(), null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
//            _scopeFactory = scopeFactory;
//            _logger = logger;
//        }
//        private async Task TimerCallback()
//        {
//            try
//            {
//                // Quét dữ liệu mỗi 10 giây
//                await PollFirebase();

//                // Kiểm tra nếu đã đến giờ lưu dữ liệu trung bình (mỗi 1 giờ)
//                if (CoreHelper.SystemTimeNows - _lastSaveTime >= TimeSpan.FromSeconds(20))
//                {
//                    await SaveHourlyData(); // save data one time
//                    _lastSaveTime = CoreHelper.SystemTimeNows; // set _lastSavetime
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Lỗi trong TimerCallback.");
//            }
//        }
//        private async Task PollFirebase()
//        {
//            var newRecords = await CheckForUpdates();
//            Console.WriteLine(lastKey);
//            if (newRecords != null && newRecords.Any())
//            {
//                _hourlyRecords.AddRange(newRecords);
//                //_logger.LogInformation($"Added {newRecords.Count} records to hourly buffer.");
//                await ToolFireBaseV2(newRecords);
//            } else Console.WriteLine("NO RECORD");
//        }

//        private async Task SaveHourlyData()
//        {
//            if (_hourlyRecords.Any())
//            {
//                // Nhóm dữ liệu theo từng bệnh nhân
//                var groupedRecords = _hourlyRecords
//                                      .GroupBy(r => r.PatientId)
//                                      .ToList();

//                // Duyệt từng nhóm bệnh nhân
//                foreach (var patientRecords in groupedRecords)
//                {
//                    var averagedRecord = AverageRecords(patientRecords.ToList());
//                    await ProcessDataAsync(averagedRecord);
//                    _logger.LogInformation($"Saved averaged data for Patient ID: {averagedRecord.PatientId} to DB.");
//                }

//                _hourlyRecords.Clear();
//            }
//        }

//        private CreateHealthRecordCombinedModel AverageRecords(List<CreateHealthRecordCombinedModel> records)
//        {
//            // Đảm bảo có dữ liệu trước khi xử lý
//            if (!records.Any()) return null;

//            // Lấy PatientId và BandId từ bản ghi đầu tiên
//            var patientId = records.First().PatientId;
//            var bandId = records.First().BandId;

//            // Tính trung bình cho từng chỉ số (MetricId)
//            var averagedMetrics = records
//                                  .SelectMany(r => r.RecordMetricItems)
//                                  .GroupBy(item => item.MetricId)
//                                  .Select(group => new CreateRecordMetricItemModel
//                                  {
//                                      MetricId = group.Key,
//                                      Value = group.Average(item => item.Value)
//                                  })
//                                  .ToList();

//            return new CreateHealthRecordCombinedModel
//            {
//                PatientId = patientId,
//                BandId = bandId,
//                GhiChu = "Averaged hourly data",
//                RecordMetricItems = averagedMetrics
//            };
//        }
//        private async Task<List<CreateHealthRecordCombinedModel>> CheckForUpdates()
//        {
//            try
//            {
//                // Lấy toàn bộ dữ liệu
//                var data = await _firebaseClient
//                    .Child("healthRecords")
//                    .OnceAsync<CreateHealthRecordCombinedModel>();
//                _logger.LogInformation($"Data count: {data?.Count()}"); // Log số lượng dữ liệu
//                if (string.IsNullOrEmpty(lastKey))
//                {
//                    lastKey = await LoadLastKeyAsync();
//                } // Load từ file
//                if (string.IsNullOrEmpty(lastKey))
//                {
//                    if (data.Any())
//                    {
//                        lastKey = data.Last().Key; // Lưu lại key cuối cùng
//                        await SaveLastKeyAsync(lastKey); // Cập nhật lại file
//                        _logger.LogInformation($"First scan - LastKey set: {lastKey}");
//                    }

//                    return data.Select(r => r.Object).ToList();
//                }
//                else
//                {
//                    // Lọc ra các phần tử mới dựa trên lastKey
//                    var newData = data
//                        .SkipWhile(x => x.Key != lastKey) // Bỏ qua các phần tử cũ
//                        .Skip(1) // Bỏ luôn phần tử có lastKey
//                        .ToList();
//                    if (newData.Any())
//                    {
//                        lastKey = newData.Last().Key; // Cập nhật lastKey với bản ghi mới nhất
//                        await SaveLastKeyAsync(lastKey); // Cập nhật lại file
//                        _logger.LogInformation($"New scan - LastKey updated: {lastKey}");
//                    }

//                    return newData.Select(r => r.Object).ToList();
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error fetching data: {ex.Message}");
//                return new List<CreateHealthRecordCombinedModel>(); // Trả về danh sách rỗng nếu lỗi
//            }

//        }
//        private async Task ToolFireBaseV2(List<CreateHealthRecordCombinedModel> HealthRecords)
//        {

//            if (HealthRecords != null && HealthRecords.Any())
//            {
//                // Tạo từ điển để lưu dữ liệu phân loại theo bệnh nhân
//                Dictionary<int?, List<CreateHealthRecordCombinedModel>> patientRecords = new();

//                foreach (var record in HealthRecords)
//                {
//                    int? patientId = record.PatientId; // Giả sử có thuộc tính PatientId

//                    if (!patientRecords.ContainsKey(patientId))
//                    {
//                        patientRecords[patientId] = new List<CreateHealthRecordCombinedModel>();
//                    }

//                    patientRecords[patientId].Add(record);
//                }
//                // Xuất danh sách từng bệnh nhân
//                foreach (var patient in patientRecords)
//                {
//                    Console.WriteLine($"Patient ID: {patient.Key}, Record Count: {patient.Value.Count}");
//                    foreach (var record in patient.Value)
//                    {
//                        decimal diastolic = 0; // tâm trương
//                        decimal systolic = 0; // tâm thu
//                        decimal heartbeat = 0; // nhịp tim 
//                        foreach (var metric in record.RecordMetricItems)
//                        {

//                            if (metric.MetricId == 1) // Tâm thu
//                            {
//                                systolic = metric.Value;
//                            }
//                            else if (metric.MetricId == 2) // Tâm truong
//                            {
//                                diastolic = metric.Value;
//                            }
//                            else if (metric.MetricId == 3) // Nhịp tim
//                            {
//                                heartbeat = metric.Value;
//                            }
//                        }
//                        var ResultList = CheckOverAsync(systolic, diastolic, heartbeat);
//                        foreach (var result in ResultList.Result)
//                        {
//                            //TODO 
//                            if (result.Status.Equals("Over"))
//                            {
//                                Console.WriteLine("User ID: " + record.PatientId + " " + record.BandId + " " + result.CurrentValue);
//                                Console.WriteLine(result.MetricName);
//                                Console.WriteLine("Note" + record.GhiChu);
//                                var _token = await GetFCMToken(record.PatientId);
//                                await CallNotification(result, record.PatientId, _token);
//                            }
//                        }
//                    }
//                }
//            }
//            else
//            {
//                Console.WriteLine("No new data found.");
//            }
//        }
//        private async Task ToolFireBase()
//        {
//            var HealthRecords = CheckForUpdates().Result;

//            if (HealthRecords != null && HealthRecords.Any())
//            {
//                // Tạo từ điển để lưu dữ liệu phân loại theo bệnh nhân
//                Dictionary<int?, List<CreateHealthRecordCombinedModel>> patientRecords = new();

//                foreach (var record in HealthRecords)
//                {
//                    int? patientId = record.PatientId; // Giả sử có thuộc tính PatientId

//                    if (!patientRecords.ContainsKey(patientId))
//                    {
//                        patientRecords[patientId] = new List<CreateHealthRecordCombinedModel>();
//                    }

//                    patientRecords[patientId].Add(record);
//                }
//                // Xuất danh sách từng bệnh nhân
//                foreach (var patient in patientRecords)
//                {
//                    Console.WriteLine($"Patient ID: {patient.Key}, Record Count: {patient.Value.Count}");
//                    decimal diastolicAverage = 0; // tâm trương
//                    decimal systolicAverage = 0; // tâm thu
//                    int diastolicCount = 0;
//                    int systolicCount = 0;
//                    foreach (var record in patient.Value)
//                    {
//                        decimal diastolic = 0;
//                        decimal systolic = 0;
//                        foreach (var metric in record.RecordMetricItems)
//                        {

//                            if (metric.MetricId == 1) // Tâm thu
//                            {
//                                systolic += metric.Value;
//                                systolicAverage += metric.Value;
//                                systolicCount++;
//                            }
//                            else if (metric.MetricId == 2) // Tâm truong
//                            {
//                                diastolic = metric.Value;
//                                diastolicAverage += metric.Value;
//                                diastolicCount++;
//                            }
//                        }
//                        var ResultList = CheckOverAsync(systolic, diastolic, 90); 
//                        foreach (var result in ResultList.Result) 
//                        {
//                            //TODO 
//                            if (result.Status.Equals("Over"))
//                            {
//                                Console.WriteLine("User ID: " + record.PatientId + " " + record.BandId + " " + result.CurrentValue);
//                                Console.WriteLine(result.MetricName);
//                                Console.WriteLine("Note" + record.GhiChu);
//                                var _token = await GetFCMToken(record.PatientId);
//                                await CallNotification(result, record.PatientId, _token);
//                            }
//                        }
//                    }
//                    if (diastolicCount > 0)
//                        diastolicAverage /= diastolicCount;
//                    if (systolicCount > 0)
//                        systolicAverage /= systolicCount;
//                    // Tạo object mới và gán giá trị
//                    var healthRecord = new CreateHealthRecordCombinedModel
//                    {
//                        PatientId = patient.Key,
//                        BandId = patient.Value.First().BandId, // Nếu bạn có dữ liệu về BandId, hãy gán vào đây
//                        GhiChu = "Tổng hợp trung bình chỉ số",
//                        RecordMetricItems = new List<CreateRecordMetricItemModel>
//                        {
//                            new CreateRecordMetricItemModel { MetricId = 1, Value = systolicAverage }, // Tâm trương
//                            new CreateRecordMetricItemModel { MetricId = 2, Value = diastolicAverage }  // Tâm thu
//                        }
//                    };
//                    await ProcessDataAsync(healthRecord); // Save into DB
//                }
//            }
//            else
//            {
//                Console.WriteLine("No new data found.");
//            }
//        }
//        public async Task ProcessDataAsync(CreateHealthRecordCombinedModel healthRecord)
//        {
//            using (var scope = _scopeFactory.CreateScope())
//            {
//                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

//                try
//                {
//                    var RecordMetrics = new List<RecordMetricItem>();
//                    foreach(var record in healthRecord.RecordMetricItems) 
//                    {
//                        var model = new RecordMetricItem
//                        {
//                            MetricId = record.MetricId,
//                            Value = record.Value,
//                            CreatedTime = CoreHelper.SystemTimeNow,
//                            LastUpdatedTime = CoreHelper.SystemTimeNow
//                        };
//                        RecordMetrics.Add(model);

//                    }
//                    // Lấy dữ liệu từ Firebase
//                    var scannedData = new HealthRecord
//                    {
//                        PatientId = healthRecord.PatientId,
//                        BandId = healthRecord.BandId, // Nếu bạn có dữ liệu về BandId, hãy gán vào đây
//                        GhiChu = "Tổng hợp trung bình chỉ số",
//                        CreatedTime = CoreHelper.SystemTimeNow,
//                        LastUpdatedTime = CoreHelper.SystemTimeNow,
//                        RecordMetricItems = RecordMetrics
//                    };               
//                    // Lưu dữ liệu vào database
//                    await unitOfWork.GetRepository<HealthRecord>().InsertAsync(scannedData);
//                    await unitOfWork.SaveAsync();

//                    _logger.LogInformation("Dữ liệu đã được lưu thành công.");
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogError(ex, "Lỗi khi lưu dữ liệu.");
//                }
//            }
//        }
//        public async Task<List<CheckResult>> CheckOverAsync(decimal systolic, decimal diastolic, decimal heartbeat)
//        {
//            using (var scope = _scopeFactory.CreateScope())
//            {
//                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

//                try
//                {

//                    var metric = unitOfWork.GetRepository<Metric>().Entities.Where(n => n.Name.Equals("Tam Thu") || n.Name.Equals("Tam Truong") || n.Name.Equals("Nhip Tim"));
//                    List<CheckResult> results = new List<CheckResult>();
//                    foreach (Metric mt in metric)
//                    {
//                        if (mt.Name.Equals("Tam Thu"))
//                        {
//                            bool isOutOfRange = mt.MinValue > systolic || mt.MaxValue < systolic;
//                            results.Add(new CheckResult
//                            {
//                                MetricName = "Tâm Thu",
//                                CurrentValue = systolic,
//                                Status = isOutOfRange ? "Over" : "Normal"
//                            });
//                        }
//                        if (mt.Name.Equals("Tam Truong"))
//                        {
//                            bool isOutOfRange = mt.MinValue > diastolic || mt.MaxValue < diastolic;
//                            results.Add(new CheckResult
//                            {
//                                MetricName = "Tâm Trương",
//                                CurrentValue = diastolic,
//                                Status = isOutOfRange ? "Over" : "Normal"
//                            });
//                        }
//                        if (mt.Name.Equals("Nhip Tim"))
//                        {
//                            bool isOutOfRange = mt.MinValue > heartbeat || mt.MaxValue < heartbeat;
//                            results.Add(new CheckResult
//                            {
//                                MetricName = "Nhịp Tim",
//                                CurrentValue = heartbeat,
//                                Status = isOutOfRange ? "Over" : "Normal"
//                            });
//                        }
//                    }
//                    return results;
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogError(ex, "Lỗi khi lưu dữ liệu.");
//                    return new List<CheckResult>
//                    {
//                        new CheckResult
//                        {
//                            MetricName = "Lỗi hệ thống",
//                            Status = $"Lỗi: {ex.Message}"
//                        }
//                    };
//                }
//            }
//        }
//        public async Task CallNotification(CheckResult result, int? patientId, string FCMToken)
//        {
//            using (var scope = _scopeFactory.CreateScope())
//            {
//                var _metricService = scope.ServiceProvider.GetRequiredService<IMetricService>();
//                var _notiService = scope.ServiceProvider.GetRequiredService<INotificationService>();

//                var title = "Metric Value Alert";
//                var message = $"Default value {result.CurrentValue} is out of range for metric {result.MetricName}.";
//                await _metricService.SendNotificationAsync(title, message, FCMToken);
//                await _notiService.CreateNotification(title, message, patientId);
//            }
//        }
//        public async Task<string> GetFCMToken(int? patientId)
//        {
//            using (var scope = _scopeFactory.CreateScope())
//            {
//                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
//                var accounts = unitOfWork.GetRepository<Account>().Entities.FirstOrDefaultAsync(n => n.Id == patientId);
//                return accounts.Result.FCMToken;
//            }
//        }
//        public async Task GetMetricId(string metricName)
//        {
//            using (var scope = _scopeFactory.CreateScope())
//            {
//                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
//                var metric = unitOfWork.GetRepository<Metric>().Entities.Where(n => n.Name.Equals(metricName));
//            }
//        }

//        private async Task SaveLastKeyAsync(string lastKey)
//        {
//            await File.WriteAllTextAsync(_lastKeyFile, lastKey);
//        }

//        private async Task<string> LoadLastKeyAsync()
//        {
//            if (!File.Exists(_lastKeyFile))
//            {
//                Console.WriteLine("File lastkey.txt không tồn tại.");
//                return string.Empty;
//            }

//            using (var reader = new StreamReader(_lastKeyFile))
//            {
//                var key = await reader.ReadToEndAsync();
//                Console.WriteLine($"LastKey loaded: {key}");
//                return key.Trim();
//            }
//        }
//    }
//}
