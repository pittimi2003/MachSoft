
using System.Text.Json;
using Mss.WorkForce.Code.Models.ConvertedModel;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.Models;

namespace Mss.WorkForce.Code.Web
{
    public class ModelConverter
    {
        public static string ConvertJson(string jsonModel)
        {
            var originalModel = ConvertirStringAModelo<OrganizationDto>(jsonModel);

            var convertedModel = new ModelJson
            {
                New = new Section
                {
                },

                Update = new Section
                {
                    Organizations = new List<Organization>
                    {
                        new Organization{
                        Id = originalModel.Id,
                        Code = originalModel.Code,
                        Name = originalModel.Name,
                        Description = originalModel.Description,
                        Address = originalModel.Adress.CompleteAdress,
                        ZIPCode = originalModel.Adress.ZipCode,
                        AddressLine = originalModel.Adress.AdressLine,
                        City = originalModel.Adress.City,
                        State = originalModel.Adress.State,
                        CountryId = originalModel.Adress.Country.Id,
                        Country = originalModel.Adress.Country,
                        AddressComment = originalModel.Adress.Comment,
                        ContactName = originalModel.Contact.Name,
                        Telephone = originalModel.Contact.Telephone,
                        Extension = originalModel.Contact.Extension,
                        Fax = originalModel.Contact.Fax,
                        Email = originalModel.Contact.Email,
                        ContactComment = originalModel.Contact.Comment,
                        DecimalSeparatorId = originalModel.RegionalSettings.DecimalSeparator.Id,
                        DecimalSeparator = originalModel.RegionalSettings.DecimalSeparator,
                        ThousandsSeparatorId = originalModel.RegionalSettings.ThousandsSeparator.Id,
                        ThousandsSeparator = originalModel.RegionalSettings.ThousandsSeparator,
                        DateFormatId = originalModel.RegionalSettings.DateFormat.Id,
                        DateFormat = originalModel.RegionalSettings.DateFormat,
                        HourFormatId = originalModel.RegionalSettings.HourFormat.Id,
                        HourFormat = originalModel.RegionalSettings.HourFormat,
                        LanguageId = originalModel.RegionalSettings.Language.Id,
                        Language = originalModel.RegionalSettings.Language,
                        SystemOfMeasurementId = originalModel.RegionalSettings.SystemOfMeasurement.Id,
                        SystemOfMeasurement = originalModel.RegionalSettings.SystemOfMeasurement,
                        Logo = originalModel.Logo,
                        }
                    }
                },

                Delete = new Section
                {
                },
            };

            return JsonSerializer.Serialize(convertedModel, new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
        }

        private static TModel ConvertirStringAModelo<TModel>(string json)
        {
            var options = new JsonSerializerOptions
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
            };

            return JsonSerializer.Deserialize<TModel>(json, options);
        }
    }
}
