using Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ModelViews.AccountModelViews.Request
{
    public class UpdatePaitientInfo
    {
        public string? Gender { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(NullableDateTimeConverter))]
        public DateTime? DateOfBirth { get; set; }
    }
}
