using Microsoft.Band.Portable.Sensors;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PunchingBand.Band;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PunchingBand.Recognition.Recognizers
{
    public class AzureMachineLearningRecognizer : IPunchRecognizer
    {
        private string serviceUrl;
        private string apiKey;

        public Task Initialize(FistSides fistSide)
        {
            if (fistSide == FistSides.Right)
            {
                serviceUrl = "https://ussouthcentral.services.azureml.net/workspaces/b67bf94b67004a95b0bb0be9be60b916/services/a472d8313a2b4af398dba9b7a310ae82/execute?api-version=2.0&details=true";
                apiKey = "0altQZMnmD/uuvva0NADe2hjoM1m6e/Prx2yx+NICqpQSKEZwhdvsJrPzjhRi/ksYggU4VX1tWqBuh5EQ59npQ==";
            }
            else
            {
                throw new NotImplementedException("Left fist has no Azure Machine Learning model");
            }

            return Task.FromResult(0);
        }

        public async Task<PunchRecognition> Recognize(IEnumerable<GyroscopeAccelerometerReading> readings)
        {
            var punchVector = readings.ToList();
            var punchVectorSize = punchVector.Count;

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
                    if (i < punchVector.Count)
                    {
                        values.Add(punchVector[i].AccelerationX.ToString());
                        values.Add(punchVector[i].AccelerationY.ToString());
                        values.Add(punchVector[i].AccelerationZ.ToString());
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

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                client.BaseAddress = new Uri(serviceUrl);

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
                return new PunchRecognition(punchType, 0.0, sw.Elapsed);
            }
        }
    }
}
