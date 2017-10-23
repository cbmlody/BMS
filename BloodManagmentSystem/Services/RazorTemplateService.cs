using RazorEngine.Templating;
using System;
using System.IO;

namespace BloodManagmentSystem.Services
{
    public static class RazorTemplateService
    {
        public static string RenderTemplate(string templateName, object model = null, DynamicViewBag viewBag = null)
        {
            var templateFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Views\Email");
            var templateService = new TemplateService();
            var confirmationTemplatePath = Path.Combine(templateFolderPath, templateName);

            return templateService.Parse(
                File.ReadAllText(confirmationTemplatePath),
                model,
                viewBag,
                "TemplateCache"
            );
        }
    }
}