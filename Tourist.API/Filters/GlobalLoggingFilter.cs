using Microsoft.AspNetCore.Mvc.Filters;

namespace Tourist.API.Filters
{
    public class GlobalLoggingFilter : IActionFilter
    {
        private readonly ILogger<GlobalLoggingFilter> _logger;

        public GlobalLoggingFilter(ILogger<GlobalLoggingFilter> logger)
        {
            _logger = logger;
        }
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var action = context.ActionDescriptor.DisplayName;
            var args = string.Join(", ", context.ActionArguments.Select(a => $"{a.Key}={a.Value}"));

            _logger.LogInformation("Executing {Action} with arguments {Arguments}", action, args);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            var action = context.ActionDescriptor.DisplayName;
            if (context.Exception == null)
                _logger.LogInformation("Executed {Action} successfully", action);
            else
                _logger.LogError(context.Exception, "Error while executing {Action}", action);
        }
    }
}
