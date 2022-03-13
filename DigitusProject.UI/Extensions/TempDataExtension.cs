using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DigitusProject.WebUI.Extensions
{
    public static class TempDataExtension
    {
        public static void Put<T>(this ITempDataDictionary tempData,string key,T value) where T:class
        {
            tempData[key] = JsonConvert.SerializeObject(value);
        }
        public static T Get<T>(this ITempDataDictionary tempData,string key) where T : class
        {
            object o;
            tempData.TryGetValue(key, out o);
            return o==null?null:JsonConvert.DeserializeObject<T>((string)o);
        }
    }
}
