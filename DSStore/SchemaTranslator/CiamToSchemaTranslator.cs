 // public interface ICiamToSchemaTranslator
 //    {
 //        Jsonsc TranslaterCiamSchema(JObject ciamSchema, bool addCiamPaths = false);
 //    }
 //    public class CiamToSchemaTranslator : ICiamToSchemaTranslator
 //    {
 //        public JSchema TranslaterCiamSchema(JObject ciamSchema, bool addCiamPaths= false)
 //        {
 //            var fields = GetCdflFields(ciamSchema);
 //            var requiredArray = new List<string>();
 //            var properties = new JObject();
 //            foreach (var field in fields)
 //            {
 //                var fieldValue = new JObject();
 //
 //                //required
 //                var requiredValue = field.Value?.Value<bool?>("required");
 //                if (requiredValue.HasValue && requiredValue.Value)
 //                    requiredArray.Add(field.Name);
 //
 //                //type
 //                var type = GetType(field.Value?.Value<string?>("type"));
 //                fieldValue.Add("type", type);
 //
 //                //format
 //                SetPattern(field, fieldValue, type);
 //
 //                if (addCiamPaths)
 //                    AddCiamPath(field.Name, fieldValue);
 //
 //                var fieldname = GetFieldName(field.Name);
 //
 //                properties.Add(fieldname, fieldValue);
 //            }
 //
 //            var result = new JObject();
 //            result.Add("properties", properties);
 //            if (requiredArray.Any())
 //                result.Add("required", new JArray(requiredArray));
 //            result.Add("type", "object");
 //            try
 //            {
 //                JSchema schema = JSchema.Parse(result.ToString(Formatting.None));
 //                return schema;
 //            }catch(Exception e)
 //            {
 //                throw new Exception("Failed to parse translated schema into json-schema", e);
 //            }
 //        }
 //
 //        private List<JProperty> GetCdflFields(JObject ciamSchema)
 //        {
 //            var fields = ciamSchema?.Value<JObject>("fields")
 //                ?.Properties().EmptyIfNull().
 //                Where(property => SchemaUpdateValidator.ALLOWED_FIELDS.Contains(property.Name, StringComparer.OrdinalIgnoreCase));
 //            return fields.ToList();
 //        }
 //
 //        private string GetType(string ciamType)
 //        {
 //            switch (ciamType)
 //            {
 //                case "integer":
 //                case "long":
 //                    return "integer";
 //                case "string":
 //                    return "string";
 //                case "boolean":
 //                    return "boolean";
 //                default:
 //                    return "string";
 //            }
 //
 //        }
 //
 //        private void SetPattern(JProperty fieldInCiam, JObject field, string type)
 //        {
 //            try
 //            {
 //                if (fieldInCiam.Value.Value<string>("format").IsNullOrEmpty())
 //                    return;
 //
 //                var format = fieldInCiam.Value.Value<string>("format");
 //                switch (type)
 //                {
 //                    case "string":
 //                        field.Add("pattern", format.RemovePrefix("regex(").RemoveSuffix(")"));
 //                        break;
 //                    case "boolean":
 //                        field.Add("pattern", format);
 //                        break;
 //                    case "integer":
 //                        {
 //                            var limits = format.Split("-");
 //                            field.Add("minimum", Int64.Parse(limits[0]));
 //                            field.Add("maximum", Int64.Parse(limits[1]));
 //                            break;
 //                        }
 //                }
 //            }
 //            catch(Exception e)
 //            {
 //                throw new Exception("Failed to SetPattern in CIAM-to-schema translation", e);
 //            }
 //        }
 //
 //        private void AddCiamPath(string fieldname, JObject fieldValue)
 //        {
 //            var ciamPath = "profile." + fieldname;
 //            fieldValue.Add("ciam-path", ciamPath);
 //        }
 //
 //        private string GetFieldName(string fieldname)
 //        {
 //            return fieldname.ToLower();
 //        }
 //
 //    }
