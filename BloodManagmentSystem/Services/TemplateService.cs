using System;
using System.IO;
using RazorEngine.Templating;

namespace BloodManagmentSystem.Services
{
    public static class TemplateService
    {
        public static string RenderTemplate(string templateName, object model = null, DynamicViewBag viewBag = null)
        {
            var templateFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Views\Email");
            var templateService = new RazorEngine.Templating.TemplateService();
            var confirmationTemplatePath = Path.Combine(templateFolderPath, templateName);

            return templateService.Parse(
                File.ReadAllText(confirmationTemplatePath),
                model,
                viewBag,
                "TempalteCache"
            );
        }
    }
}