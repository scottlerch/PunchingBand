#define DISABLE_PUNCH_RECOGNITION

using Microsoft.Band.Sensors;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using PunchingBand.Models.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace PunchingBand.Utilities
{
    internal sealed class PunchDetector : IDisposable
    {
        private class LogData
        {
            public IBandAccelerometerReading AccelerometerReading { get; set; }

            public PunchInfo PunchInfo { get; set; }
        }

        private readonly TimeSpan punchCoolOffInterval = TimeSpan.FromMilliseconds(100);
        private const double punchThreshold = 0.5;
        private const double punchResetThreshold = 0.0;
        private const double maximumAcceleration = 8.0;
        private const int punchVectorSize = 10; // ~160msec (10x16msec) timeframe to detect punch

        private double lastX;
        private double maxX = double.MinValue;

        private bool readyForPunch = true;

        private DateTime nextPunchTime = DateTime.MinValue;

        private PunchStatus lastPunchStatus = PunchStatus.Unknown;

        private bool punchStarted;

        private BlockingCollection<LogData> logData;
        private Task logTask;

        private BlockingCollection<string> punchVectors;
        private Task punchVectorsTask;

        private PunchBuffer punchBuffer = new PunchBuffer(punchVectorSize);
        private int? punchBufferWindowCount = null;

        public double LastPunchStrength { get; private set; }

        private FistSides fistSide;

        public PunchDetector(FistSides fistSide)
        {
            this.fistSide = fistSide;

            TrainPunchType = "Jab";
        }

        public async Task<PunchInfo> GetPunchInfo(IBandAccelerometerReading reading)
        {
            var status = PunchStatus.Unknown;
            double? punchStrength;

            if (IsPunchDetected(reading, out punchStrength))
            {
                status = PunchStatus.Finish; 
            }
            else
            {
                if (IsDetectingPunch(reading))
                {
                    // Use previous previous values in recognizing punch type
                    punchBufferWindowCount = 2;
                    // We know a punch is occuring but don't know the final strength
                    status = PunchStatus.Start;
                }
                else if (punchStarted && punchStrength.HasValue)
                {
                    status = PunchStatus.InProgress;
                }
                else if (!readyForPunch)
                {
                    status = PunchStatus.Resetting;
                }
                else if (lastPunchStatus == PunchStatus.Resetting)
                {
                    status = PunchStatus.Reset;
                }
            }

            punchBuffer.Add(reading);

            if (punchBufferWindowCount.HasValue)
            {
                punchBufferWindowCount++;
            }

            var bufferFull = punchBufferWindowCount >= punchBuffer.Size;

            if (bufferFull)
            {
                punchVectors.Add(punchBuffer.GetVector());
                punchBufferWindowCount = null;
            }

            var punchInfo = new PunchInfo(fistSide, status, punchStrength);

            Log(reading, punchInfo);

            lastPunchStatus = status;

            if (bufferFull)
            {
                var punchType = await DeterminePunchType(punchBuffer.ToList()).ConfigureAwait(false);
                return new PunchInfo(fistSide, status, punchStrength, punchType);
            }
            else
            {
                return punchInfo;
            }
        }

        private async Task<PunchType> DeterminePunchType(List<IBandAccelerometerReading> readings)
        {
#if DISABLE_PUNCH_RECOGNITION
            return await Task.FromResult(PunchType.Unknown);
#else
            // TODO: there has to be a more efficient way to do all this... ideally Azure ML web service can take simpler input or maybe binary input
            // TODO: see if we can execute Azure ML trained model locally using .NET machine learning library
            using (var client = new HttpClient())
            {
                var columnNames = new List<string>();
                columnNames.Add("PunchType");
                for (int i = 0; i < punchVectorSize; i++)
                {
                    columnNames.Add("X" + i);
                    columnNames.Add("Y" + i);
                    columnNames.Add("Z" + i);
                }

                var values = new List<string>();
                values.Add("");
                for (int i = 0; i < punchVectorSize; i++)
                {
                    if (i < readings.Count)
                    {
                        values.Add(readings[i].AccelerationX.ToString());
                        values.Add(readings[i].AccelerationY.ToString());
                        values.Add(readings[i].AccelerationZ.ToString());
                    }
                    else
                    {
                        values.Add("0");
                        values.Add("0");
                        values.Add("0");
                    }
                }

                var scoreRequest = new
                {
                    Inputs = new
                    {
                        punchData = new
                        {
                            ColumnNames = columnNames,
                            Values = new[] { values },
                        }
                    },
                    GlobalParameters = new { },
                };

                const string apiKey = "0altQZMnmD/uuvva0NADe2hjoM1m6e/Prx2yx+NICqpQSKEZwhdvsJrPzjhRi/ksYggU4VX1tWqBuh5EQ59npQ==";
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                client.BaseAddress = new Uri("https://ussouthcentral.services.azureml.net/workspaces/b67bf94b67004a95b0bb0be9be60b916/services/a472d8313a2b4af398dba9b7a310ae82/execute?api-version=2.0&details=true");

                var jsonBody = JsonConvert.SerializeObject(scoreRequest);

                var sw = Stopwatch.StartNew();
                var response = await client.PostAsync("", new StringContent(jsonBody, Encoding.UTF8, "application/json")).ConfigureAwait(false);
                sw.Stop();

                PunchType punchType;

                if (response.IsSuccessStatusCode)
                {
                    var result = JObject.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    var v = result["Results"]["punchType"]["value"]["Values"][0];
                    punchType = (PunchType)Enum.Parse(typeof(PunchType), v.Last.ToObject<string>());
                }
                else
                {
                    dynamic result = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    punchType = PunchType.Unknown;
                }

                Debug.WriteLine("{0} {1}", sw.ElapsedMilliseconds, punchType);
                return punchType;
            }
#endif
        }

        private bool IsPunchDetected(IBandAccelerometerReading reading, out double? punchStrength)
        {
            punchStrength = null;
            bool punchDetected = false;

            if (readyForPunch)
            {
                if (reading.AccelerationX > punchThreshold)
                {
                    maxX = Math.Max(maxX, reading.AccelerationX);
                    punchStrength = (maxX - punchThreshold) / (maximumAcceleration - punchThreshold);

                    if (reading.AccelerationX >= maximumAcceleration || reading.AccelerationX - lastX < 0)
                    {
                        LastPunchStrength = punchStrength.Value;

                        readyForPunch = false;
                        maxX = double.MinValue;

                        punchDetected = true;

                        nextPunchTime = DateTime.UtcNow + punchCoolOffInterval;
                    }
                }
            }
            else if (DateTime.UtcNow > nextPunchTime && reading.AccelerationX < punchResetThreshold)
            {
                punchStarted = false;
                readyForPunch = true;
                maxX = double.MinValue;
            }

            lastX = reading.AccelerationX;

            return punchDetected;
        }

        public void Reset()
        {
            lastX = 0;
            maxX = 0;

            readyForPunch = true;
        }

        private bool IsDetectingPunch(IBandAccelerometerReading reading)
        {
            if (!punchStarted && readyForPunch && reading.AccelerationX > punchThreshold)
            {
                punchStarted = true;
                return true;
            }

            return false;
        }

        private void Log(IBandAccelerometerReading reading, PunchInfo punchInfo)
        {
            if (logData != null)
            {
                logData.Add(new LogData
                {
                    AccelerometerReading = reading,
                    PunchInfo = punchInfo,
                });
            }
        }

        public void InitializeLogging()
        {
            logData = new BlockingCollection<LogData>();

            logTask = Task.Run(async () =>
            {
                var local = ApplicationData.Current.LocalFolder;
                var dataFolder = await local.CreateFolderAsync("LogData", CreationCollisionOption.OpenIfExists);
                var file = await dataFolder.CreateFileAsync(string.Format("PunchData{0}.csv", fistSide), CreationCollisionOption.ReplaceExisting);
                var startTimestamp = DateTime.Now;

                using (var fileStream = await file.OpenStreamForWriteAsync())
                {
                    var streamWriter = new StreamWriter(fileStream) {AutoFlush = true};

                    foreach (var data in logData.GetConsumingEnumerable())
                    {
                        streamWriter.WriteLine(
                            "{0},{1},{2},{3},{4}",
                            (int)(data.AccelerometerReading.Timestamp - startTimestamp).TotalMilliseconds,
                            data.AccelerometerReading.AccelerationX,
                            data.AccelerometerReading.AccelerationY,
                            data.AccelerometerReading.AccelerationZ,
                            data.PunchInfo.Status);
                        streamWriter.Flush();
                        fileStream.Flush();
                    }
                }
            });

            punchVectors = new BlockingCollection<string>();

            punchVectorsTask = Task.Run(async () =>
            {
                var local = ApplicationData.Current.LocalFolder;
                var dataFolder = await local.CreateFolderAsync("LogData", CreationCollisionOption.OpenIfExists);
                var file = await dataFolder.CreateFileAsync(string.Format("PunchVectors{0}.csv", fistSide, TrainPunchType), CreationCollisionOption.ReplaceExisting);
                var startTimestamp = DateTime.Now;

                using (var fileStream = await file.OpenStreamForWriteAsync())
                {
                    var streamWriter = new StreamWriter(fileStream) { AutoFlush = true };

                    streamWriter.Write("PunchType");
                    for(int i = 0; i < punchVectorSize; i++)
                    {
                        streamWriter.Write(",X{0},Y{0},Z{0}", i);
                    }

                    foreach (var vector in punchVectors.GetConsumingEnumerable())
                    {
                        streamWriter.WriteLine(TrainPunchType + "," + vector);
                        streamWriter.Flush();
                        fileStream.Flush();
                    }
                }
            });
        }

        internal string TrainPunchType { get; set; }

        public void Dispose()
        {
            if (logData != null)
            {
                logData.CompleteAdding();
                logTask.Wait();
                logData.Dispose();
            }

            if (punchVectors != null)
            {
                punchVectors.CompleteAdding();
                punchVectorsTask.Wait();
                punchVectors.Dispose();
            }
        }
    }
}
