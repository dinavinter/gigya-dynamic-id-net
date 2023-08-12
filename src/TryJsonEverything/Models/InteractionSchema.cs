using System.Text.Json;
using Json.Schema;
using TryJsonEverything.Services;
#pragma warning disable 8618

namespace TryJsonEverything.Models
{
	[Schema(typeof(InteractionSchema), nameof(SchemaInput))]
	public class InteractionSchema
	{
		public JsonSchema Schema { get; set; }
		public ValidationOptionsInput? Options { get; set; }

        public static readonly JsonSchema SchemaInput =
            			new JsonSchemaBuilder()
            				.Schema(MetaSchemas.Draft202012Id)
            				.Id("https://json-everything.net/schemas/schema")
            				.Type(SchemaValueType.Object)
            				.Defs(
            					("validationOptions", new JsonSchemaBuilder()
            						.Type(SchemaValueType.Object)
            						.Properties(
            							("outputFormat", new JsonSchemaBuilder()
            								.OneOf(
            									new JsonSchemaBuilder()
            										.Enum(nameof(OutputFormat.Flag),
            											nameof(OutputFormat.Basic),
            											nameof(OutputFormat.Detailed),
            											nameof(OutputFormat.Verbose)
            										),
            									new JsonSchemaBuilder().Type(SchemaValueType.Null)
            								)
            							),
            							("validateAs", new JsonSchemaBuilder()
            								.OneOf(
            									new JsonSchemaBuilder()
            										.Enum(6, 7, "6", "7", "2019-09", "2020-12"),
            									new JsonSchemaBuilder().Type(SchemaValueType.Null)
            								)
            							),
            							("defaultBaseUri", new JsonSchemaBuilder()
            								.Type(SchemaValueType.String | SchemaValueType.Null)
            								.Format(Formats.Uri)
            							),
            							("requireFormatValidation", new JsonSchemaBuilder().Type(SchemaValueType.Boolean | SchemaValueType.Null))
            						)
            					)
            				)
            				.Properties(
            					("schema", new JsonSchemaBuilder()
            						.Type(SchemaValueType.Object | SchemaValueType.Boolean)
            					),
            					("options", new JsonSchemaBuilder().Ref("#/$defs/validationOptions"))
            				)
            				.Required("schema" );
	}


}
