using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace Trainacc.Filters
{
    public class ParamDescriptionFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null) return;
            var controller = context.ApiDescription.ActionDescriptor.RouteValues["controller"];
            var action = context.ApiDescription.HttpMethod?.ToLower();
            foreach (var param in operation.Parameters)
            {
                if (param.Name == "mode")
                {
                    switch (controller)
                    {
                        case "Accounts":
                            param.Description = "Режим запроса. Возможные значения: user-balance, summary, by-record, balance-history.";
                            param.Schema.Enum = new List<IOpenApiAny>
                            {
                                new OpenApiString("user-balance"),
                                new OpenApiString("summary"),
                                new OpenApiString("by-record"),
                                new OpenApiString("balance-history")
                            };
                            break;
                        case "Transactions":
                            param.Description = "Режим запроса. Возможные значения: summary, top, filter, by-record, export.";
                            param.Schema.Enum = new List<IOpenApiAny>
                            {
                                new OpenApiString("summary"),
                                new OpenApiString("top"),
                                new OpenApiString("filter"),
                                new OpenApiString("by-record"),
                                new OpenApiString("export")
                            };
                            break;
                        case "Restrictions":
                            param.Description = "Режим запроса. Возможные значения: by-record, exceeded.";
                            param.Schema.Enum = new List<IOpenApiAny>
                            {
                                new OpenApiString("by-record"),
                                new OpenApiString("exceeded")
                            };
                            break;
                        case "Records":
                            param.Description = "Режим запроса. Возможные значения: by-user.";
                            param.Schema.Enum = new List<IOpenApiAny>
                            {
                                new OpenApiString("by-user")
                            };
                            break;
                        case "Deposits":
                        case "Credits":
                            param.Description = "Режим запроса. Возможные значения: by-record.";
                            param.Schema.Enum = new List<IOpenApiAny>
                            {
                                new OpenApiString("by-record")
                            };
                            break;
                        default:
                            param.Description = "Режим запроса (mode).";
                            break;
                    }
                }
                if (param.Name == "recordId")
                {
                    param.Description = "ID записи/профиля, к которому относится запрос.";
                }
                if (param.Name == "userId")
                {
                    param.Description = "ID пользователя, для которого выполняется запрос.";
                }
                if (param.Name == "topN")
                {
                    param.Description = "Количество верхних записей (например, топ расходов).";
                }
                if (param.Name == "from")
                {
                    param.Description = "Начальная дата периода (формат: yyyy-MM-ddTHH:mm:ss, пример: 2025-05-21T00:00:00).";
                }
                if (param.Name == "to")
                {
                    param.Description = "Конечная дата периода (формат: yyyy-MM-ddTHH:mm:ss, пример: 2025-05-21T23:59:59).";
                }
                if (param.Name == "min")
                {
                    param.Description = "Минимальное значение суммы/фильтра.";
                }
                if (param.Name == "max")
                {
                    param.Description = "Максимальное значение суммы/фильтра.";
                }
            }
        }
    }
}
