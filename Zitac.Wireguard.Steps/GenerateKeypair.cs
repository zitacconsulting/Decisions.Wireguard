using System;
using DecisionsFramework.Design.Flow;
using DecisionsFramework.Design.ConfigurationStorage.Attributes;
using DecisionsFramework.Design.Flow.CoreSteps;
using DecisionsFramework.Design.Flow.Mapping;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace Zitac.Wireguard.Steps;

[AutoRegisterStep("Convert JSON to Fixed Schema", "Integration", "Wireguard")]
[Writable]
public class ConvertToFixedJsonSchema : BaseFlowAwareStep, ISyncStep, IDataProducer, IDataConsumer
{

    public DataDescription[] InputData
    {
        get
        {

            List<DataDescription> dataDescriptionList = new List<DataDescription>();
            dataDescriptionList.Add(new DataDescription((DecisionsType)new DecisionsNativeType(typeof(String)), "JSON"));
            return dataDescriptionList.ToArray();
        }
    }
    public override OutcomeScenarioData[] OutcomeScenarios
    {
        get
        {
            List<OutcomeScenarioData> outcomeScenarioDataList = new List<OutcomeScenarioData>();

            outcomeScenarioDataList.Add(new OutcomeScenarioData("Done", new DataDescription(typeof(String), "Result", false)));
            return outcomeScenarioDataList.ToArray();
        }
    }
    public ResultData Run(StepStartData data)
    {
        string? JSON = data.Data["JSON"] as string;
        string fixedSchemaJson = ConvertToFixedSchema(JSON);

        Dictionary<string, object> dictionary = new Dictionary<string, object>();
        dictionary.Add("Result", (object)fixedSchemaJson);
        return new ResultData("Done", (IDictionary<string, object>)dictionary);
    }
    public static string ConvertToFixedSchema(string json)
    {
        var jObject = JObject.Parse(json);
        var fixedObject = ConvertJObject(jObject);

        return JsonConvert.SerializeObject(fixedObject, Formatting.Indented);
    }

    private static object ConvertJObject(JObject jObject)
    {
        if (IsDynamicCollection(jObject))
        {
            var fixedItems = new List<Dictionary<string, object>>();

            foreach (var property in jObject.Properties())
            {
                var fixedItem = new Dictionary<string, object>
                {
                    { "key", property.Name },
                    { "value", ConvertJToken(property.Value) }
                };

                fixedItems.Add(fixedItem);
            }

            return fixedItems;
        }
        else
        {
            var fixedItem = new Dictionary<string, object>();

            foreach (var property in jObject.Properties())
            {
                fixedItem[property.Name] = ConvertJToken(property.Value);
            }

            return fixedItem;
        }
    }

    private static object ConvertJToken(JToken jToken)
    {
        return jToken switch
        {
            JObject jObject => ConvertJObject(jObject),
            JArray jArray => jArray.Select(ConvertJToken).ToList(),
            _ => jToken
        };
    }

    private static bool IsDynamicCollection(JObject jObject)
    {
        return jObject.Properties().All(p => IsGuidOrIpAddress(p.Name));
    }

    private static bool IsGuidOrIpAddress(string value)
    {
        var ipPattern = @"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}(?:\/[0-9]{1,2})?$";
        var guidPattern = @"^[a-fA-F0-9\-]+$";

        return Regex.IsMatch(value, ipPattern) || Regex.IsMatch(value, guidPattern);
    }
}