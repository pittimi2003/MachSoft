using Mss.WorkForce.Code.Models.DBContext;
using Mss.WorkForce.Code.Models.Models;
using System.Globalization;

namespace Mss.WorkForce.Code.DataBaseManager
{
    public static class SeedDatabaseUtil
    {
        public static async Task SeedDatabaseAsync(this IServiceProvider services, AppSettings appSettings)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            Console.WriteLine($"Default insert: {appSettings.DefaultInsert}");

            if (appSettings.DefaultInsert == "deploy" && (context.Organizations == null || !context.Organizations.Any()))
            {

                Console.WriteLine("Inserting data...");

                if (appSettings?.Languages != null)
                {
                    foreach (var language in appSettings.Languages)
                    {
                        if (!context.Languages.Any(u => u.Name == language.Name)) context.Languages.Add(language);
                    }
                }
                if (appSettings?.SystemOfMeasurements != null)
                {
                    foreach (var system in appSettings.SystemOfMeasurements)
                    {
                        if (!context.SystemOfMeasurements.Any(u => u.Name == system.Name)) context.SystemOfMeasurements.Add(system);
                    }
                }
                if (appSettings?.DecimalSeparators != null)
                {
                    foreach (var decimalSeparators in appSettings.DecimalSeparators)
                    {
                        if (!context.DecimalSeparators.Any(u => u.Name == decimalSeparators.Name)) context.DecimalSeparators.Add(decimalSeparators);
                    }
                }
                if (appSettings?.ThousandsSeparators != null)
                {
                    foreach (var thousandsSeparator in appSettings.ThousandsSeparators)
                    {
                        if (!context.ThousandsSeparators.Any(u => u.Name == thousandsSeparator.Name)) context.ThousandsSeparators.Add(thousandsSeparator);
                    }
                }
                if (appSettings?.DateFormats != null)
                {
                    foreach (var dateFormat in appSettings.DateFormats)
                    {
                        if (!context.DateFormats.Any(u => u.Name == dateFormat.Name)) context.DateFormats.Add(dateFormat);
                    }
                }

                if (appSettings?.HourFormats != null)
                {
                    foreach (var hourFormat in appSettings.HourFormats)
                    {
                        if (!context.HourFormats.Any(u => u.Name == hourFormat.Name)) context.HourFormats.Add(hourFormat);
                    }
                }

                foreach (TimeZoneInfo timeZoneInfo in TimeZoneInfo.GetSystemTimeZones())
                {
                    if (!context.TimeZones.Any(u => u.Name == timeZoneInfo.DisplayName))
                    {
                        Models.Models.TimeZone timeZone = new Models.Models.TimeZone
                        {
                            Id = Guid.NewGuid(),
                            Name = timeZoneInfo.DisplayName,
                            OffSet = timeZoneInfo.BaseUtcOffset.Hours
                        };
                        context.SaveChanges();
                        context.TimeZones.Add(timeZone);                       
                    }                    
                }

                CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
                foreach (var culture in cultures)
                {
                    RegionInfo region = new RegionInfo(culture.Name);

                    if (!context.Countries.Any(m => m.Name == region.ToString()))
                    {
                        Country country = new Country
                        {
                            Id = Guid.NewGuid(),
                            Name = region.TwoLetterISORegionName,
                            IsActive = true
                        };

                        context.Countries.Add(country);
                        context.SaveChanges();
                    }
                }


                context.SaveChanges();
                if (appSettings?.Organizations != null)
                {
                    foreach (var organization in appSettings.Organizations)
                    {
                        organization.DecimalSeparator = context.DecimalSeparators.FirstOrDefault()!;
                        organization.DecimalSeparatorId = context.DecimalSeparators.FirstOrDefault(x => x.Id == organization.DecimalSeparator.Id)!.Id;
                        organization.ThousandsSeparator = context.ThousandsSeparators.FirstOrDefault()!;
                        organization.ThousandsSeparatorId = context.ThousandsSeparators.FirstOrDefault(x => x.Id == organization.ThousandsSeparator.Id)!.Id;
                        organization.DateFormat = context.DateFormats.FirstOrDefault()!;
                        organization.DateFormatId = context.DateFormats.FirstOrDefault(x => x.Id == organization.DateFormat.Id)!.Id;
                        organization.HourFormat = context.HourFormats.FirstOrDefault()!;
                        organization.HourFormatId = context.HourFormats.FirstOrDefault(x => x.Id == organization.HourFormat.Id)!.Id;
                        organization.Language = context.Languages.FirstOrDefault()!;
                        organization.LanguageId = context.Languages.FirstOrDefault(x => x.Id == organization.Language.Id)!.Id;
                        organization.SystemOfMeasurement = context.SystemOfMeasurements.FirstOrDefault()!;
                        organization.SystemOfMeasurementId = context.SystemOfMeasurements.FirstOrDefault(x => x.Id == organization.SystemOfMeasurement.Id)!.Id;
                        if (!context.Organizations.Any(u => u.Name == organization.Name)) context.Organizations.Add(organization);
                    }
                }
                context.SaveChanges();
                if (appSettings?.Users != null)
                {
                    foreach (var user in appSettings.Users)
                    {
                        user.Organization = context.Organizations.FirstOrDefault(x => x.Name == appSettings.Organizations.FirstOrDefault().Name);
                        user.OrganizationId = user.Organization.Id;
                        user.DecimalSeparator = user.DecimalSeparator == null || user.DecimalSeparator.Name == null ? null : context.DecimalSeparators.FirstOrDefault(x => x.Name == user.DecimalSeparator.Name);
                        user.DecimalSeparatorId = user.DecimalSeparator == null ? null : user.DecimalSeparator.Id;
                        user.ThousandsSeparator = user.ThousandsSeparator == null || user.ThousandsSeparator.Name == null ? null : context.ThousandsSeparators.FirstOrDefault(x => x.Name == user.ThousandsSeparator.Name);
                        user.ThousandsSeparatorId = user.ThousandsSeparator == null ? null : user.ThousandsSeparator.Id;
                        user.Language = user.Language == null || user.Language.Name == null ? null : context.Languages.FirstOrDefault(x => x.Name == user.Language.Name);
                        user.LanguageId = user.Language == null ? null : user.Language.Id;
                        user.DateFormat = user.DateFormat == null || user.DateFormat.Name == null ? null : context.DateFormats.FirstOrDefault(x => x.Name == user.DateFormat.Name);
                        user.DateFormatId = user.DateFormat == null ? null : user.DateFormat.Id;
                        user.HourFormat = user.HourFormat == null || user.HourFormat.Name == null ? null : context.HourFormats.FirstOrDefault(x => x.Name == user.HourFormat.Name);
                        user.HourFormatId = user.HourFormat == null ? null : user.HourFormat.Id;
                        user.CreationDate = DateTime.UtcNow;
                        if (!context.Users.Any(u => u.Name == user.Name)) context.Users.Add(user);
                    }
                }

                if (appSettings?.DockSelectionStrategies != null)
                {
                    foreach (var dockStrategy in appSettings.DockSelectionStrategies)
                    {
                        dockStrategy.Id = Guid.NewGuid();
                        dockStrategy.Code = dockStrategy.Code;
                        dockStrategy.OrganizationId = context.Organizations.FirstOrDefault(x => x.Name == appSettings.Organizations.FirstOrDefault().Name).Id;
                        if (!context.DockSelectionStrategies.Any(u => u.Code == dockStrategy.Code)) context.DockSelectionStrategies.Add(dockStrategy);
                    }
                }
                await context.SaveChangesAsync();
            }
        }
    }
}
