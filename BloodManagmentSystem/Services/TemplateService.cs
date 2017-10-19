using System;
using System.IO;

namespace BloodManagmentSystem.Services
{
    public static class TemplateService
    {
        public static string RenderTemplate(string templateName, object model)
        {
            var templateFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Views\Email");
            var templateService = new RazorEngine.Templating.TemplateService();
            var confirmationTemplatePath = Path.Combine(templateFolderPath, templateName);

            return templateService.Parse(
                File.ReadAllText(confirmationTemplatePath),
                model,
                null,
                "TempalteCache"
            );
        }
    }
}