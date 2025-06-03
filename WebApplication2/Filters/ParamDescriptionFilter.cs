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
                            param.Description = "Режим запроса. Возможные значения: summary, top, filter, by-account, export.";
                            param.Schema.Enum = new List<IOpenApiAny>
                            {
                                new OpenApiString("summary"),
                                new OpenApiString("top"),
                                new OpenApiString("filter"),
                                new OpenApiString("by-account"),
                                new OpenApiString("export")
                            };
                            break;
                        case "Restrictions":
                            param.Description = "Режим запроса. Возможные значения: by-account, exceeded.";
                            param.Schema.Enum = new List<IOpenApiAny>
                            {
                                new OpenApiString("by-account"),
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
                            param.Description = "Режим запроса. Возможные значения: by-account.";
                            param.Schema.Enum = new List<IOpenApiAny>
                            {
                                new OpenApiString("by-account")
                            };
                            break;
                        case "Credits":
                            param.Description = "Режим запроса. Возможные значения: by-account.";
                            param.Schema.Enum = new List<IOpenApiAny>
                            {
                                new OpenApiString("by-account")
                            };
                            break;
                        default:
                            param.Description = "Режим запроса (mode).";
                            break;
                    }
                }
                if (param.Name == "id")
                {
                    param.Description = "Уникальный идентификатор объекта. Используется для получения, обновления или удаления конкретной сущности (например, пользователя, счёта, депозита, транзакции, ограничения и т.д.). Обязателен для операций над одной записью.";
                }
                if (param.Name == "recordId")
                {
                    param.Description = "ID записи/профиля пользователя, к которому относится объект (например, счёт, депозит, кредит, транзакция, ограничение). Используется для фильтрации или создания объектов, связанных с определённым профилем. Обязателен для режимов by-record.";
                }
                if (param.Name == "userId")
                {
                    param.Description = "ID пользователя, для которого выполняется запрос. Необходим для получения данных, относящихся к конкретному пользователю (например, балансы, транзакции, ограничения). Часто обязателен для режимов summary, top, filter, balance-history, exceeded.";
                }
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
                            param.Description = "Режим запроса. Возможные значения: summary, top, filter, by-account, export.";
                            param.Schema.Enum = new List<IOpenApiAny>
                            {
                                new OpenApiString("summary"),
                                new OpenApiString("top"),
                                new OpenApiString("filter"),
                                new OpenApiString("by-account"),
                                new OpenApiString("export")
                            };
                            break;
                        case "Restrictions":
                            param.Description = "Режим запроса. Возможные значения: by-account, exceeded.";
                            param.Schema.Enum = new List<IOpenApiAny>
                            {
                                new OpenApiString("by-account"),
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
                            param.Description = "Режим запроса. Возможные значения: by-account.";
                            param.Schema.Enum = new List<IOpenApiAny>
                            {
                                new OpenApiString("by-account")
                            };
                            break;
                        case "Credits":
                            param.Description = "Режим запроса. Возможные значения: by-account.";
                            param.Schema.Enum = new List<IOpenApiAny>
                            {
                                new OpenApiString("by-account")
                            };
                            break;
                        default:
                            param.Description = "Режим запроса (mode).";
                            break;
                    }
                }
                if (param.Name == "topN")
                {
                    param.Description = "Количество верхних записей, которые требуется вернуть (например, топ N категорий расходов или доходов). Используется в режиме top для получения наиболее значимых категорий.";
                }
                if (param.Name == "from")
                {
                    param.Description = "Начальная дата периода для фильтрации данных. Формат: yyyy-MM-ddTHH:mm:ss. Используется для ограничения выборки по времени (например, транзакции за период). Обязателен для некоторых режимов (summary, top, filter, balance-history).";
                }
                if (param.Name == "to")
                {
                    param.Description = "Конечная дата периода для фильтрации данных. Формат: yyyy-MM-ddTHH:mm:ss. Используется совместно с параметром from для задания диапазона дат.";
                }
                if (param.Name == "min")
                {
                    param.Description = "Минимальное значение суммы или фильтра. Используется для ограничения выборки транзакций или других сущностей по сумме (например, только транзакции больше определённой суммы).";
                }
                if (param.Name == "max")
                {
                    param.Description = "Максимальное значение суммы или фильтра. Используется для ограничения выборки транзакций или других сущностей по сумме (например, только транзакции меньше определённой суммы).";
                }
                if (param.Name == "category")
                {
                    param.Description = "Категория транзакции, ограничения или другой сущности (например, 'Продукты', 'Транспорт', 'Здоровье'). Используется для фильтрации, группировки и аналитики. Может быть обязательным для некоторых операций.";
                }
                if (param.Name == "type")
                {
                    param.Description = "Тип транзакции, ограничения или другой сущности. Например: 'Доход', 'Расход', 'Плановая', 'Фактическая'. Используется для фильтрации, аналитики и создания объектов определённого типа.";
                }
                if (param.Name == "period")
                {
                    param.Description = "Период, к которому относится операция или фильтрация (например, 'месяц', 'год', 'неделя'). Используется для агрегирования или ограничения данных по времени.";
                }
                if (param.Name == "name")
                {
                    param.Description = "Название объекта (например, счёта, депозита, кредита, ограничения). Используется для идентификации и отображения пользователю. Может быть обязательным при создании.";
                }
            }
        }
    }
}
